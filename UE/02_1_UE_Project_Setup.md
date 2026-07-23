# 02-1. 프로젝트 생성 및 Ground

> **목적**: Unreal Engine 5.8 프로젝트를 생성하고 Ground(지면)를 만듭니다  
> **소요 시간**: 약 20~30분  
> **전제 조건**: [1단계: UE5 설치](01_UE_Installation.md) 완료

---

## 목차

1. [C++ 프로젝트 생성](#1-c-프로젝트-생성)
2. [프로젝트 설정](#2-프로젝트-설정)
3. [Ground(지면) 만들기](#3-ground지면-만들기)
4. [기본 조명 설정](#4-기본-조명-설정)
5. [다음 단계로](#5-다음-단계로)

---

## 1. C++ 프로젝트 생성

### 1-1. Unreal Engine 실행

```
 Epic Games Launcher → Unreal Engine → 버전: 5.8 → Launch
```

### 1-2. 새 프로젝트 선택

```
New Project
├── Category: Games
├── Template: Blank (빈 프로젝트)
├── Project Type: C++    ← 중요!
├── Starter Content: ✅ 체크 (선택 사항)
├── Target Platform: Desktop
├── Quality Preset: Maximum
└── Starter Content: Yes
```

> ⚠️ **중요**: Blueprint가 아닌 **C++** 템플릿을 선택해야 합니다.

### 1-3. 프로젝트 이름 및 위치

```
Project Name: RobotSimulation
Location: C:\Users\Administrator\Desktop\
```

> 💡 **팁**: 프로젝트 이름은 영문, 공백 없이 작성하세요.

---

## 2. 프로젝트 설정

### 2-1. 프로젝트 설정 열기

```
Menu Bar → Edit → Project Settings
```

### 2-2. 입력 설정 (Enhanced Input System)

```
Project Settings → Engine → Input
├── Default Classes
│   ├── Default Player Input Class: EnhancedPlayerInput    ← 변경
│   └── Default Input Component Class: EnhancedInputComponent    ← 변경
└── Default Input
    └── Default Player Input Class: EnhancedPlayerInput
```

> 💡 **팁**: UE 5.8에서는 Enhanced Input System이 기본값이지만, 확인해 두는 것이 좋습니다.

### 2-3. Active Input 설정

```
Project Settings → Engine → Input
└── Default Input
    └── Default Ranges:     ← 확인
```

> 💡 **팁**: UE 5.8에서는 Enhanced Input System을 사용하므로 별도의 Active Input 설정이 필요 없습니다.

### 2-4. 에디터 설정

```
Project Settings → Editor
├── Appearance
│   └── Grid Snapping: ✅ 활성화
└── General - Autosave
    └── Autosave Interval: 5분 (선택 사항)
```

### 2-5. 빌드 설정

```
Project Settings → Platforms - Windows
├── Targeted RHIs
│   ├── DX12: ✅ (기본값)
│   └── DX11: ✅
└── Default RHI: DX12
```

---

## 3. Ground(지면) 만들기

### 3-1. 새 레벨 생성

```
Menu Bar → File → New Level → Default
```

### 3-2. Ground Actor 생성

```
Menu Bar → Place Actors → Geometry
├── Shapes
│   └── Cube    ← 클릭하여 레벨에 배치
```

또는:

```
Content Browser → Add → New Class → Actor
```

### 3-3. Ground 이름 변경

```
Level Editor → World Outliner → Cube
└── 이름 변경: Ground    ← 선택 후 F2 또는 이름 더블클릭
```

### 3-4. Ground Transform 설정

```
Ground
├── Transform
│   ├── Location: X=0, Y=0, Z=0
│   ├── Rotation: X=0, Y=0, Z=0
│   └── Scale: X=20, Y=20, Z=0.1    ← 크기 설정
```

> 💡 **팁**: Scale Z를 0.1로 설정하여 얇은 판 형태로 만듭니다.

### 3-5. Ground Material 적용

#### 방법 1: 기본 Material 사용

```
Content Browser → Basic
└── M_Grid_Material    ← 드래그하여 Ground에 적용
```

#### 방법 2: 새 Material 생성

```
Content Browser → Materials
└── 우클릭 → Material
    └── 이름: M_Ground
        └── 더블클릭하여 Material Editor 열기
```

**Material Editor 설정:**

```
Material Editor
├── Details Panel
│   ├── Material
│   │   └── Blend Mode: Opaque
│   └── Shading Model: Default Lit
└── Node Graph
    └── Constant3Vector → Base Color (회색: R=0.4, G=0.4, B=0.4)
```

### 3-6. Ground Collision 설정

```
Ground 선택 → Details Panel
├── Collision
│   ├── Collision Presets: BlockAll    ← 기본값
│   └── Collision Enabled: Query and Physics
└── Static Mesh Component
    └── Static Mesh: Cube
```

> ⚠️ **중요**: Collision이 BlockAll로 설정되어야 로봇이 Ground 위에 서 있을 수 있습니다.

---

## 4. 기본 조명 설정

### 4-1. Directional Light

```
Place Actors → Lights
└── Directional Light
    ├── Transform
    │   ├── Location: X=0, Y=0, Z=300
    │   └── Rotation: X=-50, Y=0, Z=0
    └── Details Panel
        ├── Intensity: 10 Lux
        └── Light Color: 흰색 (기본값)
```

### 4-2. Sky Light

```
Place Actors → Lights
└── Sky Light
    ├── Transform
    │   └── Location: X=0, Y=0, Z=0
    └── Details Panel
        ├── Intensity: 1
        └── Sky Light State: Movable
```

### 4-3. Sky Atmosphere

```
Place Actors → Visual Effects
└── Sky Atmosphere
    └── Transform
        └── Location: X=0, Y=0, Z=0
```

---

## 5. 다음 단계로

Ground가 준비되었습니다. 다음 단계에서는 로봇의 Body와 Wheels를 만듭니다.

**다음 단계**: [2. 로봇 구조 만들기](02_2_UE_Robot_Structure.md)

---

## 빠른 참조

| 항목 | 위치 |
|------|------|
| Place Actors | Window → Place Actors |
| World Outliner | Window → World Outliner |
| Details Panel | Window → Details |
| Content Browser | Window → Content Browser |
| 프로젝트 설정 | Edit → Project Settings |
| 레벨 열기 | File → Open Level |

---

> **이전 단계**: [1. UE5 설치 및 기초 학습](01_UE_Installation.md)  
> **다음 단계**: [2. 로봇 구조 만들기](02_2_UE_Robot_Structure.md)

---

> **저작권**: 본 교육 자료는 교육 목적으로 자유롭게 사용할 수 있습니다.  
> **최종 업데이트**: 2026년 7월
