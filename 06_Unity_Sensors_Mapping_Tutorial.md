# 6단계: Unity 센서 시뮬레이션 - LiDAR, 맵핑, 오도메트리, IMU

> **목적**: TurtleBot3 Burger에 LiDAR / 오도메트리 / IMU 센서를 구현하고 장애물 환경에서 거리 기반 맵을 그린다  
> **소요 시간**: 약 180 ~ 240분  
> **전제 조건**: 05단계 완료 (TurtleBot3 URDF 임포트 + Rigidbody 구동)

---

## 이 문서의 목표

05단계에서 만든 TurtleBot3에 다음 센서를 추가하고 데이터를 생성합니다.

| # | 기능 | 의미 |
|---|------|------|
| 1 | **LiDAR** | 회전하며 레이저를 발사, 반사 거리로 장애물 감지 |
| 2 | **맵핑** | 장애물을 추가하고 LiDAR 거리 데이터로 2D 점유격자 맵 그리기 |
| 3 | **오도메트리** | 바퀴 회전을 적분하여 로봇 위치/방향 계산 |
| 4 | **IMU** | 로봇의 각속도와 선형 가속도 측정 |

> **Isaac Sim 대응**: Isaac Sim에서는 OmniGraph의 `Isaac Compute Odometry`, `RTX Lidar`, ROS2 IMU 브릿지로 동일 기능을 구현합니다. 여기서는 Unity C# 스크립트로 동일한 센서 데이터 개념을 직접 구현합니다.

---

## 목차

1. [LiDAR 사양 이해](#1-lidar-사양-이해)
2. [LiDAR 센서 구현](#2-lidar-센서-구현)
3. [장애물 추가](#3-장애물-추가)
4. [맵 그리기 (2D 점유격자)](#4-맵-그리기-2d-점유격자)
5. [오도메트리 구현](#5-오도메트리-구현)
6. [IMU 센서 구현](#6-imu-센서-구현)
7. [전체 테스트](#7-전체-테스트)
8. [문제 해결 체크리스트](#8-문제-해결-체크리스트)

---

## 1. LiDAR 사양 이해

### 1-1. TurtleBot3의 실제 LiDAR: LDS-02 / LDS-03

TurtleBot3 Burger에 장착되는 실제 2D LiDAR는 제조 시기에 따라 **LDS-02**(구형) 또는 **LDS-03**(신형)입니다.

| 버전 | 공식 문서 | 출시/교체 시기 | 특징 |
|------|-----------|--------------|------|
| **LDS-02** | https://docs.robotis.com/docs/systems/turtlebot3/more_info/lds_02/ | 2022년부터 LDS-01을 대체 | 측정 0.16 ~ 8m, 1°, 5Hz |
| **LDS-03** | https://docs.robotis.com/docs/systems/turtlebot3/more_info/lds_03/ | 2025년부터 LDS-02를 대체 | 측정 0.05 ~ 12m, 0.9°, 10Hz |

> 📎 ROBOTIS 공식 버전별 사양/데이터 패킷은 위 링크에서 확인할 수 있습니다.
> - **LDS-02**: https://docs.robotis.com/docs/systems/turtlebot3/more_info/lds_02/
> - **LDS-03**: https://docs.robotis.com/docs/systems/turtlebot3/more_info/lds_03/

#### 각 버전별 주요 사양

**LDS-02 (구형, 이전 버전)** — https://docs.robotis.com/docs/systems/turtlebot3/more_info/lds_02/

| 항목 | 값 | 비고 |
|------|-----|------|
| 회전 각도 | 360° | 한 바퀴 전체 스캔 |
| 분해능 (각도 간격) | 1° | 360개 포인트 |
| 측정 거리 (최소) | 0.16m | 이보다 가까우면 무시 |
| 측정 거리 (최대) | 8m | 이보다 먼 곳은 감지 안 됨 |
| 스캔 주기 | 0.2s | 5Hz (초당 5회 전체 스캔) |

**LDS-03 (신형, 최신 버전)** — https://docs.robotis.com/docs/systems/turtlebot3/more_info/lds_03/

| 항목 | 값 | 비고 |
|------|-----|------|
| 회전 각도 | 360° | 한 바퀴 전체 스캔 |
| 분해능 (각도 간격) | 0.9° | 약 400개 포인트 |
| 측정 거리 (최소) | 0.05m | 이보다 가까우면 무시 |
| 측정 거리 (최대) | 12m | 이보다 먼 곳은 감지 안 됨 |
| 스캔 주기 | 0.1s | 10Hz (초당 10회 전체 스캔) |

> 💡 **시뮬레이션에서는** 두 버전 모두 "360° 전체를 회전하며 한 바퀴 누적된 각도별 거리값을 출력"한다는 원리는 동일합니다. 아래 구현에서는 **LDS-02(1° / 360포인트)** 사양을 기본값으로 사용하며, 필요하면 LDS-03 사양(0.9° / 10Hz)으로 바꿀 수 있습니다.

> **핵심**: 실제 센서는 매우 빠르게 회전하며, 한 바퀴 누적된 각도의 거리값을 일정 주기로 출력합니다. Unity에서는 이 "회전하며 누적" 동작을 **시각적으로** 보여주는 모드와 **한 번에 360° 스캔**하는 데이터 모드 두 가지를 구현합니다.

### 1-2. 2D 레이저 스캔 데이터 구조 (ROS LaserScan 개념)

```text
angle_min = 0        (rad)
angle_max = 2π       (rad)  → 360°
angle_increment = 0.0174 rad (1°)   ← LDS-02: 1° / LDS-03: 0.0157 rad (0.9°)
range_min = 0.12m                  ← LDS-02: 0.16m / LDS-03: 0.05m
range_max = 3.5m                   ← 이 값은 시뮬레이션 기본값 (LDS-02: 8.0m / LDS-03: 12.0m)
ranges[360]  ← 각도별 거리값 배열 (LDS-03이면 약 400개)
```

> **참고**: `range_min`/`range_max`는 시뮬레이션 기본값이며, 실제 하드웨어 버전(LDS-02/LDS-03) 사양으로 바꾸면 탐지 거리가 달라집니다. 나중에 ROS2 브릿지로 연결하면 `sensor_msgs/LaserScan` 메시지의 같은 필드로 바로 변환 가능합니다.

---

## 2. LiDAR 센서 구현

### 2-1. 센서 부착 위치 확인

05단계에서 임포트한 Hierarchy 구조:

```
turtlebot3_burger
└─ base_footprint
   └─ base_link
      ├─ wheel_left_link
      ├─ wheel_right_link
      ├─ caster_back_link
      ├─ imu_link
      └─ base_scan       ← ★ LiDAR를 여기에 부착
```

`base_scan` 오브젝트가 이미 LiDAR(lds.stl) 시각 모델 자리입니다.

### 2-2. LiDAR 시각화 (회전하며 발사)

**핵심 아이디어**
- `base_scan` 자식으로 회전용 빈 오브젝트 `LidarRotator`를 만들고, 그 안에 `LineRenderer`로 레이저 라인을 그림
- `LidarRotator`를 Z축(로봇 위축)으로 빠르게 회전시키면서 매 프레임 Raycast로 거리 측정
- 결과 거리값을 360개 배열(`ranges`)에 저장

**LidarSensor 스크립트 생성**

Project 창 → **Assets** 우클릭 → **Create > C# Script** → 이름: `LidarSensor`

```csharp
using UnityEngine;
using System.Collections.Generic;

public class LidarSensor : MonoBehaviour
{
    [Header("LDS-02 사양 (기본) / LDS-03 사양으로 조정 가능")]
    public int rayCount = 360;        // 분해능 1° → 360개 (LDS-03은 0.9° → 약 400개)
    public float rangeMin = 0.12f;    // 최소 측정 거리 (m). LDS-02: 0.16, LDS-03: 0.05
    public float rangeMax = 3.5f;     // 최대 측정 거리 (m). LDS-02: 8.0, LDS-03: 12.0
    public float scanRate = 5f;       // 스캔 주기 5Hz (0.2s). LDS-03: 10Hz
    public float rotationSpeed = 1800f; // 시각적 회전 (RPM 개념, deg/s 단위로 환산)

    [Header("시각화")]
    public bool drawRays = true;
    public Color rayColor = Color.green;
    public float rayHeight = 0.15f;   // 레이저 높이 (base_scan 기준 위쪽)

    // 각도별 거리값 (ROS LaserScan.range 구조와 동일)
    public float[] ranges;

    private Transform rotator;
    private LineRenderer[] lines;
    private int lastPointIndex = -1;

    void Awake()
    {
        CreateRotator();
        CreateRayLines();
        ranges = new float[rayCount];
        for (int i = 0; i < rayCount; i++)
            ranges[i] = rangeMax;
    }

    void CreateRotator()
    {
        // base_scan 아래 회전용 자식 오브젝트
        GameObject rotGO = new GameObject("LidarRotator");
        rotGO.transform.SetParent(transform, false);
        rotator = rotGO.transform;
    }

    void CreateRayLines()
    {
        // 레이저 라인 수가 많지 않도록 새로 고침 트릭
        // 사실 LineRenderer 하나로 360개 점을 이으면 링이 됨
        lines = new LineRenderer[1];
        LineRenderer lr = gameObject.AddComponent<LineRenderer>();
        lr.positionCount = rayCount + 1;
        lr.startWidth = 0.005f;
        lr.endWidth = 0.005f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = rayColor;
        lr.endColor = rayColor;
        lines[0] = lr;
    }

    void Update()
    {
        // 회전 (시각화용)
        rotator.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // 스캔 업데이트 (5Hz)
        // 실제 스캔은 회전 주기와 무관하게 누적 데이터를 출력하지만,
        // 학습 목적으로 매 프레임 전체 360°를 측정해도 무방
        if (drawRays)
            UpdateScan();
    }

    void UpdateScan()
    {
        Vector3 origin = transform.position + Vector3.up * rayHeight;

        for (int i = 0; i < rayCount; i++)
        {
            float angleRad = i * Mathf.Deg2Rad; // 0 ~ 359°
            // 로봇의 forward(전방)가 로컬 Z축이라는 점에 주의
            // Y축 회전  →  방향 벡터
            Vector3 direction = new Vector3(Mathf.Sin(angleRad), 0, Mathf.Cos(angleRad));

            if (Physics.Raycast(origin, direction, out RaycastHit hit, rangeMax))
            {
                float dist = hit.distance;
                if (dist < rangeMin)
                    ranges[i] = float.PositiveInfinity; // 최소거리 미만은 무시
                else
                    ranges[i] = dist;
            }
            else
            {
                ranges[i] = rangeMax; // 화면 바깥은 최대값
            }

            // 라인렌더러 점 저장
            lines[0].SetPosition(i, origin);
            float validDist = (float.IsInfinity(ranges[i])) ? rangeMax : ranges[i];
            lines[0].SetPosition(i + 1, origin + direction * validDist);
        }

        // 마지막 점 → 첫 점 연결 (링 닫기)
        lines[0].SetPosition(rayCount, lines[0].GetPosition(0) + Vector3.up * 0.001f);
    }

    // 외부(맵핑)에서 각도별 거리를 얻는 API
    public float GetRange(int index) => ranges[index];
    public float GetRangeAtAngle(float angleDeg) => ranges[((int)angleDeg + 360) % rayCount];
}
```

> ⚠️ **회전 방향 주의**: TurtleBot3는 바퀴로 이동하므로 로봇의 forward는 `transform.forward`(Z+)입니다. LiDAR 0°는 로봇 정면에서 시작해 시계방향으로 360°를 훑습니다.

### 2-3. 스크립트 연결

> ✅ **자동 연결**: `TurtleBot3Setup.cs`(05단계)가 `Awake()`에서 base_scan을 찾아 `LidarSensor`가 없으면 **자동으로 부착**합니다. 별도로 Add Component를 하지 않아도 됩니다.

수동으로 직접 붙이고 싶다면 다음 순서로 합니다:

1. Hierarchy에서 **base_scan** 선택
2. **Add Component > LidarSensor** 추가
3. 값 확인 (기본값이 **LDS-02 사양**. LDS-03 하드웨어를 쓰면 1-1의 사양표를 참고해 rayCount/rangeMax/scanRate 조정)

> 💡 **중복 방지**: 자동 부착은 이미 직접 추가해 둔 LidarSensor가 있으면 다시 붙이지 않습니다.

### 2-4. Play 테스트 (레이저 확인)

1. Play 시작
2. Scene 뷰에서 **base_scan** 위치에 레이저 라인(녹색 링)이 보이는지 확인
3. 로봇 주변에 가까운 물체(손, 벽) 대면 라인이 그 지점에서 줄어드는지 확인

> 💡 **팁**: 레이저 준비물 5Hz가 아닌 매 프레임 갱신되므로 Scene 뷰에서 실시간으로 장애물 반사를 확인할 수 있습니다.

---

## 3. 장애물 추가

### 3-1. 장애물 만들기

1. Hierarchy 우클릭 → **3D Object > Cube**
2. 이름을 `Obstacle1`로 변경
3. Inspector 위치 설정:

| 오브젝트 | Position X | Position Y | Position Z |
|---------|-----------|-----------|-----------|
| Obstacle1 | 1.0 | 0.15 | 0.0 |
| Obstacle2 | -0.8 | 0.2 | 1.2 |
| Obstacle3 | 0.3 | 0.1 | -1.5 |

> 각 Cube의 Scale Y = 0.3 정도로 낮은 벽처럼 만들고, 위치 Y는 높이의 절반으로 맞춥니다.

### 3-2. 장애물 색상 구분 (선택)

각 장애물에 Material을 만들어 구분하면 LiDAR 반사가 보기 좋습니다:

```csharp
// Inspector 대신 코드로 색 지정 시 (Cube 선택 후) - 사용 편의용
```

1. Project 창 → 우클릭 → **Create > Material** → `ObsMat`
2. Albedo 색상을 **빨간색**으로 설정
3. 각 장애물 Cube에 드래그하여 적용 (선택)

### 3-3. 확인

Play 후 Scene 뷰에서 레이저 라인이 장애물 표면에서 **끊기거나 줄어드는** 것을 확인합니다.

---

## 4. 맵 그리기 (2D 점유격자)

### 4-1. 개념: 점유격자 맵 (Occupancy Grid)

LiDAR로 측정한 거리값을 **격자(Grid)** 형태로 변환하여 장애물이 있는 칸을 표시합니다.

| 격자 값 | 의미 | 색 |
|---------|------|-----|
| 0 | 비어 있음 (free) | 검정 |
| 100 | 장애물 (occupied) | 흰색 |
| -1 | 미탐색 (unknown) | 회색 |

### 4-2. 맵 해상도 설정

| 항목 | 값 | 설명 |
|------|-----|------|
| 맵 크기 | 10m × 10m | 로봇 기준 |
| 해상도 | 0.05m/픽셀 | 1픽셀 = 5cm |
| 픽셀 수 | 200 × 200 | |

### 4-3. MapRenderer 스크립트 생성

Project 창 → **Assets** 우클릭 → **Create > C# Script** → 이름: `MapRenderer`

```csharp
using UnityEngine;

public class MapRenderer : MonoBehaviour
{
    [Header("맵 설정")]
    public int gridSize = 200;         // 200x200 픽셀
    public float resolution = 0.05f;   // 5cm/픽셀
    public Transform robotTransform;
    public LidarSensor lidar;

    [Header("연결")]
    public bool autoUpdate = true;

    private Texture2D mapTexture;
    private int[,] occupancy; // 0 free, 100 occupied, -1 unknown
    private float mapWorldSize;

    void Start()
    {
        mapWorldSize = gridSize * resolution; // 200 * 0.05 = 10m

        occupancy = new int[gridSize, gridSize];
        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
                occupancy[x, y] = -1; // 초기 미탐색

        mapTexture = new Texture2D(gridSize, gridSize);
        mapTexture.filterMode = FilterMode.Point;
        GetComponent<Renderer>().material.mainTexture = mapTexture;
        Redraw();

        // 자식 Quad 크기를 맵 크기에 맞춤
        transform.localScale = new Vector3(mapWorldSize, mapWorldSize, 1);
    }

    void Update()
    {
        if (autoUpdate && lidar != null)
            DrawLidarScan();
    }

    // LiDAR 스캔을 격자에 반영
    void DrawLidarScan()
    {
        for (int i = 0; i < lidar.rayCount; i++)
        {
            float range = lidar.GetRange(i);
            if (float.IsInfinity(range)) continue;

            float angleDeg = i; // 0~359
            float angleRad = angleDeg * Mathf.Deg2Rad;

            // 로봇의 월드 회전 고려 (로봇이 돌면 레이저도 같이 돔)
            float worldAngle = transform.eulerAngles.y * Mathf.Deg2Rad + angleRad;
            Vector3 dir = new Vector3(Mathf.Sin(worldAngle), 0, Mathf.Cos(worldAngle));

            // 로봇 위치 (맵 중심 기준 월드 좌표)
            Vector3 robotPos = robotTransform.position;

            // 장애물 끝점
            Vector3 hitPoint = robotPos + dir * range;

            // 로봇~장애물 라인 사이의 칸을 free(0)로, 장애물 칸을 occupied(100)로
            int steps = Mathf.CeilToInt(range / resolution);
            for (int s = 1; s <= steps; s++)
            {
                Vector3 point = Vector3.Lerp(robotPos, hitPoint, (float)s / steps);
                SetOccupancy(point, s == steps ? 100 : 0);
            }
        }
        Redraw();
    }

    void SetOccupancy(Vector3 worldPos, int value)
    {
        // 월드 좌표 → 격자 좌표 (맵 중심 = robotTransform 초기 위치)
        Vector3 mapCenter = transform.position;
        float localX = worldPos.x - mapCenter.x;
        float localZ = worldPos.z - mapCenter.z;

        int gx = Mathf.RoundToInt((localX / mapWorldSize + 0.5f) * gridSize);
        int gz = Mathf.RoundToInt((localZ / mapWorldSize + 0.5f) * gridSize);

        if (gx < 0 || gx >= gridSize || gz < 0 || gz >= gridSize) return;

        // occupied를 free로 덮어쓰지 않게 보존
        if (value == 100 || occupancy[gx, gz] == -1)
            occupancy[gx, gz] = value;
    }

    void Redraw()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                int v = occupancy[x, y];
                if (v == -1)
                    mapTexture.SetPixel(x, y, Color.gray);
                else if (v == 100)
                    mapTexture.SetPixel(x, y, Color.white);
                else
                    mapTexture.SetPixel(x, y, Color.black);
            }
        }
        mapTexture.Apply();
    }
}
```

### 4-4. 맵 표시용 Quad 만들기

1. Hierarchy 우클릭 → **3D Object > Quad**
2. 이름 `MapDisplay`
3. **Add Component > MapRenderer** 추가
4. Inspector 연결:
   - **Robot Transform** = `turtlebot3_burger` (Rigidbody 오브젝트)
   - **Lidar** = `base_scan`의 LidarSensor
   - **Position**: 맵 Quad를 위에서 내려다보게 `(0, 0.15, 0)` 정도로 (Y는 완충값)

> ⚠️ **맵 중심 주의**: MapRenderer의 `transform.position`을 맵 중심으로 사용합니다. Quad를 월드 원점(0,0,0)에 두고 로봇도 0 근처에서 출발해야 정확합니다.

### 4-5. Play 테스트 (맵 확인)

1. 로봇을 움직이며(W/S/A/D) 장애물 근처를 지나가게 함
2. Quad를 위에서 보면 로봇 경로와 장애물 위치가 **하얀 점**으로 표시되는지 확인
3. 지나간 자리가 **검정(free)**으로 채워지는지 확인

> 💡 **팁**: Quad를 바라보도록 Main Camera를 위에서 내려다보는 각도로 조정하면 맵이 실시간으로 그려지는 모습을 보기 좋습니다.

---

## 5. 오도메트리 구현

### 5-1. 개념: 오도메트리

**오도메트리(Odometry)** 는 센서(바퀴 회전, IMU)를 적분하여 로봇의 **현재 위치와 방향**을 추정하는 것입니다.

```
위치 = 이전 위치 + (속도 × 시간)
방향 = 이전 방향 + (각속도 × 시간)
```

### 5-2. 실제 바퀴 회전 기반 vs Rigidbody 기반

| 방식 | 설명 |
|------|------|
| **바퀴 회전 각도 적분** | 실제 로봇처럼 각 바퀴의 회전량(엔코더)을 적분. 슬립이 있으면 오차 발생 |
| **Rigidbody 직접 사용** | 물리엔진이 정확한 위치를 주므로 오차 없음 (하지만 엔코더 모사가 아님) |

여기서는 **바퀴 회전 관찰** 방식으로 오도메트리 개념을 학습하되, 기준은 Rigidbody 위치로 보정합니다.

### 5-3. OdometrySensor 스크립트 생성

Project 창 → **Assets** 우클릭 → **Create > C# Script** → 이름: `OdometrySensor`

```csharp
using UnityEngine;

public class OdometrySensor : MonoBehaviour
{
    [Header("바퀴 반경 (인치→m)")]
    public float wheelRadius = 0.033f;     // 33mm
    public float wheelBase = 0.160f;       // 바퀴 간 거리 (m), 좌우 0.08 + 0.08

    [Header("기준 Rigidbody (물리 보정용)")]
    public Rigidbody targetRb;

    [Header("출력")]
    // 실제 로봇이 발행하는 nav_msgs/Odometry 필드
    public Vector3 position;      // x, z 평면 위치 (y=0)
    public float yaw;             // 방향 (라디안)
    public Vector2 linearVel;     // 선속도 (전진 v, 후진)
    public float angularVel;      // 각속도 (rady/s)

    // 좌우 바퀴 회전 관찰용
    private Transform wheelLeft;
    private Transform wheelRight;
    private float lastLeftAngle;
    private float lastRightAngle;
    private Vector3 lastPos;

    void Start()
    {
        FindWheels();
        if (targetRb != null)
        {
            position = targetRb.position;
            lastPos = targetRb.position;
        }
    }

    void FindWheels()
    {
        // 05단계에서 만든 바퀴 transform
        wheelLeft = FindChildRecursive(transform, "wheel_left_link");
        wheelRight = FindChildRecursive(transform, "wheel_right_link");
        if (wheelLeft != null) lastLeftAngle = GetWheelAngle(wheelLeft);
        if (wheelRight != null) lastRightAngle = GetWheelAngle(wheelRight);
    }

    float GetWheelAngle(Transform wheel)
    {
        return wheel.localEulerAngles.x * Mathf.Deg2Rad;
    }

    void FixedUpdate()
    {
        if (targetRb == null) return;

        // --- 실시간 위치는 Rigidbody로부터 (정확) ---
        Vector3 rbPos = targetRb.position;
        position = new Vector3(rbPos.x, 0, rbPos.z);
        yaw = targetRb.rotation.eulerAngles.y * Mathf.Deg2Rad;

        // --- 속도는 Rigidbody 속도로 ---
        Vector3 vel = targetRb.velocity;
        // 로컬 전진 방향 성분
        Vector3 localVel = targetRb.transform.InverseTransformDirection(vel);
        linearVel = new Vector2(localVel.z, localVel.x); // 전진=z
        angularVel = targetRb.angularVelocity.y;

        // --- 바퀴 회전 관찰 (엔코더 개념) ---
        // 참고용: 실제 바퀴 회전량을 거리로 환산
        if (wheelLeft != null && wheelRight != null)
        {
            float dLeft = (GetWheelAngle(wheelLeft) - lastLeftAngle) * wheelRadius;
            float dRight = (GetWheelAngle(wheelRight) - lastRightAngle) * wheelRadius;
            // (여기선 물리 보정을 쓰므로 엔코더 거리는 표시만)
            lastLeftAngle = GetWheelAngle(wheelLeft);
            lastRightAngle = GetWheelAngle(wheelRight);
        }

        lastPos = position;
    }

    Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform found = FindChildRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
```

### 5-4. 스크립트 연결

1. Hierarchy에서 **turtlebot3_burger** 선택
2. **Add Component > OdometrySensor** 추가
3. Inspector에서 **Target Rb** = `turtlebot3_burger` (Rigidbody) 드래그

### 5-5. 확인 방법 (Debug 로그)

OdometrySensor에 표시 확인용 메서드를 추가하여 로그로 확인:

```csharp
    // Inspector에서 수동 호출 가능한 확인용 (또는 Update에서 로그)
    void OnGUI()
    {
        // 화면 좌상단에 오도메트리 출력
        GUILayout.BeginArea(new Rect(10, 10, 300, 120));
        GUILayout.Label($"Pos: ({position.x:F2}, {position.z:F2})");
        GUILayout.Label($"Yaw: {yaw * Mathf.Rad2Deg:F1}°");
        GUILayout.Label($"Linear: {linearVel.x:F2} m/s");
        GUILayout.Label($"Angular: {angularVel:F2} rad/s");
        GUILayout.EndArea();
    }
```

> **ROS 대응**: 이 값들이 실제 로봇에서는 `nav_msgs/Odometry` `/odom` 토픽으로 발행되며, `odom → base_link` TF가 함께 나갑니다. 나중에 ROS2 브릿지에서 이 필드를 그대로 매핑합니다.

---

## 6. IMU 센서 구현

### 6-1. 개념: IMU

**IMU(Inertial Measurement Unit)** 는:
- **가속도계** (accelerometer): 선형 가속도 (m/s²)
- **자이로스코프** (gyroscope): 각속도 (rad/s)
- (선택) **자력계** (magnetometer): 방위각

### 6-2. 실제 데이터 출처

Unity에서 Rigidbody로부터:
- **각속도** = `Rigidbody.angularVelocity` (rad/s, 로컬 좌표)
- **선형 가속도** = 속도 변화율 (`velocity`를 매 프레임 미분)

> 참고: 실제 IMU는 "중력 가속도(1g)"도 측정합니다. 로봇이 정지해 있어도 z축에 약 9.81이 찍힙니다. 여기서는 학습 목적으로 이 부분을 선택적으로 포함합니다.

### 6-3. ImuSensor 스크립트 생성

Project 창 → **Assets** 우클릭 → **Create > C# Script** → 이름: `ImuSensor`

```csharp
using UnityEngine;

public class ImuSensor : MonoBehaviour
{
    [Header("기준 Rigidbody")]
    public Rigidbody targetRb;

    [Header("중력 포함 여부 (실제 IMU는 항상 1g 받음)")]
    public bool includeGravity = true;

    [Header("출력 (sensor_msgs/Imu 구조)")]
    public Vector3 angularVelocity;   // rad/s (로컬 xyz)
    public Vector3 linearAcceleration; // m/s² (로컬 xyz)

    // 가속도 계산용 오차 보정
    private Vector3 prevVelocity;
    private float filter = 0.8f; // 저역통과 필터 (노이즈 제거)

    void Start()
    {
        if (targetRb != null)
            prevVelocity = targetRb.velocity;
    }

    void FixedUpdate()
    {
        if (targetRb == null) return;

        // --- 자이로스코프: 각속도 (로컬 좌표) ---
        // Rigidbody.angularVelocity는 월드 좌표 → 로컬 변환
        angularVelocity = targetRb.transform.InverseTransformDirection(targetRb.angularVelocity);

        // --- 가속도계: 선형 가속도 ---
        Vector3 worldVel = targetRb.velocity;
        Vector3 worldAccel = (worldVel - prevVelocity) / Time.fixedDeltaTime;
        prevVelocity = worldVel;

        // 로컬 좌표로 변환
        Vector3 localAccel = targetRb.transform.InverseTransformDirection(worldAccel);

        // 중력 가속도 추가 (실제 IMU는 z축에 -9.81 고정 관측)
        if (includeGravity)
        {
            // 로봇 좌표계에서 중력은 아래(y) 방향
            localAccel.y -= 9.81f;
        }

        // 저역통과 필터로 노이즈 제거
        linearAcceleration = Vector3.Lerp(linearAcceleration, localAccel, 1f - filter /* = 0.2 */);

        // filter 변수를 0.2로 해석하도록 값 조정
        // (위 Lerp t = 0.2로 새 값 비중)
    }
}
```

> ⚠️ **필터 설명**: 위 코드에서 `Vector3.Lerp(a, b, t)`의 `t`는 0~1 사이 비율입니다. `1f - filter = 0.2`이므로 새 값의 20%만 반영되어 부드러워집니다. (`filter = 0.8`)

### 6-4. 스크립트 연결

1. Hierarchy에서 **imu_link** 선택
2. **Add Component > ImuSensor** 추가
3. **Target Rb** = `turtlebot3_burger` (Rigidbody)
4. (선택) 확인용 OnGUI 추가

**ImuSensor에 확인용 추가:**
```csharp
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 140, 300, 120));
        GUILayout.Label("-- IMU --");
        GUILayout.Label($"Gyro: ({angularVelocity.x:F2}, {angularVelocity.y:F2}, {angularVelocity.z:F2}) rad/s");
        GUILayout.Label($"Accel: ({linearAcceleration.x:F2}, {linearAcceleration.y:F2}, {linearAcceleration.z:F2}) m/s²");
        GUILayout.EndArea();
    }
```

### 6-5. Play 테스트 (IMU 확인)

| 동작 | 기대값 |
|------|--------|
| 로봇 정지 | Gyro ≈ 0, Accel y ≈ -9.81 (중력) |
| A/D로 회전 | Gyro y 값이 회전 방향에 따라 ±로 표시 |
| W/S로 직진 | Accel z 값이 가감속 시 변화 |

> **ROS 대응**: 이 값들이 `sensor_msgs/Imu` `/imu` 토픽으로 발행됩니다. 실제 ROS SLAM(예: cartographer)은 `/odom`, `/scan`, `/tf`를 사용하고 IMU는 보조로 사용합니다.

---

## 7. 전체 테스트

### 7-1. 최종 Hierarchy 구조 (요약)

```
turtlebot3_burger
├─ TurtleBot3Setup        (05단계)
├─ TurtleBot3Controller   (05단계, W/S/A/D 이동)
├─ OdometrySensor         (이번 단계)
└─ base_footprint
   └─ base_link
      ├─ wheel_left_link
      ├─ wheel_right_link
      ├─ imu_link
      │   └─ ImuSensor     (이번 단계)
      └─ base_scan
          ├─ LidarRotator  (LidarSensor 생성)
          └─ LidarSensor   (이번 단계)

Obstacle1 / Obstacle2 / Obstacle3   (장애물)
MapDisplay (Quad + MapRenderer)     (맵)
```

### 7-2. 페이즈별 시나리오

1. **Play 시작**
2. Scene 뷰에서 LiDAR 녹색 링 확인
3. 화면 좌상단에 Odometry / IMU 값 표시 확인
4. W/S/A/D로 로봇 이동
5. 장애물 주변을 지나며 MapDisplay가 검정/흰색으로 채워지는지 확인
6. 맵이 로봇 경로를 따라 점진적으로 그려지는 모습 관찰

### 7-3. 확인 포인트

```
✅ LiDAR: 회전 레이저 + 360° 각도별 거리 (ranges[360])
✅ 장애물: 3개의 Cube가 레이저 반사 지점 생성
✅ 맵: MapDisplay에 장애물이 흰 점, 빈 공간이 검정으로 표시
✅ 오도메트리: 위치/방향/속도가 이동에 따라 갱신
✅ IMU: 회전 시 각속도, 가감속 시 가속도 변화
```

---

## 8. 문제 해결 체크리스트

### 문제 1: 레이저 라인이 보이지 않음

| 확인 | 해결 |
|------|------|
| LidarSensor가 base_scan에 있는지 | Add Component 확인 |
| drawRays가 true인지 | Inspector 확인 |
| 레이어 충돌 | Raycast가 물리를 포함하는지 (기본) |
| 카메라 위치 | Scene 뷰를 로봇 위/옆에서 확인 |

### 문제 2: 레이저가 장애물을 통과함

| 확인 | 해결 |
|------|------|
| 장애물에 Collider가 있는지 | Cube는 기본 BoxCollider 있음 |
| 장애물 Position Y가 rayHeight와 겹치는지 | 레이저 높이(0.15)와 장애물 높이 확인 |

### 문제 3: 맵이 그려지지 않음

| 확인 | 해결 |
|------|------|
| MapRenderer에 LidarSensor 연결됐는지 | Inspector 드래그 |
| Quad Position이 (0,?,0)인지 | 맵 중심을 원점에 |
| autoUpdate 체크 | true 확인 |
| 로봇이 움직이는지 | W 키로 이동 후 확인 |

### 문제 4: 오도메트리 값이 0

| 확인 | 해결 |
|------|------|
| Target Rb가 turtlebot3_burger인지 | Inspector 연결 |
| Rigidbody.isKinematic이 해제인지 | 05단계 확인 |

### 문제 5: IMU 가속도가 튐

| 확인 | 해결 |
|------|------|
| fixedDeltaTime 기본값(0.02) 유지 | Time Manager |
| filter 값 | 노이즈가 심하면 더 부드럽게 |

---

## 파일 구조 (06단계 최종)

```
Assets\
  URDF\
    turtlebot3_burger.urdf
    meshes\
      bases\burger_base.stl
      wheels\left_tire.stl, right_tire.stl
      sensors\lds.stl
  TurtleBot3Setup.cs          ← 05단계
  TurtleBot3Controller.cs     ← 05단계
  LidarSensor.cs              ← 이번 단계 (LiDAR)
  MapRenderer.cs              ← 이번 단계 (맵핑)
  OdometrySensor.cs           ← 이번 단계 (오도메트리)
  ImuSensor.cs                ← 이번 단계 (IMU)
```

---

## Isaac Sim 대응 요약

이 Unity 구현은 Isaac Sim의 다음 기능들과 개념적으로 동일합니다:

| Unity (이 문서) | Isaac Sim |
|----------------|-----------|
| LidarSensor + Raycast | RTX Lidar (Example_Rotary_2D) |
| ranges[360] 배열 | `sensor_msgs/LaserScan` `/scan` |
| MapRenderer (occupancy) | SLAM 패키지 (cartographer/gmapping) |
| OdometrySensor | `Isaac Compute Odometry` + `/odom` |
| ImuSensor | ROS2 IMU 브릿지 `/imu` |
| TF 개념 | `odom → base_link → base_scan` 트리 |

> **다음 단계(7단계)**: 이 센서 데이터를 **TCP/IP나 ROS2 브릿지로 외부 Python으로 전송**하여 실제 SLAM 패키지(cartographer)로 맵을 만드는 확장.

---

> **출처**: NVIDIA Isaac Sim ROS2 튜토리얼 (RTX Lidar, Transform Trees and Odometry)를 Unity 기반으로 번안  
> **최종 업데이트**: 2026년 9월
