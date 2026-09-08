using UnityEngine;
using UnityEngine.Rendering;   // AsyncGPUReadback (비동기 픽셀 읽기, 메인 스레드 블로킹 방지)
using Unity.Collections;       // NativeArray (Readback 픽셀 데이터)

// ############################################################
// # CameraSensor
// # 역할: TurtleBot3 전방 카메라(Raspberry Pi Camera)를 시뮬레이션합니다.
// #       - Unity Camera(RenderTexture)로 장면을 렌더링합니다.
// #       - 픽셀 읽기는 AsyncGPUReadback(비동기)을 사용하므로
// #         동기 ReadPixels로 인한 에디터/게임 멈춤(GPU 스털)이 없습니다.
// #         (GPU가 없는 실습 PC/WARP 환경에서도 부담이 적음)
// #       - 원본 RGBA와 Grayscale(흑백, mono8) 바이트(grayBytes)로 제공해
// #         10장 RosBridge가 sensor_msgs/Image(/image_raw)로 전송합니다.
// # 부착 위치: camera_link (base_link의 자식, 전방 +Z를 바라보는 방향)
// ############################################################
[RequireComponent(typeof(Camera))]
public class CameraSensor : MonoBehaviour
{
    // ---------- [TurtleBot3 카메라 사양 (Raspberry Pi Camera v2 기준)] ----------
    // 실제 로봇: 640x480 @ 30Hz, 화각 약 62°. 시뮬레이션은 부담을 줄인 기본값 사용.
    [Tooltip("영상 가로 해상도(픽셀). GPU 없는 실습 PC에서는 320 이하 권장")]
    public int width = 320;
    [Tooltip("영상 세로 해상도(픽셀)")]
    public int height = 240;
    [Tooltip("CPU 데이터 캡처 주기(Hz). 기본 5Hz — 낮을수록 부담 감소")]
    public float captureRate = 5f;
    [Tooltip("수평 화각(FOV, 도). RPi Camera v2 기본 약 62°")]
    public float fov = 62f;

    [Header("표시 설정 (GPU 없는 PC에서는 기본 false 권장)")]
    [Tooltip("Game 뷰 우측에 실시간 영상 패널 표시 (RViz ImagePanel이 대체 가능)")]
    public bool showOnScreen = false;
    [Tooltip("흑백(Grayscale) 결과도 표시 (영상 처리 학습용)")]
    public bool showGrayscale = false;

    [Header("캡처 결과 (외부에서 읽음)")]
    public Texture2D capturedTexture;   // 원본 컬러 텍스처 (표시용)
    public byte[] colorBytes;           // 원본 RGBA32 바이트 (width*height*4바이트)
    public byte[] grayBytes;            // Grayscale 바이트 (width*height바이트, 0~255)

    private Camera cam;                 // 영상 렌더링용 Unity Camera
    private RenderTexture rt;           // 렌더링 대상 렌더텍스처
    private Texture2D grayTexture;      // 흑백 표시용 텍스처
    private byte[] grayRgba;            // 흑백 표시용 RGBA 버퍼 (회색 = R=G=B)
    private float captureTimer = 0f;

    void Awake()
    {
        // 1) Unity Camera 설정 (자식 트리이므로 로봇이 움직이면 카메라도 함께 따라갑니다)
        cam = GetComponent<Camera>();
        cam.fieldOfView = fov;                          // 화각 (실제 카메라와 유사한 62°)
        cam.nearClipPlane = 0.02f;                      // 아주 가까운 곳도 보이도록
        cam.farClipPlane = 20f;
        cam.clearFlags = CameraClearFlags.SolidColor;   // 배경을 단색으로 (하늘색 방지)
        cam.backgroundColor = new Color(0.25f, 0.27f, 0.30f);
        cam.enabled = false;                            // 수동 렌더 전용 (중복 렌더링 방지)

        // 2) 렌더 텍스처 생성 — 카메라가 여기에 렌더링되고 우리가 픽셀을 읽습니다.
        //    targetTexture가 지정되면 Game 뷰 화면에는 중복 표시되지 않습니다.
        rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;

        // 3) 데이터 버퍼/텍스처 초기화
        colorBytes = new byte[width * height * 4];   // RGBA32 (픽셀당 4바이트)
        grayBytes = new byte[width * height];        // 흑백(단채널) 데이터 (외부 전송용)
        grayRgba = new byte[width * height * 4];     // 흑백 표시용 RGBA (GUI는 단채널 R8을 그리면 빨갛게 나온다)
        capturedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        grayTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
    }

    void Start()
    {
        // 초기화 확인용 로그: 이 로그가 보이면 "영상 입력 경로"가 정상 동작 중입니다.
        Debug.Log($"[CameraSensor] 초기화됨. camera={gameObject.name}, 해상도={width}x{height}@{captureRate}Hz (비동기 캡처)");
    }

    void Update()
    {
        // 캡처 주기 제한: 비동기 캡처라도 렌더 비용이 있으므로 captureRate(기본 5Hz)마다 1회만 실행
        captureTimer += Time.deltaTime;
        if (captureTimer >= 1f / captureRate)
        {
            captureTimer = 0f;
            RequestAsyncCapture();   // ★ 비동기 픽셀 읽기 (메인 스레드를 막지 않음)
        }
    }

    // 비동기 영상 입력 파이프라인:
    //   ① RenderTexture에 수동 렌더 → ② AsyncGPUReadback(비동기)으로 픽셀 읽기
    //   → ③ 프레임 끝 콜백에서 colorBytes/grayBytes 갱신
    //
    // 왜 비동기인가? 동기 ReadPixels()는 GPU 작업이 끝날 때까지 메인 스레드가 멈춰
    // 에디터 전체가 "응답 없음"이 되는 스털(stall)을 만들 수 있습니다.
    // AsyncGPUReadback은 GPU가 준비되면 프레임 끝에서 콜백으로 결과를 주므로
    // 렌더링 흐름을 막지 않습니다. (GPU 없는 WARP 환경에서도 안정적)
    void RequestAsyncCapture()
    {
        if (rt == null) return;

        cam.Render();   // ① RenderTexture에 수동 렌더 (메인 스레드 차단 없음)

        // ② 비동기 readback — 완료 콜백은 메인 스레드에서 안전하게 호출됩니다.
        AsyncGPUReadback.Request(rt, 0, TextureFormat.RGBA32, (req) =>
        {
            if (req.hasError || !req.done) return;   // 실패 시 이전 프레임 유지
            NativeArray<byte> data = req.GetData<byte>();
            if (data.Length >= colorBytes.Length)
            {
                data.CopyTo(colorBytes);   // ③ 원본 RGBA32 순서 그대로 저장
                BuildGrayscale();          // ④ 영상 처리: 컬러 → Grayscale
            }
            data.Dispose();
        });
    }

    // 컬러 → Grayscale 전처리 (ITU-R BT.601 계수: Y = 0.299R + 0.587G + 0.114B)
    // R8 단채널을 GUI로 그리면 붉게 보이므로, 회색(R=G=B) RGBA 버퍼로도 만듭니다.
    void BuildGrayscale()
    {
        int n = grayBytes.Length;
        for (int i = 0; i < n; i++)
        {
            byte r = colorBytes[i * 4 + 0];
            byte g = colorBytes[i * 4 + 1];
            byte b = colorBytes[i * 4 + 2];
            byte gv = (byte)(0.299f * r + 0.587f * g + 0.114f * b);
            grayBytes[i] = gv;

            // 표시용: R8(단채널) 텍스처를 GUI로 그리면 붉은색만 나오므로,
            // 회색(R=G=B)으로 채운 RGBA 버퍼를 만들어 GPU에 올립니다.
            grayRgba[i * 4 + 0] = gv;
            grayRgba[i * 4 + 1] = gv;
            grayRgba[i * 4 + 2] = gv;
            grayRgba[i * 4 + 3] = 255;
        }
        grayTexture.SetPixelData(grayRgba, 0);
        grayTexture.Apply();
    }

    // Game 뷰 우측에 원본 + 흑백 영상을 표시 (RViz ImagePanel과 같은 역할, 기본 OFF)
    // ⚠️ OnGUI에서 예외가 발생하면 Unity 에디터는 OnGUI를 무한 재호출해 "응답 없음"이 됩니다.
    //     → null/크기 가드 + 3프레임에 1번만 그리기로 이를 방지합니다.
    void OnGUI()
    {
        if (!showOnScreen || rt == null || capturedTexture == null || rt.width == 0) return;
        if (Time.frameCount % 3 != 0) return;   // 부하 감소 (GPU 없는 PC에서 중요)

        float panelW = width < 320 ? 160f : 200f;
        float panelH = panelW * 0.75f;
        float right = Screen.width - panelW - 10f;
        float top = 10f;

        // RGB 패널은 CPU ReadPixels 없이 RenderTexture(rt)를 직접 그립니다.
        // (픽셀 원점이 좌하단이라 GUI(좌상단)에 그리면 상하 반전 → 높이를 음수로 보정)
        GUI.DrawTexture(new Rect(right, top + panelH, panelW, -panelH), rt);
        GUI.Label(new Rect(right, top + panelH - 16, panelW, 16), "  Camera RGB");

        if (showGrayscale && grayTexture != null)
        {
            GUI.DrawTexture(new Rect(right, top + panelH * 2 + 10, panelW, -panelH), grayTexture);
            GUI.Label(new Rect(right, top + panelH * 2 + 10 - 16, panelW, 16), "  Grayscale(흑백)");
        }
    }

    // ---------- 외부 데이터 API ----------
    // (10장 RosBridge 확장에서 sensor_msgs/Image payload로 그대로 직렬화 가능)
    public byte[] GetColorBytes() => colorBytes;
    public byte[] GetGrayBytes() => grayBytes;
    public Texture2D GetCapturedTexture() => capturedTexture;
}