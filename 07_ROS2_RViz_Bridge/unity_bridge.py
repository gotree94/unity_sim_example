#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
unity_bridge.py
==============
Unity(TurtleBot3 시뮬레이션) ↔ ROS2 Jazzy 브릿지 노드.

동작 구조:
    Unity  (RosBridge.cs, TCP 서버)  0.0.0.0:8765  (Windows)
       ↑ TCP
    본 노드 (TCP 클라이언트)  host.docker.internal:8765  (Docker ros_jazzy1)
       → ROS2 토픽 발행: /map, /scan, /odom, /image_raw, /tf, /tf_static
       → RViz2에서 표시

실행 방법 (Docker 컨테이너 ros_jazzy1 안에서):
    python3 /mnt/d/github/unity_sim_example/07_ROS2_RViz_Bridge/unity_bridge.py

프로토콜 (little-endian, Unity RosBridge.cs와 동일):
    [헤더 11바이트] uint32 magic("TBR1"), uint8 msgType, uint32 payloadLen
    [map  =1] float32 res, int32 w, int32 h,
              float64 ox, float64 oy, float64 oyaw,
              uint32 dataLen, int8[dataLen]
    [scan =2] float32 angle_min, angle_max, angle_increment,
              float32 range_min, range_max, uint32 rayCount, float32[rayCount]
    [odom =3] float64 x, float64 z, float64 yaw_deg(Unity), float64 linear_x, float64 angular_z
    [image=4] int32 width, int32 height, int32 encoding(0=mono8/1=rgba8),
              uint32 dataLen, int8[dataLen]

좌표 변환 (Unity ↔ ROS):
    Unity(x 오른쪽, z 북쪽, y 위) → ROS(x 동쪽, y 북쪽, z 위)
      - ROS 위치 = (Unity x, Unity z, 0)
      - ROS 요각 = π/2 - yaw_deg(Unity)  (REP-103 기준 프레임 정합)
    TF 트리: map(=odom) → base_footprint → base_scan(lidar), camera_link(camera)
"""
import math
import socket
import struct
import threading
import time

import rclpy
from rclpy.node import Node
from rclpy.qos import QoSProfile, QoSReliabilityPolicy, QoSDurabilityPolicy

from geometry_msgs.msg import TransformStamped
from nav_msgs.msg import OccupancyGrid, Odometry
from sensor_msgs.msg import Image, LaserScan
from std_msgs.msg import Header
from tf2_ros import TransformBroadcaster, StaticTransformBroadcaster

# ROS 메시지에 사용할 프레임 이름
FRAME_MAP = "map"
FRAME_ODOM = "odom"
FRAME_BASE = "base_footprint"
FRAME_LIDAR = "base_scan"
FRAME_CAMERA = "camera_link"   # 카메라 프레임 (7장 camera_link, /image_raw용)

# TCP 접속 설정 (Unity RosBridge.cs와 일치)
TCP_HOST = "host.docker.internal"
TCP_PORT = 8765
MAGIC = 0x31524254   # "TBR1"


def quat_from_yaw(yaw_rad: float):
    """z축(yaw) 회전의 쿼터니언 (x, y, z, w) 반환 (tf_transformations 없이 직접 계산)."""
    return (0.0, 0.0, math.sin(yaw_rad * 0.5), math.cos(yaw_rad * 0.5))


class UnityBridge(Node):
    def __init__(self):
        super().__init__("unity_bridge")

        # --- 발행자 생성 ---
        # /map 은 TRANSIENT_LOCAL(최신 맵 값 유지)로, /scan과 /odom은 기본 QOS로 발행.
        qos_map = QoSProfile(
            depth=1,
            reliability=QoSReliabilityPolicy.RELIABLE,
            durability=QoSDurabilityPolicy.TRANSIENT_LOCAL,
        )
        self.map_pub = self.create_publisher(OccupancyGrid, "/map", qos_map)
        self.scan_pub = self.create_publisher(LaserScan, "/scan", 10)
        self.odom_pub = self.create_publisher(Odometry, "/odom", 10)
        self.image_pub = self.create_publisher(Image, "/image_raw", 10)

        # TF 브로드캐스터
        self.tf_broadcaster = TransformBroadcaster(self)
        self.tf_static_broadcaster = StaticTransformBroadcaster(self)

        # 정적 TF 한 번 발행: map→odom(동일), base_footprint→base_scan(0,0,0.055)
        self.publish_static_tf()

        # 연결 상태 (로드 로깅용)
        self.connected = False

    # ------------------------------------------------------------------
    def publish_static_tf(self):
        """정적 TF: map→odom(identity), base_footprint→base_scan(0,0,0.055)."""
        tfs = []

        # map → odom : identity (Unity 맵은 전역 고정이므로 odom=map)
        t_om = TransformStamped()
        t_om.header.frame_id = FRAME_MAP
        t_om.child_frame_id = FRAME_ODOM
        t_om.transform.translation.x = 0.0
        t_om.transform.translation.y = 0.0
        t_om.transform.translation.z = 0.0
        q = quat_from_yaw(0.0)
        t_om.transform.rotation.x, t_om.transform.rotation.y = q[0], q[1]
        t_om.transform.rotation.z, t_om.transform.rotation.w = q[2], q[3]
        tfs.append(t_om)

        # base_footprint → base_scan : 센서 위치(0, 0, 0.055)
        t_bs = TransformStamped()
        t_bs.header.frame_id = FRAME_BASE
        t_bs.child_frame_id = FRAME_LIDAR
        t_bs.transform.translation.x = 0.0
        t_bs.transform.translation.y = 0.0
        t_bs.transform.translation.z = 0.055
        q = quat_from_yaw(0.0)
        t_bs.transform.rotation.x, t_bs.transform.rotation.y = q[0], q[1]
        t_bs.transform.rotation.z, t_bs.transform.rotation.w = q[2], q[3]
        tfs.append(t_bs)

        # base_footprint → camera_link : 카메라 위치(0, 0.03, 0.055)
        t_cam = TransformStamped()
        t_cam.header.frame_id = FRAME_BASE
        t_cam.child_frame_id = FRAME_CAMERA
        t_cam.transform.translation.x = 0.0
        t_cam.transform.translation.y = 0.03
        t_cam.transform.translation.z = 0.055
        q = quat_from_yaw(0.0)
        t_cam.transform.rotation.x, t_cam.transform.rotation.y = q[0], q[1]
        t_cam.transform.rotation.z, t_cam.transform.rotation.w = q[2], q[3]
        tfs.append(t_cam)

        # stamp 오래된 시간이면 안 되므로 현재 시간 사용
        now = self.get_clock().now().to_msg()
        for t in tfs:
            t.header.stamp = now

        self.tf_static_broadcaster.sendTransform(tfs)
        self.get_logger().info(f"[유니티 브릿지] 정적 TF 발행: map→odom, {FRAME_BASE}→{FRAME_LIDAR}, {FRAME_BASE}→{FRAME_CAMERA}")

    # ------------------------------------------------------------------
    def recv_exact(self, conn, n):
        """소켓에서 정확히 n바이트를 읽을 때까지 수신(부분 수신/헤더 분리 대응)."""
        buf = b""
        while len(buf) < n:
            chunk = conn.recv(n - len(buf))
            if not chunk:
                raise ConnectionError("연결 종료")
            buf += chunk
        return buf

    # ------------------------------------------------------------------
    def handle_frame(self, msg_type: int, payload: bytes):
        """수신한 프레임을 msg_type에 따라 ROS2 토픽으로 발행한다."""
        if msg_type == 1:
            self.handle_map(payload)
        elif msg_type == 2:
            self.handle_scan(payload)
        elif msg_type == 3:
            self.handle_odom(payload)
        elif msg_type == 4:
            self.handle_image(payload)
        else:
            self.get_logger().warn(f"[브릿지] 알 수 없는 msgType: {msg_type}")

    # ------------------- /map -------------------
    def handle_map(self, payload: bytes):
        # float32(4)+int32×2(8)+float64×3(24) = 36
        head = struct.unpack_from("<fii", payload, 0)
        res, w, h = head
        ox, oy, oyaw = struct.unpack_from("<ddd", payload, 36)
        data_len = struct.unpack_from("<I", payload, 60)[0]
        raw = payload[64:64 + data_len]
        # int8 배열 → 0~100(-1=unknown) 점유값
        data = list(struct.unpack(f"<{data_len}b", raw))

        msg = OccupancyGrid()
        msg.header = Header()
        msg.header.stamp = self.get_clock().now().to_msg()
        msg.header.frame_id = FRAME_MAP
        msg.info.resolution = float(res)
        msg.info.width = w
        msg.info.height = h
        msg.info.origin.position.x = ox          # 좌하단 원점 (Unity x)
        msg.info.origin.position.y = oy          # 좌하단 원점 (Unity z → ROS y)
        msg.info.origin.position.z = 0.0
        # yaw=0 (맵은 정북 정렬)
        msg.info.origin.orientation.x = 0.0
        msg.info.origin.orientation.y = 0.0
        msg.info.origin.orientation.z = 0.0
        msg.info.origin.orientation.w = 1.0
        msg.data = data

        self.map_pub.publish(msg)

    # ------------------- /scan -------------------
    def handle_scan(self, payload: bytes):
        # float32×5(20) + uint32(4) = 24, 이후 float32[rayCount]
        (a_min, a_max, a_inc, r_min, r_max) = struct.unpack_from("<fffff", payload, 0)
        ray_count = struct.unpack_from("<I", payload, 20)[0]
        ranges = struct.unpack_from(f"<{ray_count}f", payload, 24)

        msg = LaserScan()
        msg.header = Header()
        msg.header.stamp = self.get_clock().now().to_msg()
        msg.header.frame_id = FRAME_LIDAR
        msg.angle_min = float(a_min)
        msg.angle_max = float(a_max)
        msg.angle_increment = float(a_inc)
        msg.range_min = float(r_min)
        msg.range_max = float(r_max)
        msg.ranges = list(ranges)

        self.scan_pub.publish(msg)

    # ------------------- /odom + TF -------------------
    def handle_odom(self, payload: bytes):
        # float64 × 5: x, z, yaw_deg(Unity), linear_x, angular_z
        x, z, yaw_deg, lin_x, ang_z = struct.unpack("<ddddd", payload)

        # 좌표 변환: Unity(x, z) → ROS(x, y), 요각 변환
        px, py, pz = x, z, 0.0
        yaw_ros = math.pi * 0.5 - math.radians(yaw_deg)

        # --- /odom 메시지 (Odometry) ---
        odom = Odometry()
        odom.header = Header()
        odom.header.stamp = self.get_clock().now().to_msg()
        odom.header.frame_id = FRAME_ODOM
        odom.child_frame_id = FRAME_BASE
        odom.pose.pose.position.x = px
        odom.pose.pose.position.y = py
        odom.pose.pose.position.z = pz
        qx, qy, qz, qw = quat_from_yaw(yaw_ros)
        odom.pose.pose.orientation.x = qx
        odom.pose.pose.orientation.y = qy
        odom.pose.pose.orientation.z = qz
        odom.pose.pose.orientation.w = qw
        # 공분산은 0(미지)으로 두면 RViz Odometry 표시에 문제없음
        odom.twist.twist.linear.x = lin_x
        odom.twist.twist.angular.z = ang_z

        self.odom_pub.publish(odom)

        # --- TF: odom → base_footprint ---
        tf = TransformStamped()
        tf.header.stamp = odom.header.stamp
        tf.header.frame_id = FRAME_ODOM
        tf.child_frame_id = FRAME_BASE
        tf.transform.translation.x = px
        tf.transform.translation.y = py
        tf.transform.translation.z = pz
        tf.transform.rotation.x = qx
        tf.transform.rotation.y = qy
        tf.transform.rotation.z = qz
        tf.transform.rotation.w = qw

        self.tf_broadcaster.sendTransform(tf)

    # ------------------- /image_raw -------------------
    def handle_image(self, payload: bytes):
        # int32×3(12) + uint32(4) = 16, 이후 픽셀 dataLen바이트
        w, h, enc = struct.unpack_from("<iii", payload, 0)
        data_len = struct.unpack_from("<I", payload, 12)[0]
        raw = payload[16:16 + data_len]

        msg = Image()
        msg.header = Header()
        msg.header.stamp = self.get_clock().now().to_msg()
        msg.header.frame_id = FRAME_CAMERA
        msg.height = int(h)
        msg.width = int(w)
        if enc == 0:
            msg.encoding = "mono8"        # 흑백 (픽셀당 1바이트)
            msg.step = int(w)
        else:
            msg.encoding = "rgba8"        # 원본 컬러 (픽셀당 4바이트 R,G,B,A)
            msg.step = int(w) * 4
        # bytes는 uint8 배열로 바로 직렬화되므로 30만개 리스트 생성 오버헤드를 피할 수 있습니다.
        msg.data = raw

        self.image_pub.publish(msg)

    # ------------------------------------------------------------------
    def run(self):
        """Unity(TCP 서버)에 접속하여 프레임을 읽고 발행하는 메인 루프."""
        self.get_logger().info(
            f"[유니티 브릿지] 대기 중: Unity가 {TCP_HOST}:{TCP_PORT}에서 서버를 열기를 기다립니다..."
        )

        while rclpy.ok():
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            try:
                sock.settimeout(2.0)   # 재접속 대기용 타임아웃
                sock.connect((TCP_HOST, TCP_PORT))
                self.connected = True
                self.get_logger().info("[유니티 브릿지] Unity에 연결됨! 데이터 수신 시작.")

                while rclpy.ok():
                    # 프레임 헤더: magic(4) + msgType(1) + payloadLen(4)
                    header = self.recv_exact(sock, 9)
                    magic, msg_type, payload_len = struct.unpack("<IBI", header)
                    if magic != MAGIC:
                        self.get_logger().warn(f"[브릿지] magic 불일치: {hex(magic)} — 동기화 끊김, 접속 재시도")
                        break
                    payload = self.recv_exact(sock, payload_len)
                    self.handle_frame(msg_type, payload)

            except (ConnectionError, socket.timeout, OSError) as e:
                self.connected = False
                self.get_logger().warn(f"[유니티 브릿지] 연결 끊김/오류: {e} — 2초 후 재접속")
                time.sleep(2.0)
            except Exception as e:   # 파싱 오류 시에도 루프 유지
                self.connected = False
                self.get_logger().error(f"[유니티 브릿지] 처리 오류: {e}")
                time.sleep(1.0)
            finally:
                try:
                    sock.close()
                except Exception:
                    pass
                self.connected = False


def main(args=None):
    rclpy.init(args=args)
    bridge = UnityBridge()
    try:
        bridge.run()
    except KeyboardInterrupt:
        pass
    finally:
        bridge.destroy_node()
        if rclpy.ok():
            rclpy.shutdown()


if __name__ == "__main__":
    main()