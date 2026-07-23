# 2단계: C++ 기반 로봇 시뮬레이션

## 2-1. 로봇 프로젝트 구조

### UE 프로젝트 구조 (C++ 기반)
```
RobotSimulation/
├── Source/
│   └── RobotSimulation/
│       ├── RobotSimulation.h      # 모듈 헤더
│       ├── RobotSimulation.cpp    # 모듈 구현
│       ├── RobotMovement.h        # 로봇 이동 컴포넌트
│       ├── RobotMovement.cpp
│       ├── RobotSensor.h          # 로봇 센서 컴포넌트
│       ├── RobotSensor.cpp
│       └── RobotSimulation.Build.cs  # 빌드 설정
├── Content/
│   ├── Robots/                    # 로봇 메시
│   ├── Materials/                 # 머티리얼
│   └── Maps/                      # 레벨
└── Config/                        # 프로젝트 설정
```

### Unity와의 구조 비교
| Unity | Unreal Engine 5 |
|-------|-----------------|
| Assets/ | Content/ + Source/ |
| MonoBehaviour | ActorComponent |
| GameObject | Actor |
| Transform | SceneComponent |
| Rigidbody | PrimitiveComponent (Physics) |

---

## 2-2. 로봇 액터 생성

### C++ 클래스 구조
```cpp
// RobotActor.h
#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "RobotActor.generated.h"

UCLASS()
class ROBOTSIMULATION_API ARobotActor : public AActor
{
    GENERATED_BODY()
    
public:    
    ARobotActor();

protected:
    virtual void BeginPlay() override;

public:    
    virtual void Tick(float DeltaTime) override;

    // 로봇 메시 컴포넌트
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly)
    UStaticMeshComponent* RobotMesh;

    // 이동 컴포넌트
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly)
    URobotMovementComponent* MovementComponent;

    // 센서 컴포넌트
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly)
    URobotSensorComponent* SensorComponent;
};
```

### 컴포넌트 기반 아키텍처
Unity의 `MonoBehaviour`와 유사하게 UE5에서는 `ActorComponent`를 사용:
- **SceneComponent**: 트랜스폼(위치, 회전, 스케일) 관리
- **StaticMeshComponent**: 메시 렌더링
- **ActorComponent**: 로직 및 기능 구현

---

## 2-3. 로봇 이동 구현

### 이동 컴포넌트
```cpp
// RobotMovement.h
UCLASS(ClassGroup=(Custom), meta=(BlueprintSpawnableComponent))
class ROBOTSIMULATION_API URobotMovementComponent : public UActorComponent
{
    GENERATED_BODY()

public:    
    URobotMovementComponent();

    // 이동 속도
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Movement")
    float MoveSpeed = 500.0f;

    // 회전 속도
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Movement")
    float RotationSpeed = 100.0f;

    // 이동 함수
    UFUNCTION(BlueprintCallable, Category = "Movement")
    void MoveForward(float Value);

    UFUNCTION(BlueprintCallable, Category = "Movement")
    void MoveRight(float Value);

    UFUNCTION(BlueprintCallable, Category = "Movement")
    void Rotate(float Value);
};
```

### 이동 구현
```cpp
// RobotMovement.cpp
void URobotMovementComponent::MoveForward(float Value)
{
    if (Value != 0.0f)
    {
        FVector Direction = GetOwner()->GetActorForwardVector();
        FVector DeltaMovement = Direction * Value * MoveSpeed * GetWorld()->GetDeltaSeconds();
        GetOwner()->AddActorWorldOffset(DeltaMovement, true);
    }
}

void URobotMovementComponent::MoveRight(float Value)
{
    if (Value != 0.0f)
    {
        FVector Direction = GetOwner()->GetActorRightVector();
        FVector DeltaMovement = Direction * Value * MoveSpeed * GetWorld()->GetDeltaSeconds();
        GetOwner()->AddActorWorldOffset(DeltaMovement, true);
    }
}

void URobotMovementComponent::Rotate(float Value)
{
    if (Value != 0.0f)
    {
        FRotator DeltaRotation(0.0f, Value * RotationSpeed * GetWorld()->GetDeltaSeconds(), 0.0f);
        GetOwner()->AddActorWorldRotation(DeltaRotation);
    }
}
```

---

## 2-4. 로봇 센서 구현

### 센서 컴포넌트
```cpp
// RobotSensor.h
UCLASS(ClassGroup=(Custom), meta=(BlueprintSpawnableComponent))
class ROBOTSIMULATION_API URobotSensorComponent : public UActorComponent
{
    GENERATED_BODY()

public:    
    URobotSensorComponent();

    // 감지 범위
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Sensor")
    float DetectionRange = 1000.0f;

    // 감지 각도 (도)
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Sensor")
    float DetectionAngle = 90.0f;

    // 라인 트레이스로 장애물 감지
    UFUNCTION(BlueprintCallable, Category = "Sensor")
    bool DetectObstacle(FVector& HitLocation);

    // 방향 벡터로 감지
    UFUNCTION(BlueprintCallable, Category = "Sensor")
    TArray<AActor*> DetectObjectsInDirection(FVector Direction);
};
```

### 센서 구현
```cpp
// RobotSensor.cpp
bool URobotSensorComponent::DetectObstacle(FVector& HitLocation)
{
    AActor* Owner = GetOwner();
    if (!Owner) return false;

    FVector Start = Owner->GetActorLocation();
    FVector End = Start + Owner->GetActorForwardVector() * DetectionRange;

    FHitResult HitResult;
    FCollisionQueryParams QueryParams;
    QueryParams.AddIgnoredActor(Owner);

    if (GetWorld()->LineTraceSingleByChannel(HitResult, Start, End, ECC_Visibility, QueryParams))
    {
        HitLocation = HitResult.ImpactPoint;
        return true;
    }
    return false;
}

TArray<AActor*> URobotSensorComponent::DetectObjectsInDirection(FVector Direction)
{
    TArray<AActor*> DetectedActors;
    AActor* Owner = GetOwner();
    if (!Owner) return DetectedActors;

    FVector Start = Owner->GetActorLocation();
    FVector End = Start + Direction * DetectionRange;

    TArray<FHitResult> HitResults;
    FCollisionQueryParams QueryParams;
    QueryParams.AddIgnoredActor(Owner);

    GetWorld()->LineTraceMultiByChannel(HitResults, Start, End, ECC_Visibility, QueryParams);

    for (const FHitResult& Hit : HitResults)
    {
        if (AActor* HitActor = Hit.GetActor())
        {
            DetectedActors.AddUnique(HitActor);
        }
    }
    return DetectedActors;
}
```

---

## 2-5. 로봇 컨트롤러

### 입력 시스템 설정
1. 편집 → 프로젝트 설정 → 엔진 → 입력
2. 액션 매핑 추가:
   - `MoveForward`: W, S 키
   - `MoveRight`: A, D 키
   - `Rotate`: Q, E 키

### 입력 처리
```cpp
// RobotController.h
UCLASS()
class ROBOTSIMULATION_API ARobotController : public APlayerController
{
    GENERATED_BODY()

protected:
    virtual void SetupInputComponent() override;

    void MoveForward(float Value);
    void MoveRight(float Value);
    void Rotate(float Value);
};

// RobotController.cpp
void ARobotController::SetupInputComponent()
{
    Super::SetupInputComponent();

    InputComponent->BindAxis("MoveForward", this, &ARobotController::MoveForward);
    InputComponent->BindAxis("MoveRight", this, &ARobotController::MoveRight);
    InputComponent->BindAxis("Rotate", this, &ARobotController::Rotate);
}

void ARobotController::MoveForward(float Value)
{
    if (ARobotActor* Robot = Cast<ARobotActor>(GetPawn()))
    {
        Robot->MovementComponent->MoveForward(Value);
    }
}

void ARobotController::MoveRight(float Value)
{
    if (ARobotActor* Robot = Cast<ARobotActor>(GetPawn()))
    {
        Robot->MovementComponent->MoveRight(Value);
    }
}

void ARobotController::Rotate(float Value)
{
    if (ARobotActor* Robot = Cast<ARobotActor>(GetPawn()))
    {
        Robot->MovementComponent->Rotate(Value);
    }
}
```

---

## 2-6. 좌표축 변환 (Unity → UE)

### Unity와 UE 좌표축 비교
| 항목 | Unity | Unreal Engine 5 |
|------|-------|-----------------|
| 상향 축 | Y-up | Z-up |
| 전향 축 | Z+ (전방) | X+ (전방) |
| 우향 축 | X+ (우측) | Y+ (우측) |

### 변환 공식
```
Unity (x, y, z) → UE5 (x, z, y)
```

### 변환 함수
```cpp
FVector ConvertUnityToUE(FVector UnityVector)
{
    return FVector(UnityVector.X, UnityVector.Z, UnityVector.Y);
}

FRotator ConvertUnityToUE(FRotator UnityRotation)
{
    return FRotator(UnityRotation.Pitch, UnityRotation.Yaw, UnityRotation.Roll);
}
```

### 주의사항
- Isaac Sim → Unity 변환 후 UE로 변환 시 2단계 변환 필요
- 회전축 변환 시 짐벌락(Gimbal Lock) 주의
- 스케일 변환은 일반적으로 1:1 유지

---

## 2-7. 빌드 및 테스트

### 프로젝트 빌드
1. Unreal Editor → 파일 → 프로젝트 빌드
2. 또는 Visual Studio에서 F5로 디버깅 모드 실행

### 테스트 방법
1. 에디터에서 "재생" 버튼 클릭
2. 키보드로 로봇 조작:
   - W/S: 전후진
   - A/D: 좌우 이동
   - Q/E: 회전
3. 센서 동작 확인 (라인 트레이스)

### 다음 단계
- [3단계: Blueprint 기반 로봇 시뮬레이션](./03_UE_Blueprint_Robot_Tutorial.md)