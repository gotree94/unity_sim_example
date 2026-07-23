# 3단계: Blueprint 기반 로봇 시뮬레이션

## 3-1. Blueprint란?

### Blueprint 개요
- UE5의 비주얼 스크립팅 시스템
- C++ 없이 게임 로직 구현 가능
- 프로토타이핑 및 빠른 개발에 유리
- Unity의 Visual Scripting과 유사하지만 더 성숙한 시스템

### Blueprint vs C++
| 항목 | Blueprint | C++ |
|------|-----------|-----|
| 개발 속도 | 빠름 | 느림 |
| 성능 | 약간 느림 | 빠름 |
| 디버깅 | 비주얼 | 코드 기반 |
| 버전 관리 | 어려움 | 쉬움 |
| 팀 협업 | 적합 | 적합 |
| 프로토타이핑 | 적합 | 비적합 |

###何时使用 Blueprint
- 프로토타이핑 및 빠른 테스트
- 간단한 게임 로직
- 디자이너 및 아티스트의 자율 개발
- C++ 코드의 기능 테스트

---

## 3-2. Blueprint 프로젝트 구조

### Blueprint 프로젝트 구조
```
RobotSimulation/
├── Content/
│   ├── Blueprints/
│   │   ├── BP_RobotActor        # 로봇 액터 블루프린트
│   │   ├── BP_RobotController   # 로봇 컨트롤러
│   │   └── BP_GameMode          # 게임 모드
│   ├── Maps/
│   │   └── MainLevel            # 메인 레벨
│   ├── Materials/
│   │   └── M_Robot              # 로봇 머티리얼
│   └── StaticMeshes/
│       └── SM_Robot             # 로봇 메시
└── Config/                      # 프로젝트 설정
```

### Unity Blueprint 비교
| Unity | Unreal Engine 5 Blueprint |
|-------|---------------------------|
| GameObject | Actor |
| MonoBehaviour | ActorComponent |
| Visual Scripting | Blueprint |
| Prefab | Blueprint Class |
| ScriptableObject | Data Asset |

---

## 3-3. 로봇 액터 블루프린트 생성

### Blueprint 클래스 만들기
1. Content Browser → 우클릭 → Blueprint Class
2. 부모 클래스: **Actor** 선택
3. 이름: `BP_RobotActor`
4. 더블클릭하여 에디터 열기

### 컴포넌트 추가
1. Components 패널에서 "Add" 클릭
2. **Static Mesh** 추가 (로봇 메시)
3. **Capsule Component** 추가 (충돌 감지)
4. **Arrow Component** 추가 (전방 방향 표시)

### 변수 설정
1. My Blueprint → Variables → "+" 클릭
2. 변수 이름: `MoveSpeed`
3. 타입: **Float**
4. 기본값: `500.0`
5. **Instance Editable** 체크 (에디터에서 조정 가능)
6. **Blueprint Read Only** 체크 (읽기 전용)

---

## 3-4. 로봇 이동 Blueprint

### 이동 로직 구현
1. BP_RobotActor → Event Graph
2. **Event Tick** 노드 추가
3. 다음 노드 연결:

```
Event Tick → Get Actor Forward Vector → Multiply (MoveSpeed) → Get World Delta Seconds → Multiply → Add Actor World Offset
```

### 입력 바인딩
1. **InputAction MoveForward** 노드 추가
2. **InputAction MoveRight** 노드 추가
3. **InputAction Rotate** 노드 추가

### 전체 Blueprint 구조
```
Event BeginPlay
    └── Print String: "로봇 생성됨!"

Event Tick
    ├── Delta Time * MoveSpeed
    ├── Get Actor Forward Vector
    ├── Multiply
    └── Add Actor World Offset

InputAction MoveForward
    ├── Axis Value
    └── 로봇 전후 이동

InputAction MoveRight
    ├── Axis Value
    └── 로봇 좌우 이동

InputAction Rotate
    ├── Axis Value
    └── 로봇 회전
```

---

## 3-5. 로봇 센서 Blueprint

### 센서 구현 (Blueprint)
1. BP_RobotActor에서 새로운 함수 생성: `DetectObstacle`
2. 함수 그래프:

```
Function DetectObstacle
    ├── Get Actor Location
    ├── Get Actor Forward Vector
    ├── Multiply (DetectionRange)
    ├── Line Trace By Channel
    │   ├── Start: Actor Location
    │   ├── End: Start + Forward * Range
    │   └── Hit Result
    ├── Break Hit Result
    └── Return: Hit Actor
```

### 센서 시각화
1. **Draw Debug Line** 노드로 라인 트레이스 시각화
2. **Draw Debug Sphere** 노드로 감지 범위 시각화
3. 에디터에서 실시간으로 센서 동작 확인 가능

---

## 3-6. 로봇 컨트롤러 Blueprint

### Player Controller 블루프린트
1. Content Browser → 우클릭 → Blueprint Class
2. 부모 클래스: **Player Controller** 선택
3. 이름: `BP_RobotController`

### 입력 시스템 설정
1. 편집 → 프로젝트 설정 → 엔진 → Input
2. 액션 매핑 추가:
   - `MoveForward`: W, S
   - `MoveRight`: A, D
   - `Rotate`: Q, E
3. 축 매핑 추가:
   - `MoveForward`: W (1.0), S (-1.0)
   - `MoveRight`: D (1.0), A (-1.0)
   - `Rotate`: E (1.0), Q (-1.0)

### 컨트롤러 로직
```
Event BeginPlay
    └── Set Input Mode Game Only
        └── Set Show Mouse Cursor: False

InputAction MoveForward
    ├── Get Controlled Pawn
    ├── Cast to BP_RobotActor
    └── Call Function: MoveForward

InputAction MoveRight
    ├── Get Controlled Pawn
    ├── Cast to BP_RobotActor
    └── Call Function: MoveRight

InputAction Rotate
    ├── Get Controlled Pawn
    ├── Cast to BP_RobotActor
    └── Call Function: Rotate
```

---

## 3-7. 게임 모드 설정

### Game Mode 블루프린트
1. Content Browser → 우클릭 → Blueprint Class
2. 부모 클래스: **Game Mode Base** 선택
3. 이름: `BP_GameMode`

### 기본 클래스 설정
1. BP_GameMode → Class Defaults
2. **Default Pawn Class**: BP_RobotActor
3. **Player Controller Class**: BP_RobotController

### 레벨에 적용
1. MainLevel 열기
2. 월드 설정 → GameMode Override: BP_GameMode
3. 또는 프로젝트 설정에서 기본 게임모드로 설정

---

## 3-8. 좌표축 변환 (Blueprint)

### Blueprint 변환 함수
1. Content Browser → 우클릭 → Blueprint Class
2. 부母 클래스: **Blueprint Function Library** 선택
3. 이름: `BP_CoordinateConverter`

### 변환 함수 구현
```
Function ConvertUnityToUE
    ├── 입력: UnityVector (Vector)
    ├── 출력: UEVector (Vector)
    └── 로직:
        ├── UnityVector.X → X
        ├── UnityVector.Z → Y
        └── UnityVector.Y → Z

Function ConvertUnityToUE_Rotation
    ├── 입력: UnityRotation (Rotator)
    ├── 출력: UERotation (Rotator)
    └── 로직:
        ├── UnityRotation.Pitch → Pitch
        ├── UnityRotation.Yaw → Yaw
        └── UnityRotation.Roll → Roll
```

### 사용 예시
```
Unity 위치 (100, 200, 300) → UE 위치 (100, 300, 200)
```

---

## 3-9. 디버깅 및 테스트

### Blueprint 디버깅
1. **Print String** 노드로 값 출력
2. **Draw Debug Line**으로 시각적 디버깅
3. Blueprint 디버거로 실행 흐름 추적

### 실시간 변수 확인
1. 에디터 상단 "Details" 패널에서 변수 값 확인
2. **Watch Value** 기능으로 변수 모니터링
3. **Breakpoint** 설정으로 실행 일시 정지

### 테스트 체크리스트
- [ ] 로봇이 제대로 이동하는가?
- [ ] 회전이 올바르게 동작하는가?
- [ ] 센서가 장애물을 감지하는가?
- [ ] 입력이 제대로 처리되는가?
- [ ] 좌표축 변환이 올바른가?

---

## 3-10. 고급 기능

### 타이머 이벤트
```
Event BeginPlay
    └── Set Timer by Function Name
        ├── Function Name: "PeriodicCheck"
        ├── Time: 1.0
        └── Looping: True

Function PeriodicCheck
    ├── 센서로 주변 탐색
    └── 조건에 따라 로봇 행동 결정
```

### 이벤트 디스패처
1. **Custom Event**로 이벤트 생성
2. **Event Dispatcher**로 다른 Blueprint에 이벤트 전달
3. 예: `OnObstacleDetected` 이벤트

### 인터페이스
1. **Blueprint Interface** 생성: `BPI_Robot`
2. 공통 함수 정의: `MoveForward`, `Rotate`, `DetectObstacle`
3. 모든 로봇 Blueprint에 인터페이스 구현

### 다음 단계
- [4단계: Python TCP/IP 연결](./04_UE_Python_Connection.md)