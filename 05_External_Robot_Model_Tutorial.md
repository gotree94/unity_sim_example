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

public class TurtleBot3Setup : MonoBehaviour
{
    [Header("물리 설정")]
    public float robotMass = 0.826f;
    public bool useGravity = true;

    [Header("바퀴 설정")]
    public float wheelRadius = 0.033f;
    public float wheelWidth = 0.018f;

    void Awake()
    {
        RemoveArticulationBodies();
        SetupRigidbody();
        ReplaceWheelColliders();
        SetupGround();
        Debug.Log("TurtleBot3 Setup 완료! Rigidbody 방식으로 전환됨.");
    }

    void RemoveArticulationBodies()
    {
        ArticulationBody[] abs = GetComponentsInChildren<ArticulationBody>();
        foreach (var ab in abs)
            DestroyImmediate(ab);

        foreach (var comp in GetComponentsInChildren<Component>())
            if (comp.GetType().Name.Contains("UrdfJoint"))
                DestroyImmediate(comp);

        Debug.Log($"ArticulationBody/UrdfJoint 제거 완료: {abs.Length}개");
    }

    void SetupRigidbody()
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.mass = robotMass;
        rb.useGravity = useGravity;
        rb.isKinematic = false;
        rb.drag = 0f;
        rb.angularDrag = 0.05f;
    }

    void ReplaceWheelColliders()
    {
        string[] wheelNames = { "wheel_left_link", "wheel_right_link" };

        foreach (string wheelName in wheelNames)
        {
            Transform wheelTransform = FindChildRecursive(transform, wheelName);
            if (wheelTransform == null) continue;

            foreach (Transform child in wheelTransform)
            {
                Collider col = child.GetComponent<Collider>();
                if (col != null)
                    DestroyImmediate(col);
            }

            CapsuleCollider capsule = wheelTransform.gameObject.AddComponent<CapsuleCollider>();
            capsule.radius = wheelRadius;
            capsule.height = wheelWidth;
            capsule.direction = 2;
            capsule.center = Vector3.zero;

            PhysicMaterial mat = new PhysicMaterial("WheelPhysMat");
            mat.dynamicFriction = 0.8f;
            mat.staticFriction = 0.8f;
            mat.bounciness = 0f;
            mat.frictionCombine = PhysicMaterialCombine.Maximum;
            capsule.material = mat;
        }

        Transform casterTransform = FindChildRecursive(transform, "caster_back_link");
        if (casterTransform != null)
            foreach (Transform child in casterTransform)
            { Collider c = child.GetComponent<Collider>(); if (c != null) DestroyImmediate(c); }

        Transform baseScanTransform = FindChildRecursive(transform, "base_scan");
        if (baseScanTransform != null)
            foreach (Transform child in baseScanTransform)
            { Collider c = child.GetComponent<Collider>(); if (c != null) DestroyImmediate(c); }
    }

    void SetupGround()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground == null) return;

        PhysicMaterial groundMat = new PhysicMaterial("GroundPhysMat");
        groundMat.dynamicFriction = 0.4f;
        groundMat.staticFriction = 0.4f;
        groundMat.bounciness = 0f;
        groundMat.frictionCombine = PhysicMaterialCombine.Average;

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

public class TurtleBot3Controller : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 0.22f;
    public float rotationSpeed = 2.84f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        Vector3 moveDirection = transform.forward * moveInput * moveSpeed;
        rb.MovePosition(rb.position + moveDirection * Time.fixedDeltaTime);

        float rotation = turnInput * rotationSpeed * Mathf.Rad2Deg * Time.fixedDeltaTime;
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
| ArticulationBody 모두 제거 | base_footprint, base_link, 바퀴 등 |
| UrdfJoint 모두 제거 | Controller, JointControl 등 |
| Rigidbody 추가 | turtlebot3_burger에 1개 추가 |
| 바퀴 Collider 교체 | Mesh Collider → Capsule Collider (Direction=Z) |
| 캐스터/LiDAR Collider 제거 | 불필요한 충돌체 제거 |
| Ground에 Physic Material 적용 | 마찰력 0.4, Average 합산 |

> 💡 **참고**: Setup은 Play 시 `Awake()`에서 자동 실행됩니다.

---

## 5단계: Play 테스트

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

Hierarchy에서 **Main Camera**를 선택하고 Inspector에서:

| 설정 | 값 | 설명 |
|------|-----|------|
| **Position X** | `0` | 중앙 정렬 |
| **Position Y** | `0.2` | 약간 위에서 내려다보기 |
| **Position Z** | `-0.7` | 로봇 앞쪽에서 바라보기 |

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

### 문제 2: 로봇이 바닥으로 떨어짐

| 확인 | 해결 |
|------|------|
| TurtleBot3Setup 스크립트가 turtlebot3_burger에 연결되었는지 | Add Component에서 확인 |
| Rigidbody의 Use Gravity가 체크되어 있는지 | Play 중 Inspector에서 확인 |
| Ground에 Collider가 있는지 | Ground 선택 → Inspector에서 확인 |

### 문제 3: 바퀴가 안 돌아감 (로봇이 안 움직임)

| 확인 | 해결 |
|------|------|
| TurtleBot3Controller 스크립트가 연결되었는지 | turtlebot3_burger에 있는지 확인 |
| Rigidbody의 Is Kinematic이 체크되어 있는지 | 체크 해제 필요 |
| Game View를 클릭했는지 | 키보드 입력은 Game View 포커스 필요 |

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


