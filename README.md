# Unity 로봇 만들기 튜토리얼 - Isaac Sim 예제 Unity 구현

> **대상**: Unity 초심자  
> **소요 시간**: 약 130 ~ 180분  
> **사용 환경**: Windows 10/11  
> **Unity 버전**: Unity 2022.3 LTS (2022년 9월 ~ )  
> **목표**: Unity에 익숙해진 뒤, NVIDIA Isaac Sim 튜토리얼 2.2 "Add Simple Objects"를 Unity에서 구현하고, 외부 Python 프로그램에서 키보드 입력으로 로봇을 원격 제어하는 기능까지 구현

---

## 튜토리얼 구성

| 단계 | 파일 | 내용 | 소요 시간 |
|------|------|------|----------|
| **1단계** | [Unity 설치 및 기초 학습](01_unity_installation.md) | Unity Hub/Editor 설치, 에디터 조작법, Asset Store 체험 | 약 45~60분 |
| **2단계** | [로봇 제작](02_robot_creation.md) | 프로젝트 생성, 3D 모델링, 물리 효과, 키보드 조작 | 약 90~120분 |
| **3단계** | [Python 연결](03_python_connection.md) | TCP/IP 통신, Python 원격 제어 | 약 40~60분 |
| **4단계** | [Unity AI 개발](04_Unity_AI_Development_Tutorial.md) | Unity AI 기능, 2D 게임, 자동차 시뮬레이션, ROS 연결 | 약 240~360분 |

---

## 전체 흐름

```
[1단계] Unity 설치 및 기초
    |
    +-- Unity Hub/Editor 설치
    +-- 에디터 레이아웃 및 기본 개념 학습
    +-- Asset Store 예제로 조작법 익히기
    |
[2단계] 로봇 제작
    |
    +-- 프로젝트 생성 및 좌표축 변환 계획
    +-- Ground, Body(Cube), Wheel(Cylinder) 생성
    +-- Rigidbody + Collider 물리 효과 적용
    +-- Material로 색상 지정
    +-- RobotController 스크립트로 키보드 조작
    |
[3단계] Python 연결
    |
    +-- TCPServer 스크립트로 Unity에서 서버 실행
    +-- Python 클라이언트 프로그램 작성
    +-- 키보드 입력으로 로봇 원격 제어
    |
[4단계] Unity AI 개발
    |
    +-- Unity AI, Sentis, Behavior 기능 개요
    +-- AI로 2D 슈팅게임 만들기
    +-- 3D 자동차 장난감 시뮬레이션
    +-- Unity와 ROS2 연결
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

## 파일 구조

```
unity_sim_example/
├── README.md                              <- 이 파일 (튜토리얼 인덱스)
├── 01_unity_installation.md               <- 1단계: Unity 설치 및 기초
├── 02_robot_creation.md                   <- 2단계: 로봇 제작
├── 03_python_connection.md                <- 3단계: Python 연결
└── 04_Unity_AI_Development_Tutorial.md    <- 4단계: Unity AI 개발
```

---

> **출처**: NVIDIA Omniverse Isaac Sim 튜토리얼 2.2 "Add Simple Objects"를 Unity에 맞게 번안 및 확장  
> **최종 업데이트**: 2025년 7월
