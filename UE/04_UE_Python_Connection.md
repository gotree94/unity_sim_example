# 4단계: Python TCP/IP 연결

## 4-1. Python 통합 개요

### UE5에서 Python 사용하기
- **Python Editor Script Plugin**: 에디터 내 Python 실행 가능
- **C++ Python Integration**: 런타임에서 Python 스크립트 실행
- **TCP/IP 통신**: Python과 UE 간 데이터 교환

### Unity와의 비교
| 항목 | Unity | Unreal Engine 5 |
|------|-------|-----------------|
| Python 지원 | 공식 미지원 | Python Editor Script Plugin |
| TCP/IP 구현 | C# TcpClient | C++ FSocket |
| 네이티브 언어 | C# | C++ |
| 스크립팅 | C# | C++ / Blueprint / Python |

---

## 4-2. Python TCP/IP 서버 (Unity 스타일)

### Python 서버 코드 (Unity 튜토리얼과 동일)
```python
# python_server.py
import socket
import json
import threading

class UnityTCPServer:
    def __init__(self, host='127.0.0.1', port=5555):
        self.host = host
        self.port = port
        self.server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.server_socket.bind((self.host, self.port))
        self.server_socket.listen(5)
        self.clients = []
        
    def handle_client(self, client_socket, address):
        print(f"클라이언트 연결: {address}")
        self.clients.append(client_socket)
        
        try:
            while True:
                data = client_socket.recv(4096).decode('utf-8')
                if not data:
                    break
                    
                # JSON 파싱
                message = json.loads(data)
                print(f"수신: {message}")
                
                # 응답 전송
                response = {
                    'status': 'ok',
                    'received': message
                }
                client_socket.send(json.dumps(response).encode('utf-8'))
                
        except Exception as e:
            print(f"오류: {e}")
        finally:
            self.clients.remove(client_socket)
            client_socket.close()
            
    def start(self):
        print(f"서버 시작: {self.host}:{self.port}")
        while True:
            client_socket, address = self.server_socket.accept()
            client_thread = threading.Thread(target=self.handle_client, args=(client_socket, address))
            client_thread.start()
            
if __name__ == "__main__":
    server = UnityTCPServer()
    server.start()
```

### 실행 방법
```bash
python python_server.py
```

---

## 4-3. UE5 TCP/IP 클라이언트 (C++)

### C++ TCP/IP 클라이언트
```cpp
// TCPClient.h
#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "Sockets.h"
#include "SocketSubsystem.h"
#include "TCPClient.generated.h"

UCLASS(ClassGroup=(Custom), meta=(BlueprintSpawnableComponent))
class ROBOTSIMULATION_API UTCPClient : public UActorComponent
{
    GENERATED_BODY()

public:    
    UTCPClient();

    // 서버 주소
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "TCP")
    FString ServerAddress = "127.0.0.1";

    // 서버 포트
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "TCP")
    int32 ServerPort = 5555;

    // 연결 상태
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "TCP")
    bool bIsConnected = false;

    // 서버 연결
    UFUNCTION(BlueprintCallable, Category = "TCP")
    bool ConnectToServer();

    // 서버 연결 해제
    UFUNCTION(BlueprintCallable, Category = "TCP")
    void DisconnectFromServer();

    // 데이터 전송
    UFUNCTION(BlueprintCallable, Category = "TCP")
    bool SendMessage(const FString& Message);

    // 데이터 수신
    UFUNCTION(BlueprintCallable, Category = "TCP")
    FString ReceiveMessage();

private:
    FSocket* ClientSocket;
    FRunnableThread* ReceiveThread;
    bool bShouldRun;
    
    // 수신 스레드 함수
    void ReceiveData();
};
```

### C++ TCP/IP 클라이언트 구현
```cpp
// TCPClient.cpp
#include "TCPClient.h"
#include "SocketSubsystem.h"
#include "Sockets.h"
#include "Common/TcpSocketBuilder.h"

UTCPClient::UTCPClient()
{
    PrimaryComponentTick.bCanEverTick = false;
    ClientSocket = nullptr;
    bShouldRun = false;
}

bool UTCPClient::ConnectToServer()
{
    // 소켓 생성
    ClientSocket = FTcpSocketBuilder(TEXT("TCPClient"))
        .AsNonBlocking()
        .AsReusable()
        .BoundToEndpoint(FIPv4Endpoint(FIPv4Address::Any, 0))
        .ConnectedToEndpoint(FIPv4Endpoint(FIPv4Address::Parse(ServerAddress), ServerPort))
        .Build();

    if (!ClientSocket)
    {
        UE_LOG(LogTemp, Error, TEXT("소켓 생성 실패"));
        return false;
    }

    // 연결 확인
    int32 ConnectionResult = 0;
    bool bCanBind = ClientSocket->Wait(ESocketWaitConditions::WaitForConnected, FTimespan::FromSeconds(5.0));
    
    if (!bCanBind)
    {
        UE_LOG(LogTemp, Error, TEXT("서버 연결 실패"));
        ClientSocket->Close();
        ClientSocket = nullptr;
        return false;
    }

    bIsConnected = true;
    bShouldRun = true;
    
    // 수신 스레드 시작
    ReceiveThread = FRunnableThread::Create(
        FRunnable::CreateLambda([this]() { ReceiveData(); }),
        TEXT("TCPReceiveThread"),
        0, TPri_BelowNormal
    );

    UE_LOG(LogTemp, Log, TEXT("서버 연결 성공: %s:%d"), *ServerAddress, ServerPort);
    return true;
}

void UTCPClient::DisconnectFromServer()
{
    bShouldRun = false;
    
    if (ReceiveThread)
    {
        ReceiveThread->WaitForCompletion();
        delete ReceiveThread;
        ReceiveThread = nullptr;
    }
    
    if (ClientSocket)
    {
        ClientSocket->Close();
        ClientSocket = nullptr;
    }
    
    bIsConnected = false;
    UE_LOG(LogTemp, Log, TEXT("서버 연결 해제"));
}

bool UTCPClient::SendMessage(const FString& Message)
{
    if (!ClientSocket || !bIsConnected)
    {
        return false;
    }

    // UTF-8로 변환
    FTCHARToUTF8 Converter(*Message);
    int32 BytesSent = ClientSocket->Send(
        (uint8*)Converter.Get(), 
        Converter.Length()
    );

    return BytesSent > 0;
}

FString UTCPClient::ReceiveMessage()
{
    if (!ClientSocket || !bIsConnected)
    {
        return TEXT("");
    }

    uint8 Buffer[4096];
    int32 BytesRead = 0;
    
    if (ClientSocket->Recv(Buffer, sizeof(Buffer), BytesRead))
    {
        // UTF-8로 변환
        FUTF8ToTCHAR Converter((const ANSICHAR*)Buffer, BytesRead);
        return FString(Converter.Length(), Converter.Get());
    }
    
    return TEXT("");
}

void UTCPClient::ReceiveData()
{
    while (bShouldRun && ClientSocket)
    {
        FString Message = ReceiveMessage();
        if (!Message.IsEmpty())
        {
            // 메인 스레드에서 처리하기 위해 스케줄링
            AsyncTask(ENamedThreads::GameThread, [this, Message]()
            {
                UE_LOG(LogTemp, Log, TEXT("수신: %s"), *Message);
            });
        }
        
        FPlatformProcess::Sleep(0.01f);
    }
}
```

---

## 4-4. Blueprint에서 TCP/IP 사용

### TCP 클라이언트 블루프린트
1. Content Browser → 우클릭 → Blueprint Class
2. 부모 클래스: **Actor** 선택
3. 이름: `BP_TCPClient`

### 컴포넌트 추가
1. **TCPClient** 컴포넌트 추가 (C++에서 생성한 클래스)

### 연결 로직
```
Event BeginPlay
    ├── TCPClient → Set Server Address: "127.0.0.1"
    ├── TCPClient → Set Server Port: 5555
    └── TCPClient → Connect To Server
        ├── True → Print String: "연결 성공"
        └── False → Print String: "연결 실패"

Event Tick
    └── TCPClient → Is Connected
        ├── True → 메시지 전송/수신
        └── False → 재연결 시도
```

### 메시지 전송
```
Custom Event SendMessage
    ├── JSON 구조체 생성
    │   ├── Type: "robot_control"
    │   ├── Action: "move"
    │   └── Parameters: {...}
    ├── JSON 직렬화
    └── TCPClient → Send Message
```

### 메시지 수신
```
Custom Event ReceiveMessage
    ├── TCPClient → Receive Message
    ├── JSON 파싱
    └── 메시지 타입별 처리
        ├── "robot_status" → 로봇 상태 업데이트
        ├── "command" → 명령 실행
        └── "error" → 오류 처리
```

---

## 4-5. JSON 데이터 구조

### 공통 JSON 프로토콜 (Unity 튜토리얼과 동일)
```json
// 클라이언트 → 서버 메시지
{
    "type": "robot_control",
    "action": "move",
    "parameters": {
        "x": 100.0,
        "y": 200.0,
        "z": 0.0,
        "speed": 500.0
    }
}

// 서버 → 클라이언트 응답
{
    "type": "response",
    "status": "ok",
    "data": {
        "position": {"x": 100.0, "y": 200.0, "z": 0.0},
        "rotation": {"pitch": 0.0, "yaw": 90.0, "roll": 0.0}
    }
}
```

### Python에서 JSON 처리
```python
import json

# 서버에서 처리
def handle_message(message):
    data = json.loads(message)
    
    if data['type'] == 'robot_control':
        if data['action'] == 'move':
            # 로봇 이동 처리
            return {
                'type': 'response',
                'status': 'ok',
                'data': {'position': get_robot_position()}
            }
    
    return {'type': 'error', 'message': 'Unknown action'}
```

### UE5에서 JSON 처리
```cpp
#include "Dom/JsonObject.h"
#include "Serialization/JsonReader.h"
#include "Serialization/JsonWriter.h"

// JSON 파싱
FString ParseJSON(const FString& JsonString)
{
    TSharedPtr<FJsonObject> JsonObject;
    TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(JsonString);
    
    if (FJsonSerializer::Deserialize(Reader, JsonObject))
    {
        FString Type = JsonObject->GetStringField("type");
        // 처리 로직
    }
    
    return FString();
}

// JSON 생성
FString CreateJSON(const FString& Type, const FString& Action)
{
    TSharedPtr<FJsonObject> JsonObject = MakeShareable(new FJsonObject);
    JsonObject->SetStringField("type", Type);
    JsonObject->SetStringField("action", Action);
    
    FString OutputString;
    TSharedRef<TJsonWriter<>> Writer = TJsonWriterFactory<>::Create(&OutputString);
    FJsonSerializer::Serialize(JsonObject.ToSharedRef(), Writer);
    
    return OutputString;
}
```

---

## 4-6. 데이터 교환 예제

### 로봇 위치 동기화
```python
# Python 서버
def sync_robot_position(client_socket):
    position = get_robot_position()  # 가상 로봇 위치
    response = {
        'type': 'robot_status',
        'position': position,
        'timestamp': time.time()
    }
    client_socket.send(json.dumps(response).encode('utf-8'))
```

### UE에서 위치 수신
```
Event Tick
    └── TCPClient → Receive Message
        ├── JSON 파싱
        ├── "robot_status" 타입 확인
        └── Set Actor Location
            ├── X: position.x
            ├── Y: position.z (좌표축 변환)
            └── Z: position.y (좌표축 변환)
```

---

## 4-7. 오류 처리

### 연결 오류 처리
```
Event Tick
    └── TCPClient → Is Connected
        ├── False → 재연결 시도
        │   ├── Wait Time: 5.0초
        │   └── Connect To Server
        └── True → 정상 동작
```

### 데이터 오류 처리
```
Function HandleMessage
    ├── JSON 파싱 성공?
    │   ├── True → 메시지 처리
    │   └── False → 오류 응답 전송
    └── 처리 중 오류 발생?
        ├── True → 오류 로그 출력
        └── False → 정상 완료
```

### 타임아웃 처리
```cpp
// C++에서 타임아웃 처리
bool UTCPClient::SendMessageWithTimeout(const FString& Message, float TimeoutSeconds)
{
    // 논블로킹 모드에서 타임아웃 처리
    double StartTime = FPlatformTime::Seconds();
    
    while (FPlatformTime::Seconds() - StartTime < TimeoutSeconds)
    {
        if (SendMessage(Message))
        {
            return true;
        }
        FPlatformProcess::Sleep(0.001f);
    }
    
    return false;
}
```

---

## 4-8. 테스트 및 디버깅

### 테스트 순서
1. Python 서버 실행
2. UE5 프로젝트 실행
3. 자동 연결 확인
4. 메시지 전송 테스트
5. 메시지 수신 테스트
6. 좌표축 변환 확인

### 디버깅 팁
- **UE Log**: Output Log에서 TCP 통신 상태 확인
- **Python Print**: 서버 터미널에서 수신 데이터 확인
- **Wireshark**: 네트워크 패킷 캡처 (고급)
- **Blueprint Debug**: 실행 흐름 시각적 확인

### 성능 최적화
- 메시지 빈도 제한 (1초에 30회 이하)
- 데이터 압축 (선택사항)
- 연결 유지 (Keep-Alive)
- 배치 처리 (여러 메시지 묶어서 전송)

### 다음 단계
- [5단계: Unity AI 기능 대응 UE 기능](./05_UE_AI_Integration.md)