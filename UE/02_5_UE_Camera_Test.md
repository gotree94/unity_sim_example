# 02-5. 카메라 및 테스트

> **목적**: 카메라 추적, 프로젝트 빌드, 테스트 및 문제 해결을 진행합니다  
> **소요 시간**: 약 20~30분  
> **전제 조건**: [02-4. C++ 클래스 구현](02_4_UE_CPP_Code.md) 완료

---

## 목차

1. [카메라 설정](#1-카메라-설정)
2. [프로젝트 빌드](#2-프로젝트-빌드)
3. [테스트 및 디버깅](#3-테스트-및-디버깅)
4. [문제 해결 FAQ](#4-문제-해결-faq)
5. [Unity와 비교](#5-unity와-비교)

---

## 1. 카메라 설정

### 1-1. Spring Arm 컴포넌트 추가

```
Blueprint Editor
└── Components Panel
    └── Add Component
        └── Spring Arm
        └── 이름: CameraBoom
```

### 1-2. Spring Arm 설정

```
CameraBoom (Spring Arm Component)
├── Details Panel
│   ├── Target Arm Length: 500.0
│   ├── Socket Offset: X=0, Y=0, Z=200.0
│   ├── Use Pawn Control Rotation: ✅
│   ├── Inherit Pitch: ✅
│   ├── Inherit Yaw: ✅
│   ├── Inherit Roll: ❌
│   └── Camera Collision
│       ├── Do Collision Test: ✅
│       └── Socket Offset: X=0, Y=0, Z=200.0
```

### 1-3. Camera 컴포넌트 추가

```
Blueprint Editor
└── Components Panel
    └── Add Component
        └── Camera
        └── 이름: FollowCamera
```

### 1-4. Camera 설정

```
FollowCamera (Camera Component)
├── Details Panel
│   ├── Camera Settings
│   │   ├── Field of View: 90.0
│   │   └── Aspect Ratio: 16:9
│   ├── Post Process
│   │   └── (기본값 사용)
│   └── Transform
│       └── Location: X=0, Y=0, Z=0
```

### 1-5. 카메라 위치 시각화

```
[전방 (X+)]
    ↑
    |
    |  Camera (Z=200)
    |    /
    |   /
    |  /
    | /
    +----→ Body (Z=50)
    |
[Ground (Z=0)]
```

---

## 2. 프로젝트 빌드

### 2-1. 소스 코드 컴파일

```
IDE (Visual Studio / Rider)
├── 파일 열기
│   └── C:\Users\Administrator\Desktop\RobotSimulation\RobotSimulation.sln
├── 빌드 설정
│   └── Configuration: Development Editor
│   └── Platform: Win64
└── 빌드 실행
    └── Build → Build Solution (Ctrl+Shift+B)
```

### 2-2. UE Editor에서 빌드

```
UE Editor
├── Toolbar
│   └── Compile
│       └── Compile RobotSimulation (Ctrl+Alt+F11)
└── 빌드 진행률 확인
    └── Output Log에서 확인
```

### 2-3. 빌드 성공 확인

```
Output Log
├── Compile complete!
├── [Compile] of [RobotSimulation] succeeded
└── (오류 없음)
```

### 2-4. 빌드 시간

| 항목 | 예상 시간 |
|------|----------|
| 초기 빌드 | 약 5~10분 |
|增量 빌드 | 약 1~3분 |
| 전체 재빌드 | 약 10~15분 |

> 💡 **팁**: 초기 빌드 후에는增量 빌드가 훨씬 빠릅니다.

---

## 3. 테스트 및 디버깅

### 3-1. Play in Editor (PIE)

```
UE Editor
├── Toolbar
│   └── Play
│       └── Play in Editor (Alt+P)
└── 레벨 실행
    └── 로봇이 화면에 나타남
```

### 3-2. 키보드 테스트

| 키 | 동작 | 예상 결과 |
|----|------|----------|
| W | 전진 | 로봇이 전방으로 이동 |
| S | 후진 | 로봇이 후방으로 이동 |
| D | 우측 이동 | 로봇이 우측으로 이동 |
| A | 좌측 이동 | 로봇이 좌측으로 이동 |
| E | 우회전 | 로봇이 우측으로 회전 |
| Q | 좌회전 | 로봇이 좌측으로 회전 |

### 3-3. 카메라 테스트

```
테스트 항목:
├── 카메라가 로봇을 추적하는지 확인
├── 카메라가 벽에 부딪히지 않는지 확인
├── 카메라 각도가 적절한지 확인
└── 카메라 줌인/줌아웃 동작 확인
```

### 3-4. 물리 테스트

```
테스트 항목:
├── 로봇이 중력에 의해 떨어지는지 확인
├── 로봇이 Ground 위에 서 있는지 확인
├── 로봇이 벽에 부딪혔을 때 반응하는지 확인
├── 로봇의 속도가 적절한지 확인
└── 로봇의 회전이 부드러운지 확인
```

### 3-5. 디버깅 방법

#### 방법 1: Output Log 확인

```
UE Editor
└── Window → Developer Tools → Output Log
    └── 로그 메시지 확인
```

#### 방법 2: Visual Studio 디버깅

```
Visual Studio
├── 디버그 → 시작 디버깅 (F5)
├── 중단점 설정
│   └── 코드에서 중단점 설정
└── 변수 값 확인
```

#### 방법 3: Blueprint 디버깅

```
Blueprint Editor
├── Event Graph
│   └── 실행 흐름 추적
└── 변수 값 실시간 확인
```

---

## 4. 문제 해결 FAQ

### Q1: 빌드 오류 발생 시?

```
해결 방법:
1. 프로젝트 클린
   └── File → Refresh Visual Studio Project
   └── File → Refresh Visual Studio Code Project
2. 소스 다시 빌드
   └── Build → Rebuild Solution
3. 캐시 삭제
   └── Binaries, Intermediate, Saved 폴더 삭제
4. 다시 빌드
```

### Q2: 로봇이 움직이지 않을 때?

```
해결 방법:
1. 입력 설정 확인
   └── Project Settings → Engine → Input
   └── Enhanced Input System 활성화 확인
2. 입력 액션 확인
   └── Input Mapping Context가 올바르게 설정되어 있는지 확인
3. 로봇 액터 확인
   └── RobotMovement 컴포넌트가 추가되어 있는지 확인
```

### Q3: 로봇이 떨어질 때?

```
해결 방법:
1. 물리 설정 확인
   └── RobotActor에 Simulate Physics가 활성화되어 있는지 확인
2. Collision 확인
   └── Ground와 로봇의 Collision이 BlockAll로 설정되어 있는지 확인
3. 위치 확인
   └── 로봇이 Ground 위에 올바르게 배치되어 있는지 확인
```

### Q4: 카메라가 추적하지 않을 때?

```
해결 방법:
1. Spring Arm 확인
   └── CameraBoom이 로봇에 부착되어 있는지 확인
2. Camera 설정 확인
   └── FollowCamera가 CameraBoom에 부착되어 있는지 확인
3. Use Pawn Control Rotation 확인
   └── Spring Arm의 Use Pawn Control Rotation이 활성화되어 있는지 확인
```

### Q5: 프레임 저하 발생 시?

```
해결 방법:
1. 그래픽 설정 확인
   └── Edit → Project Settings → Engine → Rendering
   └── Graphics settings 낮추기
2. 디버그 시각화 비활성화
   └── DrawDebugSphere 등 주석 처리
3. LOD 설정
   └── 메시 LOD 설정
```

---

## 5. Unity와 비교

### 5-1. 구현 방식 비교

| 항목 | Unity | Unreal Engine 5.8 |
|------|-------|-------------------|
| 이동 구현 | Rigidbody.MovePosition | SetActorLocation |
| 회전 구현 | Rigidbody.MoveRotation | SetActorRotation |
| 입력 시스템 | Input Manager / Input System | Enhanced Input System |
| 카메라 추적 | CameraFollow 스크립트 | Spring Arm + Camera |
| 물리 엔진 | PhysX | Chaos Physics |

### 5-2. Unity 튜토리얼과의 차이점

| 구분 | Unity (02_robot_creation.md) | UE5 (이 튜토리얼) |
|------|------------------------------|-------------------|
| **언어** | C# | C++ |
| **물리** | PhysX (Rigidbody) | Chaos Physics |
| **입력** | Input System | Enhanced Input |
| **카메라** | CameraFollow.cs | Spring Arm + Camera |
| **에셋 구조** | Assets/ | Content/ + Source/ |
| **오브젝트** | GameObject | Actor |
| **컴포넌트** | MonoBehaviour | ActorComponent |

### 5-3. 좌표축 비교

| 구분 | Unity | UE5 |
|------|-------|-----|
| 위쪽 | Y축 | Z축 |
| 앞쪽 | Z축 | X축 |
| 오른쪽 | X축 | Y축 |

---

## 빠른 참조

| 항목 | 위치 |
|------|------|
| Spring Arm | Place Actors → Spring Arm |
| Camera | Place Actors → Camera |
| Compile | Toolbar → Compile |
| Play | Toolbar → Play |
| Output Log | Window → Developer Tools → Output Log |

---

> **이전 단계**: [4. C++ 클래스 구현](02_4_UE_CPP_Code.md)  
> **첫 단계로 돌아가기**: [02. C++ 기반 로봇 시뮬레이션 (메인)](02_UE_CPP_Robot_Tutorial.md)

---

> **저작권**: 본 교육 자료는 교육 목적으로 자유롭게 사용할 수 있습니다.  
> **최종 업데이트**: 2026년 7월
