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
9. [ROS2 RViz2로 맵/센서 전송 (브릿지 연동)](#9-ros2-rviz2로-맵센서-전송-브릿지-연동)

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

// ############################################################
// # LidarSensor
// # 역할: TurtleBot3의 2D LiDAR(LDS-02/LDS-03)를 시뮬레이션합니다.
// #       - 회전 로테이터를 만들어 시각적으로 레이저를 회전시킵니다.
// #       - 매 프레임 360° Raycast로 각도별 거리값(ranges[])을 측정합니다.
// #       - LineRenderer로 녹색 레이저 링을 Scene/Game 뷰에 그립니다.
// # 부착 위치: base_scan (LiDAR 링크)에 부착해야 정확한 원점에서 측정됩니다.
// ############################################################
public class LidarSensor : MonoBehaviour
{
    // ---------- [LDS-02 사양 (기본) / LDS-03 사양으로 조정 가능] ----------
    // 각도별 분해능 개수. LDS-02는 1° → 360개, LDS-03은 0.9° → 약 400개.
    public int rayCount = 360;
    // 최소 측정 거리(m). 이보다 가까운 물체는 무시. (LDS-02: 0.16, LDS-03: 0.05)
    public float rangeMin = 0.12f;
    // 최대 측정 거리(m). 이보다 먼 곳은 감지 안 됨. (LDS-02: 8.0, LDS-03: 12.0)
    public float rangeMax = 3.5f;
    // 스캔 주기(Hz). LDS-02는 5Hz, LDS-03은 10Hz.
    public float scanRate = 5f;
    // 시각적 회전 속도(RPM 개념, deg/s로 환산). 실제 센서 모터 회전 표현용.
    public float rotationSpeed = 1800f;

    // ---------- [시각화] ----------
    public bool drawRays = true;
    public Color rayColor = Color.green;
    // 레이저 발사 높이(m). base_scan 위쪽으로 올려 로봇 몸체와 겹치지 않게 함.
    public float rayHeight = 0.15f;

    // 각도별 거리값 배열 (ROS LaserScan.range 구조와 동일). 외부(맵핑)에서 읽습니다.
    public float[] ranges;

    private Transform rotator;
    private LineRenderer[] lines;      // 레이저 링을 그리는 라인렌더러
    private int lastPointIndex = -1;

    void Awake()
    {
        CreateRotator();   // 회전용 자식 오브젝트 생성
        CreateRayLines();  // LineRenderer 생성 (녹색 링)
        // ranges 배열 초기화: 기본값을 최대 측정 거리로 채움 (아무것도 없으면 최대값)
        ranges = new float[rayCount];
        for (int i = 0; i < rayCount; i++)
            ranges[i] = rangeMax;
    }

    void Start()
    {
        // 초기화 확인용 로그: 이 로그가 보이면 스크립트가 정상 실행 중이라는 뜻.
        bool shaderOk = lines != null && lines.Length > 0 && lines[0] != null
                        && lines[0].material != null && lines[0].material.shader != null;
        // base_scan의 실제 월드 위치를 출력해 레이저 원점(origin)이 맞는지 확인한다.
        Vector3 origin = transform.position + Vector3.up * rayHeight;
        Debug.Log($"[LidarSensor] 초기화됨. base_scan={gameObject.name}, LineRenderer={shaderOk}, rayCount={rayCount}, 월드위치={transform.position}, 레이저원점={origin}");
    }

    // 회전용 빈 오브젝트("LidarRotator")를 base_scan 아래 자식으로 생성.
    // 라인 자체는 고정하고 로테이터만 회전시켜 레이저가 돌며 훑는 듯한 시각 효과를 줍니다.
    void CreateRotator()
    {
        GameObject rotGO = new GameObject("LidarRotator");
        rotGO.transform.SetParent(transform, false);
        rotator = rotGO.transform;
    }

    // 360개 거리 점을 잇는 LineRenderer 1개를 base_scan에 추가하여 링 모양을 그림.
    void CreateRayLines()
    {
        lines = new LineRenderer[1];
        LineRenderer lr = gameObject.AddComponent<LineRenderer>();

        // 렌더러 기본 설정을 명시적으로 지정 (셰이더/월드좌표/길이 보정)
        lr.useWorldSpace = true;              // 월드 좌표로 점 배치 (로봇 이동에도 따라감)
        lr.widthMultiplier = 1f;
        lr.loop = false;                      // 닫는 점은 수동으로 추가
        lr.receiveShadows = false;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        lr.positionCount = rayCount + 1;      // 360점 + 닫는 점 1개
        lr.startWidth = 0.005f;               // 선 두께
        lr.endWidth = 0.005f;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        // 셰이더를 여러 후보에서 차례로 시도해 안전하게 생성 (null이면 예외 방지)
        Shader sh = Shader.Find("Sprites/Default");
        if (sh == null) sh = Shader.Find("Legacy Shaders/Diffuse");
        if (sh == null) sh = Shader.Find("Unlit/Color");
        if (sh == null) sh = Shader.Find("Hidden/Internal-Colored");
        lr.material = new Material(sh);
        lr.material.color = Color.white;      // 타일 컬러는 흰색 → startColor가 그대로 보임
        lr.startColor = rayColor;
        lr.endColor = rayColor;
        lines[0] = lr;
    }

    void Update()
    {
        // 시각화: 회전 오브젝트를 매 프레임 회전시켜 돌아가는 레이저 표현
        rotator.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // 데이터/실제 선: drawRays가 켜져 있으면 매 프레임 360° 전체를 측정해 라인 갱신
        // (실제 센서는 회전 주기로 누적 출력하지만, 학습 목적으로 매 프레임 측정해도 무방)
        if (drawRays)
            UpdateScan();
    }

    // 매 프레임 360°를 Raycast로 측정하고 ranges[]와 레이저 링을 갱신
    void UpdateScan()
    {
        // 발사 시작점: base_scan 월드위치에서 위로 rayHeight만큼 (장애물/라인 높이)
        Vector3 origin = transform.position + Vector3.up * rayHeight;

        for (int i = 0; i < rayCount; i++)
        {
            float angleRad = i * Mathf.Deg2Rad; // 0 ~ 359°
            // 로봇의 forward(전방)가 로컬 Z축이라는 점에 주의.
            // X = sin(각도), Z = cos(각도) 로 Y축 회전한 방향 벡터를 만듦.
            // ★ 중요: Physics.Raycast는 월드 좌표로 방향을 받으므로,
            //   TransformDirection으로 로봇(base_scan)의 회전을 반드시 반영해야 합니다.
            //   이 변환이 없으면 ranges[i]가 월드 고정 각도로 측정되어,
            //   맵핑에서 로봇 회전 보정과 어긋나 맵이 로봇과 함께 돌아가는 버그가 생깁니다.
            Vector3 localDir = new Vector3(Mathf.Sin(angleRad), 0, Mathf.Cos(angleRad));
            Vector3 direction = transform.TransformDirection(localDir);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, rangeMax))
            {
                float dist = hit.distance;
                if (dist < rangeMin)
                    ranges[i] = float.PositiveInfinity; // 최소거리 미만은 "감지 안 됨" 처리
                else
                    ranges[i] = dist;                   // 실제 반사 거리 저장
            }
            else
            {
                ranges[i] = rangeMax; // 화면(맵) 바깥은 최대값 = 감지 안 됨
            }

            float validDist = (float.IsInfinity(ranges[i])) ? rangeMax : ranges[i];

            // 라인렌더러: i번째 링 점 = 원점 + 방향 × 거리 (링을 이루는 점)
            // (주의: SetPosition(i, origin)처럼 원점을 찍으면 링이 점 하나로 뭉개짐)
            lines[0].SetPosition(i, origin + direction * validDist);

            // 보조 시각화: Scene 뷰(Gizmos ON)에 무조건 보이는 레이저 선.
            // LineRenderer와 무관하게 동작하므로, 렌더링 원인을 분리 확인하는 용도.
            if (i % 6 == 0)
                Debug.DrawRay(origin, direction * validDist, rayColor);
        }

        // 링을 닫기 위해 마지막 점(rayCount)을 첫 점과 이어줌.
        // (0.001m 위로 살짝 올려 점이 겹쳐 깜빡이는 걸 방지)
        lines[0].SetPosition(rayCount, lines[0].GetPosition(0) + Vector3.up * 0.001f);
    }

    // 외부(맵핑/ROS)에서 각도별 거리를 얻는 API
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

> 🚨 **전제: LidarSensor 회전 보정이 선행돼야 합니다 (2-2 참고)**
>
> `LidarSensor.UpdateScan()`에서 레이저 방향을 `transform.TransformDirection(localDir)`으로
> **로봇(base_scan) 회전을 반영**해 발사해야 합니다. 반영하지 않으면 `ranges[i]`가 월드 고정
> 각도로 측정되어, 맵핑에서 로봇 회전을 다시 더할 때 **맵이 로봇과 함께 돌아가고 장애물
> 위치가 어긋나거나 지워지는** 버그가 발생합니다. (실제 씬에서 확인된 대표 버그)

**이번 구현: 확률 기반 점유격자 (Probabilistic Occupancy Grid)**

앞서 장애물 위치를 고정값(0/100/-1)으로 저장하던 방식을, 실제 SLAM(gmapping/cartographer)처럼
**각 칸에 점유확률(0~1)** 을 저장하는 방식으로 개선했습니다.

| 표시 | 점유확률 조건 | RViz Map 색 |
|------|--------------|------------|
| 장애물 (occupied) | 확률 ≥ 0.7 | **검정 (black)** |
| 비어 있음 (free) | 확률 ≤ 0.3 | **흰색 (white)** |
| 미탐색 (unknown) | 0.3 ~ 0.7 | **회색 (gray)** |

> 🎨 **RViz 색상 규칙**: ROS `nav_msgs/OccupancyGrid`를 RViz 기본 Map 스킴으로 표시하면
> **점유 = 검정, 자유 = 흰색, 미탐색 = 회색**입니다. (초안에서 "자유=검정/장애물=흰색"으로
> 적었던 것은 RViz와 반대였으므로 수정. 실제 TurtleBot3 SLAM 맵(`.pgm`)도 동일한 규칙입니다.)

**왜 확률(누적) 방식인가?**
- 레이저 **끝점(장애물)** → 해당 칸의 확률을 `+hitIncrease` 올림
- 레이저 **통과 경로(free)** → 해당 칸의 확률을 `-missDecrease` 내림
- 장애물은 계속 레이저에 맞아 검정(장애물)으로 선명해지고, 지나간 경로는 흰색(free)으로 흐려짐
- **한 번 보인 장애물이 무한정 남는 게 아니라**, 이후 그 자리가 빈 공간으로 판정되면 확률이
  내려가 실제 상황이 반영됨 (동적 환경에서도 유효)

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

// 실제 SLAM(gmapping/cartographer)의 점유확률(Occupancy Probability) 방식을 따른 맵 렌더러입니다.
// - 각 격자에 확률(0~1)을 저장합니다. 0.5가 초기(unknown)값입니다.
// - 레이저 끝점(장애물)은 확률을 높이고, 경로(free)는 확률을 낮춥니다.
// - 표시 (RViz Map 규칙): 확률이 높으면 검정(장애물), 낮으면 흰색(free), 중간값이면 회색(unknown).
//   결과적으로 장애물만 선명하게 남고 지나간 경로는 흐려집니다.
public class MapRenderer : MonoBehaviour
{
    [Header("맵 설정")]
    public int gridSize = 200;         // 200x200 픽셀
    public float resolution = 0.05f;   // 5cm/픽셀
    [Tooltip("참고용 로봇 Transform (현재 맵핑은 lidar.transform을 기준으로 계산하므로 선택 사항)")]
    public Transform robotTransform;
    public LidarSensor lidar;

    [Header("연결")]
    public bool autoUpdate = true;

    [Header("화면 표시")]
    [Tooltip("MapDisplay Quad(3D 공간)에 맵 텍스처를 표시할지 (true면 3D 바닥에도 보임)")]
    public bool showOnQuad = true;
    [Tooltip("화면 코너에 RViz처럼 별도 2D 맵 패널을 표시할지")]
    public bool showOnScreen = true;
    private Rect panelRect = new Rect(10, 250, 300, 300); // 화면 좌상단 아래에 패널

    [Header("점유확률 파라미터")]
    [Tooltip("레이저 끝점(장애물)일 때 확률 증가량 (기본 0.3)")]
    public float hitIncrease = 0.3f;
    [Tooltip("경로(빈 공간)일 때 확률 감소량 (기본 0.1)")]
    public float missDecrease = 0.1f;
    [Tooltip("확률이 이 값 이상이면 검정(장애물)으로 판정")]
    public float occupiedThreshold = 0.7f;
    [Tooltip("확률이 이 값 이하이면 흰색(free)으로 판정")]
    public float freeThreshold = 0.3f;

    private Texture2D mapTexture;
    private float[,] occupancy; // 점유확률 0~1, 0.5=unknown
    private float mapWorldSize;

    void Start()
    {
        mapWorldSize = gridSize * resolution; // 200 * 0.05 = 10m

        occupancy = new float[gridSize, gridSize];
        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
                occupancy[x, y] = 0.5f; // 초기 미탐색(unknown)

        mapTexture = new Texture2D(gridSize, gridSize);
        mapTexture.filterMode = FilterMode.Point;
        GetComponent<Renderer>().material.mainTexture = mapTexture;

        // 3D Quad에 표시할지 여부에 따라 렌더러 켜기/끄기
        GetComponent<Renderer>().enabled = showOnQuad;
        Redraw();

        // 자식 Quad 크기를 맵 크기에 맞춤
        transform.localScale = new Vector3(mapWorldSize, mapWorldSize, 1);
    }

    void Update()
    {
        if (autoUpdate && lidar != null)
            DrawLidarScan();
    }

    // LiDAR 스캔을 격자에 반영 (확률 갱신)
    void DrawLidarScan()
    {
        for (int i = 0; i < lidar.rayCount; i++)
        {
            float range = lidar.GetRange(i);
            if (float.IsInfinity(range)) continue;

            float angleRad = i * Mathf.Deg2Rad;

            // 로봇의 월드 회전 고려 (로봇이 돌면 레이저도 같이 돔)
            // 주의: LidarSensor가 TransformDirection으로 base_scan 회전을 반영해 측정하므로,
            //       맵핑에서도 반드시 동일한 기준(lidar.transform)을 사용해야 합니다.
            //       robotTransform(루트)을 쓰면 base_scan의 로컬 회전 오프셋이 어긋나 맵이 틀어질 수 있습니다.
            float worldAngle = lidar.transform.eulerAngles.y * Mathf.Deg2Rad + angleRad;
            Vector3 dir = new Vector3(Mathf.Sin(worldAngle), 0, Mathf.Cos(worldAngle));

            // 레이저 원점 위치 (로봇 위치) — 센서 기준으로 사용
            Vector3 robotPos = lidar.transform.position;
            Vector3 hitPoint = robotPos + dir * range;

            // 경로(free) 칸: 확률 감소, 끝점(장애물) 칸: 확률 증가
            int steps = Mathf.CeilToInt(range / resolution);
            for (int s = 1; s <= steps; s++)
            {
                Vector3 point = Vector3.Lerp(robotPos, hitPoint, (float)s / steps);
                if (s == steps)
                    UpdateOccupancy(point, hitIncrease);   // 장애물 표면
                else
                    UpdateOccupancy(point, -missDecrease); // 빈 공간
            }
        }
        Redraw();
    }

    void UpdateOccupancy(Vector3 worldPos, float delta)
    {
        // 월드 좌표 → 격자 좌표 (맵 중심 = transform.position)
        Vector3 mapCenter = transform.position;
        float localX = worldPos.x - mapCenter.x;
        float localZ = worldPos.z - mapCenter.z;

        int gx = Mathf.RoundToInt((localX / mapWorldSize + 0.5f) * gridSize);
        int gz = Mathf.RoundToInt((localZ / mapWorldSize + 0.5f) * gridSize);

        if (gx < 0 || gx >= gridSize || gz < 0 || gz >= gridSize) return;

        // 확률을 0~1 범위로 clamp하며 갱신
        occupancy[gx, gz] = Mathf.Clamp01(occupancy[gx, gz] + delta);
    }

    // 외부(RosBridge/ROS2)에서 접근: 격자 점유확률을 ROS OccupancyGrid 값(-1/0/100)으로 변환해 반환.
    // - data 인덱스는 ROS 규칙(행 우선, 좌하단 기점)과 일치시킨다: data[gz * gridSize + gx]
    // - gx = 월드 X 방향(열, 컬럼), gz = 월드 Z 방향(행, 로우)
    public sbyte[] GetOccupancyData()
    {
        sbyte[] data = new sbyte[gridSize * gridSize];
        for (int gz = 0; gz < gridSize; gz++)
        {
            for (int gx = 0; gx < gridSize; gx++)
            {
                float p = occupancy[gx, gz];
                if (p >= occupiedThreshold)
                    data[gz * gridSize + gx] = 100;      // occupied
                else if (p <= freeThreshold)
                    data[gz * gridSize + gx] = 0;        // free
                else
                    data[gz * gridSize + gx] = -1;       // unknown
            }
        }
        return data;
    }

    // 외부(RosBridge)에서 맵 원점(좌하단) 구하기: 맵 중심을 기준으로 -크기/2 만큼 이동한 점.
    // Unity 좌표 (x=오른쪽, z=위) ↔ ROS 좌표 (x=오른쪽, y=위) 대응.
    public Vector3 GetMapOrigin()
    {
        float half = mapWorldSize * 0.5f;
        return transform.position + new Vector3(-half, 0f, -half);
    }

    void Redraw()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                float p = occupancy[x, y];
                // RViz Map 색상 규칙과 동일:
                //   점유(occupied)  = 검정(black)
                //   자유(free)      = 흰색(white)
                //   미탐색(unknown) = 회색(gray)
                if (p >= occupiedThreshold)
                    mapTexture.SetPixel(x, y, Color.black);      // 장애물
                else if (p <= freeThreshold)
                    mapTexture.SetPixel(x, y, Color.white);      // 빈 공간
                else
                    mapTexture.SetPixel(x, y, Color.gray);       // 미탐색/불확실
            }
        }
        mapTexture.Apply(); // 텍스처 변경사항을 GPU에 반영
    }

    // RViz처럼 화면 코너에 별도 2D 맵 패널을 그립니다.
    // - 검은 테두리 + 흰 프레임 안에 정사각형 맵 텍스처 → 3D 환경과 별도 화면에서 확인 가능
    void OnGUI()
    {
        if (!showOnScreen || mapTexture == null) return;

        // 외곽 검은 테두리 + 안쪽 흰 배경(패널 프레임)
        GUI.DrawTexture(panelRect, Texture2D.blackTexture);
        Rect inner = new Rect(panelRect.x + 2, panelRect.y + 2,
                              panelRect.width - 4, panelRect.height - 4);
        GUI.DrawTexture(inner, Texture2D.whiteTexture);

        // 맵 텍스처를 패널 중앙에 정사각형으로 그리기 (비율 유지)
        float size = Mathf.Min(panelRect.width - 10, panelRect.height - 10);
        Rect mapRect = new Rect(panelRect.x + (panelRect.width - size) / 2f,
                                panelRect.y + (panelRect.height - size) / 2f,
                                size, size);
        GUI.DrawTexture(mapRect, mapTexture);
    }
}
```

```
                    +----------------------------------------+
                    |             MapRenderer                |
                    | (gridSize, resolution, hit/miss delta) |
                    +----------------------------------------+
                                        |
                 +----------------------+----------------------+
                 |                                             |
                 v                                             v
  [1. 데이터 구조 및 초기화]                       [2. 매 프레임 업데이트 (Update)]
+---------------------------------+             +---------------------------------+
| - 2D Array: occupancy[200, 200] |             | - autoUpdate && lidar != null   |
|   (초기값: 0.5f = Unknown)      |             | - DrawLidarScan() 호출          |
| - Texture2D: mapTexture         |             +---------------------------------+
| - World Size = 200 * 0.05 = 10m |                            |
+---------------------------------+                            v
                                                [3. LiDAR 레이저 광선 처리]
                                                +---------------------------------+
                                                | for each Ray (0 ~ rayCount-1)   |
                                                |  - 로봇 위치(Origin) & 방위 계산 |
                                                |  - hitPoint (장애물 충돌 지점)  |
                                                +---------------------------------+
                                                               |
                                                               v
                                                [4. Ray Casting & 확률 업데이트]
                                                +---------------------------------+
                                                | Lerp(Origin, hitPoint, step)    |
                                                |  - 중간 경로 (Free Space)       |
                                                |    -> delta = -missDecrease     |
                                                |  - 끝점 (Obstacle Surface)      |
                                                |    -> delta = +hitIncrease      |
                                                |                                 |
                                                | UpdateOccupancy(worldPos, delta)|
                                                |  - World -> Grid 좌표 변환      |
                                                |  - occupancy = Clamp01(p + d)   |
                                                +---------------------------------+
                                                               |
                                                               v
                                                [5. 시각화 & 데이터 동기화]
                                                +---------------------------------+
                                                | Redraw()                        |
                                                |  - p >= 0.7  -> 검정 (Occupied) |
                                                |  - p <= 0.3  -> 흰색 (Free)     |
                                                |  - Else      -> 회색 (Unknown)  |
                                                |  - mapTexture.Apply()           |
                                                +---------------------------------+
                                                 /                               \
                                                /                                 \
                                               v                                   v
                                [6a. 3D Quad Display]               [6b. 2D Screen GUI]
                                +-------------------+               +-------------------+
                                | Material Texture  |               | OnGUI()           |
                                | 3D 바닥면 렌더링  |               | Screen Panel      |
                                +-------------------+               +-------------------+

                                                  [7. External ROS2 Interface]
                                                +---------------------------------+
                                                | GetOccupancyData()              |
                                                |  - sbyte[] 배열 변환            |
                                                |  - -1(Unknown) / 0 / 100        |
                                                | GetMapOrigin()                  |
                                                |  - 좌하단 Origin 좌표 반환      |
                                                +---------------------------------+
```

```
using UnityEngine;

// ############################################################
// # LidarSensor
// # 역할: TurtleBot3의 2D LiDAR(LDS-02/LDS-03)를 시뮬레이션합니다.
// #       - 회전 로테이터를 만들어 시각적으로 레이저를 회전시킵니다.
// #       - 매 프레임 360° Raycast로 각도별 거리값(ranges[])을 측정합니다.
// #       - LineRenderer로 녹색 레이저 링을 Scene/Game 뷰에 그립니다.
// # 부착 위치: base_scan (LiDAR 링크)에 부착해야 정확한 원점에서 측정됩니다.
// ############################################################
public class LidarSensor : MonoBehaviour
{
    // ---------- [LDS-02 사양 (기본) / LDS-03 사양으로 조정 가능] ----------
    // 각도별 분해능 개수. LDS-02는 1° → 360개, LDS-03은 0.9° → 약 400개.
    public int rayCount = 360;
    // 최소 측정 거리(m). 이보다 가까운 물체는 무시. (LDS-02: 0.16, LDS-03: 0.05)
    public float rangeMin = 0.12f;
    // 최대 측정 거리(m). 이보다 먼 곳은 감지 안 됨. (LDS-02: 8.0, LDS-03: 12.0)
    public float rangeMax = 3.5f;
    // 스캔 주기(Hz). LDS-02는 5Hz, LDS-03은 10Hz.
    public float scanRate = 5f;
    // 시각적 회전 속도(RPM 개념, deg/s로 환산). 실제 센서 모터 회전 표현용.
    public float rotationSpeed = 1800f;

    // ---------- [시각화] ----------
    public bool drawRays = true;
    public Color rayColor = Color.green;
    // 레이저 발사 높이(m). base_scan 위쪽으로 올려 로봇 몸체와 겹치지 않게 함.
    public float rayHeight = 0.15f;

    // 각도별 거리값 배열 (ROS LaserScan.range 구조와 동일). 외부(맵핑)에서 읽습니다.
    public float[] ranges;

    private Transform rotator;
    private LineRenderer[] lines;      // 레이저 링을 그리는 라인렌더러
    private int lastPointIndex = -1;

    void Awake()
    {
        CreateRotator();   // 회전용 자식 오브젝트 생성
        CreateRayLines();  // LineRenderer 생성 (녹색 링)
        // ranges 배열 초기화: 기본값을 최대 측정 거리로 채움 (아무것도 없으면 최대값)
        ranges = new float[rayCount];
        for (int i = 0; i < rayCount; i++)
            ranges[i] = rangeMax;
    }

    void Start()
    {
        // 초기화 확인용 로그: 이 로그가 보이면 스크립트가 정상 실행 중이라는 뜻.
        bool shaderOk = lines != null && lines.Length > 0 && lines[0] != null
                        && lines[0].material != null && lines[0].material.shader != null;
        // base_scan의 실제 월드 위치를 출력해 레이저 원점(origin)이 맞는지 확인한다.
        Vector3 origin = transform.position + Vector3.up * rayHeight;
        Debug.Log($"[LidarSensor] 초기화됨. base_scan={gameObject.name}, LineRenderer={shaderOk}, rayCount={rayCount}, 월드위치={transform.position}, 레이저원점={origin}");
    }

    // 회전용 빈 오브젝트("LidarRotator")를 base_scan 아래 자식으로 생성.
    // 라인 자체는 고정하고 로테이터만 회전시켜 레이저가 돌며 훑는 듯한 시각 효과를 줍니다.
    void CreateRotator()
    {
        GameObject rotGO = new GameObject("LidarRotator");
        rotGO.transform.SetParent(transform, false);
        rotator = rotGO.transform;
    }

    // 360개 거리 점을 잇는 LineRenderer 1개를 base_scan에 추가하여 링 모양을 그림.
    void CreateRayLines()
    {
        lines = new LineRenderer[1];
        LineRenderer lr = gameObject.AddComponent<LineRenderer>();

        // 렌더러 기본 설정을 명시적으로 지정 (셰이더/월드좌표/길이 보정)
        lr.useWorldSpace = true;              // 월드 좌표로 점 배치 (로봇 이동에도 따라감)
        lr.widthMultiplier = 1f;
        lr.loop = false;                      // 닫는 점은 수동으로 추가
        lr.receiveShadows = false;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        lr.positionCount = rayCount + 1;      // 360점 + 닫는 점 1개
        lr.startWidth = 0.005f;               // 선 두께
        lr.endWidth = 0.005f;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        // 셰이더를 여러 후보에서 차례로 시도해 안전하게 생성 (null이면 예외 방지)
        Shader sh = Shader.Find("Sprites/Default");
        if (sh == null) sh = Shader.Find("Legacy Shaders/Diffuse");
        if (sh == null) sh = Shader.Find("Unlit/Color");
        if (sh == null) sh = Shader.Find("Hidden/Internal-Colored");
        lr.material = new Material(sh);
        lr.material.color = Color.white;      // 타일 컬러는 흰색 → startColor가 그대로 보임
        lr.startColor = rayColor;
        lr.endColor = rayColor;
        lines[0] = lr;
    }

    void Update()
    {
        // 시각화: 회전 오브젝트를 매 프레임 회전시켜 돌아가는 레이저 표현
        rotator.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // 데이터/실제 선: drawRays가 켜져 있으면 매 프레임 360° 전체를 측정해 라인 갱신
        // (실제 센서는 회전 주기로 누적 출력하지만, 학습 목적으로 매 프레임 측정해도 무방)
        if (drawRays)
            UpdateScan();
    }

    // 매 프레임 360°를 Raycast로 측정하고 ranges[]와 레이저 링을 갱신
    void UpdateScan()
    {
        // 발사 시작점: base_scan 월드위치에서 위로 rayHeight만큼 (장애물/라인 높이)
        Vector3 origin = transform.position + Vector3.up * rayHeight;

        for (int i = 0; i < rayCount; i++)
        {
            float angleRad = i * Mathf.Deg2Rad; // 0 ~ 359°
            // 로봇의 forward(전방)가 로컬 Z축이라는 점에 주의.
            // X = sin(각도), Z = cos(각도) 로 Y축 회전한 방향 벡터를 만듦.
            // ★ 중요: Physics.Raycast는 월드 좌표로 방향을 받으므로,
            //   TransformDirection으로 로봇(base_scan)의 회전을 반드시 반영해야 합니다.
            //   이 변환이 없으면 ranges[i]가 월드 고정 각도로 측정되어,
            //   맵핑에서 로봇 회전 보정과 어긋나 맵이 로봇과 함께 돌아가는 버그가 생깁니다.
            Vector3 localDir = new Vector3(Mathf.Sin(angleRad), 0, Mathf.Cos(angleRad));
            Vector3 direction = transform.TransformDirection(localDir);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, rangeMax))
            {
                float dist = hit.distance;
                if (dist < rangeMin)
                    ranges[i] = float.PositiveInfinity; // 최소거리 미만은 "감지 안 됨" 처리
                else
                    ranges[i] = dist;                   // 실제 반사 거리 저장
            }
            else
            {
                ranges[i] = rangeMax; // 화면(맵) 바깥은 최대값 = 감지 안 됨
            }

            float validDist = (float.IsInfinity(ranges[i])) ? rangeMax : ranges[i];

            // 라인렌더러: i번째 링 점 = 원점 + 방향 × 거리 (링을 이루는 점)
            // (주의: SetPosition(i, origin)처럼 원점을 찍으면 링이 점 하나로 뭉개짐)
            lines[0].SetPosition(i, origin + direction * validDist);

            // 보조 시각화: Scene 뷰(Gizmos ON)에 무조건 보이는 레이저 선.
            // LineRenderer와 무관하게 동작하므로, 렌더링 원인을 분리 확인하는 용도.
            if (i % 6 == 0)
                Debug.DrawRay(origin, direction * validDist, rayColor);
        }

        // 링을 닫기 위해 마지막 점(rayCount)을 첫 점과 이어줌.
        // (0.001m 위로 살짝 올려 점이 겹쳐 깜빡이는 걸 방지)
        lines[0].SetPosition(rayCount, lines[0].GetPosition(0) + Vector3.up * 0.001f);
    }

    // 외부(맵핑/ROS)에서 각도별 거리를 얻는 API
    public float GetRange(int index) => ranges[index];
    public float GetRangeAtAngle(float angleDeg) => ranges[((int)angleDeg + 360) % rayCount];
}

```

### 4-4. 맵 표시용 Quad 만들기 + 화면 패널

1. Hierarchy 우클릭 → **3D Object > Quad**
2. 이름 `MapDisplay`
3. **Add Component > MapRenderer** 추가
4. **Rotation**을 `(90, 0, 0)`으로 설정!
   - ⚠️ **중요**: 기본 Quad는 세로로 서 있는 평면(법선 -Z)입니다. 이대로 두면 맵 텍스처가
     "수직으로 선 평면"처럼 보입니다. **X축 90° 눕혀** 바닥에 평평하게 깔아야 위에서
     내려다보는 지도가 됩니다. (Rotation X = 90)
5. Inspector 연결:
   - **Lidar** = `base_scan`의 LidarSensor ← **필수**. 위치/회전 기준을 센서 자체에서 가져옵니다.
   - **Robot Transform** = `turtlebot3_burger` (선택. 연결해도 되지만 확률 계산은 센서 기준입니다)
   - **Position**: 맵 Quad를 위에서 내려다보게 `(0, 0.15, 0)` 정도로 (Y는 완충값)
   - **autoUpdate** = `true` 유지

> ⚠️ **맵 중심 주의**: MapRenderer의 `transform.position`을 맵 중심으로 사용합니다. Quad를 월드 원점(0,0,0)에 두고 로봇도 0 근처에서 출발해야 정확합니다.

**화면 패널 설정 (RViz처럼 3D와 분리 보기)**

같은 화면에 3D 환경과 맵 Quad가 겹쳐 보이면 인식이 혼란스러울 수 있습니다.
`MapDisplay`의 MapRenderer 컴포넌트에서:

| 설정 | 값 | 효과 |
|------|-----|------|
| `showOnScreen` | `true` (기본) | 화면 좌상단 아래에 **별도 2D 맵 패널** 표시 (RViz Map 창처럼) |
| `showOnQuad` | `false`로 해제 | 3D 바닥 Quad 맵을 끄고 **화면 패널로만** 표시 → 3D 로봇 환경과 완전 분리 |

> 💡 화면 패널 위치/크기는 코드의 `panelRect` 값(`new Rect(10, 250, 300, 300)`)을 수정해 조절합니다.
> RViz처럼 3D 장면은 Game 뷰 그대로, 맵은 별도 패널에서 실시간으로 확인할 수 있습니다.

### 4-5. Play 테스트 (맵 확인)

1. 로봇을 움직이며(W/S/A/D) 장애물 근처를 지나가게 함
2. **RViz 색상 기준으로 확인**:
   - **장애물(검정)**: 레이저가 맞는 지점이 검정 점으로 선명하게 남음
   - **자유(흰색)**: 레이저가 통과한 경로가 흰색으로 채워짐
   - **미탐색(회색)**: 아직 스캔하지 않은 영역
3. **회전 안정성 확인**: 로봇을 A/D로 제자리 회전시켜도 **이미 그려진 맵은 회전하지 않고 고정**된 채
   새 방향 영역만 채워지는지 확인 (이전 버그였던 "맵이 로봇과 함께 도는" 현상이 없어야 정상)
4. 화면 패널(`showOnScreen`)에서도 동일한 맵이 그려지는지 확인

> 💡 **팁**: Quad를 바라보도록 Main Camera를 위에서 내려다보는 각도로 조정하거나,
> `showOnQuad = false`로 두고 화면 패널만 띄우면 RViz처럼 3D 환경과 맵을 분리해 볼 수 있습니다.

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

        // --- ★ 자동 탐색 ---
        // Target Rb를 Inspector에서 드래그하지 않아도, 자신 또는 하위 오브젝트에서
        // Rigidbody를 자동으로 찾습니다. (URDF 임포트로 Rigidbody가 하위에 있는 경우 대응)
        if (targetRb == null)
        {
            targetRb = GetComponentInChildren<Rigidbody>();
            if (targetRb != null)
                Debug.Log($"[OdometrySensor] Rigidbody 자동 탐색됨: {targetRb.gameObject.name}");
            else
                Debug.LogWarning("[OdometrySensor] Rigidbody를 찾지 못했습니다. 씬의 오브젝트에 Rigidbody를 추가하거나 Target Rb를 연결하세요.");
        }

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
2. Inspector에서 **Add Component** 클릭 → 검색란에 `OdometrySensor` 입력 → **OdometrySensor** 클릭
3. **Target Rb = 비워둬도 됩니다.** (5-3 코드의 `Start()`가 Rigidbody를 `GetComponentInChildren`으로 자동 탐색)

> 💡 **Rigidbody가 하나도 없을 때** (드래그해도 안 들어가는 경우가 바로 이것):
> 위 하이어러키처럼 `turtlebot3_burger ~ base_scan` 전부 **Rigidbody 없음(X)** 이면
> **Add Component > Rigidbody** 를 최상위 `turtlebot3_burger`에 추가하세요.
> 그러면 콘솔에 `[OdometrySensor] Rigidbody 자동 탐색됨: turtlebot3_burger` 로그가 찍힙니다.
> (로봇을 물리적으로 움직이는 05단계 컨트롤러가 Rigidbody를 쓴다면, 이 과정은 필수입니다)

> ⚠️ **Add Component 목록에 OdometrySensor가 안 보일 때 (2번이 안 되는 경우)**
> Unity는 **프로젝트 폴더(`Assets/`)에 컴파일 오류가 하나라도 있으면 모든 스크립트 컴포넌트를 비활성화**합니다.
> 이것이 "2번부터 안 되는" 가장 흔한 원인이며, 반드시 이것부터 확인하세요:
>
> 1. **Console 열기**: `Window > General > Console` → **빨간색 오류**가 있는지 확인.
>    오류 줄을 클릭하면 어느 파일/몇 번째 줄인지 나옵니다. 빨간 오류가 사라져야 Add Component에 스크립트가 나타납니다.
> 2. **코드를 방금 붙여넣었다면**: Unity 에디터 창을 클릭해 **재컴파일**을 유도 (오류가 사라질 때까지 기다림).
> 3. **파일명 = 클래스명**: `OdometrySensor.cs` 파일 안에 `public class OdometrySensor` 인지 확인.
>    파일명과 클래스명이 다르면 컴파일 오류가 됩니다.
> 4. **스크립트 위치**: 스크립트가 `Assets/` 폴더 **바깥**에 있으면 씬에서 못 씁니다. `Assets/` 안으로 이동.

### 5-5. 확인 방법 (Debug 로그)

OdometrySensor에 표시 확인용 메서드를 추가하여 로그로 확인:

> ✅ **프로젝트 파일에는 이미 반영되어 있습니다.** 아래 코드는 참고용으로, 직접 넣으려면
> `OdometrySensor.cs`의 **`FindChildRecursive(...)` 메서드 아래, 클래스 닫는 중괄호 `}` 앞**에 넣으면 됩니다.

```csharp
    // 화면 좌상단에 오도메트리 출력 (Inspector에서 확인하지 않아도 게임 뷰에서 바로 확인)
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 120));
        // 텍스트 색상: GUI.contentColor로 글씨 색을 바꿀 수 있음 (미지정 시 흰색)
        GUI.contentColor = Color.yellow;
        GUILayout.Label($"Pos: ({position.x:F2}, {position.z:F2})");
        GUILayout.Label($"Yaw: {yaw * Mathf.Rad2Deg:F1}°");
        GUILayout.Label($"Linear: {linearVel.x:F2} m/s");
        GUILayout.Label($"Angular: {angularVel:F2} rad/s");
        GUI.contentColor = Color.white; // 원래 색(흰색)으로 복원
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
        // --- ★ 자동 탐색 (상향 + 하향) ---
        // 이 스크립트는 imu_link 같은 하위 링크에 붙으므로, Rigidbody가 있는
        // 부모(예: turtlebot3_burger 루트)의 Rigidbody도 찾아야 합니다.
        // 순서: ① 부모 방향(자기 포함) → ② 자식 방향
        if (targetRb == null)
            targetRb = GetComponentInParent<Rigidbody>();
        if (targetRb == null)
            targetRb = GetComponentInChildren<Rigidbody>();
        if (targetRb != null)
            Debug.Log($"[ImuSensor] Rigidbody 자동 탐색됨: {targetRb.gameObject.name}");
        else
            Debug.LogWarning("[ImuSensor] Rigidbody를 찾지 못했습니다. 씬의 오브젝트에 Rigidbody를 추가하거나 Target Rb를 연결하세요.");

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
3. **Target Rb = 비워둬도 됩니다.** (6-3 코드의 `Start()`가 **부모 → 자식 순서**로 Rigidbody를 자동 탐색)
   - ⚠️ `imu_link`에 붙이므로 `GetComponentInChildren`만으론 부모(루트)의 Rigidbody를 못 찾습니다.
     수정된 코드는 `GetComponentInParent`(부모) → `GetComponentInChildren`(자식) 순으로 찾습니다.
   - ⚠️ 이 프로젝트의 Rigidbody는 `TurtleBot3Setup.Awake()`가 **Play 시점에 추가**하므로, 에디터에서
     드래그하려 해도 전부 X로 보입니다. 드래그하지 말고 **비워두세요.**
   - Play 시 콘솔에 `[ImuSensor] Rigidbody 자동 탐색됨: turtlebot3_burger` 로그가 찍히면 정상입니다.
4. (선택) 확인용 OnGUI 추가

**ImuSensor에 확인용 추가:**
```csharp
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 140, 300, 120));
        GUI.contentColor = Color.yellow; // 글씨 색 변경
        GUILayout.Label("-- IMU --");
        GUILayout.Label($"Gyro: ({angularVelocity.x:F2}, {angularVelocity.y:F2}, {angularVelocity.z:F2}) rad/s");
        GUILayout.Label($"Accel: ({linearAcceleration.x:F2}, {linearAcceleration.y:F2}, {linearAcceleration.z:F2}) m/s²");
        GUI.contentColor = Color.white; // 원래 색(흰색) 복원
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
✅ 맵: MapDisplay에 장애물이 검정 점, 빈 공간이 흰색(free)으로 표시 (RViz Map 규칙)
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

## 9. ROS2 RViz2로 맵/센서 전송 (브릿지 연동)

이 단계까지 만든 Unity 센서/맵 데이터를 **실제 ROS2 환경(Jazzy, Docker)의 RViz2**에 그대로 표시하는 확장입니다. Unity는 TCP 서버가 되고, 컨테이너 안의 Python 노드가 데이터를 받아 `nav_msgs/OccupancyGrid`, `sensor_msgs/LaserScan`, `nav_msgs/Odometry`, `tf`로 발행합니다.

### 9-1. 통신 구조와 좌표 변환 원리

```
Windows                                          Docker 컨테이너(ros_jazzy1)
Unity (RosBridge.cs, TCP 서버 0.0.0.0:8765)
   │  이진 프레임 ("TBR1" + msgType + payloadLen)
   ▼
UnityBridge (unity_bridge.py, TCP 클라이언트 → host.docker.internal:8765)
   └─ 발행: /map(OccupancyGrid)  /scan(LaserScan)  /odom(Odometry)  /tf, /tf_static
      → RViz2 표시
```

- Unity 좌표계(x=오른쪽, z=북쪽, y=위)를 ROS REP-103 규칙(x=동쪽, y=북쪽, z=위)으로 변환합니다.
  - **ROS 위치 = (Unity x, Unity z, 0)**
  - **ROS 요각 = π/2 − Unity yaw**  (`unity_bridge.py`의 `quat_from_yaw`에 반영)
  - **스캔 각도 반전**: Unity는 각도를 정면(+Z)·동쪽 방향(시계방향)으로 증가시키지만, ROS LaserScan은 +X(전방)에서 반시계방향으로 증가하므로 `ranges[]`를 역순으로 전송합니다 (`RosBridge.SerializeScan`).
- TF 트리: `map ←(static)→ odom ←(동적)→ base_footprint ←(static, 0.055m)→ base_scan`

> **왜 요각 변환이 필요한가?** Unity는 +Z가 "정면"이고 +X가 "오른쪽"이지만, ROS(REP-103)는 +X가 정면입니다. 따라서 Unity 0°(정북·+Z)를 ROS에서는 90°(동쪽)로 보정해야 로봇/스캔이 맵 위에 정확히 정렬됩니다.

### 9-2. 파일 배치와 실행 절차

새 파일 3개 (기존 `MapRenderer.cs`, `LidarSensor.cs`는 4장에서 만든 버전 그대로 사용):

| # | 파일 | 위치 | 역할 |
|---|------|------|------|
| 1 | `RosBridge.cs` | Unity `Assets/` | TCP 서버(8765). 맵/스캔/오도메트리를 이진 프레임으로 직렬화해 전송 |
| 2 | `unity_bridge.py` | `D:\github\unity_sim_example\07_ROS2_RViz_Bridge\` | TCP 클라이언트 → ROS2 토픽 발행 |
| 3 | `rviz_map_view.rviz` | 같은 폴더 | RViz2 표시 설정 (Fixed Frame = `map`) |


**RosBridge**

```
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
```

**07_ROS2_RViz_Bridge/rviz_map_view.rviz**


```
# RViz2 설정 파일 — Unity TurtleBot3 시뮬레이션 데이터 표시용
# 사용: rviz2 -d /mnt/d/github/unity_sim_example/07_ROS2_RViz_Bridge/rviz_map_view.rviz
Panels:
  - Class: rviz_common/Displays
    Name: Displays
  - Class: rviz_common/Views
    Name: Views
    Views:
      - Class: rviz_default_plugins/Orbit
        Name: Orbit View
Visibility:
  Grid: true
  Map: true
  LaserScan: true
  TF: true
Visualization Manager:
  Class: ""
  Displays:
    - Class: rviz_default_plugins/Grid
      Enabled: true
      Name: Grid
      Value: true
      Plane Cell Count: 20
      Plane Cell Size: 1
      Plane State: XY
    - Alpha: 0.9
      Class: rviz_default_plugins/Map
      Color Scheme: map
      Enabled: true
      Name: Map
      Topic:
        Depth: 1
        Durability Policy: Transient Local
        History Policy: Keep Last
        Reliability Policy: Reliable
        Value: /map
      Update Topic:
        QOS:
          Depth: 1
          Durability Policy: Transient Local
          Reliability Policy: Reliable
        Value: /map
      Value: true
    - Class: rviz_default_plugins/LaserScan
      Enabled: true
      Name: LaserScan
      Topic:
        Depth: 5
        Durability Policy: Volatile
        History Policy: Keep Last
        Reliability Policy: Reliable
        Value: /scan
      Value: true
      Size (Pixels): 3
      Color: 255; 0; 0
      Color Style: Flat Color
    - Class: rviz_default_plugins/TF
      Enabled: true
      Name: TF
      Value: true
      Show Names: true
      Show Arrows: false
      Frame Timeout: 15
  Global Options:
    Background Color: 77; 77; 77
    Fixed Frame: map
    Frame Rate: 30
  Name: root
  Tools:
    - Class: rviz_default_plugins/Interact
      Hide Inactive Objects: true
    - Class: rviz_default_plugins/MoveCamera
    - Class: rviz_default_plugins/Select
  Window Geometry:
    Width: 1400
    Height: 900
```

**07_ROS2_RViz_Bridgeunity_bridge.py**


```
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
       → ROS2 토픽 발행: /map, /scan, /odom, /tf, /tf_static
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

좌표 변환 (Unity ↔ ROS):
    Unity(x 오른쪽, z 북쪽, y 위) → ROS(x 동쪽, y 북쪽, z 위)
      - ROS 위치 = (Unity x, Unity z, 0)
      - ROS 요각 = π/2 - yaw_deg(Unity)  (REP-103 기준 프레임 정합)
    TF 트리: map(=odom) → base_footprint → base_scan(lidar)
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
from sensor_msgs.msg import LaserScan
from std_msgs.msg import Header
from tf2_ros import TransformBroadcaster, StaticTransformBroadcaster

# ROS 메시지에 사용할 프레임 이름
FRAME_MAP = "map"
FRAME_ODOM = "odom"
FRAME_BASE = "base_footprint"
FRAME_LIDAR = "base_scan"

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

        # stamp 오래된 시간이면 안 되므로 현재 시간 사용
        now = self.get_clock().now().to_msg()
        for t in tfs:
            t.header.stamp = now

        self.tf_static_broadcaster.sendTransform(tfs)
        self.get_logger().info(f"[유니티 브릿지] 정적 TF 발행: map→odom, {FRAME_BASE}→{FRAME_LIDAR}")

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
```


**실행 순서** (Unity 씬에 `RosBridge` 컴포넌트를 아무 GameObject에 추가 후):

1. Unity 에디터에서 **Play** 실행 → 하단 로그에 `[RosBridge] TCP 서버 시작됨 - 0.0.0.0:8765` 확인.
2. ROS2 컨테이너에서 브릿지 노드 실행 (라우터의 `host.docker.internal` → Windows 호스트가 보임):
   ```bash
   docker exec -it ros_jazzy1 bash
   python3 /mnt/d/github/unity_sim_example/07_ROS2_RViz_Bridge/unity_bridge.py
   ```
   → 로그 `[유니티 브릿지] Unity에 연결됨!` 확인.
3. 다른 터미널에서 RViz2 실행 (VcXsrv가 켜져 있는지 확인):
   ```bash
   docker exec -it ros_jazzy1 bash
   rviz2 -d /mnt/d/github/unity_sim_example/07_ROS2_RViz_Bridge/rviz_map_view.rviz
   ```
4. Unity에서 로봇을 이동/회전 → RViz2에서 **검정 맵(장애물) · 흰색(자유공간) + 붉은 LaserScan**이 실시간 갱신됩니다.

> **Tip**: Unity Play와 브릿지 실행 순서는 무관합니다. `unity_bridge.py`는 2초 간격으로 Unity 서버에 재접속을 시도합니다.
>
> **주의**: Windows 방화벽이 8765 포트 인바운드를 막으면 컨테이너가 접속하지 못하므로, 01단계에서 했던 것처럼 해당 포트를 허용해야 합니다.

### 9-3. 오류 확인 요령

| 증상 | 확인할 것 |
|------|-----------|
| Unity 로그 "서버 시작 실패" | `netstat -ano \| findstr 8765` 로 포트 점유 확인 |
| 브릿지가 반복 "연결 끊김" | Windows 방화벽 인바운드 8765 허용 여부, Unity Play 상태 |
| RViz2에 맵이 안 뜸 | RViz2 하단 토픽 목록에 `/map`이 나타나는지, Fixed Frame = `map` |
| 센서/맵이 어긋남 | TF 패널에서 `map → odom → base_footprint → base_scan` 연결 확인 |

---

> **출처**: NVIDIA Isaac Sim ROS2 튜토리얼 (RTX Lidar, Transform Trees and Odometry)를 Unity 기반으로 번안  
> **최종 업데이트**: 2026년 9월
