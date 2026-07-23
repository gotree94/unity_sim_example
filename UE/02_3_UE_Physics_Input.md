# 02-3. 물리 시스템 및 입력 설정

> **목적**: Chaos Physics를 설정하고 Enhanced Input System, Materials를 설정합니다  
> **소요 시간**: 약 30~40분  
> **전제 조건**: [02-2. 로봇 구조 만들기](02_2_UE_Robot_Structure.md) 완료

---

## 목차

1. [Chaos Physics 설정](#1-chaos-physics-설정)
2. [Collision 설정](#2-collision-설정)
3. [Enhanced Input System](#3-enhanced-input-system)
4. [Materials 및 색상](#4-materials-및-색상)
5. [Physics Asset 설정](#5-physics-asset-설정)
6. [다음 단계로](#6-다음-단계로)

---

## 1. Chaos Physics 설정

### 1-1. RobotActor에 Physics 추가

```
Blueprint Editor
└── Components Panel
    └── RobotActor (루트)
        └── Details Panel
            └── Physics
                ├── Simulate Physics: ✅ 체크
                ├── Gravity: ✅ 체크
                ├── Mass: 100.0 kg (기본값)
                ├── Linear Damping: 0.0
                └── Angular Damping: 0.0
```

### 1-2. 물리 설정 상세

| 설정 | 값 | 설명 |
|------|-----|------|
| Simulate Physics | ✅ | 물리 시뮬레이션 활성화 |
| Gravity | ✅ | 중력 적용 |
| Mass | 100.0 kg | 로봇 무게 |
| Linear Damping | 0.0 | 선속도 감쇠 |
| Angular Damping | 0.0 | 각속도 감쇠 |

### 1-3. 추가 Physics 설정

```
RobotActor
├── Details Panel
│   ├── Physics
│   │   ├── Enable Gravity: ✅
│   │   ├── Simulate Physics: ✅
│   │   ├── Mass: 100.0
│   │   ├── Linear Damping: 0.0
│   │   ├── Angular Damping: 0.0
│   │   ├── Interpolate: ✅ (선택 사항)
│   │   └── Override Gravity: ❌
│   └── Constraints
│       └── (기본값 사용)
```

> 💡 **팁**: Interpolate를 활성화하면 물리 시뮬레이션이 부드럽게 보입니다.

---

## 2. Collision 설정

### 2-1. RobotActor Collision 설정

```
RobotActor
├── Details Panel
│   ├── Collision
│   │   ├── Collision Presets: BlockAll
│   │   └── Collision Enabled: Query and Physics
│   └── Object Type: PhysicsBody
```

### 2-2. Ground Collision 설정

```
Ground
├── Details Panel
│   ├── Collision
│   │   ├── Collision Presets: BlockAll
│   │   └── Collision Enabled: Query and Physics
│   └── Object Type: WorldStatic
```

### 2-3. Collision Channel 설정

```
Project Settings → Engine → Collision
├── Collision Channels
│   ├── Default (DTC_Default)
│   ├── Visibility (ECC_Visibility)
│   ├── Camera (ECC_Camera)
│   └── Custom
│       └── Robot: DTC_GameTraceChannel1 (선택 사항)
├── Collision Responses
│   ├── Ignore: ❌
│   ├── Overlap: ⚠️
│   └── Block: ✅ (기본값)
```

### 2-4. 바퀴 Collision 확인

```
각 바퀴 (Front_Left, Front_Right, Rear_Left, Rear_Right)
├── Details Panel
│   ├── Collision
│   │   ├── Collision Presets: BlockAll
│   │   └── Collision Enabled: Query and Physics
│   └── Object Type: PhysicsBody
```

> ⚠️ **중요**: 모든 바퀴가 BlockAll로 설정되어야 Ground와 충돌할 수 있습니다.

---

## 3. Enhanced Input System

### 3-1. Input Action 생성

```
Content Browser
└── 우클릭 → Input → Input Action
    └── 이름: IA_MoveForward
```

### 3-2. Input Action 설정

```
IA_MoveForward
├── Details Panel
│   ├── Value Type: Axis1D (Float)
│   │   └── 또는: Digital (bool) - 키보드 입력의 경우
│   └── Negate: ❌
```

### 3-3. 추가 Input Actions

| 이름 | Value Type | 설명 |
|------|------------|------|
| IA_MoveForward | Axis1D | 전진/후진 |
| IA_MoveRight | Axis1D | 좌우 이동 |
| IA_TurnRight | Axis1D | 우측 회전 |
| IA_TurnLeft | Axis1D | 좌측 회전 |

### 3-4. Input Mapping Context 생성

```
Content Browser
└── 우클릭 → Input → Input Mapping Context
    └── 이름: IMC_RobotControl
```

### 3-5. Input Mapping 설정

```
IMC_RobotControl
├── Details Panel
│   └── Mappings
│       ├── IA_MoveForward
│       │   ├── Key: W
│       │   ├── Modifiers: Negate (Shift 키)
│       │   └── Scale: 1.0
│       ├── IA_MoveForward
│       │   ├── Key: S
│       │   ├── Modifiers: (없음)
│       │   └── Scale: 1.0
│       ├── IA_MoveRight
│       │   ├── Key: D
│       │   ├── Modifiers: (없음)
│       │   └── Scale: 1.0
│       ├── IA_MoveRight
│       │   ├── Key: A
│       │   ├── Modifiers: Negate
│       │   └── Scale: 1.0
│       ├── IA_TurnRight
│       │   ├── Key: E
│       │   ├── Modifiers: (없음)
│       │   └── Scale: 1.0
│       └── IA_TurnLeft
│           ├── Key: Q
│           ├── Modifiers: Negate
│           └── Scale: 1.0
```

### 3-6. Input Mapping 설정 (간단한 방법)

```
IMC_RobotControl
├── Mappings
│   ├── IA_MoveForward + W: 1.0
│   ├── IA_MoveForward + S: -1.0
│   ├── IA_MoveRight + D: 1.0
│   ├── IA_MoveRight + A: -1.0
│   ├── IA_TurnRight + E: 1.0
│   └── IA_TurnLeft + Q: -1.0
```

### 3-7. Player Controller 설정

```
Content Browser
└── 우클릭 → Blueprint Class
    └── Parent Class: Player Controller
    └── 이름: RobotPlayerController
```

```
RobotPlayerController
├── Details Panel
│   ├── Input
│   │   └── Input Component: EnhancedInputComponent
│   └── Classes
│       └── Default Pawn Class: (RobotActor로 설정)
```

---

## 4. Materials 및 색상

### 4-1. Body Material 생성

```
Content Browser → Materials
└── 우클릭 → Material
    └── 이름: M_Body
        └── 더블클릭하여 Material Editor 열기
```

**Material Editor 설정:**

```
M_Body
├── Details Panel
│   ├── Material
│   │   ├── Blend Mode: Opaque
│   │   └── Shading Model: Default Lit
└── Node Graph
    └── Constant3Vector → Base Color
        └── 색상: 파란색 (R=0.1, G=0.3, B=0.8)
```

### 4-2. Wheel Material 생성

```
Content Browser → Materials
└── 우클릭 → Material
    └── 이름: M_Wheel
        └── 더블클릭하여 Material Editor 열기
```

**Material Editor 설정:**

```
M_Wheel
├── Details Panel
│   ├── Material
│   │   ├── Blend Mode: Opaque
│   │   └── Shading Model: Default Lit
└── Node Graph
    └── Constant3Vector → Base Color
        └── 색상: 검은색 (R=0.1, G=0.1, B=0.1)
```

### 4-3. Material 적용

```
Body
└── Details Panel
    └── Materials
        └── Element 0: M_Body

각 바퀴 (Front_Left, Front_Right, Rear_Left, Rear_Right)
└── Details Panel
    └── Materials
        └── Element 0: M_Wheel
```

---

## 5. Physics Asset 설정

### 5-1. Physics Asset 확인

```
Blueprint Editor
└── Components Panel
    └── RobotActor (루트)
        └── Details Panel
            └── Physics
                └── Physics Asset: (자동 생성된 Asset)
```

### 5-2. Physics Asset 편집

```
Content Browser → RobotSimulation → Physics
└── RobotActor_PhysicsAsset
    └── 더블클릭하여 Physics Asset Editor 열기
```

### 5-3. Physics Body 설정

```
Physics Asset Editor
├── Bodies
│   ├── RobotActor
│   │   ├── Type: Capsule
│   │   ├── Size: X=50, Y=50, Z=50
│   │   └── Offset: X=0, Y=0, Z=50
│   ├── Body
│   │   ├── Type: Box
│   │   ├── Size: X=50, Y=100, Z=25
│   │   └── Offset: X=0, Y=0, Z=50
│   ├── Front_Left
│   │   ├── Type: Capsule
│   │   ├── Size: X=37.5, Y=37.5, Z=12.5
│   │   └── Offset: X=50, Y=75, Z=50
│   ├── Front_Right
│   │   ├── Type: Capsule
│   │   ├── Size: X=37.5, Y=37.5, Z=12.5
│   │   └── Offset: X=50, Y=-75, Z=50
│   ├── Rear_Left
│   │   ├── Type: Capsule
│   │   ├── Size: X=37.5, Y=37.5, Z=12.5
│   │   └── Offset: X=-50, Y=75, Z=50
│   └── Rear_Right
│       ├── Type: Capsule
│       ├── Size: X=37.5, Y=37.5, Z=12.5
│       └── Offset: X=-50, Y=-75, Z=50
└── Constraints
    └── (기본값 사용)
```

### 5-4. Physics Body 타입 선택

| 파트 | Physics Body 타입 | 크기 |
|------|-------------------|------|
| RobotActor | Capsule | X=50, Y=50, Z=50 |
| Body | Box | X=50, Y=100, Z=25 |
| Front_Left | Capsule | X=37.5, Y=37.5, Z=12.5 |
| Front_Right | Capsule | X=37.5, Y=37.5, Z=12.5 |
| Rear_Left | Capsule | X=37.5, Y=37.5, Z=12.5 |
| Rear_Right | Capsule | X=37.5, Y=37.5, Z=12.5 |

> 💡 **팁**: Capsule은 바퀴에 적합하고, Box는 Body에 적합합니다.

---

## 6. 다음 단계로

물리 시스템, 입력, Material이 설정되었습니다. 다음 단계에서는 C++ 클래스를 구현합니다.

**다음 단계**: [4. C++ 클래스 구현](02_4_UE_CPP_Code.md)

---

## 빠른 참조

| 항목 | 위치 |
|------|------|
| Input Action | Content Browser → Input → Input Action |
| Input Mapping | Content Browser → Input → Input Mapping Context |
| Material | Content Browser → Materials |
| Physics Asset | Content Browser → Physics |
| Project Settings | Edit → Project Settings |

---

> **이전 단계**: [2. 로봇 구조 만들기](02_2_UE_Robot_Structure.md)  
> **다음 단계**: [4. C++ 클래스 구현](02_4_UE_CPP_Code.md)

---

> **저작권**: 본 교육 자료는 교육 목적으로 자유롭게 사용할 수 있습니다.  
> **최종 업데이트**: 2026년 7월
