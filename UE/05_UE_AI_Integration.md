# 5단계: Unity AI 기능 대응 UE 기능

## 5-1. AI 기능 비교 개요

### Unity AI vs UE AI 비교표
| 기능 | Unity AI | Unreal Engine 5 |
|------|----------|-----------------|
| **NPC 행동 시스템** | Unity Behavior (자연어 기반) | Behavior Trees + EQS |
| **지능형 NPC** | Unity AI + Behavior | AI Controller + Blackboard |
| **머신러닝 추론** | Unity Sentis (ONNX) | Neural Network Inference Plugin |
| **물리 기반 애니메이션** | Unity 6.2+ Animation Rigging | ML Deformer |
| **오디오 AI** | Unity Audio (기본) | MetaSounds (프로시저RAL) |
| **에이전트 학습** | Unity ML-Agents | Game AI Pro (커뮤니티) |
| **내비게이션** | NavMesh (Baked) | Navigation System (Dynamic) |
| **Sense 시스템** | 없음 (직접 구현) | AI Perception (시각/청각/촉각) |
| **시나리오 테스트** | 없음 | EQS (Environment Query System) |

---

## 5-2. NPC 행동 시스템

### Unity Behavior vs UE Behavior Trees

#### Unity Behavior (Unity AI)
- 자연어 명령으로 NPC 행동 트리 생성
- 예: "플레이어를 감지하면 추적하고, 공격 범위에 있으면 공격"
- AI가 자동으로 행동 트리 구조 생성

#### UE Behavior Trees
- 비주얼 에디터에서 행동 트리 설계
- **Blackboard**: NPC 상태 저장소
- **Decorator**: 조건 검사 (예: 플레이어 거리)
- **Task**: 행동 실행 (예: 이동, 공격)
- **Service**: 주기적 업데이트 (예: 주변 탐색)

### UE Behavior Tree 구현 예시
```
Root
├── Selector
│   ├── Sequence [플레이어 감지]
│   │   ├── Decorator: Is Player Visible?
│   │   ├── Task: Move To Player
│   │   └── Task: Attack Player
│   └── Sequence [순찰]
│       ├── Task: Get Next Patrol Point
│       └── Task: Move To Patrol Point
```

### Unity → UE 전환 가이드
| Unity Behavior Concepts | UE Behavior Tree Concepts |
|------------------------|---------------------------|
| 자연어 명령 | 수동 트리 설계 |
| 자동 생성 | 수동 생성 |
| 간단한 설정 | 복잡하지만 강력한 설정 |
| 프로토타이핑 적합 | 프로덕션 적합 |

---

## 5-3. 지능형 NPC 시스템

### Unity AI vs UE AI Controller

#### Unity AI
- Unity AI + Behavior 조합
- 자연어로 NPC 지능 구현
- 간단한 설정으로 빠른 개발

#### UE AI Controller
- **AI Controller**: NPC 제어
- **Blackboard**: NPC 상태 관리
- **Behavior Trees**: 행동 결정
- **Perception System**: 환경 인식

### UE AI Controller 구현
```cpp
// AIController.h
UCLASS()
class ROBOTSIMULATION_API AAIController : public AAIController
{
    GENERATED_BODY()

public:
    AAIController();

protected:
    virtual void BeginPlay() override;
    virtual void Tick(float DeltaTime) override;

    // Blackboard 데이터 설정
    UFUNCTION(BlueprintCallable, Category = "AI")
    void SetupBlackboard();

    // 플레이어 감지
    UFUNCTION(BlueprintCallable, Category = "AI")
    void DetectPlayer();

    // 행동 결정
    UFUNCTION(BlueprintCallable, Category = "AI")
    void MakeDecision();

private:
    UBlackboardComponent* BlackboardComp;
    UBehaviorTreeComponent* BehaviorTreeComp;
    
    // 감지 범위
    float DetectionRange = 2000.0f;
    float AttackRange = 500.0f;
};
```

### Blackboard 설정
1. Content Browser → 우클릭 → Artificial Intelligence → Blackboard
2. 이름: `BB_RobotAI`
3. 키 추가:
   - `TargetLocation` (Vector): 목표 위치
   - `IsPlayerVisible` (Bool): 플레이어 가시성
   - `CurrentState` (Enum): 현재 상태
   - `LastKnownPlayerLocation` (Vector): 마지막 플레이어 위치

---

## 5-4. 머신러닝 추론

### Unity Sentis vs UE Neural Network Inference

#### Unity Sentis
- ONNX 모델 지원
- 런타임에서 추론
- C# API로 간편한 사용
- 모델 포맷: ONNX

#### UE Neural Network Inference Plugin
- ONNX Runtime 사용
- C++ API
- 더 높은 성능
- 모델 포맷: ONNX

### UE에서 ONNX 모델 사용
```cpp
// NeuralNetworkInferenceComponent.h
UCLASS(ClassGroup=(Custom), meta=(BlueprintSpawnableComponent))
class ROBOTSIMULATION_API UNeuralNetworkInferenceComponent : public UActorComponent
{
    GENERATED_BODY()

public:
    UNeuralNetworkInferenceComponent();

    // ONNX 모델 경로
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "AI")
    FString ModelPath;

    // 추론 실행
    UFUNCTION(BlueprintCallable, Category = "AI")
    TArray<float> RunInference(const TArray<float>& InputData);

    // 모델 로드
    UFUNCTION(BlueprintCallable, Category = "AI")
    bool LoadModel();

private:
    // ONNX Runtime 세션 (C++ 구현)
    void* InferenceSession;
    
    // 전처리
    TArray<float> PreprocessInput(const TArray<float>& RawInput);
    
    // 후처리
    TArray<float> PostprocessOutput(const TArray<float>& RawOutput);
};
```

### 모델 통합 절차
1. ONNX 모델 준비 (Python에서 학습 후 내보내기)
2. UE5 프로젝트에 모델 파일 추가
3. Neural Network Inference Plugin 활성화
4. 컴포넌트에 모델 경로 설정
5. 추론 실행 및 결과 처리

---

## 5-5. 물리 기반 애니메이션

### Unity Animation Rigging vs UE ML Deformer

#### Unity Animation Rigging
- Unity 6.2+에서 지원
- 물리 기반 애니메이션
- 실시간 리깅 시스템

#### UE ML Deformer
- 머신러닝 기반 메시 변형
- 물리 시뮬레이션 결과를 애니메이션에 적용
- 고품질 물리 표현

### ML Deformer 구현
```cpp
// MLDeformerComponent.h
UCLASS(ClassGroup=(Custom), meta=(BlueprintSpawnableComponent))
class ROBOTSIMULATION_API UMLDeformerComponent : public UActorComponent
{
    GENERATED_BODY()

public:
    UMLDeformerComponent();

    // ML 모델 경로
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "ML")
    FString ModelPath;

    // 물리 시뮬레이션 결과
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "ML")
    TArray<FVector> PhysicsSimulationResult;

    // ML 추론 실행
    UFUNCTION(BlueprintCallable, Category = "ML")
    void RunMLInference();

    // 메시 변형 적용
    UFUNCTION(BlueprintCallable, Category = "ML")
    void ApplyDeformation();

private:
    UStaticMeshComponent* TargetMesh;
    UNeuralNetworkInferenceComponent* InferenceComponent;
    
    // 물리 시뮬레이션 데이터 수집
    void CollectPhysicsData();
    
    // 변형 매핑
    void MapDeformationToMesh(const TArray<float>& MLOutput);
};
```

---

## 5-6. 오디오 AI

### Unity Audio vs UE MetaSounds

#### Unity Audio
- 기본 오디오 시스템
- 제한된 프로시저RAL 기능

#### UE MetaSounds
- 프로시저RAL 오디오 생성
- 실시간 오디오 합성
- AI 기반 오디오 처리

### MetaSounds 구현 예시
```
MetaSound Graph
├── Input: RobotEngine
│   ├── RPM (Float)
│   ├── Load (Float)
│   └── Temperature (Float)
├── Processing
│   ├── Engine Sound Generator
│   ├── Filter: Low Pass
│   └── Modulation: RPM-based
└── Output: Audio Output
```

### 오디오 AI 활용
- 로봇 엔진 소리 시뮬레이션
- 환경 소리 프로시저RAL 생성
- 음성 인식/합성 통합
- AI 기반 사운드 이펙트

---

## 5-7. 내비게이션 시스템

### Unity NavMesh vs UE Navigation System

#### Unity NavMesh
- Bake 기반 내비게이션
- 정적 환경에 적합
- 제한적인 동적 네비게이션

#### UE Navigation System
- 동적 내비게이션
- 런타임 메시 생성
- 복잡한 환경 지원

### UE Navigation System 구현
```cpp
// NavigationComponent.h
UCLASS(ClassGroup=(Custom), meta=(BlueprintSpawnableComponent))
class ROBOTSIMULATION_API UNavigationComponent : public UActorComponent
{
    GENERATED_BODY()

public:
    UNavigationComponent();

    // 목표 위치로 이동
    UFUNCTION(BlueprintCallable, Category = "Navigation")
    void MoveToLocation(FVector TargetLocation);

    // 경로 생성
    UFUNCTION(BlueprintCallable, Category = "Navigation")
    TArray<FVector> FindPath(FVector Start, FVector End);

    // 장애물 회피
    UFUNCTION(BlueprintCallable, Category = "Navigation")
    FVector AvoidObstacles(FVector CurrentDirection);

private:
    UNavigationSystemV1* NavSystem;
    TArray<FVector> CurrentPath;
    int32 CurrentPathIndex;
    
    // 경로 업데이트
    void UpdatePath();
    
    // 다음 경로 포인트로 이동
    void MoveToNextPathPoint();
};
```

---

## 5-8. Sense 시스템

### Unity (직접 구현) vs UE AI Perception

#### Unity
- Sense 시스템 없음
- 레이캐스팅으로 직접 구현
- 유연하지만 개발 비용 높음

#### UE AI Perception
- 시각 (Sight)
- 청각 (Hearing)
- 촉각 (Touch)
- 감지 (Damage)
- 커스텀 감지

### UE AI Perception 구현
```cpp
// AIPerceptionComponent.h
UCLASS(ClassGroup=(Custom), meta=(BlueprintSpawnableComponent))
class ROBOTSIMULATION_API UAIPerceptionComponent : public UActorComponent
{
    GENERATED_BODY()

public:
    UAIPerceptionComponent();

    // 감지 범위 설정
    UFUNCTION(BlueprintCallable, Category = "Perception")
    void SetupPerception(float SightRange, float HearingRange);

    // 감지된 아クト 목록
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Perception")
    TArray<AActor*> DetectedActors;

    // 감지 이벤트
    UFUNCTION(BlueprintCallable, Category = "Perception")
    void OnActorDetected(AActor* Actor, float Distance);

    // 감지 해제 이벤트
    UFUNCTION(BlueprintCallable, Category = "Perception")
    void OnActorLost(AActor* Actor);

private:
    // 시각 감지
    void CheckSight();
    
    // 청각 감지
    void CheckHearing();
    
    // 감지 범위 내 아クト 확인
    TArray<AActor*> GetActorsInRange(float Range);
};
```

---

## 5-9. 통합 예제

### 로봇 AI 시스템 통합
```cpp
// RobotAIController.h
UCLASS()
class ROBOTSIMULATION_API ARobotAIController : public AAIController
{
    GENERATED_BODY()

public:
    ARobotAIController();

protected:
    virtual void BeginPlay() override;
    virtual void Tick(float DeltaTime) override;

    // AI 컴포넌트
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "AI")
    UBehaviorTreeComponent* BehaviorTreeComp;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "AI")
    UBlackboardComponent* BlackboardComp;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "AI")
    UNavigationComponent* NavigationComp;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "AI")
    UAIPerceptionComponent* PerceptionComp;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "AI")
    UNeuralNetworkInferenceComponent* InferenceComp;

private:
    // AI 초기화
    void InitializeAI();
    
    // 행동 결정
    void MakeDecision();
    
    // 상태 업데이트
    void UpdateState();
};
```

### Python AI 통합
```python
# robot_ai_controller.py
import json
import numpy as np
from typing import Dict, List

class RobotAIController:
    def __init__(self):
        self.state = {
            'position': [0, 0, 0],
            'rotation': [0, 0, 0],
            'velocity': [0, 0, 0],
            'detected_objects': [],
            'current_action': 'idle'
        }
        
    def process_sensor_data(self, sensor_data: Dict) -> Dict:
        """센서 데이터 처리 및 AI 결정"""
        # 감지된 객체 분석
        detected = sensor_data.get('detected_objects', [])
        self.state['detected_objects'] = detected
        
        # 행동 결정
        action = self.decide_action()
        self.state['current_action'] = action
        
        # 제어 명령 생성
        command = self.generate_command(action)
        
        return {
            'type': 'robot_control',
            'action': action,
            'command': command,
            'state': self.state
        }
    
    def decide_action(self) -> str:
        """AI 행동 결정"""
        detected = self.state['detected_objects']
        
        if not detected:
            return 'patrol'
        
        # 가장 가까운 객체 확인
        nearest = min(detected, key=lambda x: x.get('distance', float('inf')))
        
        if nearest.get('type') == 'obstacle':
            return 'avoid'
        elif nearest.get('type') == 'target':
            return 'pursue'
        else:
            return 'investigate'
    
    def generate_command(self, action: str) -> Dict:
        """행동에 따른 명령 생성"""
        commands = {
            'patrol': {'speed': 500, 'direction': 'forward'},
            'avoid': {'speed': 300, 'direction': 'right'},
            'pursue': {'speed': 800, 'direction': 'forward'},
            'investigate': {'speed': 200, 'direction': 'forward'}
        }
        
        return commands.get(action, {'speed': 0, 'direction': 'idle'})
```

---

## 5-10. 성능 비교 및 최적화

### 성능 비교
| 항목 | Unity AI | UE AI |
|------|----------|-------|
| NPC 수 | 100-500 | 1000+ |
| Behavior Tree 깊이 | 제한적 | 무제한 |
| ML 추론 속도 | 보통 | 빠름 |
| 메모리 사용 | 적음 | 보통 |
| 개발 난이도 | 쉬움 | 어려움 |

### 최적화 기법

#### 1. LOD (Level of Detail)
```cpp
// AI LOD 시스템
void ARobotAIController::UpdateAILOD()
{
    float DistanceToPlayer = GetDistanceToPlayer();
    
    if (DistanceToPlayer < 1000.0f)
    {
        // 근거리: 전체 AI 업데이트
        SetAIUpdateRate(1.0f);
    }
    else if (DistanceToPlayer < 5000.0f)
    {
        // 중거리: 부분 업데이트
        SetAIUpdateRate(0.5f);
    }
    else
    {
        // 원거리: 최소 업데이트
        SetAIUpdateRate(0.1f);
    }
}
```

#### 2. 비동기 처리
```cpp
// 비동기 AI 처리
void ARobotAIController::ProcessAIAsync()
{
    AsyncTask(ENamedThreads::AnyBackgroundThreadNormalTask, [this]()
    {
        // 무거운 AI 계산
        CalculatePath();
        UpdatePerception();
        MakeDecision();
        
        // 메인 스레드에서 결과 적용
        AsyncTask(ENamedThreads::GameThread, [this]()
        {
            ApplyAIResults();
        });
    });
}
```

#### 3. 데이터 기반 최적화
- AI 데이터 구조 최적화
- 메모리 할당 최소화
- 캐시 활용
- 벡터 연산 최적화

---

## 5-11. 마이그레이션 가이드

### Unity → UE5 AI 마이그레이션 체크리스트

#### 1. 행동 시스템
- [ ] Unity Behavior → UE Behavior Trees 변환
- [ ] 자연어 명령 → 수동 트리 설계
- [ ] Blackboard 설정

#### 2.感知 시스템
- [ ] 레이캐스팅 → AI Perception
- [ ] 감지 범위 설정
- [ ] 이벤트 처리

#### 3. 내비게이션
- [ ] NavMesh → Navigation System
- [ ] 경로 찾기 알고리즘
- [ ] 장애물 회피

#### 4. 머신러닝
- [ ] Unity Sentis → UE Neural Network Inference
- [ ] 모델 변환
- [ ] 추론 최적화

#### 5. Python 통합
- [ ] C# TCP → C++ FSocket
- [ ] JSON 프로토콜 유지
- [ ] 성능 테스트

### 마이그레이션 도구
- 자동 변환 스크립트 (Python/C++)
- 검증 도구
- 성능 벤치마크

---

## 5-12. 결론

### UE5의 장점
1. **강력한 AI 시스템**: Behavior Trees, EQS, Perception
2. **고성능**: C++ 기반, 대규모 NPC 지원
3. **풍부한 기능**: ML Deformer, MetaSounds 등
4. **커뮤니티**:成熟的 에코시스템

### Unity의 장점
1. **쉬운 학습곡선**: C# 기반
2. **빠른 프로토타이핑**: Unity AI (자연어)
3. **가벼운 구조**: 소규모 프로젝트에 적합
4. **Python 통합**: 쉬운 연결

### 선택 가이드
- **소규모 프로젝트/빠른 개발**: Unity AI
- **대규모 프로젝트/고품질 AI**: UE AI
- **Python 통합**: 둘 다 가능 (UE가 더 유연)
- **ML 통합**: UE가 더 강력

### 추가 리소스
- UE5 공식 문서: https://docs.unrealengine.com
- Unity 공식 문서: https://docs.unity3d.com
- AI 개발 커뮤니티: Unreal Slackers, Unity Forum