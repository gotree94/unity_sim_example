# 6단계-1: TurtleBot3 URDF 임포트 가이드

> **목적**: TurtleBot3 Burger 모델을 Unity에 안정적으로 가져와서 구동까지 완성  
> **소요 시간**: 약 60~90분  
> **전제 조건**: Unity 설치 완료, 프로젝트 생성 완료

---

## 이 문서의 목표

이 문서는 하나의 목표만 달성합니다:

> **TurtleBot3 Burger URDF를 Unity에 임포트하여 키보드로 바퀴를 굴려보기**

로봇 팔, TCP/IP 통신, PID 제어 등은 포함하지 않습니다.

---

## 기본 이해

### TurtleBot3 모델 소스

| 소스 | URL | 비고 |
|------|-----|------|
| **공식 GitHub** | https://github.com/ROBOTIS-GIT/turtlebot3 | Burger, Waffle Pi 모델 |
| **커뮤니티 URDF** | https://github.com/Daniella1/urdf_files_dataset | xacro 변환 완료된 URDF |

```bash
# TurtleBot3 모델 클론
git clone https://github.com/ROBOTIS-GIT/turtlebot3.git
```

### URDF 조인트 타입

| 타입 | Unity 변환 | 설명 |
|------|-----------|------|
| `fixed` | Fixed Joint | 고정, 움직이지 않음 |
| `revolute` | Revolute Joint (제한 있음) | 1축 회전, 각도 제한 |
| `continuous` | Revolute Joint (제한 없음) | 1축 회전, 무한 회전 |
| `prismatic` | Prismatic Joint | 선형 이동(피스톤 등) |
| `floating` | 별도 Body 필요 | 자유로운 6DOF 이동 |
| `planar` | 수동 설정 필요 | 평면 이동 |

### URDF 좌표계 (ROS vs Unity)

| 축 | ROS (URDF) | Unity |
|----|-----------|-------|
| 전진 | X | Z |
| 좌우 | Y | X |
| 상하 | Z | Y |

> **핵심**: ROS는 Z축이 위(Z-up), Unity는 Y축이 위(Y-up)입니다. Import 시 Mesh Orientation을 **Y-Up**으로 설정하면 자동 변환됩니다.

### URDF/XML 정렬 및 검증

URDF를 편집한 후 XML 정렬과 검증을 할 수 있습니다:

**VS Code (추천)**
- `Shift + Alt + F` → 자동 XML 정렬

**Notepad++**
- **Plugins > XML Tools > Pretty Print** (플러그인 설치 필요)

**온라인 도구**
- https://jsonformatter.org/xml-beautifier → 붙여넣고 Beautify

**PowerShell에서 XML 검증**
```powershell
[xml](Get-Content 'Assets\URDF\turtlebot3_burger.urdf' -Raw)
```
- 에러 없으면 정상, 에러 있으면 태그 불일치 등 문제

---

## 전체 진행 순서 (요약)

```
1단계: 필요한 파일 준비 (5분)
  └─ URDF 파일 + meshes 폴더를 Unity asset 폴더로 복사

2단계: URDF 파일 수정 (10분)
  └─ 4가지 필수 변경 (xacro, namespace, 경로, inertial)

3단계: Unity에서 임포트 (5분)
  └─ "Import Robot from URDF" 실행 (Y-Up 선택)

4단계: 물리 시스템 전환 (10분)
  └─ TurtleBot3Setup으로 ArticulationBody → Rigidbody 전환

5단계: Play 테스트 (5분)
  └─ W/S/A/D 키로 구동 확인

최종: 문제 해결 체크리스트
```

---

## 1단계: 필요한 파일 준비

### 1-1. TurtleBot3 레포지토리 구조 이해하기

`git clone`으로 다운로드한 `turtlebot3` 폴더의 구조:

```
C:\Users\Administrator\Desktop\turtlebot3\
├── turtlebot3_description\        ← ★ 우리가 사용할 폴더
│   ├── urdf\                      ← URDF 파일 (로봇 설명)
│   │   ├── turtlebot3_burger.urdf ← ★ Burger 모델 (이 파일 하나만 사용)
│   │   └── ...
│   └── meshes\                    ← ★ 3D 메쉬 파일
│       ├── bases\
│       │   └── burger_base.stl    ← 본체
│       ├── wheels\
│       │   ├── left_tire.stl      ← 왼쪽 바퀴
│       │   └── right_tire.stl     ← 오른쪽 바퀴
│       └── sensors\
│           └── lds.stl            ← LiDAR
├── turtlebot3_bringup\            ← ❌ 불필요
├── turtlebot3_cartographer\       ← ❌ 불필요
└── ...                            ← ❌ 불필요
```

> **핵심**: 우리가 필요한 것은 `turtlebot3_description` 안의 **URDF 파일 1개**와 **meshes 폴더**뿐입니다.

### 1-2. Unity 프로젝트에 파일 복사

Unity 프로젝트의 asset 폴더에 `URDF` 폴더를 만들고 필요한 파일을 복사합니다.

#### 방법 A: 윈도우 탐색기에서 복사 (추천)

1. `C:\Users\Administrator\Desktop\turtlebot3\turtlebot3_description` 폴더를 엽니다
2. 다음 2개를 복사합니다 (`Ctrl+C`):
   - `urdf\turtlebot3_burger.urdf`
   - `meshes\` (폴더 통째로)
3. Unity 프로젝트의 asset 폴더로 이동합니다:
   ```
   C:\Users\Administrator\Desktop\TurtleBot3\Assets\
   ```
4. `URDF` 폴더를 만들고 안에 붙여넣기합니다 (`Ctrl+V`)

#### 방법 B: PowerShell에서 복사

```powershell
# Unity asset 폴더에 URDF 폴더 생성
New-Item -ItemType Directory -Path "C:\Users\Administrator\Desktop\TurtleBot3\Assets\URDF\meshes" -Force

# URDF 파일 복사
Copy-Item "C:\Users\Administrator\Downloads\turtlebot3\turtlebot3_description\urdf\turtlebot3_burger.urdf" `
          "C:\Users\Administrator\Desktop\TurtleBot3\Assets\URDF\"

# meshes 폴더 통째로 복사 (하위 폴더 포함)
Copy-Item "C:\Users\Administrator\Downloads\turtlebot3\turtlebot3_description\meshes\*" `
          "C:\Users\Administrator\Desktop\TurtleBot3\Assets\URDF\meshes\" -Recurse
```

```powershell
# Unity asset 폴더에 URDF 폴더 생성
New-Item -ItemType Directory -Path "C:\Users\user\TurtleBot3\Assets\URDF\meshes" -Force

# URDF 파일 복사
Copy-Item "C:\Users\user\Downloads\turtlebot3-main\turtlebot3_description\urdf\turtlebot3_burger.urdf" `
          "C:\Users\user\TurtleBot3\Assets\URDF\"

# meshes 폴더 통째로 복사 (하위 폴더 포함)
Copy-Item "C:\Users\user\Downloads\turtlebot3-main\turtlebot3_description\meshes\*" `
          "C:\Users\user\TurtleBot3\Assets\URDF\meshes\" -Recurse
```


### 1-3. 복사 후 폴더 구조 확인

이렇게 되었는지 확인합니다:

```
Assets\
  URDF\
    turtlebot3_burger.urdf        ← URDF 파일
    meshes\                       ← 3D 메쉬 파일들
      bases\
        burger_base.stl           ← 본체
      wheels\
        left_tire.stl             ← 왼쪽 바퀴
        right_tire.stl            ← 오른쪽 바퀴
      sensors\
        lds.stl                   ← LiDAR
```

> ⚠️ **이 단계에서 확인할 것**:
> - `turtlebot3_burger.urdf`가 `Assets\URDF\` 안에 있는지
> - `meshes` 폴더가 `Assets\URDF\` 안에 있는지 (meshes 안에 bases, wheels, sensors 폴더가 있는지)

---

## 2단계: URDF 파일 수정 (가장 중요!)

> ⚠️ **이 단계를 건너뛰면 로봇이 보이지 않거나 붕괴됩니다!**

TurtleBot3의 원본 URDF는 ROS 전용이므로, Unity에서 사용하려면 **4가지를 반드시 수정**해야 합니다.

### 2-1. URDF 파일 열기

`Assets\URDF\turtlebot3_burger.urdf` 파일을 텍스트 에디터로 엽니다.

### 2-2. 변경 1: xacro 선언 삭제

파일 상단에서 다음 3줄을 **삭제**합니다:

```xml
<!-- ★★★ 이 3줄을 삭제하세요 ★★★ -->
<robot name="turtlebot3_burger"
  xmlns:xacro="http://ros.org/wiki/xacro">
  <!-- <xacro:include ... /> -->
  <xacro:arg name="namespace" default=""/>
  <xacro:property name="namespace" value="$(arg namespace)"/>
```

**삭제 후**:

```xml
<?xml version="1.0" ?>
<robot name="turtlebot3_burger">
```

### 2-3. 변경 2: `${namespace}` 변수 모두 제거

파일 전체에서 `${namespace}`가 포함된 모든 곳을 수정합니다.

**변경 전** (원본):
```xml
<link name="${namespace}base_footprint"/>
<joint name="${namespace}base_joint" type="fixed">
  <parent link="${namespace}base_footprint"/>
  <child link="${namespace}base_link"/>
```

**변경 후** (Unity용):
```xml
<link name="base_footprint"/>
<joint name="base_joint" type="fixed">
  <parent link="base_footprint"/>
  <child link="base_link"/>
```

> **팁**: VS Code에서 `Ctrl+H`로 검색/바꾸기를 하면 빠릅니다.
> - 검색: `${namespace}`
> - 바꾸기: (빈 문자열)
> - "모두 바꾸기" 클릭

### 2-4. 변경 3: mesh 경로를 상대 경로로 변경

`package://turtlebot3_description/meshes/`를 `meshes/`로 변경합니다.

**변경 전**:
```xml
<mesh filename="package://turtlebot3_description/meshes/bases/burger_base.stl" scale="0.001 0.001 0.001"/>
```

**변경 후**:
```xml
<mesh filename="meshes/bases/burger_base.stl" scale="0.001 0.001 0.001"/>
```

> **핵심**: `package://turtlebot3_description/` 부분만 지우면 됩니다.

### 2-5. 변경 4: 빈 링크에 `<inertial>` 추가 (물리 안정성)

원본 URDF에는 `<inertial>`이 없는 링크가 있습니다. 이것들이 있으면 로봇이 쓰러지거나 부서집니다.

**base_footprint에 추가**:

```xml
<!-- 변경 전 -->
<link name="base_footprint"/>

<!-- 변경 후 -->
<link name="base_footprint">
  <inertial>
    <mass value="0.001"/>
    <inertia ixx="0.0001" ixy="0" ixz="0"
             iyy="0.0001" iyz="0" izz="0.0001"/>
  </inertial>
</link>
```

**imu_link에 추가**:

```xml
<!-- 변경 전 -->
<link name="imu_link"/>

<!-- 변경 후 -->
<link name="imu_link">
  <inertial>
    <mass value="0.001"/>
    <inertia ixx="0.00001" ixy="0" ixz="0"
             iyy="0.00001" iyz="0" izz="0.00001"/>
  </inertial>
</link>
```

### 변경 요약표

| 항목 | 원본 (ROS) | 수정 후 (Unity) |
|------|-----------|-----------------|
| xacro 선언 | `xmlns:xacro="..."` | **삭제** |
| namespace 변수 | `${namespace}base_link` | `base_link` |
| mesh 경로 | `package://turtlebot3_description/meshes/...` | `meshes/...` |
| 빈 링크 | `<inertial>` 없음 | **`<inertial>` 추가** |

---

## 3단계: Unity에서 URDF 임포트

### 3-1. URDF Importer 패키지 설치 (최초 1회)

1. Unity 에디터 상단 메뉴: **Window > Package Manager**
2. 좌측 상단 **+** 버튼 클릭
3. **"Add package from git URL..."** 선택
4. 다음 URL을 붙여넣고 **Add** 클릭:
   ```
   https://github.com/Unity-Technologies/URDF-Importer.git?path=/com.unity.robotics.urdf-importer
   ```
5. 설치 완료 후 Unity 메뉴 상단에 **Robotics** 메뉴가 나타남

### 3-2. URDF 임포트 실행

1. Project 창에서 **Assets > URDF** 폴더를 엽니다
2. **turtlebot3_burger.urdf** 파일을 마우스 **우클릭**합니다
3. **"Import Robot from URDF"**를 클릭합니다
4. 임포트 창에서 설정합니다:

| 설정 | 값 | 설명 |
|------|-----|------|
| **Mesh Orientation** | **Y-Up** | TurtleBot3 mesh에 맞는 설정 |
| **Convex Decomposer** | VHACD | 충돌 메시용 |

5. **"Import Robot"** 버튼을 클릭합니다

> ⚠️ **Y-Up을 선택하세요!** Z-Up으로 하면 로봇이 90도 회전되어 보입니다.

### 3-3. 임포트 결과 확인

Hierarchy 창에 다음 구조가 나타나면 성공입니다:

```
turtlebot3_burger
├─ Plugins
└─ base_footprint              ← Root
   └─ base_link                ← 본체
      ├─ wheel_left_link       ← 왼쪽 바퀴
      ├─ wheel_right_link      ← 오른쪽 바퀴
      ├─ caster_back_link      ← 뒷바퀴 캐스터
      ├─ imu_link              ← IMU (센서)
      └─ base_scan             ← LiDAR
```

Scene 창에서 로봇 모델이 보이는지 확인합니다.

> ❌ **모델이 보이지 않는다면**: 2단계의 URDF 수정을 다시 확인하세요.

---

## 4단계: 물리 시스템 전환 (ArticulationBody → Rigidbody)

> ⚠️ **핵심**: URDF Importer는 ArticulationBody를 자동 생성하지만, ArticulationBody는 Ground 충돌 시 물리 계산이 불안정하여 바퀴가 안 돌아가는 문제가 있습니다. **Rigidbody 방식**으로 전환합니다.

### 4-1. TurtleBot3Setup 스크립트 만들기

Project 창에서 **Assets** 폴더 우클릭 → **Create > C# Script** → 이름: `TurtleBot3Setup`

```csharp
using UnityEngine;

// ############################################################
// # TurtleBot3Setup
// # 역할: URDF Importer가 만든 ArticulationBody(관절 물리) 체계를
// #       일반 Rigidbody(단일 물리) 체계로 일괄 전환합니다.
// #       Play 시작 시 Awake()에서 한 번 자동 실행됩니다.
// ############################################################
public class TurtleBot3Setup : MonoBehaviour
{
    // ---------- [물리 설정] ----------
    // 로봇 전체 질량(kg). TurtleBot3 Burger 기본값은 0.826kg.
    // 가속/관성/바닥 마찰 반응에 영향을 줍니다. 가벼울수록 관성 영향이 작아 잘 미끄러집니다.
    public float robotMass = 0.826f;

    // 중력 적용 여부(true면 플레이 시 바닥으로 떨어짐). 바닥(Ground)이 없으면 추락하므로
    // 반드시 씬에 "Ground" 오브젝트가 있어야 합니다.
    public bool useGravity = true;

    // ---------- [바퀴 설정] ----------
    // 바퀴 반지름(m). TurtleBot3 Burger 기본 휠 반지름 0.033m.
    // Capsule Collider의 radius 값으로 사용됩니다.
    public float wheelRadius = 0.033f;

    // 바퀴 폭/두께(m). Capsule Collider의 height(길이)로 사용됩니다.
    public float wheelWidth = 0.018f;

    // Awake: 오브젝트가 활성화되는 최초 시점(Play 직전)에 한 번 실행됩니다.
    // 전환 순서가 중요합니다: ArticulationBody를 먼저 제거해야 Rigidbody가 충돌 없이 적용됩니다.
    void Awake()
    {
        RemoveArticulationBodies(); // 1) URDF 관절 물리(ArticulationBody) 제거
        SetupRigidbody();           // 2) 단일 Rigidbody 추가/설정
        ReplaceWheelColliders();    // 3) 바퀴/Caster/LiDAR 충돌체 정리
        SetupGround();              // 4) 바닥 물리 재질 적용
        AutoAttachLidarSensor();    // 5) base_scan에 LidarSensor 없으면 자동 부착
        Debug.Log("TurtleBot3 Setup 완료! Rigidbody 방식으로 전환됨.");
    }

    // 함수: ArticulationBody(관절 물리)를 전부 제거
    // 이유: ArticulationBody는 다중 링크(관절) 기반 물리로 바퀴 구동/Ground 충돌이
    //       불안정합니다. Rigidbody 단일 물리로 바꿔야 MovePosition 조작이 안정적입니다.
    // 중요: UrdfJoint/UrdfInertial/Controller/JointControl 같은 URDF Importer 컴포넌트가
    //       ArticulationBody를 "참조"하고 있어, ArticulationBody만 먼저 지우면
    //       "depends on it" 오류(또는 Start()에서 IndexOutOfRange)가 발생합니다.
    //       그래서 ① URDF Importer 스크립트를 먼저 제거 → ② ArticulationBody를 제거 순서로 진행합니다.
    void RemoveArticulationBodies()
    {
        // ① URDF Importer 스크립트를 먼저 제거:
        //    - 클래스 이름에 "Urdf"가 포함된 컴포넌트 (UrdfJoint, UrdfInertial, UrdfVisual ...)
        //    - 네임스페이스가 "Unity.Robotics.UrdfImporter"로 시작하는 컴포넌트
        //      (Controller, FKRobot 등. 네임스페이스에 Controller가 있어 이름 검사로는 못 걸러냄)
        //    - JointControl (네임스페이스가 없어 이름으로만 판별)
        //    위 컴포넌트들은 ArticulationBody를 참조하므로 남겨두면 에러가 납니다.
        //    컴포넌트 간 상호 참조가 있을 수 있어, 남는 것이 없을 때까지 반복 제거.
        bool found = true;
        while (found)
        {
            found = false;
            foreach (var comp in GetComponentsInChildren<Component>(true))
            {
                if (comp == null) continue;
                System.Type t = comp.GetType();
                string typeName = t.Name;
                string ns = (t.Namespace != null) ? t.Namespace : "";

                // 사용자가 만든 TurtleBot3Controller/TurtleBot3Setup은 네임스페이스가 없고
                // 이름도 위 패턴과 겹치지 않으므로 지워지지 않습니다 (안전).
                bool isUrdfImporter =
                    typeName.Contains("Urdf") ||
                    ns.StartsWith("Unity.Robotics.UrdfImporter") ||
                    typeName == "JointControl";

                if (isUrdfImporter)
                {
                    DestroyImmediate(comp);
                    found = true;
                    break;
                }
            }
        }

        // ② 이제 참조 컴포넌트가 없으므로 ArticulationBody를 안전하게 제거.
        ArticulationBody[] abs = GetComponentsInChildren<ArticulationBody>(true);
        foreach (var ab in abs)
            DestroyImmediate(ab);

        Debug.Log($"ArticulationBody/UrdfJoint 제거 완료: {abs.Length}개");
    }

    // 함수: 루트 오브젝트(turtlebot3_burger)에 Rigidbody를 추가하고 물리 값을 설정
    void SetupRigidbody()
    {
        // 이미 Rigidbody가 있으면 재사용, 없으면 새로 추가
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.mass = robotMass;         // 질량(kg) → 가속/충돌 반응 결정
        rb.useGravity = useGravity;  // 중력 적용 여부 (true면 추락 → Ground 필수)
        rb.isKinematic = false;      // false = 물리엔진이 추진/충돌을 처리.
                                     // true로 하면 외부 힘/중력이 무시되고 MovePosition만으로 이동하게 됨.
        rb.drag = 0f;                // 선형 마찰(공기저항). 0 = 감속 없음(정확한 이동)
        rb.angularDrag = 0.05f;      // 회전 마찰(각속도 감쇠). 살짝 둬서 회전이 과도하게 이어지지 않게 함
    }

    // 함수: 바퀴/Caster/LiDAR의 충돌체를 정리하고 바퀴에 회전용 Collider를 새로 배치
    // 이유: URDF 임포트 시 Mesh Collider가 붙는데, 복잡한 메시는 물리 연산이 무겁고
    //       바퀴 굴림이 안정적으로 표현되지 않습니다. 단순한 Capsule Collider로 교체합니다.
    void ReplaceWheelColliders()
    {
        // 교체 대상 바퀴 링크 이름 (URDF의 wheel_left_link / wheel_right_link)
        string[] wheelNames = { "wheel_left_link", "wheel_right_link" };

        foreach (string wheelName in wheelNames)
        {
            // 자식 계층에서 해당 이름의 링크를 재귀 탐색
            Transform wheelTransform = FindChildRecursive(transform, wheelName);
            if (wheelTransform == null) continue; // 링크가 없으면 건너뜀

            // 기존 Mesh Collider 자식들을 제거
            foreach (Transform child in wheelTransform)
            {
                Collider col = child.GetComponent<Collider>();
                if (col != null)
                    DestroyImmediate(col);
            }

            // 바퀴 링크에 Capsule Collider 추가 (원기둥형 충돌체)
            CapsuleCollider capsule = wheelTransform.gameObject.AddComponent<CapsuleCollider>();
            capsule.radius = wheelRadius;   // 반지름 = wheelRadius (0.033m)
            capsule.height = wheelWidth;    // 길이 = wheelWidth (0.018m)
            capsule.direction = 2;          // 2 = Z축 방향으로 세움 (바퀴 굴림 축)
            capsule.center = Vector3.zero;  // 링크 중심에 배치

            // 바퀴용 마찰 재질: 구르면서 미끄러지지 않도록 마찰을 높게 설정
            PhysicMaterial mat = new PhysicMaterial("WheelPhysMat");
            mat.dynamicFriction = 0.8f;  // 움직일 때(미끄러질 때) 마찰 계수
            mat.staticFriction = 0.8f;   // 정지 상태에서 움직이기 시작할 때 마찰 계수
            mat.bounciness = 0f;         // 반발력 0 (바닥에 튀지 않음)
            mat.frictionCombine = PhysicMaterialCombine.Maximum; // 바닥과의 마찰은 둘 중 큰 값을 사용
            capsule.material = mat;
        }

        // 캐스터(뒤쪽 보조바퀴)의 Mesh Collider 제거 — 불필요한 충돌면이 굴림을 방해하지 않도록
        Transform casterTransform = FindChildRecursive(transform, "caster_back_link");
        if (casterTransform != null)
            foreach (Transform child in casterTransform)
            { Collider c = child.GetComponent<Collider>(); if (c != null) DestroyImmediate(c); }

        // LiDAR(base_scan)의 Mesh Collider 제거 — 장애물 감지용이므로 충돌체가 필요 없음
        Transform baseScanTransform = FindChildRecursive(transform, "base_scan");
        if (baseScanTransform != null)
            foreach (Transform child in baseScanTransform)
            { Collider c = child.GetComponent<Collider>(); if (c != null) DestroyImmediate(c); }
    }

    // 함수: 씬의 "Ground"(이름이 정확히 Ground인 Plane)에 물리 재질을 적용
    // 이유: 바퀴와 바닥 사이의 마찰을 조절해 로봇이 미끄러지지 않고 안정적으로 굴러가게 함.
    //       이름이 "Ground"가 아니거나 오브젝트가 없으면 아무 일도 하지 않고 종료합니다.
    void SetupGround()
    {
        // 이름이 정확히 "Ground"인 오브젝트를 찾습니다.
        GameObject ground = GameObject.Find("Ground");
        if (ground == null) return; // 없으면(바닥 미생성) 함수 종료 → 이 경우 로봇이 추락합니다!

        // 바닥용 마찰 재질: 바퀴보다 낮은 마찰로 미끄러짐을 줄이고 굴림을 돕습니다.
        PhysicMaterial groundMat = new PhysicMaterial("GroundPhysMat");
        groundMat.dynamicFriction = 0.4f;  // 움직일 때 마찰
        groundMat.staticFriction = 0.4f;   // 정지 마찰
        groundMat.bounciness = 0f;         // 반발력 0
        groundMat.frictionCombine = PhysicMaterialCombine.Average; // 바퀴와의 마찰은 평균값 사용

        // Ground가 Mesh Collider면 그 재질을, 아니면 Box Collider의 재질을 교체
        MeshCollider mc = ground.GetComponent<MeshCollider>();
        if (mc != null)
        {
            mc.material = groundMat;
        }
        else
        {
            BoxCollider bc = ground.GetComponent<BoxCollider>();
            if (bc != null)
                bc.material = groundMat;
        }
    }

    // 함수: base_scan에 LidarSensor(2D LiDAR)를 아직 부착하지 않았다면 자동으로 추가
    // 이유: 06단계에서 LiDAR 시각화(녹색 레이저 링)를 보려면 base_scan 오브젝트에
    //       LidarSensor 컴포넌트가 있어야 합니다. 수동으로 Add Component를 빼먹어도
    //       Play 시 자동으로 붙도록 해, 항상 레이저가 동작하게 합니다.
    // 참고: 이미 직접 추가해 둔 경우 중복하지 않도록 GetComponent로 먼저 확인합니다.
    void AutoAttachLidarSensor()
    {
        Transform baseScanTransform = FindChildRecursive(transform, "base_scan");
        if (baseScanTransform == null) // base_scan 링크가 없으면 부착 불가 → 종료
        {
            Debug.LogWarning("[TurtleBot3Setup] base_scan 링크를 찾지 못해 LidarSensor를 부착하지 않았습니다.");
            return;
        }

        // 이미 LidarSensor가 있으면 중복 부착하지 않음
        if (baseScanTransform.GetComponent<LidarSensor>() != null)
            return;

        // base_scan에 LidarSensor 컴포넌트 자동 추가
        baseScanTransform.gameObject.AddComponent<LidarSensor>();
        Debug.Log($"[TurtleBot3Setup] base_scan에 LidarSensor 자동 부착 완료.");
    }

    // 보조 함수: 오브젝트 계층을 재귀적으로 탐색해 주어진 이름의 Transform을 찾아 반환.
    //            찾지 못하면 null을 반환. (바퀴/캐스터/LiDAR 링크를 이름으로 찾는 데 사용)
    Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
}
```

### 4-2. TurtleBot3Controller 스크립트 만들기

Project 창에서 **Assets** 폴더 우클릭 → **Create > C# Script** → 이름: `TurtleBot3Controller`

```csharp
using UnityEngine;

// ############################################################
// # TurtleBot3Controller
// # 역할: 키보드(WASD) 입력을 받아 Rigidbody 기반으로 로봇을 이동/회전시킵니다.
// #       이동은 MovePosition, 회전은 MoveRotation으로 FixedUpdate에서 처리합니다.
// ############################################################
public class TurtleBot3Controller : MonoBehaviour
{
    // ---------- [이동 설정] ----------
    // 전진/후진 속도(m/s). TurtleBot3 Burger 실측 최대 선속도 0.22 m/s.
    // (MoveSpeed = 초당 이동 거리: 1이면 1m/s로 전진)
    public float moveSpeed = 0.22f;

    // 회전 속도(deg/s). TurtleBot3 Burger 실측 최대 각속도 2.84 rad/s ≈ 162.7 deg/s를
    // 각도 기준으로 환산한 값입니다. (RotationSpeed = 초당 회전 각도)
    public float rotationSpeed = 2.84f;

    private Rigidbody rb; // 물리 이동을 제어할 Rigidbody 참조

    // Start: Play 시작 시 한 번 실행. 이 오브젝트의 Rigidbody를 가져옵니다.
    // (TurtleBot3Setup이 Awake에서 Rigidbody를 추가하므로 반드시 그 이후에 실행됨)
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // FixedUpdate: 물리 엔진 스텝마다(기본 0.02초 간격) 호출.
    //              MovePosition/MoveRotation은 물리 스텝 안에서만 동작하므로 여기서 처리합니다.
    void FixedUpdate()
    {
        if (rb == null) return; // Rigidbody가 없으면 동작하지 않음 (설정 오류 방지)

        // 입력값 받기
        // Vertical   : W(앞)=+1, S(뒤)=-1  → 전진/후진
        // Horizontal : D(오른쪽)=+1, A(왼쪽)=-1 → 좌/우 회전
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        // 이동 벡터 계산: 로봇이 향하는 방향(transform.forward) × 입력 × 속도
        // -1과 1 사이의 moveInput 덕분에 속도를 그대로 곱해 부드럽게 가감속됩니다.
        Vector3 moveDirection = transform.forward * moveInput * moveSpeed;

        // 물리 이동: 현재 위치에서 이동량(속도 × 물리 스텝시간)만큼 다음 위치로 이동.
        // Rigidbody 이동이므로 중력/충돌 영향은 물리엔진이 유지한 채 위치만 이동합니다.
        rb.MovePosition(rb.position + moveDirection * Time.fixedDeltaTime);

        // 회전 각도 계산: 입력(turnInput) × 회전속도(deg/s) × 스텝시간(초) → 스텝당 회전 각도
        float rotation = turnInput * rotationSpeed * Mathf.Rad2Deg * Time.fixedDeltaTime;

        // 물리 회전: 현재 회전에 Y축(좌우) 회전을 곱해 로봇을 돌립니다.
        // Euler(0, rotation, 0) = X·Z축은 0, Y축(방위각)만 회전 = 욜로 회전(터틀봇처럼 제자리 회전)
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, rotation, 0f));
    }
}
```

### 4-3. 스크립트 연결

1. Hierarchy에서 **turtlebot3_burger** (최상위 빈 오브젝트)를 선택
2. Inspector에서 **Add Component > TurtleBot3Setup** 추가
3. 다시 **Add Component > TurtleBot3Controller** 추가
4. TurtleBot3Controller의 **Move Speed**를 `0.22`, **Rotation Speed**를 `2.84`로 설정

### 4-4. 기존 설정 확인

Play를 눌러보세요. TurtleBot3Setup이 자동으로 다음을 수행합니다:

| 작업 | 설명 |
|------|------|
| Urdf 스크립트 먼저 제거 | UrdfInertial, UrdfJoint(UrdfJointFixed/Continuous) 등을 반복 제거 |
| ArticulationBody 모두 제거 | base_footprint, base_link, 바퀴 등 (Urdf 참조 제거 후 안전하게) |
| Rigidbody 추가 | turtlebot3_burger에 1개 추가 |
| 바퀴 Collider 교체 | Mesh Collider → Capsule Collider (Direction=Z) |
| 캐스터/LiDAR Collider 제거 | 불필요한 충돌체 제거 |
| Ground에 Physic Material 적용 | 마찰력 0.4, Average 합산 |
| LidarSensor 자동 부착 | base_scan에 없으면 06단계용 LiDAR 컴포넌트를 자동 추가 |

> 💡 **참고**: Setup은 Play 시 `Awake()`에서 자동 실행됩니다.

---

## 5단계: Play 테스트

### 5-0. 사전 준비: Ground(바닥) 생성 ★중요★

> ⚠️ **이 단계를 건너뛰면 로봇이 바닥으로 떨어져 화면 밖으로 사라집니다!**

`TurtleBot3Setup.SetupGround()`는 `GameObject.Find("Ground")`로 바닥을 찾습니다. **씬에 `Ground`라는 이름의 오브젝트가 없으면** 중력(`useGravity = true`)이 적용된 로봇이 바닥 없이 계속 떨어져서 사라집니다.

1. Hierarchy 우클릭 → **3D Object > Plane** 선택
2. 이름을 **`Ground`** 로 변경 (기본 이름 "Plane"에서 반드시 변경)
3. Inspector에서 기본값 유지:
   - **Position**: `(0, 0, 0)`
   - **Scale**: `(1, 1, 1)` → 약 10m × 10m 바닥

> 💡 **이름 규칙**: `SetupGround()`는 이름이 정확히 **"Ground"** 인 오브젝트만 찾습니다. 다르면 아무 효과가 없습니다.

> ✅ **확인**: Play 전에 Scene 뷰에서 로봇 아래에 평평한 회색/흰색 바닥이 보이는지 확인하세요.

### 5-1. 실행

1. Hierarchy에서 **turtlebot3_burger**를 선택하고 Inspector에서 다음 2개가 있는지 확인:
   - **TurtleBot3Setup** (Script)
   - **TurtleBot3Controller** (Script)
2. **▶ (Play)** 버튼을 누릅니다
3. Console에 "Setup 완료!" 로그가 나타나는지 확인
4. Game View를 **마우스로 클릭**합니다
5. 키보드로 조작합니다:

| 키 | 동작 |
|----|------|
| **W** | 전진 |
| **S** | 후진 |
| **A** | 좌회전 |
| **D** | 우회전 |

### 5-2. 카메라 위치 조정

기본 `Main Camera`는 로봇에서 멀리 떨어져 있어 로봇이 화면에 너무 작게 보입니다. **Position**을 아래 값으로 바꿔 로봇이 화면을 알맞게 채우도록 합니다.

Hierarchy에서 **Main Camera**를 선택하고 Inspector의 Transform에서:

| 설정 | 값 | 설명 |
|------|-----|------|
| **Position X** | `0` | 중앙 정렬 (로봇의 좌우 가운데) |
| **Position Y** | `0.5` | 카메라 높이. 로봇(높이 약 0.18m)보다 약간 위에서 내려다보는 각도 |
| **Position Z** | `-1.5` | 로봇 앞쪽 1.5m 거리에서 정면을 바라봄. **값이 작을수록(0에 가까울수록) 로봇이 크게 보임** |

> 💡 **왜 이 값인가?** 로봇(터틀봇 버거)의 크기는 약 0.14m(폭) × 0.18m(높이)로 매우 작습니다. 기본 카메라 위치 `(0, 1, -10)`에서는 로봇이 점처럼 보이므로, **Y=0.5**로 로봇 높이 근처에, **Z=-1.5**로 충분히 가까이 당겨 화면 비율에 맞게 배치합니다.
>
> 로봇이 너무 크게/작게 보이면 **Z 값을 조절**합니다: `-1`이면 더 작게, `-2`면 더 크게 보입니다.

> ⚠️ **카메라 회전(Rotation)**: 기본 카메라 회전 `(X=0, Y=0, Z=0)`을 유지하세요. 카메라가 정면(Z- 방향)을 향해 로봇을 바라보도록 합니다.

### 5-3. 동작 확인 포인트

```
✅ 로봇이 바닥에 안정적으로 서 있음 (떨어지지 않음)
✅ W 키를 누르면 로봇이 전진
✅ S 키를 누르면 로봇이 후진
✅ A/D 키로 회전
✅ 로봇이 쓰러지지 않음
```

---

## 문제 해결 체크리스트

### 문제 1: 로봇 모델이 Scene에 보이지 않음

| 확인 | 해결 |
|------|------|
| URDF 파일에 `package://`가 남아있는지 | `meshes/...`로 모두 변경 |
| meshes 폴더가 URDF 파일과 같은 위치에 있는지 | `Assets/URDF/meshes/` 확인 |

### 문제 2: 로봇이 바닥으로 떨어짐 (가장 흔한 원인)

| 확인 | 해결 |
|------|------|
| **씬에 `Ground` 오브젝트가 있는지 첫 확인!** | Hierarchy에서 "Ground" (Plane) 확인, 없으면 5-0에서 생성 |
| TurtleBot3Setup 스크립트가 turtlebot3_burger에 연결되었는지 | Add Component에서 확인 |
| Rigidbody의 Use Gravity가 체크되어 있는지 | Play 중 Inspector에서 확인 |
| Ground에 Collider가 있는지 | Ground 선택 → Inspector에서 확인 (Plane은 기본 MeshCollider 있음) |
| Console에 `Can't remove ArticulationBody ... depends on it` 오류가 있는지 | ArticulationBody가 남아도 바닥 충돌이 어긋나 떨어질 수 있음. 문제 3 참고 → `RemoveArticulationBodies()` 순서(Urdf 먼저) 확인 |

### 문제 3: 바퀴가 안 돌아감 (로봇이 안 움직임)

| 확인 | 해결 |
|------|------|
| TurtleBot3Controller 스크립트가 연결되었는지 | turtlebot3_burger에 있는지 확인 |
| Rigidbody의 Is Kinematic이 체크되어 있는지 | 체크 해제 필요 |
| Game View를 클릭했는지 | 키보드 입력은 Game View 포커스 필요 |
| Console에 `Can't remove ArticulationBody because ... depends on it` 오류가 있는지 | ★ **Urdf 스크립트가 ArticulationBody를 참조하고 있어 제거가 실패**한 것입니다. ArticulationBody가 남아 있으면 Rigidbody 이동과 충돌해 로봇이 안 움직입니다. → `TurtleBot3Setup.cs`의 `RemoveArticulationBodies()`가 **Urdf 스크립트 먼저** 제거하도록 수정된 코드인지 확인 (4-1 참고) |
| Play 직후 `IndexOutOfRangeException ... Controller.StoreJointColors` 오류가 있는지 | ★ URDF Importer의 **`Controller`/`JointControl`** 컴포넌트가 남아 있기 때문입니다. ArticulationBody를 제거한 뒤에도 이들이 남아 `Controller.Start()`가 빈 배열을 참조해 에러가 납니다. → 수정된 `RemoveArticulationBodies()`가 **네임스페이스 `Unity.Robotics.UrdfImporter`의 컴포넌트와 `JointControl`** 을 함께 제거하는지 확인 (4-1 참고) |

> ⚠️ **핵심 원인**: `RemoveArticulationBodies()`에서 ArticulationBody를 **먼저** 지우려 하면 Unity가 "Can't remove ArticulationBody because UrdfInertial/UrdfJointX depends on it" 오류를 냅니다. ArticulationBody가 실제로 제거되지 않아 **Rigidbody와 ArticulationBody가 공존**하고, 이 상태에선 `MovePosition`이 먹히지 않아 로봇이 움직이지 않습니다. 반드시 **Urdf 참조 스크립트를 먼저** 제거한 뒤 ArticulationBody를 제거해야 합니다.
>
> ⚠️ **또 하나의 원인**: ArticulationBody를 정상 제거했더라도 URDF Importer의 **`Controller` / `FKRobot` / `JointControl`** 컴포넌트가 남아 있으면, `Controller.Start()`(PackageCache의 `Controller.cs:49`)가 이미 비워진 ArticulationBody 배열을 참조하며 `IndexOutOfRangeException`(StoreJointColors)을 냅니다. 따라서 제거 조건은 **클래스 이름 "Urdf" 포함**뿐 아니라 **네임스페이스 `Unity.Robotics.UrdfImporter`로 시작**하거나 **이름이 `JointControl`** 인 컴포넌트까지 포함해야 합니다.

### 문제 4: 로봇이 떨어지지 않고 공중에 떠있음

| 확인 | 해결 |
|------|------|
| TurtleBot3Setup이 정상 실행되었는지 | Console에 "Setup 완료" 로그 확인 |
| Ground Collider가 로봇 아래에 있는지 | Ground Position Y 확인 |

---

## 최종 폴더 구조

```
Assets\
  URDF\
    turtlebot3_burger.urdf
    meshes\
      bases\burger_base.stl
      wheels\left_tire.stl, right_tire.stl
      sensors\lds.stl
  TurtleBot3Setup.cs       ← 자동 설정 (Play 시 ArticulationBody 제거 + Rigidbody 추가)
  TurtleBot3Controller.cs  ← 이동 (MovePosition/MoveRotation)

[Scene 오브젝트]
  Main Camera
  Directional Light
  Ground                  ← ★ 필수 (Plane, 이름 "Ground")
  turtlebot3_burger       ← URDF 임포트 결과 + TurtleBot3Setup/TurtleBot3Controller
```

---

# 로봇 URDF 파일 찾을 수 있는 사이트 리스트

## 공식 로봇 제조사 GitHub 저장소

| 로봇/제조사 | 저장소 | 비고 |
|---|---|---|
| Universal Robots | https://github.com/ros-industrial/universal_robot | UR3, UR5, UR10 등 |
| Franka Emika | https://github.com/frankaemika/franka_ros | Panda 로봇팔 |
| Boston Dynamics | https://github.com/boston-dynamics/spot-sdk | 커뮤니티 URDF 포팅 다수 |
| Unitree | https://github.com/unitreerobotics/unitree_ros | Go1, A1, B1 등 4족 로봇 |
| ROBOTIS | https://github.com/ROBOTIS-GIT | TurtleBot3, OpenManipulator 등 |
| Kinova | https://github.com/Kinovarobotics/ros_kortex | Gen3 등 소형 협동로봇 팔 |

## 통합 컬렉션 / 저장소

- **ROS-Industrial**: `github.com/ros-industrial` — ABB, Fanuc, Yaskawa, KUKA 등 산업용 로봇팔 URDF 대량 보유
- **ros2_control demos**: `github.com/ros-controls/ros2_control_demos`
- **AWS RoboMaker Sample Application**: 다양한 샘플 URDF 포함
- **awesome-URDF**: GitHub 검색 시 유사 큐레이션 리스트 다수 존재

## 시뮬레이터 연계 저장소

- **NVIDIA Isaac Sim / Isaac Lab**: `isaac-sim/IsaacLab` — URDF→USD 변환 예제 다수 포함
- **MuJoCo Menagerie**: `google-deepmind/mujoco_menagerie` — MJCF 위주지만 URDF 변환본도 많고 품질 매우 높음
- **PyBullet**: `bulletphysics/bullet3/examples/pybullet/gym/pybullet_data` — 다양한 예제 URDF 내장
- **Gazebo/Ignition Fuel**: `app.gazebosim.org/fuel` — 모델 공유 플랫폼, URDF/SDF 혼재

## 검색 / 큐레이션 사이트

- **GitHub 검색**: `filename:*.urdf` 또는 `extension:urdf` 쿼리로 직접 검색 가능
- **ROS Index**: `index.ros.org` — 패키지 검색 후 저장소 내 URDF 확인
- **Robot Description Formats 비교**: ROS Wiki → 현재는 Discourse로 이전

## VT6-mini / 6축 로봇팔 프로젝트 관련 추천

- `ros-industrial/universal_robot` — UR 시리즈 구조 참고용
- `Kinovarobotics/ros_kortex` — Kinova Gen3, VT6-mini 스케일과 유사한 소형 협동로봇 팔
- MuJoCo Menagerie의 팔 모델들 — 관절 리밋, 관성값 등이 정교하게 튜닝되어 있어 참고 가치 높음


