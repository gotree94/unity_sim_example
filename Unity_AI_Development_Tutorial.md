# Unity AI 개발 교육 자료 - AI로 게임과 로봇 시뮬레이션 만들기

> **대상**: Unity 초심자~중급자  
> **소요 시간**: 약 240 ~ 360분 (4 ~ 6시간)  
> **사용 환경**: Windows 10/11  
> **Unity 버전**: Unity 6.2 이상 (Unity AI 기능 포함)  
> **목표**: Unity의 최신 AI 기능을 활용하여 게임을 빠르게 개발하고, ROS와 연동하는 로봇 시뮬레이션을 구현

---

## 목차

1. [Unity AI 기능 개요](#1-unity-ai-기능-개요)
2. [프로젝트 1: AI로 2D 슈팅게임 만들기](#2-프로젝트-1-ai로-2d-슈팅게임-만들기)
3. [프로젝트 2: 가상 공간에서 자동차 장난감 이동](#3-프로젝트-2-가상-공간에서-자동차-장난감-이동)
4. [프로젝트 3: Unity와 ROS 연결하기](#4-프로젝트-3-unity와-ros-연결하기)

---

## 1. Unity AI 기능 개요

### 1-1. Unity에서 AI란?

Unity는 2023년부터 본격적으로 AI 기능을 에디터에 통합하기 시작했습니다. 2025년 현재 Unity에서 사용할 수 있는 AI 기능은 크게 세 가지 카테고리로 나뉩니다:

| 기능 | 설명 | 상태 |
|------|------|------|
| **Unity AI (구 Muse)** | 에디터 내 AI 어시스턴트, 에셋 생성, 코드 생성 | Unity 6.2+ 에서 사용 가능 |
| **Unity Sentis** | 런타임에서 AI 모델(ONNX)을 실행하는 추론 엔진 | 활성 유지 |
| **Unity Behavior** | 자연어로 NPC 행동 트리를 생성하는 AI | Unity 6.2+ 에서 사용 가능 |

### 1-2. Unity AI (Unity AI Assistant)

Unity AI는 에디터 안에서 바로 사용할 수 있는 AI 비서입니다. 이전의 "Unity Muse"가 통합된 제품입니다.

#### 주요 기능

| 기능 | 설명 | 사용 예시 |
|------|------|----------|
| **AI Assistant** | 프로젝트 컨텍스트를 이해하는 채팅 비서 | "이 오브젝트에 Rigidbody 추가해줘" |
| **Code Generation** | C# 코드를 자동 생성하고 검증 | "플레이어 이동 스크립트 만들어줘" |
| **AI Gateway** | 외부 AI 도구를 에디터에서 연결 | ChatGPT, Claude 등 연결 |
| **MCP Server** | IDE에서 Unity를 직접 제어 | VS Code에서 Unity 작업 |
| **Asset Generation** | 텍스트로 에셋 생성 | "파란색 구체 머티리얼 만들어줘" |

#### AI Assistant 사용법

1. Unity 에디터 상단 메뉴에서 **Window > Unity AI** 를 클릭합니다.
2. AI 패널이 열리면 자연어로 질문하거나 명령을 내립니다.

**예시 프롬프트:**
```
이 프로젝트의 구조를 설명해줘
```
```
Player 오브젝트에 이동 스크립트를 만들어줘. WASD로 이동하고 스페이스바로 점프하게
```
```
이 씬에 조명이 어두운데, 야간 분위기로 바꿔줘
```

### 1-3. Unity Sentis (런타임 AI)

Unity Sentis는 훈련된 AI 모델(ONNX 형식)을 Unity 게임 안에서 직접 실행할 수 있게 해주는 엔진입니다.

#### 특징
- **로컬 실행**: 클라우드 없이 기기에서 직접 AI 추론
- **크로스 플랫폼**: 모바일, PC, 콘솔, VR/AR 지원
- **Hugging Face 연동**: 수천 개의 사전 훈련된 모델 사용 가능

#### 활용 예시

| 사용 사례 | 설명 |
|----------|------|
| **스마트 NPC** | AI가 플레이어 행동을 분석하여 자연스럽게 반응 |
| **음성 인식** | 게임 안에서 실시간 음성→텍스트 변환 |
| **객체 감지** | 카메라 입력에서 물체를 자동으로 인식 |
| **게임 예측** | 승패를 예측하거나 난이도를 자동 조절 |

### 1-4. Unity Behavior (행동 트리 AI)

Unity Behavior는 텍스트 프롬프트로 NPC의 행동 트리를 자동 생성하는 도구입니다.

#### 동작 방식
1. 자연어로 NPC의 행동을 설명 (예: "플레이어를 발견하면 추격하고, 놓치면 순찰로 복귀")
2. AI가 행동 트리 그래프를 자동으로 생성
3. 각 노드에 필요한 코드를 자동 작성
4. 개발자는 결과를 검토하고 미세 조정

### 1-5. Unity AI 시작하기

#### 1단계: Unity 6.2 이상 설치

이전 튜토리얼에서 Unity를 설치했다면 이미 준비된 것입니다. Unity 6.2 이상이면 모든 AI 기능을 사용할 수 있습니다.

#### 2단계: Unity AI 활성화

1. Unity 에디터에서 **Window > Unity AI** 를 엽니다.
2. Unity 계정으로 로그인합니다.
3. **Beta 기능**에 참여하려면 https://unity.com/ai 에서 가입합니다.

#### 3단계: AI 크레딧 확인

Unity AI는 크레딧 기반 과금제입니다:
- **무료 크레딧**: Unity Personal 사용자에게 매월 기본 크레딧 제공
- **Unity Pro/Enterprise**: 더 많은 크레딧과 기능 제공

---

## 2. 프로젝트 1: AI로 2D 슈팅게임 만들기

> **목적**: Unity AI를 사용하여 2D 슈팅게임을 빠르게 프로토타이핑  
> **소요 시간**: 약 60~90분  
> **전제 조건**: Unity 6.2 이상 설치 완료

### 2-1. 프로젝트 개요

이 프로젝트에서는 Unity AI를 활용하여 다음을 구현합니다:
- 2D 플레이어 캐릭터 (우주선)
- 총알 발사 시스템
- 적 캐릭터 AI
- 점수 시스템
- 배경 및 이펙트

### 2-2. 새 프로젝트 만들기

1. Unity Hub에서 **New Project** 를 클릭합니다.
2. 템플릿에서 **"2D Core"** 를 선택합니다.
3. 프로젝트 이름을 **"SpaceShooterAI"** 로 입력합니다.
4. **Create project** 를 클릭합니다.

### 2-3. AI Assistant로 기본 구조 만들기

Unity AI 패널을 열고 다음 프롬프트를 순서대로 입력합니다:

#### 프롬프트 1: 기본 장면 구조

```
2D 우주 슈팅게임의 기본 장면을 만들어줘. 다음 요소들을 포함해줘:
1. 플레이어 우주선 (화면 하단에서 좌우로 이동)
2. 적 우주선 (화면 상단에서 랜덤 위치에 생성)
3. 총알 발사 기능
4. 카메라와 배경
```

AI가 생성한 내용을 검토하고 적용합니다.

#### 프롬프트 2: 플레이어 이동 스크립트

```
PlayerMovement.cs 스크립트를 만들어줘:
- WASD 또는 화살표 키로 좌우 이동
- 화면 밖으로 나가지 않도록 제한
- 이동 속도는 5f
```

#### 프롬프트 3: 총알 발사 시스템

```
Bullet.cs와 Shooting.cs 스크립트를 만들어줘:
- 스페이스바를 누르면 총알이 발사됨
- 총알은 위쪽으로 직선 이동
- 총알이 화면 밖으로 나가면 자동 파괴
- 발사 간격 제한 (연사 속도 조절)
```

#### 프롬프트 4: 적 AI

```
Enemy.cs 스크립트를 만들어줘:
- 적이 아래쪽으로 천천히 이동
- 화면 밖으로 나가면 자동 파괴
- 플레이어 총알에 맞으면 파괴
- 랜덤한 이동 패턴 추가 (좌우로 살짝 움직임)
```

#### 프롬프트 5: 점수 시스템

```
ScoreManager.cs 스크립트를 만들어줘:
- 적을 파괴하면 점수 10점 증가
- UI Text로 현재 점수 표시
- 화면 상단에 표시
```

### 2-4. AI가 생성한 코드 검토 및 수정

AI가 생성한 코드를 검토하는 것은 중요합니다. 다음 사항을 확인합니다:

| 확인 사항 | 체크 포인트 |
|----------|------------|
| **네임스페이스** | 같은 네임스페이스를 사용하는지 |
| **변수명** | 의미가 명확한 이름인지 |
| **오류 처리** | Null 체크 등이 되어 있는지 |
| **성능** | Update() 안에서 무거운 연산이 없는지 |
| **유지보수** | 주석이 적절히 있는지 |

#### 수동 수정 예시

AI가 만든 코드를 개선하는 예시:

```csharp
// AI가 생성한 코드 (원본)
void Update()
{
    transform.Translate(Vector2.left * speed * Time.deltaTime);
}

// 개선한 코드 (화면 밖 제거 + 부모 자식 관계 고려)
void Update()
{
    transform.Translate(Vector2.left * speed * Time.deltaTime);
    
    // 화면 밖으로 나가면 파괴
    if (transform.position.y < -6f)
    {
        Destroy(gameObject);
    }
}
```

### 2-5. 프리팹 만들기

AI가 생성한 게임 오브젝트들을 프리팹으로 만듭니다:

1. Hierarchy에서 **Player** 오브젝트를 선택합니다.
2. Project 창의 **Assets > Prefabs** 폴더로 드래그합니다.
3. 같은 방법으로 **Enemy**, **Bullet** 프리팹을 만듭니다.

### 2-6. 적 스포너 만들기

AI에게 추가 스크립트를 요청합니다:

```
EnemySpawner.cs 스크립트를 만들어줘:
- 2초마다 적이 랜덤 위치에 생성
- 적이 점점 빨라지는 난이도 시스템
- 최대 10마리까지만 동시에 존재
```

### 2-7. 최종 테스트

1. **▶ (Play)** 버튼을 클릭합니다.
2. WASD로 플레이어를 움직입니다.
3. 스페이스바로 총알을 발사합니다.
4. 적을 파괴하며 점수가 올라가는지 확인합니다.

### 2-8. 프로젝트 1 정리

이 프로젝트를 통해 배운 것:
- Unity AI Assistant로 프롬프트 기반 코드 생성
- AI가 만든 코드를 검토하고 수정하는 과정
- 2D 게임의 기본 구조 (플레이어, 적, 총알, UI)

---

## 3. 프로젝트 2: 가상 공간에서 자동차 장난감 이동

> **목적**: 3D 환경에서 자동차 장난감을 AI를 활용하여 구현  
> **소요 시간**: 약 90~120분  
> **전제 조건**: 프로젝트 1 완료 또는 3D Unity 기초 이해

### 3-1. 프로젝트 개요

이 프로젝트에서는 다음을 구현합니다:
- 3D 가상 공간 (바닥, 벽, 장애물)
- 자동차 장난감 모델
- 물리 엔진을 이용한 이동
- AI를 이용한 자동 주행 모드
- 수동 조작 모드 (WASD)

### 3-2. 새 프로젝트 만들기

1. Unity Hub에서 **New Project** 를 클릭합니다.
2. 템플릿에서 **"3D (URP)"** 를 선택합니다.
3. 프로젝트 이름을 **"ToyCarSimulator"** 로 입력합니다.

### 3-3. 기본 환경 만들기

AI Assistant에게 다음을 요청합니다:

#### 프롬프트 1: 기본 환경

```
3D 자동차 시뮬레이션 환경을 만들어줘:
1. 넓은 바닥 (Plane, 스케일 10x1x10)
2. 바닥 테두리 벽 (4방향)
3. 중앙에 장애물 (큐 몇 개)
4. 태양 조명
5. 카메라가 위에서 내려다보는 구도
```

#### 프롬프트 2: 자동차 기본 구조

```
장난감 자동차를 만들어줘:
1. 몸통은 캡슐형 (Capsule의 스케임 조절)
2. 바퀴 4개 (Cylinder)
3. 바퀴는 몸통의 4구석에 배치
4. 빨간색 머티리얼 적용
```

### 3-4. 자동차 이동 스크립트

AI에게 자동차 제어 스크립트를 요청합니다:

```
ToyCarController.cs 스크립트를 만들어줘:
- WASD로 전진/후진/좌회전/우회전
- 물리 엔진 사용 (Rigidbody)
- 바퀴 회전 시각 효과
- 최대 속도 제한
- 마찰력으로 인한 자연스러운 감속
```

### 3-5. AI 자동 주행 시스템

이제 AI를 이용한 자동 주행 기능을 추가합니다:

#### 프롬프트: 자동 주행 AI

```
AutoDriveAI.cs 스크립트를 만들어줘:
- 장애물을 감지하여 회피하는 자동 주행
- 전방 레이캐스트로 장애물 감지
- 장애물이 없으면 전진
- 장애물이 있으면 좌우 중 하나를 선택하여 회피
- 벽에 가까워지면 반대 방향으로 회전
- 자동/수동 모드 전환 기능 (T키)
```

### 3-6. AI가 생성한 자동 주행 로직 분석

AI가 생성한 자동 주행 스크립트의 핵심 로직을 이해합니다:

```
[자동 주행 동작 흐름]

1. 전방에 레이캐스트 발사
   |
   v
2. 장애물 감지 여부 확인
   |
   +---> 장애물 없음 ---> 전진
   |
   +---> 장애물 있음 ---> 회피 동작 실행
                          |
                          +---> 좌회전 또는 우회전 선택
                          |
                          +---> 일정 시간 후 다시 전진 시도
```

#### 핵심 코드 설명

```csharp
// 전방 감지
RaycastHit hit;
bool isObstacleAhead = Physics.Raycast(
    transform.position, 
    transform.forward, 
    out hit, 
    detectionDistance
);

if (isObstacleAhead)
{
    // 장애물 회피: 랜덤으로 좌/우 선택
    float avoidDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
    transform.Rotate(Vector3.up, avoidDirection * turnSpeed * Time.deltaTime);
}
else
{
    // 전진
    transform.Translate(Vector3.forward * speed * Time.deltaTime);
}
```

### 3-7. 카메라 추적 시스템

```
CameraFollow.cs 스크립트를 만들어줘:
- 자동차를 따라다니는 카메라
- 자동차 뒤쪽 위쪽에서 내려다보는 각도
- 부드러운 따라가기 (Lerp)
- 거리와 높이 조절 가능
```

### 3-8. UI 및 상태 표시

```
GameUI.cs 스크립트를 만들어줘:
- 현재 속도 표시
- 자동/수동 모드 표시
- 장애물 감지 상태 표시
- T키로 모드 전환 안내
```

### 3-9. 머티리얼 및 시각 효과

AI를 활용하여 자동차에 색상을 입힙니다:

```
다음 머티리얼들을 만들어줘:
1. CarBody: 빨간색, 약간의 메탈릭
2. Wheel: 검은색, 매트
3. Ground: 밝은 회색, 거친 질감
4. Wall: 진한 회색
5. Obstacle: 노란색
```

### 3-10. 최종 구조

```
ToyCarSimulator Scene
├── Environment
│   ├── Ground (Plane)
│   ├── Walls (4개 벽)
│   └── Obstacles (장애물들)
├── ToyCar
│   ├── Body (Capsule)
│   ├── Wheel_FL (Cylinder)
│   ├── Wheel_FR (Cylinder)
│   ├── Wheel_RL (Cylinder)
│   └── Wheel_RR (Cylinder)
├── Main Camera
├── Directional Light
└── UIManager (Canvas)
```

### 3-11. 최종 테스트

1. **▶ (Play)** 버튼을 클릭합니다.
2. WASD로 자동차를 수동 조작합니다.
3. **T** 키를 눌러 자동 주행 모드로 전환합니다.
4. AI가 장애물을 회피하며 주행하는지 확인합니다.
5. 다시 **T** 키로 수동 모드로 전환합니다.

---

## 4. 프로젝트 3: Unity와 ROS 연결하기

> **목적**: Unity에서 만든 로봇 시뮬레이션을 실제 ROS와 통신  
> **소요 시간**: 약 90~150분  
> **전제 조건**: 프로젝트 2 완료, ROS 기본 이해 (선택)

### 4-1. ROS란?

ROS(Robot Operating System)는 로봇 소프트웨어 개발을 위한 오픈소스 프레임워크입니다. Unity와 연결하면:
- Unity에서 만든 3D 시뮬레이션을 ROS에서 제어
- ROS에서 처리된 센서 데이터를 Unity에서 시각화
- 실제 로봇과 동일한 환경에서 알고리즘 테스트

### 4-2. 연결 방식 개요

```
┌──────────────────┐                    ┌──────────────────┐
│                  │    TCP/IP (10000)   │                  │
│     Unity        │ <=================> │     ROS2         │
│                  │                    │                  │
│  - 3D 시뮬레이션  │    메시지 전송       │  - 로봇 제어      │
│  - 시각화         │                    │  - 센서 처리      │
│  - 물리 엔진      │                    │  - SLAM/_nav2     │
│                  │                    │                  │
└──────────────────┘                    └──────────────────┘
```

### 4-3. Unity 측 설정

#### 1단계: ROS TCP Connector 패키지 설치

1. Unity 에디터에서 **Window > Package Manager** 를 엽니다.
2. 상단의 **"+"** 버튼을 클릭합니다.
3. **"Add package from git URL..."** 을 선택합니다.
4. 다음 URL을 입력합니다:
```
https://github.com/Unity-Technologies/ROS-TCP-Connector.git?path=/com.unity.robotics.ros-tcp-connector
```
5. **Add** 를 클릭하여 설치합니다.

#### 2단계: ROS Settings 구성

1. Unity 상단 메뉴에서 **Robotics > ROS Settings** 를 엽니다.
2. 다음 값을 설정합니다:

| 설정 | 값 | 설명 |
|------|---|------|
| **ROS IP Address** | `127.0.0.1` | ROS가 같은 컴퓨터에서 실행될 때 |
| **Host Port** | `10000` | 통신 포트 |
| **Protocol** | **ROS2** | ROS2 사용 시 |

> 💡 **팁**: ROS가 다른 컴퓨터에서 실행되면 해당 컴퓨터의 IP 주소를 입력합니다.

#### 3단계: ROSConnection 스크립트 추가

1. Hierarchy에서 빈 오브젝트를 만들고 이름을 **"ROSManager"** 로 변경합니다.
2. **Add Component > ROSConnection** 을 추가합니다.
3. ROS Settings에서 설정한 값이 자동으로 적용됩니다.

### 4-4. ROS 메시지 생성

Unity에서 ROS 메시지를 사용하려면 C# 스크립트로 변환해야 합니다.

#### 1단계: 메시지 브라우저 열기

1. Unity 상단 메뉴에서 **Robotics > Generate ROS Messages...** 를 클릭합니다.
2. Message Browser 창이 열립니다.

#### 2단계: 표준 메시지 빌드

1. Message Browser에서 **"Build 2 msgs"** 를 클릭합니다 (Std 메시지용).
2. 기본 메시지들(String, Float32, Pose 등)이 자동으로 생성됩니다.

### 4-5. Unity에서 ROS로 데이터 보내기 (Publish)

#### 예제: 자동차 위치 정보 전송

```
ROSCarPublisher.cs 스크립트를 만들어줘:
- 자동차의 현재 위치와 회전 정보를 ROS로 전송
- topic 이름: "/car_pose"
- 메시지 타입: geometry_msgs/PoseStamped
- 1초마다 10회 전송 (10Hz)
```

#### 생성된 스크립트 핵심 코드

```csharp
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class ROSCarPublisher : MonoBehaviour
{
    private ROSConnection ros;
    
    [SerializeField]
    private string topicName = "/car_pose";
    
    private float publishFrequency = 0.1f; // 10Hz
    private float timer = 0f;

    void Start()
    {
        // ROS 연결
        ros = ROSConnection.GetOrCreateInstance();
        
        // Publisher 등록
        ros.RegisterPublisher<PoseStampedMsg>(topicName);
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= publishFrequency)
        {
            PublishCarPose();
            timer = 0f;
        }
    }

    void PublishCarPose()
    {
        PoseStampedMsg poseMsg = new PoseStampedMsg();
        
        // 위치 설정
        poseMsg.pose.position.x = transform.position.x;
        poseMsg.pose.position.y = transform.position.y;
        poseMsg.pose.position.z = transform.position.z;
        
        // 회전 설정
        poseMsg.pose.orientation.x = transform.rotation.x;
        poseMsg.pose.orientation.y = transform.rotation.y;
        poseMsg.pose.orientation.z = transform.rotation.z;
        poseMsg.pose.orientation.w = transform.rotation.w;
        
        ros.Publish(topicName, poseMsg);
        
        Debug.Log($"[ROS] 자동차 위치 전송: {transform.position}");
    }
}
```

### 4-6. Unity에서 ROS 데이터 받기 (Subscribe)

#### 예제: ROS에서 제어 명령 수신

```
ROSCarSubscriber.cs 스크립트를 만들어줘:
- ROS에서 "/cmd_vel" 토픽으로 속도 명령 수신
- 메시지 타입: geometry_msgs/Twist
- 수신한 값으로 자동차를 움직이기
```

#### 생성된 스크립트 핵심 코드

```csharp
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class ROSCarSubscriber : MonoBehaviour
{
    private ROSConnection ros;
    
    [SerializeField]
    private string topicName = "/cmd_vel";
    
    private float linearSpeed = 0f;
    private float angularSpeed = 0f;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        
        // Subscriber 등록
        ros.Subscribe<TwistMsg>(topicName, ReceiveVelocity);
    }

    void ReceiveVelocity(TwistMsg msg)
    {
        // ⚠️ 주의: 이 콜백은 별도 스레드에서 실행됨
        linearSpeed = (float)msg.linear.x;
        angularSpeed = (float)msg.angular.z;
        
        Debug.Log($"[ROS] 수신 - 전진: {linearSpeed}, 회전: {angularSpeed}");
    }

    void FixedUpdate()
    {
        // 메인 스레드에서 Unity 작업 수행
        float moveDistance = linearSpeed * Time.fixedDeltaTime;
        float turnAngle = angularSpeed * Time.fixedDeltaTime;
        
        transform.Translate(Vector3.forward * moveDistance);
        transform.Rotate(Vector3.up * turnAngle);
    }
}
```

### 4-7. ROS2 측 설정 (Ubuntu)

> 이 섹션은 ROS2를 사용하는 경우에만 필요합니다. ROS 없이 Unity만으로도 충분히 시뮬레이션 가능합니다.

#### 1단계: ROS2 설치 (Docker 사용 권장)

```bash
# Docker로 ROS2 Humble 실행
docker run -it --rm -p 10000:10000 ros:humble bash
```

#### 2단계: ROS TCP Endpoint 설치

```bash
# Docker 컨테이너 안에서
cd ~/colcon_ws/src
git clone https://github.com/Unity-Technologies/ROS-TCP-Endpoint.git
cd ~/colcon_ws
colcon build --packages-select ros_tcp_endpoint
source install/setup.bash
```

#### 3단계: TCP Endpoint 서버 시작

```bash
ros2 run ros_tcp_endpoint default_server_endpoint \
    --ros-args -p ROS_IP:=0.0.0.0 -p ROS_TCP_PORT:=10000
```

서버가 시작되면 다음 메시지가 표시됩니다:
```
[INFO] [server_endpoint]: Starting server on 0.0.0.0:10000
```

### 4-8. 연결 테스트

#### Unity 측 테스트

1. Unity에서 **▶ (Play)** 버튼을 클릭합니다.
2. Unity 콘솔에 `[ROS] Connection established` 메시지가 나타나면 연결 성공입니다.
3. Scene 뷰에서 ROS 연결 아이콘이 파란색으로 표시됩니다.

#### ROS 측 테스트

Unity에서 보낸 메시지를 확인합니다:

```bash
# Unity가 보낸 메시지 수신
ros2 topic echo /car_pose
```

Unity로 메시지를 보냅니다:

```bash
# Unity에 속도 명령 전송
ros2 topic pub /cmd_vel geometry_msgs/msg/Twist \
    "{linear: {x: 0.5}, angular: {z: 0.3}}" -1
```

### 4-9. URDF 로봇 모델 가져오기 (선택)

Unity에서 실제 로봇 모델(URDF)을 가져올 수 있습니다.

#### URDF Importer 패키지 설치

1. Package Manager에서 **"+" > Add package from git URL** 을 클릭합니다.
2. 다음 URL을 입력합니다:
```
https://github.com/Unity-Technologies/URDF-Importer.git?path=/com.unity.robotics.urdf-importer
```

#### URDF 파일 가져오기

1. Unity 상단 메뉴에서 **Robotics > Import Robot from URDF** 를 클릭합니다.
2. URDF 파일(.xacro 또는 .urdf)을 선택합니다.
3. 로봇 모델이 Unity 씬에 자동으로 생성됩니다.

### 4-10. 전체 연결 구조도

```
┌─────────────────────────────────────────────────────────────┐
│                        Unity                                │
│                                                             │
│  ┌─────────────┐   ┌─────────────┐   ┌─────────────────┐   │
│  │  3D 환경     │   │  자동차/로봇 │   │  ROS Manager    │   │
│  │  (Scene)    │   │  (Physics)  │   │  (TCP Client)   │   │
│  └─────────────┘   └─────────────┘   └─────────────────┘   │
│                                              │              │
│  ┌──────────────────────────────────────────┐│              │
│  │  ROSCarPublisher / ROSCarSubscriber      ││              │
│  │  (데이터 전송/수신)                        ││              │
│  └──────────────────────────────────────────┘│              │
└──────────────────────────────────────────────┼──────────────┘
                                               │
                                        TCP/IP (10000)
                                               │
┌──────────────────────────────────────────────┼──────────────┐
│                        ROS2                   │              │
│  ┌──────────────────────────────────────────┐│              │
│  │  ROS TCP Endpoint Server                 ││              │
│  │  (메시지 라우팅)                           │◄┘              │
│  └──────────────────────────────────────────┘               │
│           │                  │                               │
│  ┌────────▼─────┐   ┌───────▼────────┐                     │
│  │ Publisher    │   │ Subscriber     │                     │
│  │ (데이터 전송) │   │ (데이터 수신)   │                     │
│  └──────────────┘   └────────────────┘                     │
│                                                             │
│  주요 토픽:                                                  │
│  /car_pose    : 자동차 위치 (Unity -> ROS)                   │
│  /cmd_vel     : 속도 명령 (ROS -> Unity)                     │
│  /sensor_data : 센서 데이터 (ROS -> Unity)                   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 4-11. 자주 사용하는 ROS 메시지 타입

| 메시지 타입 | 용도 | Unity에서의 역할 |
|------------|------|-----------------|
| `geometry_msgs/Twist` | 속도 명령 (선형/각속도) | 로봇 전진/회전 제어 |
| `geometry_msgs/PoseStamped` | 위치+회전 정보 | 로봇 위치 전송 |
| `geometry_msgs/Point` | 3D 좌표 | 특정 점 전송 |
| `sensor_msgs/LaserScan` | 라이다 데이터 | 장애물 감지 시뮬레이션 |
| `sensor_msgs/Image` | 카메라 이미지 | 비전 처리 |
| `std_msgs/String` | 문자열 | 일반적인 텍스트 데이터 |
| `std_msgs/Float32` | 실수 값 | 센서 값 전송 |

### 4-12. 문제 해결 FAQ

#### Q1: Unity에서 "Waiting for ROS connection..." 메시지가 계속 나옴

| 확인 사항 | 해결 방법 |
|----------|----------|
| ROS TCP Endpoint 실행 여부 | ROS 터미널에서 서버가 실행 중인지 확인 |
| IP 주소 일치 여부 | Unity의 ROS IP Address와 ROS의 ROS_IP가 같은지 |
| 포트 번호 일치 | 양쪽 모두 포트 10000 사용하는지 |
| 방화벽 | Windows/Linux 방화벽에서 포트 10000 허용 |

#### Q2: 메시지가 도착하지 않음

```bash
# ROS에서 토픽 목록 확인
ros2 topic list

# Unity가 보낸 메시지 확인
ros2 topic echo /car_pose

# 토픽 정보 확인 (메시지 타입, 발행자 수)
ros2 topic info /car_pose
```

#### Q3: "Type mismatch" 오류

Unity에서 생성한 메시지와 ROS의 메시지 타입이 다른 경우 발생합니다. Unity의 **Robotics > Generate ROS Messages** 를 다시 실행하여 메시지를 재생성합니다.

#### Q4: 연결은 되었는데 자동차가 움직이지 않음

Unity의 ROSCarSubscriber 스크립트가 올바르게 붙어있는지, ROSConnection 컴포넌트가 있는지 확인합니다.

---

## 요약: Unity AI 개발 로드맵

```
[단계 1] Unity AI 기초
    |
    +-- AI Assistant로 코드 생성
    +-- 프롬프트 엔지니어링 학습
    +-- 생성된 코드 검토/수정 능력
    |
[단계 2] 게임 프로토타이핑
    |
    +-- 2D/3D 게임 빠르게 만들기
    +-- AI로 에셋/스크립트 자동 생성
    +-- 반복적인 개발 과정 체득
    |
[단계 3] 런타임 AI (Unity Sentis)
    |
    +-- ONNX 모델 Unity에서 실행
    +-- 스마트 NPC / 객체 감지
    +-- Hugging Face 모델 활용
    |
[단계 4] 로봇 시뮬레이션
    |
    +-- 3D 환경에서 로봇 제어
    +-- ROS와 TCP 통신
    +-- URDF 모델 임포트
    |
[단계 5] 실제 로봇 연동
    |
    +-- ROS2 알고리즘 개발
    +-- Unity에서 시뮬레이션 테스트
    +-- 실제 로봇에 배포
```

---

## 참고 자료

| 자료 | URL |
|------|-----|
| Unity AI 공식 문서 | https://docs.unity3d.com/6000.0/Documentation/Manual/unity-ai.html |
| Unity Sentis 문서 | https://docs.unity3d.com/Packages/com.unity.sentis@latest |
| Unity Robotics Hub | https://github.com/Unity-Technologies/Unity-Robotics-Hub |
| ROS TCP Connector | https://github.com/Unity-Technologies/ROS-TCP-Connector |
| URDF Importer | https://github.com/Unity-Technologies/URDF-Importer |
| Hugging Face (Unity 모델) | https://huggingface.co/unity |

---

> **저작권**: 본 교육 자료는 교육 목적으로 자유롭게 사용할 수 있습니다.  
> **최종 업데이트**: 2025년 7월
