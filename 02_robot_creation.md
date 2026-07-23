# 2단계: 로봇 제작

> **목적**: Unity에서 직접 로봇을 만들고 물리 효과 및 키보드 조작 기능을 구현  
> **소요 시간**: 약 90~120분  
> **전제 조건**: [1단계: Unity 설치 및 기초 학습](01_unity_installation.md) 완료

---

## 목차

3. [프로젝트 생성 및 기본 설정](#3-프로젝트-생성-및-기본-설정)
4. [지면(Ground) 만들기](#4-지면ground-만들기)
5. [로봇 몸통(Cube) 만들기](#5-로봇-몸통cube-만들기)
6. [로봇 바퀴(Cylinder) 만들기](#6-로봇-바퀴cylinder-만들기)
7. [물리 효과 적용 (Rigidbody 및 Collider)](#7-물리-효과-적용-rigidbody-및-collider)
8. [충돌 검사 윤곽선 확인](#8-충돌-검사-윤곽선-확인)
9. [접촉 및 마찰 매개변수 (Physic Material)](#9-접촉-및-마찰-매개변수-physic-material)
10. [객체의 색상 변경 (Material)](#10-객체의-색상-변경-material)
11. [키보드 입력으로 로봇 조종하기](#11-키보드-입력으로-로봇-조종하기)
12. [최종 테스트 및 정리](#12-최종-테스트-및-정리)

---

## 3. 프로젝트 생성 및 기본 설정

### 3-1. Unity Hub에서 새 프로젝트 만들기

1. **Unity Hub**를 실행합니다.
2. 좌측 상단의 **"New project"** 버튼을 클릭합니다.
3. 템플릿에서 **"3D (URP)"** 또는 **"3D Core"**를 선택합니다.
   - URP(Universal Render Pipeline)를 추천합니다. 렌더링 품질이 좋고 성능도 우수합니다.
4. 프로젝트 이름을 **"RobotTutorial"**로 입력합니다.
5. 프로젝트 저장 위치를 설정한 뒤 **"Create project"** 버튼을 클릭합니다.

### 3-2. 씬(Scene) 확인

1. 프로젝트가 열리면 기본 씬이 로드됩니다.
2. Unity 상단 메뉴에서 **File > New Scene**을 클릭하여 새로운 씬을 만듭니다.
3. **File > Save As**로 씬 이름을 **"RobotScene"**으로 저장합니다.

### 3-3. 씬 구조 이해하기

Hierarchy 창(좌측)에는 현재 씬에 있는 모든 게임 오브젝트가 나열됩니다. 기본적으로 다음 오브젝트들이 있습니다:

| 오브젝트 | 역할 |
|---------|------|
| **Main Camera** | 게임 화면을 렌더링하는 카메라 |
| **Directional Light** | 장면 전체를 비추는 태양빛 같은 광원 |

> 💡 **팁**: Inspector 창(우측)에서 선택한 오브젝트의 속성을 확인하고 수정할 수 있습니다.

### 3-4. 좌표축 변환 계획 이해하기 (Isaac Sim → Unity)

이 튜토리얼은 NVIDIA Isaac Sim 튜토리얼을 Unity에서 구현하는 것이므로, **좌표축 차이**를 미리 이해하는 것이 중요합니다.

#### 핵심 차이점: 위쪽 방향 축

| 구분 | Isaac Sim | Unity |
|------|-----------|-------|
| **위쪽 방향** | **Z축** | **Y축** |
| **앞쪽 방향** | Y축 | Z축 |
| **오른쪽 방향** | X축 | X축 (동일) |

```
Isaac Sim (Z-up)          Unity (Y-up)
       Z ↑                      Y ↑
         |                       |
         |                       |
         +----→ Y            X ←─+────→ Z
        /                      /
       X                      X (화면 안쪽)
```

#### 로봇 파트별 좌표축 변환 계획

| 파트 | Isaac Sim 설정 | Unity 설정 | 변환 내용 |
|------|---------------|-----------|----------|
| **Body (Cube)** | Scale: (2, 1, 0.5) | Scale: (2, 0.5, 1) | X→X, Y→Z, Z→Y축 변환 |
| **Body 위치** | Translate Z = 0.5 | Position Y = 0.25 | Z→Y축 변환 (높이 절반) |
| **Wheel (Cylinder)** | Rotate X = 90도 | Rotation X = 90도 | X축 회전 동일 |
| **Wheel 위치** | Translate: (0.5, 0.75, 0) | Position: (0.5, 0, 0.75) | Y→Z, Z→Y축 변환 |

#### 높이 축 묶기 (부모-자식 관계)

最終적으로 Body와 Wheel은 하나의 **Robot** 빈 오브젝트 아래에 자식으로 배치됩니다:

```
Robot (빈 오브젝트, 부모)
├── Body            ← Rigidbody + Box Collider (몸통)
├── Front_Right     ← Rigidbody + Capsule Collider (오른쪽 앞바퀴)
├── Front_Left      ← Rigidbody + Capsule Collider (왼쪽 앞바퀴)
├── Rear_Right      ← Rigidbody + Capsule Collider (오른쪽 뒷바퀴)
└── Rear_Left       ← Rigidbody + Capsule Collider (왼쪽 뒷바퀴)
```

> 💡 **팁**: 부모 오브젝트의 Transform을 기준으로 자식 오브젝트들의 상대 위치가 결정됩니다.
> 나중에 5단계, 6단계에서 각 파트를 만들고, 11단계에서 이 구조로 묶게 됩니다.

### 3-5. Input 시스템 설정 (중요)

Unity 6 이상 버전에서는 기본적으로 **새 Input System Package**가 활성화되어 있어, 기존 코드에서 사용하는 `UnityEngine.Input` 클래스가 작동하지 않을 수 있습니다. 반드시 다음 설정을 확인합니다.

#### 설정 방법

1. Unity 에디터 상단 메뉴에서 **Edit > Project Settings** 를 엽니다.
2. 좌측에서 **Player** 를 선택합니다.
3. 우측의 **Other Settings** 를 찾아 스크롤합니다.
4. **Active Input Handling** 항목을 찾습니다.

| 옵션 | 설명 |
|------|------|
| Input Manager (Old) | 기존 방식만 사용 |
| Input System Package (New) | 새 방식만 사용 |
| **Both** | 두 방식 모두 지원 (추천) |

5. **"Both"** 를 선택합니다.

> ⚠️ **중요**: 변경 후 Unity 에디터를 **재시작**해야 적용됩니다.
> 이 설정이 되어 있지 않으면 `InvalidOperationException` 오류가 발생합니다.

---

## 4. 지면(Ground) 만들기

로봇이 서 있을 바닥이 필요합니다.

### 4-1. Plane 생성

1. Hierarchy 창의 빈 공간을 **우클릭**합니다.
2. **3D Object > Plane**을 클릭합니다.
3. 생성된 Plane의 이름을 **"Ground"**로 변경합니다.
   - 이름을 바꾸려면 Hierarchy에서 해당 오브젝트를 선택한 뒤, Inspector 상단의 이름 필드를 수정하거나, 오브젝트를 선택한 상태에서 **F2** 키를 누릅니다.

### 4-2. Plane 크기 설정

1. Hierarchy에서 **Ground**를 선택합니다.
2. Inspector 창에서 **Transform** 컴포넌트를 찾습니다.
3. **Scale** 값을 **X: 5, Y: 1, Z: 5**로 설정합니다.
   - 이렇게 하면 바닥이 충분히 넓어져서 로봇이 움직여도 떨어지지 않습니다.

### 4-3. Ground 색상 설정 (선택사항)

Ground 색상을 나중에 Material로 변경할 수 있지만, 일단 놔둡니다. 나중에 8절에서 색상을 지정할 때 함께 진행합니다.

---

## 5. 로봇 몸통(Cube) 만들기

Isaac Sim 튜토리얼에서는 **Create > Shape > Cube**로 상자를 만들고 Z축 위치와 Scale을 변경했습니다. Unity에서는 다음과 같이 진행합니다.

### 5-1. Cube 생성

1. Hierarchy 창의 빈 공간을 **우클릭**합니다.
2. **3D Object > Cube**를 클릭합니다.
3. 생성된 Cube의 이름을 **"Body"**로 변경합니다.

### 5-2. Transform 설정

1. Hierarchy에서 **Body**를 선택합니다.
2. Inspector 창의 **Transform** 컴포넌트에서 값을 설정합니다.

| 속성 | Isaac Sim 값 | Unity 대응 | Unity 값 |
|------|-------------|-----------|---------|
| Position | (0, 0, 0.5) | Position Y (Unity에서는 Y축이 위) | **(0, 0.25, 0)** |
| Scale | (2, 1, 0.5) | Scale (X→X, Y→Z, Z→Y) | **(2, 0.5, 1)** |

> ⚠️ **중요 차이점**: Isaac Sim에서는 Z축이 위쪽이지만, Unity에서는 **Y축이 위쪽**입니다. 
> - Isaac Sim의 Scale (2, 1, 0.5) = 길이 2, 너비 1, 높이 0.5
> - Unity의 Scale (2, 0.5, 1) = 길이 2, 높이 0.5, 너비 1

#### 상세 설정 방법

**Position:**
- X: `0`
- Y: `0.25` (지면에 닿도록 설정: 높이 0.5의 절반)
- Z: `0`

**Scale:**
- X: `2` (길이 - 앞뒤 방향)
- Y: `0.5` (높이 - 위아래 방향)
- Z: `1` (너비 - 좌우 방향)

### 5-3. 결과 확인

Scene 창에서 Body가 지면 위에 떠 있는 것을 확인할 수 있습니다. 카메라 조작법:
- **마우스 휠**: 줌 인/아웃
- **마우스 우클릭 + 드래그**: 카메라 회전
- **마우스 휠 클릭 + 드래그**: 카메라 팬(이동)

---

## 6. 로봇 바퀴(Cylinder) 만들기

Isaac Sim에서는 **Create > Shape > Cylinder**를 사용했습니다. Unity에서도 동일하게 Cylinder를 사용합니다. 최신 Isaac Sim 튜토리얼에서는 **4개의 바퀴**를 사용하므로, Unity에서도 4개의 바퀴를 만듭니다.

### 6-1. 첫 번째 바퀴 생성 (Front_Right)

1. Hierarchy 창의 빈 공간을 **우클릭**합니다.
2. **3D Object > Cylinder**를 클릭합니다.
3. 생성된 Cylinder의 이름을 **"Front_Right"**로 변경합니다.

### 6-2. 첫 번째 바퀴 Transform 설정

Isaac Sim에서는:
- Scale: (0.75, 0.75, 0.25)
- Rotate X: 90도
- Translate: (0.5, 0.75, 0)

Unity에서는 다음과 같이 설정합니다:

**Position:**
- X: `0.5` (몸통 앞쪽)
- Y: `0.5` (바닥에 닿도록 설정: 바퀴 높이의 절반)
- Z: `0.75` (몸통 오른쪽)

**Rotation:**
- X: `90` (Isaac Sim의 Rotate X 90도 = Unity의 X축 90도 회전)
- Y: `0`
- Z: `0`

**Scale:**
- X: `0.75` (반지름)
- Y: `0.25` (Isaac Sim의 Height 0.25에 대응)
- Z: `0.75` (반지름)

> 💡 **팁**: Unity의 기본 Cylinder는 Y축을 기준으로 세워져 있으므로, X축으로 90도 회전해야 Isaac Sim과 같은 방향(옆으로 눕힌 형태)이 됩니다.
> 바퀴의 Y 위치는 0.5로 설정하여 지면에 닿도록 합니다 (바퀴 높이 0.25의 절반).

### 6-3. 세 개의 바퀴 추가 (복제)

1. Hierarchy에서 **Front_Right**을 선택합니다.
2. **Ctrl + D** 키를 눌러 복제합니다.
3. 복제된 오브젝트의 이름을 **"Front_Left"**로 변경합니다.
4. Inspector의 Transform에서 **Position Z**를 **`-0.75`**로 변경합니다.

### 6-4. 뒷바퀴 생성

1. **Front_Right**을 선택하고 **Ctrl + D**로 복제합니다.
2. 이름을 **"Rear_Right"**로 변경합니다.
3. **Position X**를 **`-0.5`**로 변경합니다 (뒷쪽으로 이동).

4. **Front_Left**을 선택하고 **Ctrl + D**로 복제합니다.
5. 이름을 **"Rear_Left"**로 변경합니다.
6. **Position X**를 **`-0.5`**로 변경합니다.

### 6-5. 최종 로봇 구조 확인

Hierarchy 창에 다음과 같은 구조가 보여야 합니다:

```
RobotScene
├── Main Camera
├── Directional Light
├── Ground
├── Body            ← 로봇 몸통 (Cube)
├── Front_Right     ← 오른쪽 앞바퀴 (Cylinder)
├── Front_Left      ← 왼쪽 앞바퀴 (Cylinder)
├── Rear_Right      ← 오른쪽 뒷바퀴 (Cylinder)
└── Rear_Left       ← 왼쪽 뒷바퀴 (Cylinder)
```

> 💡 **팁**: 모든 로봇 파트를 하나의 빈 오브젝트 아래에 자식으로 넣으면 관리가 편해집니다.
> - Hierarchy 빈 공간 우클릭 > **Create Empty** > 이름을 **"Robot"**으로 변경
> - Body, Front_Right, Front_Left, Rear_Right, Rear_Left을 모두 선택한 뒤 Robot 오브젝트로 드래그하여 자식으로 만듦

---

## 7. 물리 효과 적용 (Rigidbody 및 Collider)

이제 Isaac Sim에서 했던 것처럼 물리 효과를 적용합니다. 시뮬레이션을 돌리면 객체가 중력에 의해 떨어지도록 만들어야 합니다.

### 7-1. Unity의 물리 시스템 이해하기

Unity의 물리 시스템은 두 가지 핵심 컴포넌트로 구성됩니다:

| 컴포넌트 | 역할 |
|---------|------|
| **Rigidbody** | 물리 엔진에 의해 움직이는 오브젝트에 추가. 중력, 힘, 질량 등을 다룸 |
| **Collider** | 충돌 감지용 형태. 오브젝트가 서로 통과하지 못하게 함 |

> Isaac Sim의 "Rigid Body with Colliders Preset" = Unity의 **Rigidbody + Collider** 조합

### 7-2. Body에 Rigidbody 및 Box Collider 추가

1. Hierarchy에서 **Body**를 선택합니다.
2. Inspector 창 하단의 **"Add Component"** 버튼을 클릭합니다.
3. **Rigidbody**를 검색하여 선택합니다.
4. 다시 **Add Component**를 클릭합니다.
5. **Box Collider**를 검색하여 선택합니다.
   - Unity의 Cube에는 기본적으로 Box Collider가 자동으로 추가되어 있을 수 있습니다.

#### Rigidbody 설정값

Inspector에서 Rigidbody 컴포넌트의 값을 확인합니다:

| 속성 | 값 | 설명 |
|------|---|------|
| Mass | `1` | 질량 (기본값, 필요시 조정) |
| Drag | `0` | 공기 저항 (0 = 없음) |
| Angular Drag | `0.05` | 회전 저항 |
| Use Gravity | ✅ 체크 | 중력 적용 |
| Is Kinematic | ❌ 체크 해제 | 물리 엔진에 의해 움직임 |

### 7-3. 바퀴에 Rigidbody 및 Capsule Collider 추가

4개의 바퀴 모두에 물리 효과를 적용합니다.

1. Hierarchy에서 **Front_Right**을 선택합니다.
2. **Add Component > Rigidbody**를 추가합니다.
3. **Add Component > Capsule Collider**를 추가합니다.
   - Capsule Collider는 원통에 더 적합합니다.

#### Capsule Collider 설정

| 속성 | 값 | 설명 |
|------|---|------|
| Center | (0, 0, 0) | 중심점 (기본값) |
| Radius | `0.375` | 반지름 (Isaac Sim의 0.75 스케일에 대응) |
| Height | `0.25` | 높이 (Isaac Sim의 Height 0.25에 대응) |
| Direction | **Z-Axis** | 캡슐의 방향을 Z축으로 설정 (옆으로 눕힌 원통에 맞춤) |

4. 같은 방법으로 **Front_Left**, **Rear_Right**, **Rear_Left**에도 Rigidbody와 Capsule Collider를 추가합니다.

> 💡 **팁**: 4개의 바퀴를 모두 선택한 뒤 한꺼번에 Component를 추가할 수도 있습니다.

### 7-4. Ground에 Collider 추가

1. Hierarchy에서 **Ground**를 선택합니다.
2. **Add Component > Mesh Collider**를 추가합니다.
   - Plane에는 Mesh Collider가 자동으로 추가되어 있을 수 있습니다.
3. Rigidbody는 **추가하지 않습니다**. (바닥은 움직이면 안 됨)

### 7-5. 시뮬레이션 테스트

1. Unity 상단의 **▶ (Play)** 버튼을 클릭합니다.
2. 로봇이 중력에 의해 떨어지는 것을 확인합니다.
3. **▶ (Play)** 버튼을 다시 클릭하여 정지합니다.

> ⚠️ **문제 해결**: 로봇이 떨어지지 않으면 다음을 확인하세요:
> - Rigidbody의 **Use Gravity**가 체크되어 있는지
> - Ground의 Collider가 있는지
> - Body의 Y 위치가 0보다 큰지 (지면 위에 있는지)

---

## 8. 충돌 검사 윤곽선 확인

Isaac Sim에서는 **Show By Type > Physics > Colliders > All**로 윤곽선을 표시했습니다. Unity에서는 다음 방법을 사용합니다.

### 8-1. Scene 뷰에서 충돌체 표시

1. Scene 창 상단의 **Gizmos** 드롭다운 메뉴를 클릭합니다.
2. **"Gizmos"** 버튼이 활성화(파란색)되어 있는지 확인합니다.

### 8-2. 전체 충돌체 윤곽선 표시 (Scene 뷰)

Unity의 Scene 뷰에서는 모든 Collider가 **초록색 와이어프레임**으로 자동 표시됩니다.

만약 표시되지 않는다면:
1. Scene 창 상단의 **Draw Mode**를 **Shaded**에서 **Wireframe**으로 변경해 보세요.
2. 또는 Scene 뷰의 **Gizmos** 버튼을 클릭하여 활성화하세요.

### 8-3. Play 모드에서 충돌체 표시

1. Unity 상단 메뉴에서 **Edit > Project Settings**를 엽니다.
2. 좌측에서 **Physics**를 선택합니다.
3. 우측 하단의 **"Layer Collision Matrix"** 아래에 **"Queries Hit Triggers"** 옵션을 확인합니다.

### 8-4. 커스텀 충돌체 시각화 (선택)

더 명확하게 충돌체를 보고 싶다면 스크립트를 사용할 수 있습니다:

1. Project 창에서 우클릭 > **Create > C# Script** > 이름을 **"ShowColliders"**로 변경
2. 스크립트를 더블클릭하여 Visual Studio 또는 Rider에서 엽니다.
3. 다음 코드를 입력합니다:

```csharp
using UnityEngine;

public class ShowColliders : MonoBehaviour
{
    void OnDrawGizmos()
    {
        // 모든 Collider의 윤곽선을 그립니다
        foreach (var collider in FindObjectsOfType<Collider>())
        {
            Gizmos.color = Color.green;

            if (collider is BoxCollider box)
            {
                Gizmos.matrix = collider.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (collider is CapsuleCollider capsule)
            {
                Gizmos.matrix = collider.transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(capsule.center, capsule.radius);
            }
            else if (collider is SphereCollider sphere)
            {
                Gizmos.matrix = collider.transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
        }
    }
}
```

4. 저장하고 Unity로 돌아옵니다.
5. 이 스크립트를 **任何** 빈 오브젝트(예: "GameManager")에 붙입니다.

---

## 9. 접촉 및 마찰 매개변수 (Physic Material)

Isaac Sim에서는 **Create > Physics > Physics Material**로 마찰력과 탄성을 조절했습니다. Unity에서는 **Physic Material** (유니티에서는 "Physic"이 정확한 명칭)을 사용합니다.

### 9-1. Physic Material 생성

1. Project 창의 **Assets** 폴더에서 우클릭합니다.
2. **Create > Physic Material**을 클릭합니다.
3. 이름을 **"RobotMaterial"**로 변경합니다.

> ⚠️ 주의: "Physic Material"은 "Physic**s** Material"과 다릅니다.
> - **Physic Material**: 3D 물리용 (Collider에 적용)
> - **Physics Material 2D**: 2D 물리용

### 9-2. 매개변수 설정

RobotMaterial을 선택하고 Inspector에서 다음 값을 설정합니다:

| 속성 | 값 | 설명 |
|------|---|------|
| Dynamic Friction | `0.6` | 움직이는 물체의 마찰력 (0~1) |
| Static Friction | `0.6` | 멈춰있는 물체의 마찰력 (0~1) |
| Bounciness | `0.1` | 탄성 (0=안 튕김, 1=완전 반사) |
| Friction Combine | **Multiply** | 마찰력 결합 방식 |
| Bounce Combine | **Average** | 탄성 결합 방식 |

> 💡 **팁**: Isaac Sim의 접촉 매개변수와 유사하게 설정할 수 있습니다.
> - 마찰력이 높을수록 바퀴가 미끄러지지 않음
> - 탄성이 높을수록 강하게 튕겨옴

### 9-3. Collider에 Physic Material 적용

**Body (Box Collider)에 적용:**
1. Hierarchy에서 **Body**를 선택합니다.
2. Inspector에서 **Box Collider** 컴포넌트를 찾습니다.
3. **Material** 필드에 **RobotMaterial**을 드래그하거나 클릭하여 선택합니다.

**바퀴 (Capsule Collider)에 적용:**
1. **Front_Right**, **Front_Left**, **Rear_Right**, **Rear_Left**을 각각 선택합니다.
2. 각각의 **Capsule Collider**의 **Material** 필드에 **RobotMaterial**을 적용합니다.

### 9-4. Ground용 별도 Material (선택)

지면의 마찰력을 별도로 설정하고 싶다면:

1. Project 창에서 우클릭 > **Create > Physic Material** > 이름을 **"GroundMaterial"**로 변경
2. 설정값:

| 속성 | 값 |
|------|---|
| Dynamic Friction | `0.8` |
| Static Friction | `0.8` |
| Bounciness | `0` |

3. Ground의 Mesh Collider에 적용합니다.

---

## 10. 객체의 색상 변경 (Material)

Isaac Sim에서는 **Create > Material > OmniPBR**을 사용하여 Body와 Wheel의 색상을 변경했습니다. Unity에서는 **Material**과 **Shader**를 사용합니다.

### 10-1. URP용 Material 생성 (URP 프로젝트인 경우)

1. Project 창의 **Assets** 폴더에서 우클릭합니다.
2. **Create > Material**을 클릭합니다.
3. 이름을 **"BodyMaterial"**로 변경합니다.
4. Inspector 상단의 **Shader** 드롭다운에서 **Universal Render Pipeline > Lit**을 선택합니다.

### 10-1-2. Built-in Render Pipeline용 Material (3D Core 프로젝트인 경우)

1. Project 창에서 우클릭합니다.
2. **Create > Material**을 클릭합니다.
3. 이름을 **"BodyMaterial"**로 변경합니다.
4. Inspector 상단의 **Shader**에서 **Standard**가 선택되어 있는지 확인합니다.

### 10-2. BodyMaterial 색상 설정

1. **BodyMaterial**을 선택합니다.
2. Inspector에서 **Albedo** 옆의 색상 블록을 클릭합니다.
3. 색상 선택기에서 **파란색 (R: 50, G: 100, B: 200)** 정도를 선택합니다.
4. Metallic 값을 `0.3`, Smoothness 값을 `0.7` 정도로 조정합니다.

### 10-3. WheelMaterial 생성 및 설정

1. 같은 방법으로 새 Material을 만듭니다.
2. 이름을 **"WheelMaterial"**로 변경합니다.
3. **Albedo** 색상을 **검은색 (R: 30, G: 30, B: 30)** 으로 설정합니다.
4. Metallic: `0.5`, Smoothness: `0.3`으로 설정합니다.

### 10-4. GroundMaterial 생성 및 설정

1. 새 Material을 만들고 이름을 **"GroundMaterial"**로 합니다.
2. **Albedo** 색상을 **밝은 회색 (R: 200, G: 200, B: 200)** 으로 설정합니다.

### 10-5. Material 적용

**Body에 적용:**
1. Hierarchy에서 **Body**를 선택합니다.
2. Inspector에서 **Mesh Renderer** 컴포넌트를 찾습니다.
3. **Materials > Element 0** 필드에 **BodyMaterial**을 드래그하거나 클릭하여 선택합니다.

**바퀴에 적용:**
1. **Front_Right**, **Front_Left**, **Rear_Right**, **Rear_Left**을 선택합니다.
2. 각각의 Mesh Renderer의 **Element 0**에 **WheelMaterial**을 적용합니다.

**Ground에 적용:**
1. **Ground**를 선택합니다.
2. Mesh Renderer의 **Element 0**에 **GroundMaterial**을 적용합니다.

### 10-6. 최종 구조

```
Assets/
├── Materials/
│   ├── BodyMaterial       ← 파란색 (몸통)
│   ├── WheelMaterial      ← 검은색 (바퀴)
│   └── GroundMaterial     ← 밝은 회색 (바닥)
├── PhysicMaterials/
│   └── RobotMaterial      ← 마찰/탄성 설정
└── Scenes/
    └── RobotScene.unity
```

---

## 11. 키보드 입력으로 로봇 조종하기

이제 로봇을 키보드로 조종하는 기능을 추가합니다. **W, A, S, D, X** 또는 **화살표 키**로 이동하고, **스페이스바**로 멈추는 기능입니다.

### 11-1. Robot 빈 오브젝트 만들기

모든 로봇 파트를 하나의 부모 오브젝트로 묶습니다.

1. Hierarchy에서 빈 공간을 **우클릭** > **Create Empty**를 클릭합니다.
2. 이름을 **"Robot"**으로 변경합니다.
3. **Body**, **Front_Right**, **Front_Left**, **Rear_Right**, **Rear_Left**을 모두 선택합니다.
4. 선택된 오브젝트들을 **Robot** 오브젝트 위로 **드래그**하여 자식으로 만듭니다.

### 11-2. RobotController 스크립트 만들기

1. Project 창의 **Assets** 폴더에서 우클릭합니다.
2. **Create > C# Script**를 클릭합니다.
3. 이름을 **"RobotController"**로 변경합니다.
4. 스크립트를 **더블클릭**하여 Visual Studio 또는 Visual Studio Code에서 엽니다.

### 11-3. 스크립트 코드 작성

다음 코드를 기존 코드에 **전체 교체**합니다:

```csharp
using UnityEngine;

public class RobotController : MonoBehaviour
{
    // ===== 이동 설정 =====
    [Header("이동 설정")]
    [Tooltip("이동 속도 (m/s)")]
    public float moveSpeed = 5.0f;

    [Tooltip("회전 속도 (degrees/s)")]
    public float rotationSpeed = 120.0f;

    [Tooltip("점프 힘")]
    public float jumpForce = 7.0f;

    // ===== 내부 변수 =====
    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        // Rigidbody 컴포넌트 가져오기
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("RobotController: Rigidbody 컴포넌트가 없습니다! Robot 오브젝트에 Rigidbody를 추가해주세요.");
        }
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        MoveRobot();
    }

    /// <summary>
    /// 키보드 입력을 처리합니다.
    /// </summary>
    void HandleInput()
    {
        // 스페이스바로 멈춤
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StopRobot();
        }
    }

    /// <summary>
    /// 로봇을 이동시킵니다. FixedUpdate에서 매 프레임 호출됩니다.
    /// </summary>
    void MoveRobot()
    {
        if (rb == null) return;

        float moveX = 0f;
        float moveZ = 0f;

        // ===== W, A, S, D 입력 =====
        // W 또는 UpArrow: 전진
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveZ = 1f;
        }
        // S 또는 DownArrow: 후진
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveZ = -1f;
        }
        // A 또는 LeftArrow: 좌회전
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveX = -1f;
        }
        // D 또는 RightArrow: 우회전
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveX = 1f;
        }

        // ===== X 키: 후진 (선택 기능) =====
        if (Input.GetKey(KeyCode.X))
        {
            moveZ = -1f;
        }

        // ===== 이동 적용 =====
        // 전진/후진: 로봇의 앞방향으로 이동
        Vector3 moveDirection = transform.forward * moveZ;
        moveDirection = moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveDirection);

        // 좌/우 회전
        float rotation = moveX * rotationSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, rotation, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    /// <summary>
    /// 로봇을 즉시 멈춥니다. 스페이스바로 호출됩니다.
    /// </summary>
    void StopRobot()
    {
        if (rb == null) return;

        // 속도를 0으로 만들어 완전히 멈춤
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Debug.Log("로봇이 멈췄습니다!");
    }

    /// <summary>
    /// 바닥과 접촉했는지 확인합니다. (점프를 위해 사용)
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
```

> ⚠️ **중요**: Unity 2023 이상 버전에서는 `Rigidbody.velocity`가 `Rigidbody.linearVelocity`로 변경되었습니다. 본인의 Unity 버전에 맞게 수정해주세요.
> - Unity 2022 이하: `rb.velocity`, `rb.angularVelocity`
> - Unity 2023 이상: `rb.linearVelocity`, `rb.angularVelocity`

### 11-4. 스크립트를 Robot에 적용

1. Unity로 돌아옵니다.
2. Hierarchy에서 **Robot** 오브젝트를 선택합니다.
3. Inspector 하단의 **"Add Component"** 버튼을 클릭합니다.
4. **RobotController**를 검색하여 선택합니다.
5. Inspector에서 **RobotController** 컴포넌트의 값을 확인합니다:

| 속성 | 기본값 | 설명 |
|------|-------|------|
| Move Speed | `5` | 이동 속도 |
| Rotation Speed | `120` | 회전 속도 |
| Jump Force | `7` | 점프 힘 (현재 사용 안 함) |

### 11-5. Rigidbody 추가 (Robot 부모 오브젝트)

Robot 부모 오브젝트에도 Rigidbody가 있어야 합니다.

1. **Robot** 오브젝트를 선택합니다.
2. **Add Component > Rigidbody**를 추가합니다.
3. Rigidbody 설정을 확인합니다:

| 속성 | 값 |
|------|---|
| Mass | `1` |
| Use Gravity | ✅ 체크 |
| Is Kinematic | ✅ **체크** (부모 오브젝트는 직접 물리 적용 안 함) |

> 💡 **팁**: 부모 오브젝트의 Rigidbody를 Kinematic으로 설정하면, 자식 오브젝트(바퀴)들이 물리적으로 자유롭게 움직이면서도 부모를 따라갑니다.

### 11-6. Ground에 태그(Tag) 설정

점프 기능을 위해 바닥에 "Ground" 태그를 지정합니다.

1. Hierarchy에서 **Ground**를 선택합니다.
2. Inspector 상단의 **Tag** 드롭다운을 클릭합니다.
3. **"Add Tag..."**를 클릭합니다.
4. Tags 목록에서 **"+"** 버튼을 클릭합니다.
5. 이름을 **"Ground"**로 입력하고 **Save**를 클릭합니다.
6. 다시 Hierarchy에서 **Ground**를 선택하고, Inspector의 **Tag** 드롭다운에서 **"Ground"**를 선택합니다.

### 11-7. 키보드 조작법 요약

| 키 | 동작 |
|----|------|
| **W** 또는 **↑** | 전진 (로봇 앞쪽으로 이동) |
| **S** 또는 **↓** | 후진 (로봇 뒤쪽으로 이동) |
| **A** 또는 **←** | 좌회전 |
| **D** 또는 **→** | 우회전 |
| **X** | 후진 (S와 동일) |
| **스페이스바** | 즉시 멈춤 (속도 0으로 초기화) |

### 11-8. Play 버튼으로 테스트

1. Unity 상단의 **▶ (Play)** 버튼을 클릭합니다.
2. Game 창에서 키보드를 눌러 로봇을 조종합니다.
3. **스페이스바**를 눌러 로봇이 멈추는지 확인합니다.
4. **▶ (Play)** 버튼을 다시 클릭하여 정지합니다.

---

## 12. 최종 테스트 및 정리

### 12-1. 전체 기능 테스트 체크리스트

Play 모드에서 다음 항목을 모두 확인합니다:

- [ ] 로봇이 중력에 의해 지면 위에 올바르게 서 있는지
- [ ] W키/↑로 로봇이 전진하는지
- [ ] S키/↓로 로봇이 후진하는지
- [ ] A키/←으로 로봇이 좌회전하는지
- [ ] D키/→으로 로봇이 우회전하는지
- [ ] X키로 로봇이 후진하는지
- [ ] 스페이스바로 로봇이 즉시 멈추는지
- [ ] 로봇이 지면을 뚫고 지나가지 않는지
- [ ] 로봇의 색상이 올바르게 표시되는지

### 12-2. 최종 씬 구조

```
RobotScene
├── Main Camera
├── Directional Light
├── Ground              ← Mesh Collider + GroundMaterial
├── Robot               ← Rigidbody(Kinematic) + RobotController
│   ├── Body            ← Rigidbody + Box Collider + BodyMaterial
│   ├── Front_Right     ← Rigidbody + Capsule Collider + WheelMaterial
│   ├── Front_Left      ← Rigidbody + Capsule Collider + WheelMaterial
│   ├── Rear_Right      ← Rigidbody + Capsule Collider + WheelMaterial
│   └── Rear_Left       ← Rigidbody + Capsule Collider + WheelMaterial
```

### 12-3. 최종 Assets 구조

```
Assets/
├── Materials/
│   ├── BodyMaterial       (파란색)
│   ├── WheelMaterial      (검은색)
│   └── GroundMaterial     (밝은 회색)
├── PhysicMaterials/
│   └── RobotMaterial      (마찰/탄성)
├── Scripts/
│   └── RobotController.cs (키보드 입력 처리)
└── Scenes/
    └── RobotScene.unity
```

---

## Isaac Sim vs Unity 비교표

| 항목 | Isaac Sim | Unity |
|------|----------|-------|
| 오브젝트 생성 | Create > Shape > ... | Hierarchy 우클릭 > 3D Object > ... |
| 좌표축 (위) | Z축 | Y축 |
| 물리 바디 | Physics > Rigid Body with Colliders | Add Component > Rigidbody + Collider |
| 물리 재질 | Create > Physics > Physics Material | Create > Physic Material |
| 머티리얼 | Create > Material > OmniPBR | Create > Material |
| 재질 적용 | Property > Materials | Mesh Renderer > Materials |
| 충돌체 표시 | Show By Type > Physics > Colliders | Scene 뷰 Gizmos |
| 시뮬레이션 | Play 버튼 (상단) | ▶ Play 버튼 (상단) |

---

## 문제 해결 (FAQ)

### Q1: 로봇이 움직이지 않아요
- **RobotController** 스크립트가 **Robot** 오브젝트에 붙어있는지 확인
- Rigidbody의 **Is Kinematic**이 체크되어 있는지 확인
- Play 모드에서 Game 창이 활성화되어 있는지 확인 (키보드 입력은 Game 창에서만 작동)

### Q2: 로봇이 땅을 뚫고 지나가요
- Ground의 Collider가 있는지 확인
- 바퀴/몸통의 Collider와 Ground의 Collider가 서로 다른 Layer에 있는지 확인

### Q3: 로봇이 너무 빠르거나 느려요
- RobotController의 **Move Speed** 값을 조정합니다 (기본값: 5)

### Q4: rb.linearVelocity 오류가 나요
- Unity 버전이 2022 이하라면 `rb.linearVelocity`를 `rb.velocity`로 변경하세요.

### Q5: 키보드 입력이 안 먹혀요
- Game 창을 **마우스로 클릭**하여 포커스를 맞춘 뒤 키보드를 누르세요.
- 다른 UI 오브젝트가 입력을 가로채고 있는지 확인하세요.

---

## 확장 아이디어

튜토리얼을 완료했다면, 다음 기능들을 추가로 도전해보세요:

1. **스페이스바 점프**: isGrounded 체크 후 위쪽으로 힘 주기
2. **마우스 회전**: 마우스 좌우 움직임으로 카메라가 로봇을 따라가도록
3. **가속/감속**: 키를 누르고 있으면 점점 빨라지고, 놓으면 천천히 멈추도록
4. **부스터**: Shift 키를 누르면 이동 속도 2배
5. **바퀴 회전 애니메이션**: 이동할 때 바퀴가 실제로 굴러가는 시각 효과
6. **Python에서 센서 데이터 전송**: Unity의 로봇 위치를 Python으로 실시간 전송
7. **이중 통신**: Unity에서 Python으로 상태를 보내고, Python에서 제어 명령을 받는 양방향 통신
8. **여러 로봇 제어**: Python에서 여러 대의 로봇을 동시에 제어

---

**다음 단계**: [Python 연결 (13단계)](03_python_connection.md)
