# 02-4. C++ 클래스 구현

> **목적**: 로봇 시뮬레이션을 위한 C++ 클래스들을 구현합니다  
> **소요 시간**: 약 30~40분  
> **전제 조건**: [02-3. 물리 시스템 및 입력 설정](02_3_UE_Physics_Input.md) 완료

---

## 목차

1. [C++ 클래스 구조](#1-c-클래스-구조)
2. [RobotMovement 클래스](#2-robotmovement-클래스)
3. [RobotSensor 클래스](#3-robotsensor-클래스)
4. [RobotController 클래스](#4-robotcontroller-클래스)
5. [Blueprint 연결](#5-blueprint-연결)
6. [다음 단계로](#6-다음-단계로)

---

## 1. C++ 클래스 구조

### 1-1. 전체 클래스 구조

```
RobotSimulation/
├── Source/
│   └── RobotSimulation/
│       ├── RobotMovement.h
│       ├── RobotMovement.cpp
│       ├── RobotSensor.h
│       ├── RobotSensor.cpp
│       ├── RobotController.h
│       ├── RobotController.cpp
│       └── RobotSimulation.Build.cs
```

### 1-2. 클래스 설명

| 클래스 | 역할 | 설명 |
|--------|------|------|
| RobotMovement | 이동 제어 | 키보드 입력으로 로봇 이동/회전 |
| RobotSensor | 센서 시뮬레이션 | 거리/충돌 감지 |
| RobotController | 메인 컨트롤러 | 전체 로봇 제어 |

---

## 2. RobotMovement 클래스

### 2-1. RobotMovement.h

```cpp
// RobotMovement.h
#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "InputActionValue.h"
#include "RobotMovement.generated.h"

UCLASS()
class ROBOTSIMULATION_API ARobotMovement : public AActor
{
    GENERATED_BODY()

public:
    ARobotMovement();

protected:
    virtual void BeginPlay() override;

public:
    virtual void Tick(float DeltaTime) override;

    // 입력 액션 참조
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Input")
    class UInputAction* MoveForwardAction;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Input")
    class UInputAction* MoveRightAction;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Input")
    class UInputAction* TurnRightAction;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Input")
    class UInputAction* TurnLeftAction;

    // 이동 파라미터
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Movement")
    float MoveSpeed = 500.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Movement")
    float TurnSpeed = 100.0f;

    // 현재 이동 상태
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Movement")
    FVector CurrentVelocity;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Movement")
    float CurrentAngularVelocity;

    // 함수
    UFUNCTION(BlueprintCallable, Category = "Movement")
    void MoveForward(const FInputActionValue& Value);

    UFUNCTION(BlueprintCallable, Category = "Movement")
    void MoveRight(const FInputActionValue& Value);

    UFUNCTION(BlueprintCallable, Category = "Movement")
    void TurnRight(const FInputActionValue& Value);

    UFUNCTION(BlueprintCallable, Category = "Movement")
    void TurnLeft(const FInputActionValue& Value);

    UFUNCTION(BlueprintCallable, Category = "Movement")
    void UpdateMovement(float DeltaTime);

private:
    // 입력 컴포넌트
    UPROPERTY()
    class UEnhancedInputComponent* EnhancedInputComponent;

    // 입력 매핑 컨텍스트
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Input")
    class UInputMappingContext* InputMappingContext;
};
```

### 2-2. RobotMovement.cpp

```cpp
// RobotMovement.cpp
#include "RobotMovement.h"
#include "EnhancedInputComponent.h"
#include "EnhancedInputSubsystems.h"
#include "InputActionValue.h"
#include "GameFramework/Character.h"
#include "GameFramework/PlayerController.h"
#include "Camera/CameraComponent.h"
#include "Components/CapsuleComponent.h"

ARobotMovement::ARobotMovement()
{
    PrimaryActorTick.bCanEverTick = true;

    // 기본값 설정
    MoveSpeed = 500.0f;
    TurnSpeed = 100.0f;
    CurrentVelocity = FVector::ZeroVector;
    CurrentAngularVelocity = 0.0f;
}

void ARobotMovement::BeginPlay()
{
    Super::BeginPlay();

    // Enhanced Input System 초기화
    if (APlayerController* PlayerController = Cast<APlayerController>(GetController()))
    {
        if (UEnhancedInputLocalPlayerSubsystem* Subsystem = 
            ULocalPlayer::GetSubsystem<UEnhancedInputLocalPlayerSubsystem>(PlayerController->GetLocalPlayer()))
        {
            if (InputMappingContext)
            {
                Subsystem->AddMappingContext(InputMappingContext, 0);
            }
        }
    }
}

void ARobotMovement::Tick(float DeltaTime)
{
    Super::Tick(DeltaTime);
    UpdateMovement(DeltaTime);
}

void ARobotMovement::MoveForward(const FInputActionValue& Value)
{
    float ForwardValue = Value.Get<float>();

    if (ForwardValue != 0.0f)
    {
        CurrentVelocity.X = ForwardValue * MoveSpeed;
    }
    else
    {
        CurrentVelocity.X = 0.0f;
    }
}

void ARobotMovement::MoveRight(const FInputActionValue& Value)
{
    float RightValue = Value.Get<float>();

    if (RightValue != 0.0f)
    {
        CurrentVelocity.Y = RightValue * MoveSpeed;
    }
    else
    {
        CurrentVelocity.Y = 0.0f;
    }
}

void ARobotMovement::TurnRight(const FInputActionValue& Value)
{
    float TurnValue = Value.Get<float>();

    if (TurnValue != 0.0f)
    {
        CurrentAngularVelocity = TurnValue * TurnSpeed;
    }
    else
    {
        CurrentAngularVelocity = 0.0f;
    }
}

void ARobotMovement::TurnLeft(const FInputActionValue& Value)
{
    float TurnValue = Value.Get<float>();

    if (TurnValue != 0.0f)
    {
        CurrentAngularVelocity = -TurnValue * TurnSpeed;
    }
    else
    {
        CurrentAngularVelocity = 0.0f;
    }
}

void ARobotMovement::UpdateMovement(float DeltaTime)
{
    // 이동 적용
    FVector NewLocation = GetActorLocation() + (CurrentVelocity * DeltaTime);
    SetActorLocation(NewLocation);

    // 회전 적용
    FRotator NewRotation = GetActorRotation();
    NewRotation.Yaw += CurrentAngularVelocity * DeltaTime;
    SetActorRotation(NewRotation);
}
```

---

## 3. RobotSensor 클래스

### 3-1. RobotSensor.h

```cpp
// RobotSensor.h
#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "RobotSensor.generated.h"

UCLASS()
class ROBOTSIMULATION_API ARobotSensor : public AActor
{
    GENERATED_BODY()

public:
    ARobotSensor();

protected:
    virtual void BeginPlay() override;

public:
    virtual void Tick(float DeltaTime) override;

    // 센서 설정
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Sensor")
    float MaxRange = 1000.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Sensor")
    float MinRange = 10.0f;

    // 거리 측정
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Sensor")
    float CurrentDistance;

    // 충돌 감지
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Sensor")
    bool bIsColliding;

    // 함수
    UFUNCTION(BlueprintCallable, Category = "Sensor")
    float MeasureDistance();

    UFUNCTION(BlueprintCallable, Category = "Sensor")
    bool DetectCollision();

    UFUNCTION(BlueprintCallable, Category = "Sensor")
    FVector GetForwardVector();

private:
    // 센서 컴포넌트
    UPROPERTY()
    class USphereComponent* SensorComponent;

    // 시각화 컴포넌트
    UPROPERTY()
    class UStaticMeshComponent* VisualizerComponent;
};
```

### 3-2. RobotSensor.cpp

```cpp
// RobotSensor.cpp
#include "RobotSensor.h"
#include "Components/SphereComponent.h"
#include "Components/StaticMeshComponent.h"
#include "DrawDebugHelpers.h"
#include "Engine/World.h"

ARobotSensor::ARobotSensor()
{
    PrimaryActorTick.bCanEverTick = true;

    // 센서 컴포넌트 생성
    SensorComponent = CreateDefaultSubobject<USphereComponent>(TEXT("SensorComponent"));
    RootComponent = SensorComponent;
    SensorComponent->SetSphereRadius(10.0f);
    SensorComponent->SetCollisionProfileName(TEXT("OverlapAll"));

    // 시각화 컴포넌트 생성
    VisualizerComponent = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("VisualizerComponent"));
    VisualizerComponent->SetupAttachment(RootComponent);
    VisualizerComponent->SetRelativeScale3D(FVector(0.1f));

    // 기본값 설정
    MaxRange = 1000.0f;
    MinRange = 10.0f;
    CurrentDistance = 0.0f;
    bIsColliding = false;
}

void ARobotSensor::BeginPlay()
{
    Super::BeginPlay();
}

void ARobotSensor::Tick(float DeltaTime)
{
    Super::Tick(DeltaTime);

    // 거리 측정
    CurrentDistance = MeasureDistance();

    // 충돌 감지
    bIsColliding = DetectCollision();

    // 디버그 시각화
    #if ENABLE_DRAW_DEBUG
    DrawDebugSphere(GetWorld(), GetActorLocation(), MaxRange, 12, FColor::Green, false, -1.0f, 0, 1.0f);
    DrawDebugSphere(GetWorld(), GetActorLocation(), CurrentDistance, 12, FColor::Red, false, -1.0f, 0, 1.0f);
    #endif
}

float ARobotSensor::MeasureDistance()
{
    FVector Start = GetActorLocation();
    FVector End = Start + GetForwardVector() * MaxRange;

    FHitResult HitResult;
    FCollisionQueryParams QueryParams;
    QueryParams.AddIgnoredActor(this);

    if (GetWorld()->LineTraceSingleByChannel(HitResult, Start, End, ECC_Visibility, QueryParams))
    {
        return HitResult.Distance;
    }

    return MaxRange;
}

bool ARobotSensor::DetectCollision()
{
    FVector Start = GetActorLocation();
    FVector End = Start + GetForwardVector() * MinRange;

    FHitResult HitResult;
    FCollisionQueryParams QueryParams;
    QueryParams.AddIgnoredActor(this);

    return GetWorld()->LineTraceSingleByChannel(HitResult, Start, End, ECC_Visibility, QueryParams);
}

FVector ARobotSensor::GetForwardVector()
{
    return GetActorForwardVector();
}
```

---

## 4. RobotController 클래스

### 4-1. RobotController.h

```cpp
// RobotController.h
#pragma once

#include "CoreMinimal.h"
#include "GameFramework/PlayerController.h"
#include "RobotController.generated.h"

UCLASS()
class ROBOTSIMULATION_API ARobotController : public APlayerController
{
    GENERATED_BODY()

public:
    ARobotController();

protected:
    virtual void BeginPlay() override;
    virtual void SetupInputComponent() override;

public:
    // 로봇 참조
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Robot")
    class ARobotActor* RobotActor;

    // 입력 상태
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Input")
    bool bIsForwardPressed;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Input")
    bool bIsBackwardPressed;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Input")
    bool bIsLeftPressed;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Input")
    bool bIsRightPressed;

    // 함수
    UFUNCTION(BlueprintCallable, Category = "Robot")
    void SetRobotActor(ARobotActor* NewRobotActor);

    UFUNCTION(BlueprintCallable, Category = "Input")
    void HandleForwardInput(float Value);

    UFUNCTION(BlueprintCallable, Category = "Input")
    void HandleRightInput(float Value);

    UFUNCTION(BlueprintCallable, Category = "Input")
    void HandleTurnInput(float Value);

private:
    // 입력 컴포넌트
    UPROPERTY()
    class UEnhancedInputComponent* EnhancedInputComponent;
};
```

### 4-2. RobotController.cpp

```cpp
// RobotController.cpp
#include "RobotController.h"
#include "EnhancedInputComponent.h"
#include "EnhancedInputSubsystems.h"
#include "RobotActor.h"
#include "InputActionValue.h"

ARobotController::ARobotController()
{
    PrimaryActorTick.bCanEverTick = true;

    bIsForwardPressed = false;
    bIsBackwardPressed = false;
    bIsLeftPressed = false;
    bIsRightPressed = false;
}

void ARobotController::BeginPlay()
{
    Super::BeginPlay();

    // Enhanced Input System 초기화
    if (UEnhancedInputLocalPlayerSubsystem* Subsystem = 
        ULocalPlayer::GetSubsystem<UEnhancedInputLocalPlayerSubsystem>(GetLocalPlayer()))
    {
        // Input Mapping Context 추가
        if (InputMappingContext)
        {
            Subsystem->AddMappingContext(InputMappingContext, 0);
        }
    }
}

void ARobotController::SetupInputComponent()
{
    Super::SetupInputComponent();

    // Enhanced Input Component 캐스팅
    EnhancedInputComponent = Cast<UEnhancedInputComponent>(InputComponent);
    if (EnhancedInputComponent)
    {
        // 입력 바인딩
        EnhancedInputComponent->BindAction(MoveForwardAction, ETriggerEvent::Triggered, this, &ARobotController::HandleForwardInput);
        EnhancedInputComponent->BindAction(MoveRightAction, ETriggerEvent::Triggered, this, &ARobotController::HandleRightInput);
        EnhancedInputComponent->BindAction(TurnRightAction, ETriggerEvent::Triggered, this, &ARobotController::HandleTurnInput);
        EnhancedInputComponent->BindAction(TurnLeftAction, ETriggerEvent::Triggered, this, &ARobotController::HandleTurnInput);
    }
}

void ARobotController::SetRobotActor(ARobotActor* NewRobotActor)
{
    RobotActor = NewRobotActor;
}

void ARobotController::HandleForwardInput(float Value)
{
    if (RobotActor)
    {
        RobotActor->MoveForward(Value);
    }
}

void ARobotController::HandleRightInput(float Value)
{
    if (RobotActor)
    {
        RobotActor->MoveRight(Value);
    }
}

void ARobotController::HandleTurnInput(float Value)
{
    if (RobotActor)
    {
        RobotActor->Turn(Value);
    }
}
```

---

## 5. Blueprint 연결

### 5-1. RobotActor Blueprint 설정

```
Content Browser → RobotActor
└── 더블클릭하여 Blueprint Editor 열기
    └── Components Panel
        └── Add Component → RobotMovement
            └── Details Panel
                ├── Move Speed: 500.0
                ├── Turn Speed: 100.0
                └── Input Mapping Context: IMC_RobotControl
```

### 5-2. RobotController Blueprint 설정

```
Content Browser → RobotPlayerController
└── 더블클릭하여 Blueprint Editor 열기
    └── Details Panel
        ├── Input
        │   └── Input Component: EnhancedInputComponent
        └── Classes
            └── Default Pawn Class: RobotActor
```

### 5-3. GameMode 설정

```
Content Browser → BP_GameMode
└── 더블클릭하여 Blueprint Editor 열기
    └── Details Panel
        ├── Classes
        │   ├── Default Pawn Class: RobotActor
        │   └── Player Controller Class: RobotPlayerController
        └── HUD Class: (기본값)
```

### 5-4. 레벨에 GameMode 배치

```
Level Editor → World Settings
└── GameMode Override: BP_GameMode
```

---

## 6. 다음 단계로

C++ 클래스 구현이 완료되었습니다. 다음 단계에서는 카메라 설정, 빌드, 테스트를 진행합니다.

**다음 단계**: [5. 카메라 및 테스트](02_5_UE_Camera_Test.md)

---

## 빠른 참조

| 항목 | 위치 |
|------|------|
| C++ 클래스 | Source/RobotSimulation/ |
| RobotMovement | Source/RobotSimulation/RobotMovement.h/cpp |
| RobotSensor | Source/RobotSimulation/RobotSensor.h/cpp |
| RobotController | Source/RobotSimulation/RobotController.h/cpp |
| Build.cs | Source/RobotSimulation/RobotSimulation.Build.cs |

---

> **이전 단계**: [3. 물리 시스템 및 입력 설정](02_3_UE_Physics_Input.md)  
> **다음 단계**: [5. 카메라 및 테스트](02_5_UE_Camera_Test.md)

---

> **저작권**: 본 교육 자료는 교육 목적으로 자유롭게 사용할 수 있습니다.  
> **최종 업데이트**: 2026년 7월
