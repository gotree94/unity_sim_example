# 3단계: 외부 Python 프로그램과 연결하기

> **목적**: Unity에서 만든 로봇을 외부 Python 프로그램에서 키보드 입력으로 원격 제어  
> **소요 시간**: 약 40~60분  
> **전제 조건**: [2단계: 로봇 제작](02_robot_creation.md) 완료

---

## 목차

13. [외부 Python 프로그램과 연결하기](#13-외부-python-프로그램과-연결하기)

---

## 13. 외부 Python 프로그램과 연결하기

### 13-1. 개요: 왜 외부 연결이 필요한가?

지금까지는 Unity 안에서 직접 키보드를 눌러 로봇을 조종했습니다. 하지만 실제 로봇 공학이나 자율주행 시뮬레이션에서는 다음과 같은 이유로 외부 프로그램과의 연결이 필요합니다:

| 이유 | 설명 |
|------|------|
| **로봇 제어 알고리즘** | Python으로 만든 AI/ML 모델이 로봇을 제어해야 할 때 |
| **센서 데이터 처리** | 외부 센서에서 받은 데이터를 기반으로 로봇을 움직일 때 |
| **데이터 수집** | 로봇의 상태를 Python에서 기록/분석할 때 |
| **멀티 에이전트** | 여러 로봇을 하나의 Python 프로그램에서 동시에 제어할 때 |

### 13-2. 연결 방식 선택: TCP/IP 소켓 통신

Unity와 Python을 연결하는 방법은 여러 가지가 있지만, 초심자에게 가장 이해하기 쉬운 방식인 **TCP/IP 소켓 통신**을 사용합니다.

```
┌──────────────────┐         TCP/IP          ┌──────────────────┐
│                  │    (localhost:5000)      │                  │
│   Python 클라이언트  │ <--------------------> │   Unity 서버     │
│                  │                          │                  │
│  - 키보드 입력    │    키 값 전송             │  - 로봇 제어     │
│  - 데이터 처리    │    "W", "A", "S" 등      │  - 상태 수신     │
│                  │                          │                  │
└──────────────────┘                          └──────────────────┘
```

**동작 흐름:**
1. Unity에서 TCP 서버를 실행 (포트 5000)
2. Python에서 Unity 서버에 연결 (클라이언트)
3. Python에서 키보드 입력을 감지
4. 입력된 키 값을 문자열로 전송 (예: "W", "A", "SPACE")
5. Unity에서 값을 수신하여 로봇을 움직임

### 13-3. Unity 측: TCP 서버 스크립트 만들기

#### 1) 새 스크립트 생성

1. Project 창의 **Assets > Scripts** 폴더를 엽니다.
2. 우클릭 > **Create > C# Script** > 이름을 **"TCPServer"**로 변경합니다.
3. 스크립트를 더블클릭하여 Visual Studio에서 엽니다.

#### 2) TCPServer.cs 코드 작성

다음 코드를 기존 코드에 **전체 교체**합니다:

```csharp
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

public class TCPServer : MonoBehaviour
{
    [Header("서버 설정")]
    [Tooltip("리스닝할 포트 번호")]
    public int port = 5000;

    [Tooltip("연결된 클라이언트 수 표시")]
    public int connectedClients = 0;

    // TCP 서버
    private TcpListener server;
    private Thread serverThread;
    private bool isRunning = false;

    // 수신된 명령 큐 (메인 스레드에서 처리)
    private readonly Queue<string> commandQueue = new Queue<string>();
    private readonly object queueLock = new object();

    // 수신된 마지막 명령
    [HideInInspector]
    public string lastCommand = "";

    void Start()
    {
        StartServer();
    }

    void OnDestroy()
    {
        StopServer();
    }

    void OnApplicationQuit()
    {
        StopServer();
    }

    /// <summary>
    /// TCP 서버를 시작합니다.
    /// </summary>
    void StartServer()
    {
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            isRunning = true;

            // 백그라운드 스레드에서 클라이언트 연결 대기
            serverThread = new Thread(ServerLoop);
            serverThread.IsBackground = true;
            serverThread.Start();

            Debug.Log($"[TCPServer] 서버 시작됨 - 포트: {port}");
            Debug.Log($"[TCPServer] Python에서 연결 대기 중... (localhost:{port})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TCPServer] 서버 시작 실패: {e.Message}");
        }
    }

    /// <summary>
    /// 서버 메인 루프 (백그라운드 스레드)
    /// </summary>
    void ServerLoop()
    {
        while (isRunning)
        {
            try
            {
                // 클라이언트 연결 대기
                TcpClient client = server.AcceptTcpClient();
                connectedClients++;
                Debug.Log($"[TCPServer] 클라이언트 연결됨! (현재 {connectedClients}개)");

                // 각 클라이언트를 위한 수신 스레드 생성
                Thread receiveThread = new Thread(() => HandleClient(client));
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (SocketException)
            {
                // 서버가 중지되면 발생
                break;
            }
        }
    }

    /// <summary>
    /// 클라이언트로부터 데이터를 수신합니다.
    /// </summary>
    void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        try
        {
            while (client.Connected && isRunning)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                
                // 줄바꿈으로 구분된 여러 명령 처리
                string[] commands = message.Split('\n');
                foreach (string cmd in commands)
                {
                    if (!string.IsNullOrEmpty(cmd))
                    {
                        lock (queueLock)
                        {
                            commandQueue.Enqueue(cmd);
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log($"[TCPServer] 클라이언트 처리 중 오류: {e.Message}");
        }
        finally
        {
            client.Close();
            connectedClients--;
            Debug.Log($"[TCPServer] 클라이언트 연결 해제 (현재 {connectedClients}개)");
        }
    }

    /// <summary>
    /// 메인 스레드에서 명령을 처리합니다.
    /// </summary>
    void Update()
    {
        lock (queueLock)
        {
            while (commandQueue.Count > 0)
            {
                string command = commandQueue.Dequeue();
                ProcessCommand(command);
            }
        }
    }

    /// <summary>
    /// 수신된 명령을 처리합니다.
    /// </summary>
    void ProcessCommand(string command)
    {
        lastCommand = command;
        Debug.Log($"[TCPServer] 수신 명령: {command}");
    }

    /// <summary>
    /// 클라이언트에게 메시지를 전송합니다.
    /// </summary>
    public void SendToClient(string message)
    // 메시지 전송은 별도 구현 필요 (생략)
    {
        // 실제 구현에서는 연결된 클라이언트 목록을 관리해야 합니다
    }

    /// <summary>
    /// 서버를 중지합니다.
    /// </summary>
    void StopServer()
    {
        isRunning = false;
        server?.Stop();
        serverThread?.Join(1000);
        Debug.Log("[TCPServer] 서버 중지됨");
    }
}
```

#### 3) RobotController 스크립트 수정

기존의 **RobotController.cs** 스크립트를 열고, TCPServer와 연동하도록 수정합니다.

**다음 코드를 기존 RobotController.cs에 전체 교체합니다:**

```csharp
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 120.0f;
    public float jumpForce = 7.0f;

    [Header("연결 설정")]
    [Tooltip("TCPServer 컴포넌트를 연결하세요")]
    public TCPServer tcpServer;

    private Rigidbody rb;
    private bool isGrounded;

    // 외부 제어용 입력 값
    private float externalMoveZ = 0f;
    private float externalMoveX = 0f;
    private bool externalStop = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("RobotController: Rigidbody 컴포넌트가 없습니다!");
        }

        // TCPServer 자동 연결
        if (tcpServer == null)
        {
            tcpServer = FindObjectOfType<TCPServer>();
        }
    }

    void Update()
    {
        HandleLocalInput();
        HandleExternalInput();
    }

    void FixedUpdate()
    {
        MoveRobot();
    }

    /// <summary>
    /// 로컬 키보드 입력 처리 (Unity 안에서 직접 조작)
    /// </summary>
    void HandleLocalInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StopRobot();
        }
    }

    /// <summary>
    /// 외부 Python 프로그램의 입력 처리
    /// </summary>
    void HandleExternalInput()
    {
        if (tcpServer == null) return;

        string command = tcpServer.lastCommand;
        if (string.IsNullOrEmpty(command)) return;

        // 명령 처리
        switch (command.ToUpper().Trim())
        {
            case "W":
            case "FORWARD":
                externalMoveZ = 1f;
                externalMoveX = 0f;
                externalStop = false;
                break;
            case "S":
            case "BACKWARD":
                externalMoveZ = -1f;
                externalMoveX = 0f;
                externalStop = false;
                break;
            case "A":
            case "LEFT":
                externalMoveZ = 0f;
                externalMoveX = -1f;
                externalStop = false;
                break;
            case "D":
            case "RIGHT":
                externalMoveZ = 0f;
                externalMoveX = 1f;
                externalStop = false;
                break;
            case "X":
                externalMoveZ = -1f;
                externalMoveX = 0f;
                externalStop = false;
                break;
            case "SPACE":
            case "STOP":
                externalStop = true;
                break;
            case "RELEASE":
                // 모든 입력 초기화
                externalMoveZ = 0f;
                externalMoveX = 0f;
                externalStop = true;
                break;
        }

        // 명령 처리 후 큐 초기화
        tcpServer.lastCommand = "";
    }

    /// <summary>
    /// 로봇을 이동시킵니다.
    /// </summary>
    void MoveRobot()
    {
        if (rb == null) return;

        float moveX = 0f;
        float moveZ = 0f;

        // 로컬 입력 (키보드)
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            moveZ = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            moveZ = -1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveX = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveX = 1f;
        if (Input.GetKey(KeyCode.X))
            moveZ = -1f;

        // 외부 입력이 있으면 로컬 입력을 무시
        if (externalMoveZ != 0 || externalMoveX != 0)
        {
            moveX = externalMoveX;
            moveZ = externalMoveZ;
        }

        // 정지 명령
        if (externalStop)
        {
            StopRobot();
            externalStop = false;
            return;
        }

        // 이동 적용
        Vector3 moveDirection = transform.forward * moveZ;
        moveDirection = moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveDirection);

        // 회전 적용
        float rotation = moveX * rotationSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, rotation, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    void StopRobot()
    {
        if (rb == null) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        externalMoveZ = 0f;
        externalMoveX = 0f;

        Debug.Log("로봇이 멈췄습니다!");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}
```

#### 4) Unity에서 설정하기

1. Unity로 돌아옵니다.
2. Hierarchy에서 **Robot** 오브젝트를 선택합니다.
3. Inspector에서 **RobotController** 컴포넌트를 찾습니다.
4. **Tcp Server** 필드에 TCPServer가 있는 오브젝트를 드래그합니다.
   - TCPServer를 같은 Robot 오브젝트에 붙였다면, Robot을 드래그하면 됩니다.

**TCPServer 오브젝트 추가:**
1. Hierarchy에서 빈 공간 우클릭 > **Create Empty** > 이름을 **"NetworkManager"**로 변경
2. NetworkManager를 선택하고 **Add Component > TCPServer** 추가
3. Inspector에서 **Port** 값을 `5000`으로 설정

최종 구조:
```
RobotScene
├── ...
├── NetworkManager    <- TCPServer (포트: 5000)
└── Robot             <- RobotController + Rigidbody
    ├── Body
    ├── Front_Right
    ├── Front_Left
    ├── Rear_Right
    └── Rear_Left
```

### 13-4. Python 측: TCP 클라이언트 프로그램 만들기

#### 1) Python 설치 확인

Python이 설치되어 있는지 확인합니다:

```bash
python --version
# 또는
python3 --version
```

Python이 설치되어 있지 않다면 https://www.python.org 에서 다운로드하여 설치합니다.

#### 2) 필요한 라이브러리 설치

터미널(명령 프롬프트)에서 다음 명령을 실행합니다:

```bash
pip install keyboard
```

> ⚠️ **주의**: `keyboard` 라이브러리는 관리자 권한이 필요할 수 있습니다.  
> Windows에서는 명령 프롬프트를 **관리자 권한으로 실행**해야 할 수 있습니다.  
> 또는 `pynput` 라이브러리를 대신 사용할 수도 있습니다:
> ```bash
> pip install pynput
> ```

#### 3) 기본 TCP 클라이언트 스크립트

텍스트 편집기(VS Code, 메모장 등)에서 새 파일을 만들고 **`robot_control.py`**로 저장합니다:

```python
import socket
import time
import sys

# ===== 설정 =====
HOST = "127.0.0.1"  # Unity가 실행 중인 컴퓨터 (로컬)
PORT = 5000          # Unity TCPServer의 포트 번호와 일치해야 함

def connect_to_unity():
    """Unity 서버에 연결합니다."""
    client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    
    try:
        client.connect((HOST, PORT))
        print(f"[연결 성공] Unity 서버에 연결되었습니다! ({HOST}:{PORT})")
        return client
    except ConnectionRefusedError:
        print("[연결 실패] Unity 서버가 실행 중인지 확인하세요!")
        print("  -> Unity에서 Play 버튼을 누르세요.")
        return None
    except Exception as e:
        print(f"[연결 실패] 오류: {e}")
        return None

def send_command(client, command):
    """Unity에 명령을 전송합니다."""
    try:
        message = command.encode("utf-8")
        client.sendall(message)
        print(f"  -> 전송: {command}")
    except BrokenPipeError:
        print("[오류] 연결이 끊어졌습니다!")
        return False
    except Exception as e:
        print(f"[전송 오류] {e}")
        return False
    return True

def print_controls():
    """조작법 안내를 출력합니다."""
    print("=" * 50)
    print("       로봇 원격 제어 프로그램")
    print("=" * 50)
    print()
    print("  조작법:")
    print("    W 또는 ↑  : 전진")
    print("    S 또는 ↓  : 후진")
    print("    A 또는 ←  : 좌회전")
    print("    D 또는 →  : 우회전")
    print("    X         : 후진")
    print("    SPACE     : 정지")
    print("    Q         : 프로그램 종료")
    print()
    print("=" * 50)

def main():
    print_controls()
    
    # Unity 연결
    print("\n[연결 중] Unity 서버에 연결을 시도합니다...")
    client = connect_to_unity()
    
    if client is None:
        print("\n프로그램을 종료합니다.")
        sys.exit(1)
    
    print("\n[준비 완료] 키보드를 눌러 로봇을 제어하세요!\n")
    
    try:
        # 간단한 키 입력 방식 (keyboard 라이브러리 사용)
        import keyboard
        
        last_key = None
        
        def on_key_event(event):
            nonlocal last_key
            if event.event_type == keyboard.KEY_DOWN:
                last_key = event.name.upper()
        
        # 키 이벤트 등록
        keyboard.hook(on_key_event)
        
        while True:
            if last_key is not None:
                key = last_key
                last_key = None
                
                # 명령 매핑
                command_map = {
                    "W": "W\n",
                    "UP": "W\n",
                    "S": "S\n",
                    "DOWN": "S\n",
                    "A": "A\n",
                    "LEFT": "A\n",
                    "D": "D\n",
                    "RIGHT": "D\n",
                    "X": "X\n",
                    "SPACE": "SPACE\n",
                    "Q": None,
                }
                
                if key == "Q":
                    print("\n[종료] 프로그램을 종료합니다.")
                    # 정지 명령 전송
                    send_command(client, "STOP\n")
                    break
                
                if key in command_map and command_map[key] is not None:
                    send_command(client, command_map[key])
            
            time.sleep(0.01)  # CPU 사용량 줄이기
    
    except ImportError:
        print("[대체 모드] keyboard 라이브러리 없이 실행합니다.")
        print("  명령을 직접 입력하세요 (W/S/A/D/X/SPACE/Q)\n")
        
        while True:
            command = input("명령 입력> ").strip().upper()
            
            if command == "Q":
                print("[종료] 프로그램을 종료합니다.")
                send_command(client, "STOP\n")
                break
            
            if command in ["W", "S", "A", "D", "X", "SPACE", "STOP", "RELEASE"]:
                send_command(client, command + "\n")
            else:
                print("  알 수 없는 명령입니다. W/S/A/D/X/SPACE/Q를 입력하세요.")
    
    finally:
        keyboard.unhook_all()
        client.close()
        print("[연결 해제] Unity 연결이 해제되었습니다.")

if __name__ == "__main__":
    main()
```

### 13-5. 테스트 방법

#### 1단계: Unity 실행

1. Unity 에디터에서 **▶ (Play)** 버튼을 누릅니다.
2. Console 창에 `[TCPServer] 서버 시작됨 - 포트: 5000` 메시지가 나타나는지 확인합니다.
3. `[TCPServer] Python에서 연결 대기 중...` 메시지가 보이면 정상입니다.

#### 2단계: Python 스크립트 실행

별도의 터미널(명령 프롬프트) 창을 열고:

```bash
python robot_control.py
```

또는 Python이 `python3`으로 설치된 경우:

```bash
python3 robot_control.py
```

#### 3단계: 연결 확인

Python 터미널에 다음 메시지가 나타나면 성공입니다:

```
[연결 성공] Unity 서버에 연결되었습니다! (127.0.0.1:5000)
```

Unity Console에는:

```
[TCPServer] 클라이언트 연결됨! (현재 1개)
```

#### 4단계: 로봇 제어

Python 터미널에서 키보드를 누르면 Unity의 로봇이 움직입니다!

| Python에서 누른 키 | Unity에서의 동작 |
|-------------------|-----------------|
| **W** 또는 **↑** | 로봇 전진 |
| **S** 또는 **↓** | 로봇 후진 |
| **A** 또는 **←** | 로봇 좌회전 |
| **D** 또는 **→** | 로봇 우회전 |
| **X** | 로봇 후진 |
| **SPACE** | 로봇 정지 |
| **Q** | 프로그램 종료 |

### 13-6. 고급: 키보드 없이 텍스트 입력으로 제어하기

`keyboard` 라이브러리 없이도 Python 프로그램은 동작합니다. 이 경우 터미널에서 명령어를 직접 입력합니다:

```
명령 입력> W
명령 입력> W
명령 입력> D
명령 입력> SPACE
명령 입력> Q
```

### 13-7. 문제 해결

#### 문제 1: "연결 실패" 메시지가 나타남

| 확인 사항 | 해결 방법 |
|----------|----------|
| Unity가 Play 모드인지 확인 | Unity에서 ▶ 버튼을 눌러 Play 모드로 전환 |
| 포트 번호 일치 확인 | Unity TCPServer의 port와 Python의 PORT 변수가 같은지 |
| 방화벽 차단 | Windows 방화벽에서 Python/Unity를 허용 |
| localhost 사용 | HOST를 `127.0.0.1` 또는 `localhost`로 설정 |

#### 문제 2: 연결은 되었는데 로봇이 움직이지 않음

| 확인 사항 | 해결 방법 |
|----------|----------|
| RobotController의 Tcp Server 연결 | Inspector에서 TCPServer가 연결되어 있는지 확인 |
| 명령 수신 확인 | Unity Console에서 "수신 명령: W" 같은 메시지가 보이는지 확인 |
| Rigidbody 존재 | Robot 오브젝트에 Rigidbody가 있는지 확인 |

#### 문제 3: Python에서 "keyboard" 라이브러리 오류

```bash
# 관리자 권한으로 실행
# Windows: 명령 프롬프트를 관리자 권한으로 실행
# 또는 대체 라이브러리 사용:
pip install pynput
```

`pynput`을 사용하는 대체 코드:

```python
from pynput import keyboard

def on_press(key):
    try:
        if key == keyboard.Key.up:
            send_command(client, "W\n")
        elif key == keyboard.Key.down:
            send_command(client, "S\n")
        elif key == keyboard.Key.left:
            send_command(client, "A\n")
        elif key == keyboard.Key.right:
            send_command(client, "D\n")
        elif key == keyboard.Key.space:
            send_command(client, "SPACE\n")
        elif key == keyboard.Key.esc:
            return False  # 리스너 중지
    except AttributeError:
        # 일반 키 처리
        char = key.char
        if char and char.upper() in ['W', 'A', 'S', 'D', 'X']:
            send_command(client, char.upper() + "\n")
        elif char == 'q':
            return False

with keyboard.Listener(on_press=on_press) as listener:
    listener.join()
```

#### 문제 4: 여러 Python 클라이언트 동시 연결

현재 구현에서는 여러 Python 프로그램이 동시에 연결할 수 있습니다. 각 클라이언트는 별도 스레드에서 처리됩니다. 하지만 명령은 마지막에 수신된 것만 적용됩니다.

### 13-8. 전체 연결 구조도

```
┌───────────────────────────────────────────────────────┐
│                      Unity                            │
│                                                       │
│  ┌─────────────────┐      ┌────────────────────────┐  │
│  │   TCPServer     │      │    RobotController     │  │
│  │                 │      │                        │  │
│  │  포트: 5000     │─────>│  lastCommand 수신       │  │
│  │  수신 스레드     │      │  이동/회전 실행         │  │
│  │                 │      │                        │  │
│  └─────────────────┘      └────────────────────────┘  │
│           ▲                          │                 │
│           │ TCP/IP                   ▼                 │
│           │ (localhost:5000)  ┌──────────────┐         │
│           │                  │    Robot     │         │
│           │                  │  (Rigidbody) │         │
│           │                  └──────────────┘         │
└───────────┼───────────────────────────────────────────┘
            │
            │ TCP/IP
            │
┌───────────┼───────────────────────────────────────────┐
│           ▼          Python 프로그램                    │
│  ┌─────────────────┐                                  │
│  │ robot_control.py│                                  │
│  │                 │                                  │
│  │  키보드 입력     │                                  │
│  │    W, A, S, D   │                                  │
│  │    ↑, ↓, ←, →   │                                  │
│  │    SPACE, Q     │                                  │
│  │                 │                                  │
│  │  TCP 클라이언트   │                                  │
│  │  127.0.0.1:5000 │                                  │
│  └─────────────────┘                                  │
└───────────────────────────────────────────────────────┘
```

### 13-9. 명령 프로토콜 정리

Unity와 Python 사이에서 주고받는 명령어 목록:

| 명령어 | 의미 | Unity에서의 동작 |
|--------|------|-----------------|
| `W` | 전진 | 로봇이 앞쪽으로 이동 |
| `S` | 후진 | 로봇이 뒤쪽으로 이동 |
| `A` | 좌회전 | 로봇이 왼쪽으로 회전 |
| `D` | 우회전 | 로봇이 오른쪽으로 회전 |
| `X` | 후진 | S와 동일 |
| `SPACE` | 정지 | 로봇의 속도를 0으로 |
| `STOP` | 정지 | SPACE와 동일 |
| `RELEASE` | 입력 해제 | 모든 이동 입력 초기화 |
| `FORWARD` | 전진 | W와 동일 (영문 확장) |
| `BACKWARD` | 후진 | S와 동일 (영문 확장) |
| `LEFT` | 좌회전 | A와 동일 (영문 확장) |
| `RIGHT` | 우회전 | D와 동일 (영문 확장) |

> 💡 **팁**: 명령어는 대소문자를 구분하지 않습니다. `w`, `W`, `Woow` 모두 `W`로 처리됩니다.  
> 각 명령은 줄바꿈(`\n`)으로 구분하여 전송합니다.

### 13-10. 최종 프로젝트 구조

```
RobotTutorial/
├── Assets/
│   ├── Materials/
│   │   ├── BodyMaterial
│   │   ├── WheelMaterial
│   │   └── GroundMaterial
│   ├── PhysicMaterials/
│   │   └── RobotMaterial
│   ├── Scripts/
│   │   ├── RobotController.cs    <- 로봇 제어 (외부 입력 지원)
│   │   └── TCPServer.cs          <- TCP 서버
│   └── Scenes/
│       └── RobotScene.unity
│
└── Python/
    └── robot_control.py          <- Python 클라이언트 프로그램
```

---

> **출처**: NVIDIA Omniverse Isaac Sim 튜토리얼 2.2 "Add Simple Objects"를 Unity에 맞게 번안 및 확장  
> **추가 참고**: Unity-Python TCP/IP 통신
