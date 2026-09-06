using UnityEngine;

// ############################################################
// # LidarSensor
// # 역할: TurtleBot3의 2D LiDAR(LDS-02/LDS-03)를 시뮬레이션합니다.
// #       - 회전 로테이터를 만들어 시각적으로 레이저를 회전시킵니다.
// #       - 매 프레임 360° Raycast로 각도별 거리값(ranges[])을 측정합니다.
// #       - LineRenderer로 녹색 레이저 링을 Scene/Game 뷰에 그립니다.
// # 부착 위치: base_scan (LiDAR 링크)에 부착해야 정확한 원점에서 측정됩니다.
// ############################################################
public class LidarSensor : MonoBehaviour
{
    // ---------- [LDS-02 사양 (기본) / LDS-03 사양으로 조정 가능] ----------
    // 각도별 분해능 개수. LDS-02는 1° → 360개, LDS-03은 0.9° → 약 400개.
    public int rayCount = 360;
    // 최소 측정 거리(m). 이보다 가까운 물체는 무시. (LDS-02: 0.16, LDS-03: 0.05)
    public float rangeMin = 0.12f;
    // 최대 측정 거리(m). 이보다 먼 곳은 감지 안 됨. (LDS-02: 8.0, LDS-03: 12.0)
    public float rangeMax = 3.5f;
    // 스캔 주기(Hz). LDS-02는 5Hz, LDS-03은 10Hz.
    public float scanRate = 5f;
    // 시각적 회전 속도(RPM 개념, deg/s로 환산). 실제 센서 모터 회전 표현용.
    public float rotationSpeed = 1800f;

    // ---------- [시각화] ----------
    public bool drawRays = true;
    public Color rayColor = Color.green;
    // 레이저 발사 높이(m). base_scan 위쪽으로 올려 로봇 몸체와 겹치지 않게 함.
    public float rayHeight = 0.15f;

    // 각도별 거리값 배열 (ROS LaserScan.range 구조와 동일). 외부(맵핑)에서 읽습니다.
    public float[] ranges;

    private Transform rotator;
    private LineRenderer[] lines;      // 레이저 링을 그리는 라인렌더러
    private int lastPointIndex = -1;

    void Awake()
    {
        CreateRotator();   // 회전용 자식 오브젝트 생성
        CreateRayLines();  // LineRenderer 생성 (녹색 링)
        // ranges 배열 초기화: 기본값을 최대 측정 거리로 채움 (아무것도 없으면 최대값)
        ranges = new float[rayCount];
        for (int i = 0; i < rayCount; i++)
            ranges[i] = rangeMax;
    }

    void Start()
    {
        // 초기화 확인용 로그: 이 로그가 보이면 스크립트가 정상 실행 중이라는 뜻.
        bool shaderOk = lines != null && lines.Length > 0 && lines[0] != null
                        && lines[0].material != null && lines[0].material.shader != null;
        // base_scan의 실제 월드 위치를 출력해 레이저 원점(origin)이 맞는지 확인한다.
        Vector3 origin = transform.position + Vector3.up * rayHeight;
        Debug.Log($"[LidarSensor] 초기화됨. base_scan={gameObject.name}, LineRenderer={shaderOk}, rayCount={rayCount}, 월드위치={transform.position}, 레이저원점={origin}");
    }

    // 회전용 빈 오브젝트("LidarRotator")를 base_scan 아래 자식으로 생성.
    // 라인 자체는 고정하고 로테이터만 회전시켜 레이저가 돌며 훑는 듯한 시각 효과를 줍니다.
    void CreateRotator()
    {
        GameObject rotGO = new GameObject("LidarRotator");
        rotGO.transform.SetParent(transform, false);
        rotator = rotGO.transform;
    }

    // 360개 거리 점을 잇는 LineRenderer 1개를 base_scan에 추가하여 링 모양을 그림.
    void CreateRayLines()
    {
        lines = new LineRenderer[1];
        LineRenderer lr = gameObject.AddComponent<LineRenderer>();

        // 렌더러 기본 설정을 명시적으로 지정 (셰이더/월드좌표/길이 보정)
        lr.useWorldSpace = true;              // 월드 좌표로 점 배치 (로봇 이동에도 따라감)
        lr.widthMultiplier = 1f;
        lr.loop = false;                      // 닫는 점은 수동으로 추가
        lr.receiveShadows = false;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        lr.positionCount = rayCount + 1;      // 360점 + 닫는 점 1개
        lr.startWidth = 0.005f;               // 선 두께
        lr.endWidth = 0.005f;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        // 셰이더를 여러 후보에서 차례로 시도해 안전하게 생성 (null이면 예외 방지)
        Shader sh = Shader.Find("Sprites/Default");
        if (sh == null) sh = Shader.Find("Legacy Shaders/Diffuse");
        if (sh == null) sh = Shader.Find("Unlit/Color");
        if (sh == null) sh = Shader.Find("Hidden/Internal-Colored");
        lr.material = new Material(sh);
        lr.material.color = Color.white;      // 타일 컬러는 흰색 → startColor가 그대로 보임
        lr.startColor = rayColor;
        lr.endColor = rayColor;
        lines[0] = lr;
    }

    void Update()
    {
        // 시각화: 회전 오브젝트를 매 프레임 회전시켜 돌아가는 레이저 표현
        rotator.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // 데이터/실제 선: drawRays가 켜져 있으면 매 프레임 360° 전체를 측정해 라인 갱신
        // (실제 센서는 회전 주기로 누적 출력하지만, 학습 목적으로 매 프레임 측정해도 무방)
        if (drawRays)
            UpdateScan();
    }

    // 매 프레임 360°를 Raycast로 측정하고 ranges[]와 레이저 링을 갱신
    void UpdateScan()
    {
        // 발사 시작점: base_scan 월드위치에서 위로 rayHeight만큼 (장애물/라인 높이)
        Vector3 origin = transform.position + Vector3.up * rayHeight;

        for (int i = 0; i < rayCount; i++)
        {
            float angleRad = i * Mathf.Deg2Rad; // 0 ~ 359°
            // 로봇의 forward(전방)가 로컬 Z축이라는 점에 주의.
            // X = sin(각도), Z = cos(각도) 로 Y축 회전한 방향 벡터를 만듦.
            Vector3 direction = new Vector3(Mathf.Sin(angleRad), 0, Mathf.Cos(angleRad));

            if (Physics.Raycast(origin, direction, out RaycastHit hit, rangeMax))
            {
                float dist = hit.distance;
                if (dist < rangeMin)
                    ranges[i] = float.PositiveInfinity; // 최소거리 미만은 "감지 안 됨" 처리
                else
                    ranges[i] = dist;                   // 실제 반사 거리 저장
            }
            else
            {
                ranges[i] = rangeMax; // 화면(맵) 바깥은 최대값 = 감지 안 됨
            }

            float validDist = (float.IsInfinity(ranges[i])) ? rangeMax : ranges[i];

            // 라인렌더러: i번째 링 점 = 원점 + 방향 × 거리 (링을 이루는 점)
            // (주의: SetPosition(i, origin)처럼 원점을 찍으면 링이 점 하나로 뭉개짐)
            lines[0].SetPosition(i, origin + direction * validDist);

            // 보조 시각화: Scene 뷰(Gizmos ON)에 무조건 보이는 레이저 선.
            // LineRenderer와 무관하게 동작하므로, 렌더링 원인을 분리 확인하는 용도.
            if (i % 6 == 0)
                Debug.DrawRay(origin, direction * validDist, rayColor);
        }

        // 링을 닫기 위해 마지막 점(rayCount)을 첫 점과 이어줌.
        // (0.001m 위로 살짝 올려 점이 겹쳐 깜빡이는 걸 방지)
        lines[0].SetPosition(rayCount, lines[0].GetPosition(0) + Vector3.up * 0.001f);
    }

    // 외부(맵핑/ROS)에서 각도별 거리를 얻는 API
    public float GetRange(int index) => ranges[index];
    public float GetRangeAtAngle(float angleDeg) => ranges[((int)angleDeg + 360) % rayCount];
}
