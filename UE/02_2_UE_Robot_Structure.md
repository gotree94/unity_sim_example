# 02-2. 로봇 구조 만들기

> **목적**: RobotActor Blueprint를 만들고 Body, Wheels를 생성하여 Transform 값을 설정합니다  
> **소요 시간**: 약 30~40분  
> **전제 조건**: [02-1. 프로젝트 생성 및 Ground](02_1_UE_Project_Setup.md) 완료

---

## 목차

1. [RobotActor Blueprint 생성](#1-robotactor-blueprint-생성)
2. [Body(몸통) 만들기](#2-body몸통-만들기)
3. [Front_Left 바퀴 만들기](#3-front_left-바퀴-만들기)
4. [Front_Right 바퀴 만들기](#4-front_right-바퀴-만들기)
5. [Rear_Left 바퀴 만들기](#5-rear_left-바퀴-만들기)
6. [Rear_Right 바퀴 만들기](#6-rear_right-바퀴-만들기)
7. [최종 Transform 확인](#7-최종-transform-확인)
8. [다음 단계로](#8-다음-단계로)

---

## 1. RobotActor Blueprint 생성

### 1-1. Blueprint 생성

```
Content Browser
└── 우클ICK → Blueprint Class
    └── Parent Class: Actor
    └── 이름: RobotActor
```

### 1-2. Blueprint Editor 열기

```
Content Browser → RobotActor
└── 더블클릭하여 Blueprint Editor 열기
```

### 1-3. Blueprint Editor 구성

```
Blueprint Editor
├── Components Panel (좌측)
│   └── 루트 컴포넌트: DefaultSceneRoot
├── Details Panel (우측)
│   └── 선택된 컴포넌트의 속성
└── Event Graph (중앙 하단)
    └── 이벤트 기반 로직
```

---

## 2. Body(몸통) 만들기

### 2-1. Static Mesh Component 추가

```
Components Panel
└── Add Component (상단 + 버튼)
    └── Static Mesh
    └── 이름: Body
```

### 2-2. Body Static Mesh 설정

```
Body (Static Mesh Component)
├── Details Panel
│   ├── Static Mesh
│   │   └── Cube    ← 검색하여 선택
│   ├── Materials
│   │   └── Element 0: M_Ground (또는 기본 Material)
│   └── Scale
│       └── X=1.0, Y=2.0, Z=0.5
```

> 💡 **팁**: Isaac Sim 기준으로 Unity 튜토리얼과 동일한 비율을 적용합니다.

### 2-3. Body Transform 설정

```
Body
├── Transform
│   ├── Location: X=0, Y=0, Z=50
│   ├── Rotation: X=0, Y=0, Z=0
│   └── Scale: X=1.0, Y=2.0, Z=0.5
```

> 💡 **팁**: Z=50으로 설정하여 Ground 위에 약간 띄웁니다.

### 2-4. Body Collision 설정

```
Body
├── Details Panel
│   ├── Collision
│   │   ├── Collision Presets: BlockAll
│   │   └── Collision Enabled: Query and Physics
│   └── Static Mesh Component
│       └── Simulate Physics: ❌ (Body에는 물리 비활성화)
```

> ⚠️ **중요**: Body에는 물리 시스템을 비활성화합니다. 로봇 전체에만 물리를 적용합니다.

---

## 3. Front_Left 바퀴 만들기

### 3-1. Static Mesh Component 추가

```
Components Panel
└── Add Component (상단 + 버튼)
    └── Static Mesh
    └── 이름: Front_Left
```

### 3-2. Front_Left Static Mesh 설정

```
Front_Left (Static Mesh Component)
├── Details Panel
│   ├── Static Mesh
│   │   └── Cylinder    ← 검색하여 선택
│   ├── Materials
│   │   └── Element 0: M_Ground (또는 기본 Material)
│   └── Scale
│       └── X=0.75, Y=0.75, Z=0.25
```

### 3-3. Front_Left Transform 설정

```
Front_Left
├── Transform
│   ├── Location: X=50, Y=75, Z=50
│   ├── Rotation: X=0, Y=0, Z=90
│   └── Scale: X=0.75, Y=0.75, Z=0.25
```

### 3-4. Front_Left Collision 설정

```
Front_Left
├── Details Panel
│   ├── Collision
│   │   ├── Collision Presets: BlockAll
│   │   └── Collision Enabled: Query and Physics
│   └── Static Mesh Component
│       └── Simulate Physics: ❌ (개별 바퀴에는 물리 비활성화)
```

> ⚠️ **중요**: 개별 바퀴에는 물리를 비활성화합니다. 로봇 전체(RobotActor)에만 물리를 적용합니다.

---

## 4. Front_Right 바퀴 만들기

### 4-1. Static Mesh Component 추가

```
Components Panel
└── Add Component (상단 + 버튼)
    └── Static Mesh
    └── 이름: Front_Right
```

### 4-2. Front_Right Static Mesh 설정

```
Front_Right (Static Mesh Component)
├── Details Panel
│   ├── Static Mesh
│   │   └── Cylinder    ← 검색하여 선택
│   ├── Materials
│   │   └── Element 0: M_Ground (또는 기본 Material)
│   └── Scale
│       └── X=0.75, Y=0.75, Z=0.25
```

### 4-3. Front_Right Transform 설정

```
Front_Right
├── Transform
│   ├── Location: X=50, Y=-75, Z=50
│   ├── Rotation: X=0, Y=0, Z=90
│   └── Scale: X=0.75, Y=0.75, Z=0.25
```

### 4-4. Front_Right Collision 설정

```
Front_Right
├── Details Panel
│   ├── Collision
│   │   ├── Collision Presets: BlockAll
│   │   └── Collision Enabled: Query and Physics
│   └── Static Mesh Component
│       └── Simulate Physics: ❌ (개별 바퀴에는 물리 비활성화)
```

---

## 5. Rear_Left 바퀴 만들기

### 5-1. Static Mesh Component 추가

```
Components Panel
└── Add Component (상단 + 버튼)
    └── Static Mesh
    └── 이름: Rear_Left
```

### 5-2. Rear_Left Static Mesh 설정

```
Rear_Left (Static Mesh Component)
├── Details Panel
│   ├── Static Mesh
│   │   └── Cylinder    ← 검색하여 선택
│   ├── Materials
│   │   └── Element 0: M_Ground (또는 기본 Material)
│   └── Scale
│       └── X=0.75, Y=0.75, Z=0.25
```

### 5-3. Rear_Left Transform 설정

```
Rear_Left
├── Transform
│   ├── Location: X=-50, Y=75, Z=50
│   ├── Rotation: X=0, Y=0, Z=90
│   └── Scale: X=0.75, Y=0.75, Z=0.25
```

### 5-4. Rear_Left Collision 설정

```
Rear_Left
├── Details Panel
│   ├── Collision
│   │   ├── Collision Presets: BlockAll
│   │   └── Collision Enabled: Query and Physics
│   └── Static Mesh Component
│       └── Simulate Physics: ❌ (개별 바퀴에는 물리 비활성화)
```

---

## 6. Rear_Right 바퀴 만들기

### 6-1. Static Mesh Component 추가

```
Components Panel
└── Add Component (상단 + 버튼)
    └── Static Mesh
    └── 이름: Rear_Right
```

### 6-2. Rear_Right Static Mesh 설정

```
Rear_Right (Static Mesh Component)
├── Details Panel
│   ├── Static Mesh
│   │   └── Cylinder    ← 검색하여 선택
│   ├── Materials
│   │   └── Element 0: M_Ground (또는 기본 Material)
│   └── Scale
│       └── X=0.75, Y=0.75, Z=0.25
```

### 6-3. Rear_Right Transform 설정

```
Rear_Right
├── Transform
│   ├── Location: X=-50, Y=-75, Z=50
│   ├── Rotation: X=0, Y=0, Z=90
│   └── Scale: X=0.75, Y=0.75, Z=0.25
```

### 6-4. Rear_Right Collision 설정

```
Rear_Right
├── Details Panel
│   ├── Collision
│   │   ├── Collision Presets: BlockAll
│   │   └── Collision Enabled: Query and Physics
│   └── Static Mesh Component
│       └── Simulate Physics: ❌ (개별 바퀴에는 물리 비활성화)
```

---

## 7. 최종 Transform 확인

### 7-1. 최종 Transform 테이블

| 파트 | 위치 (Location) | 회전 (Rotation) | 스케일 (Scale) |
|------|-----------------|-----------------|---------------|
| **RobotActor** (루트) | X=0, Y=0, Z=0 | X=0, Y=0, Z=0 | X=1, Y=1, Z=1 |
| **Body** | X=0, Y=0, Z=50 | X=0, Y=0, Z=0 | X=1.0, Y=2.0, Z=0.5 |
| **Front_Left** | X=50, Y=75, Z=50 | X=0, Y=0, Z=90 | X=0.75, Y=0.75, Z=0.25 |
| **Front_Right** | X=50, Y=-75, Z=50 | X=0, Y=0, Z=90 | X=0.75, Y=0.75, Z=0.25 |
| **Rear_Left** | X=-50, Y=75, Z=50 | X=0, Y=0, Z=90 | X=0.75, Y=0.75, Z=0.25 |
| **Rear_Right** | X=-50, Y=-75, Z=50 | X=0, Y=0, Z=90 | X=0.75, Y=0.75, Z=0.25 |

### 7-2. Isaac Sim과 비교

| 구분 | Isaac Sim | Unreal Engine 5.8 | Unity |
|------|-----------|-------------------|-------|
| **Body 위치** | Z=50 | Z=50 | Y=0.5 |
| **Body 스케일** | 0.5x2x1 | 1.0x2.0x0.5 | 1x0.5x2 |
| **바퀴 회전** | X=90° | Z=90° | Z=90° |
| **좌표축** | Z-up | Z-up | Y-up |

### 7-3. 최종 구조 시각화

```
[Front]
   FL -------- FR
     |          |
     |   Body   |
     |          |
   RL -------- RR
[Rear]

FL = Front_Left (X=50, Y=75)
FR = Front_Right (X=50, Y=-75)
RL = Rear_Left (X=-50, Y=75)
RR = Rear_Right (X=-50, Y=-75)
```

---

## 8. 다음 단계로

로봇 구조가 완성되었습니다. 다음 단계에서는 물리 시스템, 입력, Material을 설정합니다.

**다음 단계**: [3. 물리 시스템 및 입력 설정](02_3_UE_Physics_Input.md)

---

## 빠른 참조

| 항목 | 위치 |
|------|------|
| Components Panel | Blueprint Editor 좌측 |
| Details Panel | Blueprint Editor 우측 |
| Add Component | Components Panel 상단 + 버튼 |
| Static Mesh 선택 | Details Panel → Static Mesh 드롭다운 |
| Transform 설정 | Details Panel → Transform |

---

> **이전 단계**: [1. 프로젝트 생성 및 Ground](02_1_UE_Project_Setup.md)  
> **다음 단계**: [3. 물리 시스템 및 입력 설정](02_3_UE_Physics_Input.md)

---

> **저작권**: 본 교육 자료는 교육 목적으로 자유롭게 사용할 수 있습니다.  
> **최종 업데이트**: 2026년 7월
