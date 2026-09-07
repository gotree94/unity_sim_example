using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

// ############################################################
// # RosBridge
// # 역할: Unity(TurtleBot3 시뮬레이션)에서 만든 센서/맵 데이터를
// #       TCP/IP로 Docker 안의 ROS2(Python 브릿지 노드)에게 보내,
// #       RViz2에서 실제 /map, /scan, /odom, /tf로 표시하게 합니다.
// #
// # 통신 구조 (이 저장소 3단계 TCPServer 패턴과 동일):
// #    Unity(RosBridge, 서버) 127.0.0.1:8765
// #       ↑ TCP
// #    Docker 컨테이너(ros_jazzy1) unity_bridge.py (클라이언트)
// #       → host.docker.internal:8765 로 접속 (Docker -p 8765:8765)
// #       → /map /scan /odom /tf 를 ROS2 토픽으로 발행
// #       → RViz2에서 표시
// #
// # 프로토콜 (이진 프레임, little-endian, C#/Python 양쪽 동일):
// #   [헤더 11바이트]
// #     uint32 magic     = 0x31524254 ("TBR1")
// #     uint8  msgType   = 1:map / 2:scan / 3:odom
// #     uint32 payloadLen
// #   [map payload]
// #     float32 resolution, int32 width, int32 height
// #     float64 origin_x, float64 origin_y, float64 origin_yaw
// #     uint32 dataLen, int8[dataLen]  (-1 unknown / 0 free / 100 occupied)
// #   [scan payload]
// #     float32 angle_min, angle_max, angle_increment
// #     float32 range_min, range_max
// #     uint32 rayCount, float32[rayCount]
// #   [odom payload]
// #     float64 x, float64 z (Unity 위치, Python에서 z→ROS y 변환)
// #     float64 yaw_deg (Unity 요각, Python에서 ROS yaw로 변환)
// #     float64 linear_x (전진속도), float64 angular_z (각속도)
// ############################################################
public class RosBridge : MonoBehaviour
{
    [Header("TCP 서버 설정")]
    [Tooltip("리스닝 포트 (Docker의 -p 8765:8765 와 일치)")]
    public int port = 8765;
    [Tooltip("데이터 발행 주기(Hz). 너무 빠르면 맵 200x200 전송이 부담됨 (기본 10Hz)")]
    public float publishRate = 10f;

    [Header("데이터 참조 (Inspector 연결 or 자동 탐색)")]
    [Tooltip("LidarSensor (base_scan) — /scan 전송용")]
    public LidarSensor lidar;
    [Tooltip("MapRenderer (MapDisplay) — /map 전송용")]
    public MapRenderer mapRenderer;
    [Tooltip("로봇 루트 Transform (turtlebot3_burger) — /odom 위치/회전 기준")]
    public Transform robotTransform;

    // ----- 연결 상태 (Inspector에서 확인용) -----
    [Header("상태")]
    public int connectedClients = 0;
    public bool isServerRunning = false;

    // TCP 서버 멤버
    private TcpListener server;
    private Thread serverThread;
    private bool isRunning = false;

    // 연결된 클라이언트 목록 (백그라운드 스레드와 메인 스레드가 함께 접근 → lock 필요)
    private readonly List<TcpClient> clients = new List<TcpClient>();
    private readonly object clientLock = new object();

    // 발행 타이머
    private float sendTimer = 0f;

    void Start()
    {
        // Inspector에서 연결 안 하면 자동으로 씬에서 찾아줍니다.
        if (lidar == null)
            lidar = FindFirstObjectByType<LidarSensor>();
        if (mapRenderer == null)
            mapRenderer = FindFirstObjectByType<MapRenderer>();
        if (robotTransform == null && lidar != null && lidar.transform.parent != null)
            robotTransform = lidar.transform.root;

        if (lidar == null || mapRenderer == null)
        {
            Debug.LogWarning("[RosBridge] LidarSensor 또는 MapRenderer를 찾지 못했습니다. Inspector에서 연결해주세요.");
            return;
        }

        StartServer();

        // 초기화 확인용 로그
        Debug.Log($"[RosBridge] 시작됨. 포트={port}, lidar={lidar.gameObject.name}, mapRenderer={mapRenderer.gameObject.name}, robot={robotTransform?.name}");
    }

    void OnDestroy()
    {
        StopServer();
    }

    void OnApplicationQuit()
    {
        StopServer();
    }

    // ---------- TCP 서버 수명 관리 ----------

    // TCP 서버를 시작하고, 백그라운드 스레드에서 클라이언트 연결을 기다립니다.
    void StartServer()
    {
        try
        {
            // IPAddress.Any(0.0.0.0)으로 바인딩해야 Docker 컨테이너의 host.docker.internal 접속이 도달합니다.
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            isRunning = true;
            isServerRunning = true;

            serverThread = new Thread(AcceptLoop);
            serverThread.IsBackground = true;
            serverThread.Start();

            Debug.Log($"[RosBridge] TCP 서버 시작됨 - 0.0.0.0:{port}");
            Debug.Log($"[RosBridge] 컨테이너의 unity_bridge.py가 host.docker.internal:{port} 로 접속 대기 중...");
        }
        catch (Exception e)
        {
            Debug.LogError($"[RosBridge] 서버 시작 실패: {e.Message} (포트 {port}가 이미 사용 중일 수 있습니다)");
        }
    }

    void StopServer()
    {
        isRunning = false;
        isServerRunning = false;

        try
        {
            server?.Stop();
        }
        catch { }

        lock (clientLock)
        {
            foreach (var c in clients)
            {
                try { c.Close(); } catch { }
            }
            clients.Clear();
            connectedClients = 0;
        }
    }

    // 연결 수락 루프 (백그라운드 스레드)
    void AcceptLoop()
    {
        while (isRunning)
        {
            try
            {
                TcpClient client = server.AcceptTcpClient();
                lock (clientLock)
                {
                    clients.Add(client);
                    connectedClients = clients.Count;
                }
                Debug.Log($"[RosBridge] 클라이언트 연결됨! (현재 {connectedClients}개)");
            }
            catch
            {
                // 서버 중지 또는 accept 오류 시 루프 종료
                break;
            }
        }
    }

    // ---------- 발행 루프 (메인 스레드, Unity API 안전) ----------

    void Update()
    {
        if (!isRunning || lidar == null || mapRenderer == null) return;

        sendTimer += Time.deltaTime;
        if (sendTimer >= 1f / publishRate)
        {
            sendTimer = 0f;
            BroadcastFrames();
        }
    }

    // 모든 클라이언트에게 map/scan/odom 프레임을 전송
    void BroadcastFrames()
    {
        lock (clientLock)
        {
            for (int i = clients.Count - 1; i >= 0; i--)
            {
                TcpClient client = clients[i];
                try
                {
                    if (!client.Connected)
                    {
                        clients.RemoveAt(i);
                        continue;
                    }

                    byte[] mapData = SerializeMap();
                    byte[] scanData = SerializeScan();
                    byte[] odomData = SerializeOdom();

                    NetworkStream stream = client.GetStream();
                    stream.Write(mapData, 0, mapData.Length);
                    stream.Write(scanData, 0, scanData.Length);
                    stream.Write(odomData, 0, odomData.Length);
                    stream.Flush();
                }
                catch
                {
                    // 전송 실패(연결 끊김 등) → 클라이언트 제거
                    try { client.Close(); } catch { }
                    clients.RemoveAt(i);
                }
            }
            connectedClients = clients.Count;
        }
    }

    // ---------- 직렬화 ----------

    // /map 프레임 만들기 (nav_msgs/OccupancyGrid 데이터)
    byte[] SerializeMap()
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            // 헤더: magic + msgType(1=map) + payloadLen(나중에 채움)
            bw.Write(0x31524254u);          // "TBR1"
            bw.Write((byte)1);              // 1 = map
            long lenPos = ms.Position;
            bw.Write(0u);                   // payloadLen 자리

            // payload
            bw.Write(mapRenderer.resolution);                    // float32
            bw.Write(mapRenderer.gridSize);                      // int32 width
            bw.Write(mapRenderer.gridSize);                      // int32 height
            Vector3 origin = mapRenderer.GetMapOrigin();         // 좌하단 원점
            bw.Write((double)origin.x);                          // float64 origin_x
            bw.Write((double)origin.z);                          // float64 origin_y (Unity z ↔ ROS y)
            bw.Write(0.0);                                       // float64 origin_yaw

            sbyte[] occ = mapRenderer.GetOccupancyData();
            bw.Write((uint)occ.Length);                          // dataLen
            foreach (sbyte v in occ) bw.Write(unchecked((byte)v)); // 1바이트로 기록 (-1→255, Python에서 signed로 해석)

            // payloadLen 채우기
            long endPos = ms.Position;
            ms.Position = lenPos;
            bw.Write((uint)(endPos - lenPos - 4));
            ms.Position = endPos;

            return ms.ToArray();
        }
    }

    // /scan 프레임 만들기 (sensor_msgs/LaserScan 데이터)
    // 각도 규칙 정합: Unity는 정면(+Z)·동쪽으로 각도 증가(시계방향),
    // ROS LaserScan은 센서 +X(전방)·+Y 방향으로 증가(반시계방향) = 좌우 반대.
    // 따라서 ranges를 [역순]으로 보내 map/로봇/스캔이 일치하게 만든다.
    byte[] SerializeScan()
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(0x31524254u);
            bw.Write((byte)2);              // 2 = scan
            long lenPos = ms.Position;
            bw.Write(0u);

            int rayCount = lidar.rayCount;
            bw.Write(0f);                                       // angle_min = 0
            bw.Write((float)(Mathf.PI * 2));                    // angle_max = 2π
            bw.Write((float)(Mathf.PI * 2 / rayCount));         // angle_increment
            bw.Write(lidar.rangeMin);                           // range_min
            bw.Write(lidar.rangeMax);                           // range_max
            bw.Write((uint)rayCount);                           // rayCount

            // ranges: Infinity는 ROS에 못 보내므로 rangeMax로 clamp + 역순 전송
            for (int i = 0; i < rayCount; i++)
            {
                float r = lidar.ranges[rayCount - 1 - i];
                if (float.IsInfinity(r) || float.IsNaN(r))
                    r = lidar.rangeMax;
                bw.Write(r);
            }

            long endPos = ms.Position;
            ms.Position = lenPos;
            bw.Write((uint)(endPos - lenPos - 4));
            ms.Position = endPos;

            return ms.ToArray();
        }
    }

    // /odom 프레임 만들기 (nav_msgs/Odometry + odom→base_footprint TF)
    // 좌표 변환 주의: Unity(x, z, yaw) → ROS(x, y, yaw) 변환은 Python 쪽에서 수행합니다.
    //   - ROS(위치) = (Unity x, Unity z, 0)
    //   - ROS(요각) = π/2 - Unity yaw (프레임 정합, REP-103)
    byte[] SerializeOdom()
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(0x31524254u);
            bw.Write((byte)3);              // 3 = odom
            long lenPos = ms.Position;
            bw.Write(0u);

            // 위치: 로봇 루트 또는 센서 기준
            Vector3 pos = (robotTransform != null) ? robotTransform.position : lidar.transform.position;
            float yawDeg = (robotTransform != null) ? robotTransform.eulerAngles.y : lidar.transform.eulerAngles.y;

            // 속도: Rigidbody가 있으면 사용
            float linearX = 0f;
            float angularZ = 0f;
            if (robotTransform != null)
            {
                Rigidbody rb = robotTransform.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 localVel = robotTransform.InverseTransformDirection(rb.velocity);
                    linearX = localVel.z;                  // 전진 = 로컬 +Z
                    angularZ = rb.angularVelocity.y;       // 요 회전 속도
                }
            }

            bw.Write((double)pos.x);         // x
            bw.Write((double)pos.z);         // z (Python에서 ROS y로 배치)
            bw.Write((double)yawDeg);        // Unity 요각(deg) — Python에서 변환
            bw.Write((double)linearX);       // 전진 속도
            bw.Write((double)angularZ);      // 각속도

            long endPos = ms.Position;
            ms.Position = lenPos;
            bw.Write((uint)(endPos - lenPos - 4));
            ms.Position = endPos;

            return ms.ToArray();
        }
    }

    // ---------- 유틸 ----------

    // Unity 버전별 호환 검색: 2023+는 FindFirstObjectByType, 이전 버전은 FindObjectOfType 사용
    private static T FindFirstObjectByType<T>() where T : UnityEngine.Object
    {
#if UNITY_2023_1_OR_NEWER
        return UnityEngine.Object.FindFirstObjectByType<T>();
#else
        return UnityEngine.Object.FindObjectOfType<T>();
#endif
    }
}