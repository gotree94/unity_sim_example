# 7단계: 자율주행 최종 프로젝트 - 환경 구성, ROS 연동, 데이터 생성, AI 학습, 자율주행

> **목적**: 지금까지 만든 TurtleBot3 시뮬레이션(5~6단계)을 확장하여, 센서 사양 기반 환경·장애물 구성 → ROS 연동 → 학습 데이터 생성 → AI 학습 → **시뮬레이션 환경에서의 자율주행 완성**  
> **소요 시간**: 약 480 ~ 720분 (8시간 ~ 12시간, 여러 세션으로 분할 권장)  
> **전제 조건**: 5단계(로봇), 6단계(센서) 완료  
> **사용 환경**: Windows 10/11 + Python 3.10+ + (선택) ROS2 / Isaac Sim

---

## 이 문서의 거시적 목표

이 튜토리얼 시리즈(1~7단계)의 **궁극적 목표**는 게임 엔진(Unity)으로 만든 가상 환경에서:

```
실제 로봇(터틀봇)과 장애물을 대량으로 시험하기 어려운 한계를 극복하고,
시뮬레이션을 통해 자율주행에 필요한 데이터를 무한히 생성하며,
그 데이터로 AI를 학습시켜 자율주행 동작까지 완성한다
```

이 목표는 NVIDIA Isaac Sim이 추구하는 "**시뮬레이션 우선(sim-first)**" 접근 방식과 동일합니다.

### 자율주행 완성을 위한 로드맵

```
[5단계] 로봇 구동        [6단계] 센서       [7단계] 자율주행(이 문서)
  터틀봇 URDF+물리    →   LiDAR/맵/오도/IMU →   환경 | ROS | 데이터 | 학습 | 주행
                                                    ↑
                                         1. 환경·장애물 구성
                                         2. ROS/통신 연동
                                         3. 학습 데이터 생성
                                         4. AI 학습 (RL/DDQN)
                                         5. 자율주행 최종 완성
```

---

## 목차

1. [자율주행 파이프라인 개요](#1-자율주행-파이프라인-개요)
2. [환경 구성: 센서 사양에 맞는 씬 구축](#2-환경-구성-센서-사양에-맞는-씬-구축)
3. [장애물 생성 및 커스터마이징](#3-장애물-생성-및-커스터마이징)
4. [ROS 연동: 센서 데이터 외부 전송](#4-ros-연동-센서-데이터-외부-전송)
5. [학습 데이터 생성](#5-학습-데이터-생성)
6. [AI 학습: 강화학습 기반 자율주행](#6-ai-학습-강화학습-기반-자율주행)
7. [자율주행 최종 구현](#7-자율주행-최종-구현)
8. [종합 검증 및 응용](#8-종합-검증-및-응용)
9. [문제 해결 체크리스트](#9-문제-해결-체크리스트)

---

## 1. 자율주행 파이프라인 개요

### 1-1. 자율주행 4단계 구조 (Perception → Localization → Planning → Control)

자율주행 로봇은 4단계 파이프라인을 순환합니다:

```
 ① 인식(Perception)    ② 위치추정(Localization)   ③ 경로계획(Planning)   ④ 제어(Control)
   LiDAR 스캔            오도메트리 + 맵 매칭        목적지→경로 탐색        모터 속도 명령
   /scan                /odom + SLAM                A*/Dijkstra            /cmd_vel
```

| 단계 | 데이터 | 우리 구현 |
|------|--------|----------|
| ① 인식 | `/scan` (LiDAR) | 6단계 LidarSensor |
| ② 위치추정 | `/odom`, `/tf`, `/imu` | 6단계 OdometrySensor, ImuSensor |
| ③ 경로계획 | 점유격자 맵 | 6단계 MapRenderer + 7단계 경로탐색 |
| ④ 제어 | `/cmd_vel` (회전·선속) | 7단계 딥러닝 정책 → TurtleBot3Controller |

### 1-2. 본 문서의 구현 전략

| 구성 요소 | 도구 | 역할 |
|-----------|------|------|
| **시뮬레이터** | Unity (5~6단계) | 물리 환경 + 센서 데이터 생성 |
| **통신** | TCP/IP 또는 ROS2 브릿지 | Unity ↔ Python/ROS 데이터 교환 |
| **SLAM/맵** | Python(cartographer 유사) | LiDAR로 점유격자 맵 구축 |
| **학습/주행** | Python (PyTorch, DDQN) | 강화학습 정책으로 자율주행 |

> **핵심 개념**: Unity는 "세상의 센서 데이터"만 정확히 내보내고, **지능(학습/주행)**은 Python/ROS가 담당합니다. 이는 실제 로봇에서 하드웨어(터틀봇)와 소프트웨어(ROS/AI)를 분리하는 것과 동일합니다.

---

## 2. 환경 구성: 센서 사양에 맞는 씬 구축

### 2-1. 센서 사양 정리 (6단계 요약)

| 센서 | 사양 | 활용 |
|------|------|------|
| LiDAR | 360점, 1°, 0.12~3.5m, 5Hz | 장애물 거리 → 인식, SLAM |
| 오도메트리 | 위치/방향/선속/각속도 | 자기 위치 추정 |
| IMU | 각속도/가속도 | 자세·운동 측정 |

### 2-2. 씬 크기와 해상도 설정

Unity 씬의 물리 세계가 센서 사양(LiDAR 3.5m)과 어울리도록 구성:

| 항목 | 권장값 | 이유 |
|------|--------|------|
| 바닥(Ground) 크기 | 20m × 20m | LiDAR 3.5m가 유효하도록 여유 |
| 로봇 출발 지점 | (0, 0, 0) | 맵 중심 = 오도메트리 원점 |
| 조명 | Directional Light 1개 | 물리 기반(센서 Raycast엔 영향 없음) |

**Ground 생성**: Hierarchy → 3D Object → **Plane** → Scale (10, 1, 10) → Position (0,0,0)

> 6단계의 Ground(Quad 아닌 Plane)와 로봇이 원점 부근에 오도록 정렬합니다.

### 2-3. 기준 좌표계 일치 (ROS ↔ Unity)

자율주행에서 가장 중요한 것은 **좌표계 일치**입니다.

| 축 | Unity | ROS |
|----|-------|-----|
| 전진(forward) | +Z | +X |
| 좌우(left) | +X | +Y |
| 상하(up) | +Y | +Z |

> 6단계에서 LiDAR Raycast는 Unity 기준(Z+ forward)으로 작성했습니다. **Python/ROS로 보낼 때는 반드시 이 좌표 변환을 적용**해야 SLAM 맵이 올바르게 그려집니다.

**좌표 변환 함수 (Python 쪽, 4단계에서 사용)**
```python
def ros_from_unity(u: (float, float, float)):
    # Unity (x, y, z) → ROS (x, y, z=up)
    # Unity forward=+Z → ROS forward=+X, Unity up=+Y → ROS up=+Z
    return (u[2], -u[0], u[1])
```

---

## 3. 장애물 생성 및 커스터마이징

### 3-1. 기본 장애물 생성 (6단계 확장)

6단계에서 만든 `Obstacle1~3`을 확장해 **랜덤 지도**를 만들어 자율주행 학습 환경을 다양화합니다.

**ObstacleSpawner 스크립트 생성**

Project 창 → **Assets** 우클릭 → **Create > C# Script** → 이름: `ObstacleSpawner`

```csharp
using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("장애물 생성")]
    public int obstacleCount = 15;
    public float spawnRadius = 3.0f;   // LiDAR 탐지 범위 내
    public Vector2 obstacleSize = new Vector2(0.3f, 0.8f);
    public bool regenerateOnKeyF2 = true;

    [Header("재질")]
    public Material obstacleMaterial;

    private List<GameObject> spawned = new List<GameObject>();

    void Start()
    {
        SpawnRandomObstacles();
    }

    void Update()
    {
        if (regenerateOnKeyF2 && Input.GetKeyDown(KeyCode.F2))
        {
            Clear();
            SpawnRandomObstacles();
        }
    }

    void SpawnRandomObstacles()
    {
        for (int i = 0; i < obstacleCount; i++)
        {
            GameObject obs = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // 로봇(원점)을 중심으로 반지름 내 랜덤 위치
            Vector2 rnd = Random.insideUnitCircle * spawnRadius;
            float w = Random.Range(obstacleSize.x, obstacleSize.x * 2f);
            float h = Random.Range(obstacleSize.y, obstacleSize.y * 1.5f);

            obs.transform.position = new Vector3(rnd.x, h / 2f, rnd.y);
            obs.transform.localScale = new Vector3(w, h, w);
            obs.name = $"Obstacle_{i}";

            if (obstacleMaterial != null)
                obs.GetComponent<Renderer>().material = obstacleMaterial;

            spawned.Add(obs);
        }
    }

    void Clear()
    {
        foreach (var o in spawned) if (o != null) Destroy(o);
        spawned.Clear();
    }
}
```

### 3-2. 장애물 반사 특성 (LiDAR와의 상호작용)

| 장애물 종류 | BoxCollider | LiDAR 반사 | 학습 목적 |
|------------|-------------|-----------|----------|
| 벽(Wall) | 있음 | 거리 반환 | 회피 경로 |
| 기둥(Pillar) | 있음 | 거리 반환 | 근접 회피 |
| 낮은 물체 | 있음(낮게) | 높이 차로 미탐 | 높이 제한 인식 |

> LiDAR는 `rayHeight`(6단계, 기본 0.15m) 높이에서 Raycast합니다. **장애물 높이가 rayHeight보다 낮으면 탐지되지 않습니다.** 이를 활용해 "탐지 안 되는 장애물 회피" 시나리오도 만들 수 있습니다.

### 3-3. 장애물 배치 사전 설계

자율주행은 **난이도별 환경**이 필요합니다.

| 난이도 | 장애물 수 | 배치 | 학습 목표 |
|--------|----------|------|----------|
| 초급 | 5 | 원 바깥쪽, 넓게 | 기본 회피 |
| 중급 | 15 | 랜덤 분산 | 자유 주행 |
| 고급 | 30 | 밀집 + 이동 장애물 | 동적 회피 |

---

## 4. ROS 연동: 센서 데이터 외부 전송

### 4-1. 통신 방식 선택

| 방식 | 장점 | 단점 | 권장 시기 |
|------|------|------|----------|
| **TCP/IP (커스텀 JSON)** | 설치 불필요, 단순 | ROS 생태계 없음 | 당장은 이 방식 (3단계 기반) |
| **ROS2 브릿지** | 실제 SLAM/Nav2 재사용 | 설치/설정 복잡 | ROS2 환경일 때 |

> 본 문서는 **TCP/IP JSON 방식**을 3단계의 `TCPServer` 개념을 확장해 사용합니다. ROS2를 쓰는 경우 `ros2` 설치 후 4-5절처럼 메시지로 변환합니다.

### 4-2. 센서 데이터 직렬화 스크립트 (Unity 측)

3단계의 TCPServer를 확장하여 센서 데이터를 JSON으로 전송:

**SensorPublisher 스크립트 생성**

Project 창 → **Assets** 우클릭 → **Create > C# Script** → 이름: `SensorPublisher`

```csharp
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class SensorPublisher : MonoBehaviour
{
    [Header("연결")]
    public int port = 9000;

    [Header("센서 연결")]
    public LidarSensor lidar;
    public OdometrySensor odom;
    public ImuSensor imu;
    public Transform robot;

    [Header("전송 설정")]
    public float publishRate = 10f;   // 10Hz (LiDAR는 5Hz지만 보간 가능)
    public float lidarRate = 5f;

    private TcpListener listener;
    private TcpClient client;
    private Thread serverThread;
    private float nextScanTime;
    private float[] scanBuffer;

    void Start()
    {
        scanBuffer = new float[lidar.rayCount];
        StartServer();
    }

    void OnDestroy()
    {
        if (client != null) client.Close();
        if (listener != null) listener.Stop();
        if (serverThread != null) serverThread.Abort();
    }

    void StartServer()
    {
        serverThread = new Thread(ListenLoop);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    void ListenLoop()
    {
        try
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Debug.Log($"[SensorPublisher] TCP 서버 시작 :{port}");
            client = listener.AcceptTcpClient();
            Debug.Log("[SensorPublisher] Python 클라이언트 연결됨");
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    void Update()
    {
        if (client == null || !client.Connected) return;

        // LiDAR는 별도 5Hz로 샘플
        if (Time.time >= nextScanTime)
        {
            Array.Copy(lidar.ranges, scanBuffer, lidar.rayCount);
            nextScanTime = Time.time + 1f / lidarRate;
        }

        SendSensorData();
    }

    void SendSensorData()
    {
        string json = BuildJson();
        byte[] data = Encoding.UTF8.GetBytes(json + "\n");
        try
        {
            client.GetStream().Write(data, 0, data.Length);
        }
        catch { }
    }

    string BuildJson()
    {
        // ROS 좌표 변환 반영 (forward +Z → +X, up +Y → +Z)
        Vector3 pos = robot.position;

        // LiDAR 각도는 Unity 기준; Python에서 변환하므로 raw 그대로 보냄
        var scanArr = string.Join(",", Array.ConvertAll(scanBuffer, f => f.ToString("0.000")));

        return "{" +
            "\"t\":" + Time.time.ToString("0.000") + "," +
            "\"odom\":" + "{\"x\":" + pos.x.ToString("0.000") +
            ",\"y\":" + pos.z.ToString("0.000") +
            ",\"yaw\":" + (robot.eulerAngles.y * Mathf.Deg2Rad).ToString("0.000") +
            ",\"vx\":" + odom.linearVel.x.ToString("0.000") +
            ",\"wz\":" + odom.angularVel.ToString("0.000") + "}," +
            "\"imu\":" + "{\"gyro\":[" + imu.angularVelocity.x.ToString("0.000") +
            "," + imu.angularVelocity.y.ToString("0.000") +
            "," + imu.angularVelocity.z.ToString("0.000") + "]," +
            "\"accel\":[" + imu.linearAcceleration.x.ToString("0.000") +
            "," + imu.linearAcceleration.y.ToString("0.000") +
            "," + imu.linearAcceleration.z.ToString("0.000") + "]}," +
            "\"scan\":[" + scanArr + "]}";
    }

    // 외부(자율주행 제어)에서 속도 명령 수신
    public void SendCommand(float linear, float angular)
    {
        // TurtleBot3Controller로 전달
        TurtleBot3Controller ctrl = robot.GetComponent<TurtleBot3Controller>();
        if (ctrl != null)
        {
            ctrl.SetExternalCommand(linear, angular);
        }
    }
}
```

### 4-3. TurtleBot3Controller에 외부 명령 수신 추가

6단계 `TurtleBot3Controller`에 자율주행 제어 명령 입력을 추가합니다:

```csharp
// TurtleBot3Controller.cs 에 추가
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TurtleBot3Controller : MonoBehaviour
{
    // ... 기존 필드 ...

    [Header("외부 제어 (자율주행)")]
    public int cmdPort = 9001;
    private TcpListener cmdListener;
    private bool externalControl = false;
    private float extLinear = 0f;
    private float extAngular = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCommandServer();
    }

    void StartCommandServer()
    {
        Thread t = new Thread(() =>
        {
            cmdListener = new TcpListener(IPAddress.Any, cmdPort);
            cmdListener.Start();
            var cmdClient = cmdListener.AcceptTcpClient();
            var stream = cmdClient.GetStream();
            byte[] buf = new byte[1024];
            while (cmdClient.Connected)
            {
                int n = stream.Read(buf, 0, buf.Length);
                if (n <= 0) continue;
                string msg = Encoding.UTF8.GetString(buf, 0, n).Trim();
                // 형식: "0.5,1.57"  (linear, angular)
                var parts = msg.Split(',');
                extLinear = float.Parse(parts[0]);
                extAngular = float.Parse(parts[1]);
                externalControl = true;
            }
        });
        t.IsBackground = true;
        t.Start();
    }

    public void SetExternalCommand(float linear, float angular)
    {
        extLinear = linear;
        extAngular = angular;
        externalControl = true;
        // 임시 (키보드와 전환을 원하면 toggle)
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        float moveInput, turnInput;
        if (externalControl)
        {
            moveInput = extLinear;
            turnInput = extAngular;
        }
        else
        {
            moveInput = Input.GetAxis("Vertical");
            turnInput = Input.GetAxis("Horizontal");
        }

        Vector3 moveDirection = transform.forward * moveInput * moveSpeed;
        rb.MovePosition(rb.position + moveDirection * Time.fixedDeltaTime);
        float rotation = turnInput * rotationSpeed * Mathf.Rad2Deg * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, rotation, 0f));
    }
}
```

> ⚠️ **참고**: 위 예제는 TCP 클라이언트에서 지속적으로 명령을 받는 단순 구조입니다. 실제로는 4-6절처럼 명령 파싱과 충돌 안전 처리가 필요합니다.

### 4-4. Python 측 데이터 수신 + 좌표 변환

**`sensor_receiver.py`** — Unity에서 센서 데이터를 받는 Python API:

```python
import socket
import json
import numpy as np

class UnitySensorBridge:
    """Unity 시뮬레이터의 센서 데이터를 수신하는 브릿지"""
    def __init__(self, host='127.0.0.1', port=9000):
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.sock.connect((host, port))
        self.buffer = b''

    def receive(self):
        """JSON 라인 단위로 파싱하여 dict 반환"""
        while b'\n' not in self.buffer:
            self.buffer += self.sock.recv(4096)
        line, self.buffer = self.buffer.split(b'\n', 1)
        return json.loads(line.decode())

    # ── ROS 좌표 변환 ─────────────────────────────
    @staticmethod
    def odom_to_ros(u):
        """Unity (x,z,yaw) → ROS 기반 (x,y=좌우,반대, yaw 부호 반전)"""
        # Unity forward=+Z → ROS forward=+X, Unity left=+X → ROS left=+Y
        rox = u['x']   # l  (Unity X)
        roy = -u['y'] * ...  # 실제 코드에서 변환

    def close(self):
        self.sock.close()

# 사용 예
if __name__ == '__main__':
    bridge = UnitySensorBridge()
    while True:
        data = bridge.receive()
        print('odom:', data['odom'])
        print('scan[0:5]:', data['scan'][:5])
```

> **참고**: 위치 좌표 변환이 복잡해질 수 있어, 실제 자율주행에서는 **Unity가 이미 월드 좌표를 제공**하므로 Python에서 SLAM 맵과의 정합만 맞추면 됩니다. 좌표 축(forward +Z↔+X)은 `ros_from_unity()` 헬퍼로 일관 적용합니다.

### 4-5. (선택) ROS2 메시지 변환

ROS2 환경이라면 위 JSON을 `sensor_msgs/LaserScan`, `nav_msgs/Odometry`, `sensor_msgs/Imu` 메시지로 변환합니다:

```python
# Python에서 유효한 RS2 메시지로 변환 예 (sensor_msgs)
from sensor_msgs.msg import LaserScan
scan = LaserScan()
scan.angle_min = 0.0
scan.angle_max = 2 * np.pi
scan.angle_increment = 2 * np.pi / lidar.rayCount
scan.range_min = 0.12
scan.range_max = 3.5
scan.ranges = data['scan']  # Unity가 보낸 360개 거리
```

---

## 5. 학습 데이터 생성

### 5-1. 학습 데이터 종류

| 데이터 | 수집 방법 | 용도 |
|--------|----------|------|
| **상태(state)** | LiDAR 360점 + 오도메트리 | 강화학습 상태 입력 |
| **행동(action)** | 선속·각속도 명령 | 목표 회피 행동 |
| **보상(reward)** | 목표 도달·충돌·거리 | 학습 신호 |
| **전이(transition)** | (s, a, r, s') 튜플 | 경험 리플레이 |

### 5-2. 데이터 수집 루프 (Unity 자동 주행)

자율주행 학습 전에 **지도 주행(supervised)** 데이터부터 수집합니다. Unity에서 키보드/자동으로 로봇을 움직이며 상태와 행동을 기록합니다.

**`record_episode.py`** — 데이터 녹화기:

```python
import json
import numpy as np
from sensor_receiver import UnitySensorBridge
import time

class DataRecorder:
    def __init__(self, save_path='dataset/'):
        self.bridge = UnitySensorBridge()
        self.save_path = save_path
        self.data = []

    def record(self, n_steps=1000):
        for _ in range(n_steps):
            obs = self.bridge.receive()

            # 상태: LiDAR 360 + 오도메트리(yaw, vx)
            state = np.array(obs['scan'], dtype=np.float32)  # 360

            # 행동: 현재 제어 입력 (키보드/자동 조종에서 기록)
            # 실제로는 Unity에서 현재 ctrl input도 함께 보내도록 확장
            action = self.get_current_action()  # (linear, angular)

            # 보상: (목표까지 거리 감소 여부 등으로 계산)
            reward = self.compute_reward(obs)

            self.data.append({
                'state': state.tolist(),
                'action': action,
                'reward': reward,
                # s' 는 다음 스텝에서 기록
            })

            if len(self.data) % 100 == 0:
                self.save()

    def save(self):
        with open(f'{self.save_path}episode_{time.time()}.json', 'w') as f:
            json.dump(self.data, f)
        self.data = []
```

### 5-3. 상태 표현 정규화

강화학습 안정성을 위해 입력을 정규화합니다:

```python
def normalize_scan(scan, rmin=0.12, rmax=3.5):
    # 거리를 0~1로 정규화, 무한대(최대값)는 1로
    s = np.clip(scan, rmin, rmax)
    return (s - rmin) / (rmax - rmin)
```

> 상태 벡터: `[scan_0..scan_359(360)], [yaw], [vx]` → 총 362차원

---

## 6. AI 학습: 강화학습 기반 자율주행

### 6-1. 강화학습 개념

자율주행 학습은 **강화학습(RL)** 을 사용합니다. 에이전트(로봇)가 환경과 상호작용하며 보상을 최대화하는 정책을 학습합니다.

```
상태 s (LiDAR)
    │
    ▼
정책 π(a|s) ──► 행동 a (선속·각속도)
    │             │
    └── 환경(Unity)에서 다음 상태 s' + 보상 r
```

### 6-2. 환경 정의 (Gymnasium 스타일)

**`turtlebot_env.py`** — Unity를 Gymnasium 환경으로 래핑:

```python
import gymnasium as gym
import numpy as np
from gymnasium import spaces

class TurtleBotEnv(gym.Env):
    """Unity 시뮬레이터를 Gymnasium 환경으로 추상화"""

    def __init__(self, bridge):
        super().__init__()
        self.bridge = bridge
        self.scan_size = 360

        # 행동: 선속(0~0.22), 각속도(-2.84~2.84)
        self.action_space = spaces.Box(
            low=np.array([0.0, -2.84]),
            high=np.array([0.22, 2.84]),
            dtype=np.float32
        )
        # 상태: LiDAR 360 + yaw + vx
        self.observation_space = spaces.Box(
            low=0, high=1, shape=(self.scan_size + 2,), dtype=np.float32
        )

    def reset(self):
        # Unity에서 로봇 초기화 (R 키 등으로 구현 필요)
        obs = self._get_obs()
        return obs, {}

    def step(self, action):
        # 행동을 Unity로 전송 (/cmd_vel 대응)
        self.bridge.send_command(action[0], action[1])
        obs = self._get_obs()

        # 보상 계산: 전진 거리 + 회피 - 충돌 패널티 - 회전 패널티
        reward = self._compute_reward(action)
        terminated = self._is_collision()   # 충돌 시 종료
        truncated = False

        return obs, reward, terminated, truncated, {}

    def _get_obs(self):
        data = self.bridge.receive()
        scan = normalize_scan(np.array(data['scan']))
        yaw = np.array([data['odom']['yaw'] / (2*np.pi)])
        vx = np.array([data['odom']['vx'] / 0.22])
        return np.concatenate([scan, yaw, vx]).astype(np.float32)

    def _compute_reward(self, action):
        # ① 전진 보상 (+)
        # ② 근접 장애물 회피 (+)
        # ③ 회전 페널티 (-)
        # ④ 충돌 페널티 (큰 -)
        pass

    def _is_collision(self):
        # LiDAR 최소 거리 < 임계값이면 충돌 가정
        pass
```

### 6-3. DDQN (Deep Q-Network) 구현

**`train_dqn.py`** — PyTorch 기반 DDQN 학습:

```python
import torch
import torch.nn as nn
import torch.optim as optim
import numpy as np
from collections import deque
import random

class DQN(nn.Module):
    """LiDAR 상태 → 행동 Q-값"""
    def __init__(self, state_dim, action_dim):
        super().__init__()
        self.net = nn.Sequential(
            nn.Linear(state_dim, 256),
            nn.ReLU(),
            nn.Linear(256, 256),
            nn.ReLU(),
            nn.Linear(256, action_dim)
        )

    def forward(self, x):
        return self.net(x)

class DDQNAgent:
    def __init__(self, state_dim, action_dim):
        self.policy = DQN(state_dim, action_dim)
        self.target = DQN(state_dim, action_dim)
        self.target.load_state_dict(self.policy.state_dict())
        self.optim = optim.Adam(self.policy.parameters(), lr=1e-4)
        self.replay = deque(maxlen=100_000)
        self.gamma = 0.99
        self.eps = 1.0
        self.eps_min = 0.05
        self.eps_decay = 0.999

    def act(self, state):
        # ε-greedy
        if random.random() < self.eps:
            return random.randint(0, self.action_dim - 1)
        with torch.no_grad():
            q = self.policy(torch.FloatTensor(state).unsqueeze(0))
            return q.argmax().item()

    def learn(self, batch_size=64):
        if len(self.replay) < batch_size:
            return
        batch = random.sample(self.replay, batch_size)
        # ... (표준 DDQN 업데이트)

    def update_target(self):
        self.target.load_state_dict(self.policy.state_dict())

# 학습 메인 루프
def train():
    env = TurtleBotEnv(bridge)
    agent = DDQNAgent(state_dim=362, action_dim=5)  # 행동 이산화: [정지,전진,좌,우,...]
    # ... 에피소드 루프 ...
```

> **행동 이산화**: 연속 행동 대신 `[정지, 직진, 좌회전, 우회전, 급회전]` 5개 이산 행동으로 학습을 단순화할 수 있습니다. 이산 Q-Learning이 연속보다 안정적입니다.

### 6-4. 학습 진행 가이드

| 구간 | 구성 | 기대 |
|------|------|------|
| **초기** | ε=1.0 (랜덤 탐험) | 충돌 반복, 점수 낮음 |
| **중반** | ε 감소 (탐험→활용) | 회피 시작, 목표 도달 시도 |
| **수렴** | ε=0.05 | 안정적 회피·주행 |

**모니터링 항목**: 평균 보상, 충돌 횟수, 평균 주행 거리, 에피소드 길이.

---

## 7. 자율주행 최종 구현

### 7-1. 학습된 정책 배포

학습이 끝나면 `policy.pt` 가중치를 저장하고, 추론(inference) 모드로 자율주행을 실행합니다.

**`drive_autonomous.py`** — 학습된 정책으로 자율주행:

```python
import torch
import numpy as np

class AutonomousDriver:
    def __init__(self, bridge, model_path='policy.pt'):
        self.bridge = bridge
        self.policy = DQN(362, 5)
        self.policy.load_state_dict(torch.load(model_path))
        self.policy.eval()

    def drive(self, steps=1000):
        for _ in range(steps):
            obs = self.get_state()
            action = self.act(obs)      # argmax (탐험 없음)
            linear, angular = self.action_to_vel(action)
            self.bridge.send_command(linear, angular)  # /cmd_vel-like

    def act(self, state):
        with torch.no_grad():
            q = self.policy(torch.FloatTensor(state).unsqueeze(0))
            return q.argmax().item()
```

### 7-2. 경로 계획 통합 (목적지 주행)

완전한 자율주행은 **목적지 지정 → 경로 탐색 → 추종**을 포함합니다.

**A* 경로 탐색 (간단 구현):**
```python
import heapq

def astar(grid, start, goal):
    """점유격자 맵(grid)에서 start→goal 최단경로"""
    open_set = [(0, start)]
    came_from = {}
    g = {start: 0}
    while open_set:
        _, current = heapq.heappop(open_set)
        if current == goal:
            return reconstruct_path(came_from, current)
        for neighbor in walkable_neighbors(grid, current):
            tentative = g[current] + 1
            if tentative < g.get(neighbor, float('inf')):
                came_from[neighbor] = current
                g[neighbor] = tentative
                heapq.heappush(open_set, (tentative + heuristic(neighbor, goal), neighbor))
    return []  # 경로 없음

def heuristic(a, b):
    return abs(a[0]-b[0]) + abs(a[1]-b[1])
```

### 7-3. 자율주행 파이프라인 완성 (전체 연동)

```
[Unity] 센서 데이터 ──TCP/JSON──► [Python]
  LiDAR /scan                       │
  odom /odom                        ├─ SLAM/맵 (6단계 맵 + A* 경로)
  IMU  /imu                         ├─ 정책 추론 (DDQN) → 행동
  장애물 (씬)                       └─ /cmd_vel → Unity 로봇 제어
```

**`main_autonomous.py`** 통합 실행 순서:
1. Unity에서 Play 시작 (SensorPublisher + 장애물 배치)
2. Python에서 `bridge = UnitySensorBridge()`
3. `SLAM`으로 맵 구축 (또는 기존 맵 로드)
4. 목적지 지정 → `A*` 경로 탐색
5. `AutonomousDriver`로 추종 + `DDQN`으로 실시간 회피

---

## 8. 종합 검증 및 응용

### 8-1. 자율주행 검증 시나리오

| 시나리오 | 구성 | 판정 기준 |
|----------|------|----------|
| **회피 주행** | 랜덤 장애물 15개 | 무충돌로 일정 거리 주행 |
| **목적지 도달** | 고정 장애물 + 목적지 | 장애물 피해 목적지 도착 |
| **동적 환경** | 이동 장애물 | 충돌 없이 회피 |
| **다중 로봇** | 터틀봇 N대 (복제) | 서로 회피하며 주행 |

### 8-2. 데이터 규모와 반복 학습

| 항목 | 이상적 규모 | 비고 |
|------|------------|------|
| 에피소드 | 1,000 ~ 10,000 | Unity Play 중 자동 반복 |
| 프레임/에피소드 | 200 ~ 1,000 | 상태-행동 전이 수집 |
| 랜덤 환경 | 100 ~ 1,000회 | F2 키로 재생성 |
| 학습 시간 | 십만 ~ 백만 스텝 | GPU/CPU에 따라 |

### 8-3. Isaac Sim / 실제 로봇으로 확장

| 단계 | 확장 |
|------|------|
| **Unity → Isaac Sim** | 동일한 ROS2 토픽(`/scan`, `/odom`, `/imu`, `/cmd_vel`) 사용 → Python 학습 코드 재사용 |
| **시뮬 → 실제 터틀봇** | ROS2 토픽을 그대로 실제 로봇의 `/cmd_vel`, `/scan`에 연결 (sim-to-real) |
| **멀티 로봇** | Unity에서 로봇 복제 → 데이터 병렬 수집 속도↑ |

---

## 9. 문제 해결 체크리스트

### 문제 1: Python이 Unity 센서 데이터를 못 받음

| 확인 | 해결 |
|------|------|
| Unity Play 중인지 | Play 눌러 서버 시작 확인 |
| 포트 일치 | Unity 9000 / Python 9000 |
| 방화벽 | 로컬(127.0.0.1)은 통과 필요 없음 |
| Console 에러 | "[SensorPublisher] TCP 서버 시작" 로그 확인 |

### 문제 2: SLAM 맵이 뒤집히거나 회전됨

| 확인 | 해결 |
|------|------|
| 좌표 변환 | `/scan`과 `/odom` 모두 동일 규칙(ROS 좌표) 적용 |
| yaw 부호 | Unity yaw(반시계+) vs ROS(시계+) 부호 반전 |

### 문제 3: 강화학습이 수렴하지 않음

| 확인 | 해결 |
|------|------|
| 보상이 0에 가까움 | 전진 보상 + 회피 보상 비율 조정 |
| 행동 너무 연속적 | 이산 행동(5개)으로 단순화 |
| 상태 노이즈 | LiDAR 정규화 + IMU 저역통과 필터 |
| 탐험 부족 | ε 감소 속도 조정 |

### 문제 4: 로봇이 움직이는데 맵이 안 그려짐

| 확인 | 해결 |
|------|------|
| SensorPublisher가 LidarSensor 참조 | Inspector 연결 |
| publishRate vs lidarRate | LiDAR 5Hz 유지, 전송 10Hz |
| 맵 중심 | 로봇이 원점(0,0,0) 근처 출발 |

---

## 파일 구조 (7단계 최종)

```
Assets\
  URDF\
    turtlebot3_burger.urdf
    meshes\
  TurtleBot3Setup.cs          ← 5단계
  TurtleBot3Controller.cs     ← 5단계 (+외부 제어 추가)
  LidarSensor.cs              ← 6단계
  MapRenderer.cs              ← 6단계
  OdometrySensor.cs           ← 6단계
  ImuSensor.cs                ← 6단계
  ObstacleSpawner.cs          ← 7단계 (랜덤 장애물)
  SensorPublisher.cs          ← 7단계 (ROS 연동)
  └─ ...

[Python]
  sensor_receiver.py          ← 7단계 (데이터 수신)
  record_episode.py           ← 7단계 (학습 데이터)
  turtlebot_env.py            ← 7단계 (Gym 환경)
  train_dqn.py                ← 7단계 (DDQN 학습)
  drive_autonomous.py         ← 7단계 (자율주행 추론)
  main_autonomous.py          ← 7단계 (전체 실행)
```

---

## 커리큘럼 총정리 (1~7단계 로드맵)

```
[1단계] Unity 설치·기초 ─── 게임엔진 조작 능력
   │
[2단계] 로봇 제작 ─────── 기본 물리·3D·키보드
   │
[3단계] Python 연결 ───── TCP/IP 통신 기초
   │
[4단계] Unity AI ──────── AI/ROS 개념 개요
   │
[5단계] 외부 로봇 임포트 ─ 실제 터틀봇 URDF + 물리
   │
[6단계] 센서 시뮬레이션 ── LiDAR/맵/오도메트리/IMU
   │
[7단계] 자율주행 (본 문서)─ 환경 → ROS → 데이터 → 학습 → 주행
        ▲
        └── 최종 목표: 시뮬레이션 환경에서의 자율주행 학습 및 자율주행 완성
```

---

> **출처**: NVIDIA Isaac Sim "시뮬레이션 우선(sim-first)" 접근, ROS2 튜토리얼 시리즈를 Unity/게임엔진 기반으로 번안·확장  
> **최종 업데이트**: 2026년 9월
