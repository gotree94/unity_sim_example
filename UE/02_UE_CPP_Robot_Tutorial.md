# 2단계: C++ 기반 로봇 시뮬레이션

> **목적**: Unreal Engine 5.8에서 C++을 사용하여 로봇을 만들고 물리 효과 및 키보드 조작 기능을 구현  
> **소요 시간**: 약 120~180분  
> **전제 조건**: [1단계: UE5 설치 및 기초 학습](01_UE_Installation.md) 완료  
> **UE 버전**: Unreal Engine 5.8

---

## 목차

| 단계 | 파일 | 내용 | 소요 시간 |
|------|------|------|----------|
| **1** | [프로젝트 생성 및 Ground](02_1_UE_Project_Setup.md) | 프로젝트 생성, Ground(지면) 만들기 | 약 20~30분 |
| **2** | [로봇 구조 만들기](02_2_UE_Robot_Structure.md) | Body, Wheels 생성 + Transform 값 | 약 30~40분 |
| **3** | [물리 시스템 및 입력 설정](02_3_UE_Physics_Input.md) | Chaos Physics, Enhanced Input, Materials | 약 30~40분 |
| **4** | [C++ 클래스 구현](02_4_UE_CPP_Code.md) | 전체 C++ 코드 구현 | 약 30~40분 |
| **5** | [카메라 및 테스트](02_5_UE_Camera_Test.md) | 카메라 추적, 빌드, 테스트, FAQ | 약 20~30분 |

---

## 전체 흐름

```
[1단계] 프로젝트 생성 및 Ground
    |
    +-- UE 5.8 프로젝트 생성 (C++)
    +-- Ground(지면) 생성
    +-- 기본 레벨 설정
    |
[2단계] 로봇 구조 만들기
    |
    +-- RobotActor Blueprint 생성
    +-- Body (Static Mesh: Cube) 생성
    +-- 바퀴 4개 (Static Mesh: Cylinder) 생성
    +-- Transform 값 설정 (위치, 회전, 스케일)
    |
[3단계] 물리 시스템 및 입력 설정
    |
    +-- Chaos Physics 설정 (Simulate Physics)
    +-- Collision 설정 (BlockAll)
    +-- Enhanced Input System 설정
    +-- Material/색상 적용
    |
[4단계] C++ 클래스 구현
    |
    +-- RobotActor.h/cpp (액터)
    +-- RobotMovement.h/cpp (이동)
    +-- RobotSensor.h/cpp (센서)
    +-- RobotController.h/cpp (컨트롤러)
    |
[5단계] 카메라 및 테스트
    |
    +-- Spring Arm + Camera 설정
    +-- 프로젝트 빌드
    +-- 테스트 및 디버깅
    +-- 문제 해결
```

---

## Isaac Sim → UE 좌표축 변환

| 구분 | Isaac Sim | Unreal Engine 5.8 | Unity |
|------|-----------|-------------------|-------|
| **위쪽 방향** | Z축 | **Z축** (동일) | Y축 |
| **앞쪽 방향** | Y축 | X축 | Z축 |
| **오른쪽 방향** | X축 | Y축 | X축 |

```
Isaac Sim (Z-up)          UE5 (Z-up)
       Z ↑                      Z ↑
         |                       |
         |                       |
         +----→ Y            X ←─+────→ Y
        /                      /
       X                      X (전방)
```

> 💡 **팁**: Isaac Sim과 UE5는 모두 Z-up 좌표계를 사용합니다. Unity(Y-up)와 달리 좌표축 변환이 적습니다.

---

## Unity와의 주요 차이점

| 항목 | Unity | Unreal Engine 5.8 |
|------|-------|-------------------|
| 주 언어 | C# | C++ / Blueprint |
| 에셋 구조 | Assets/ | Content/ + Source/ |
| 오브젝트 | GameObject | Actor |
| 컴포넌트 | MonoBehaviour | ActorComponent |
| 물리 엔진 | PhysX | Chaos Physics |
| 입력 시스템 | Input Manager / Input System | Enhanced Input System |
| 렌더링 | URP/HDRP | Nanite/Lumen |

---

## 참고 자료

| 자료 | URL |
|------|-----|
| UE 5.8 문서 | https://docs.unrealengine.com/5.8/en-US/ |
| Enhanced Input System | https://docs.unrealengine.com/5.8/en-US/enhanced-input-in-unreal-engine/ |
| Chaos Physics | https://docs.unrealengine.com/5.8/en-US/physics-and-collision/ |
| C++ API 레퍼런스 | https://docs.unrealengine.com/5.8/en-US/API/ |

---

**다음 단계**: [1. 프로젝트 생성 및 Ground](02_1_UE_Project_Setup.md)

---

> **저작권**: 본 교육 자료는 교육 목적으로 자유롭게 사용할 수 있습니다.  
> **최종 업데이트**: 2026년 7월
