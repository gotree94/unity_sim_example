using UnityEngine;

// 실제 SLAM(gmapping/cartographer)의 점유확률(Occupancy Probability) 방식을 따른 맵 렌더러입니다.
// - 각 격자에 확률(0~1)을 저장합니다. 0.5가 초기(unknown)값입니다.
// - 레이저 끝점(장애물)은 확률을 높이고, 경로(free)는 확률을 낮춥니다.
// - 표시 (RViz Map 규칙): 확률이 높으면 검정(장애물), 낮으면 흰색(free), 중간값이면 회색(unknown).
//   결과적으로 장애물만 선명하게 남고 지나간 경로는 흐려집니다.
public class MapRenderer : MonoBehaviour
{
    [Header("맵 설정")]
    public int gridSize = 200;         // 200x200 픽셀
    public float resolution = 0.05f;   // 5cm/픽셀
    public Transform robotTransform;
    public LidarSensor lidar;

    [Header("연결")]
    public bool autoUpdate = true;

    [Header("화면 표시")]
    [Tooltip("MapDisplay Quad(3D 공간)에 맵 텍스처를 표시할지 (true면 3D 바닥에도 보임)")]
    public bool showOnQuad = true;
    [Tooltip("화면 코너에 RViz처럼 별도 2D 맵 패널을 표시할지")]
    public bool showOnScreen = true;
    private Rect panelRect = new Rect(10, 250, 300, 300); // 화면 좌상단 아래에 패널

    [Header("점유확률 파라미터")]
    [Tooltip("레이저 끝점(장애물)일 때 확률 증가량 (기본 0.3)")]
    public float hitIncrease = 0.3f;
    [Tooltip("경로(빈 공간)일 때 확률 감소량 (기본 0.1)")]
    public float missDecrease = 0.1f;
    [Tooltip("확률이 이 값 이상이면 흰색(장애물)으로 판정")]
    public float occupiedThreshold = 0.7f;
    [Tooltip("확률이 이 값 이하이면 검정(free)으로 판정")]
    public float freeThreshold = 0.3f;

    private Texture2D mapTexture;
    private float[,] occupancy; // 점유확률 0~1, 0.5=unknown
    private float mapWorldSize;

    void Start()
    {
        mapWorldSize = gridSize * resolution; // 200 * 0.05 = 10m

        occupancy = new float[gridSize, gridSize];
        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
                occupancy[x, y] = 0.5f; // 초기 미탐색(unknown)

        mapTexture = new Texture2D(gridSize, gridSize);
        mapTexture.filterMode = FilterMode.Point;
        GetComponent<Renderer>().material.mainTexture = mapTexture;

        // 3D Quad에 표시할지 여부에 따라 렌더러 켜기/끄기
        GetComponent<Renderer>().enabled = showOnQuad;
        Redraw();

        // 자식 Quad 크기를 맵 크기에 맞춤
        transform.localScale = new Vector3(mapWorldSize, mapWorldSize, 1);
    }

    void Update()
    {
        if (autoUpdate && lidar != null)
            DrawLidarScan();
    }

    // LiDAR 스캔을 격자에 반영 (확률 갱신)
    void DrawLidarScan()
    {
        for (int i = 0; i < lidar.rayCount; i++)
        {
            float range = lidar.GetRange(i);
            if (float.IsInfinity(range)) continue;

            float angleRad = i * Mathf.Deg2Rad;

            // 로봇의 월드 회전 고려 (로봇이 돌면 레이저도 같이 돔)
            // 주의: LidarSensor가 TransformDirection으로 base_scan 회전을 반영해 측정하므로,
            //       맵핑에서도 반드시 동일한 기준(lidar.transform)을 사용해야 합니다.
            //       robotTransform(루트)을 쓰면 base_scan의 로컬 회전 오프셋이 어긋나 맵이 틀어질 수 있습니다.
            float worldAngle = lidar.transform.eulerAngles.y * Mathf.Deg2Rad + angleRad;
            Vector3 dir = new Vector3(Mathf.Sin(worldAngle), 0, Mathf.Cos(worldAngle));

            // 레이저 원점 위치 (로봇 위치) — 센서 기준으로 사용
            Vector3 robotPos = lidar.transform.position;
            Vector3 hitPoint = robotPos + dir * range;

            // 경로(free) 칸: 확률 감소, 끝점(장애물) 칸: 확률 증가
            int steps = Mathf.CeilToInt(range / resolution);
            for (int s = 1; s <= steps; s++)
            {
                Vector3 point = Vector3.Lerp(robotPos, hitPoint, (float)s / steps);
                if (s == steps)
                    UpdateOccupancy(point, hitIncrease);   // 장애물 표면
                else
                    UpdateOccupancy(point, -missDecrease); // 빈 공간
            }
        }
        Redraw();
    }

    void UpdateOccupancy(Vector3 worldPos, float delta)
    {
        // 월드 좌표 → 격자 좌표 (맵 중심 = transform.position)
        Vector3 mapCenter = transform.position;
        float localX = worldPos.x - mapCenter.x;
        float localZ = worldPos.z - mapCenter.z;

        int gx = Mathf.RoundToInt((localX / mapWorldSize + 0.5f) * gridSize);
        int gz = Mathf.RoundToInt((localZ / mapWorldSize + 0.5f) * gridSize);

        if (gx < 0 || gx >= gridSize || gz < 0 || gz >= gridSize) return;

        // 확률을 0~1 범위로 clamp하며 갱신
        occupancy[gx, gz] = Mathf.Clamp01(occupancy[gx, gz] + delta);
    }

    // 외부(RosBridge/ROS2)에서 접근: 격자 점유확률을 ROS OccupancyGrid 값(-1/0/100)으로 변환해 반환.
    // - data 인덱스는 ROS 규칙(행 우선, 좌하단 기점)과 일치시킨다: data[gz * gridSize + gx]
    // - gx = 월드 X 방향(열, 컬럼), gz = 월드 Z 방향(행, 로우)
    public sbyte[] GetOccupancyData()
    {
        sbyte[] data = new sbyte[gridSize * gridSize];
        for (int gz = 0; gz < gridSize; gz++)
        {
            for (int gx = 0; gx < gridSize; gx++)
            {
                float p = occupancy[gx, gz];
                if (p >= occupiedThreshold)
                    data[gz * gridSize + gx] = 100;      // occupied
                else if (p <= freeThreshold)
                    data[gz * gridSize + gx] = 0;        // free
                else
                    data[gz * gridSize + gx] = -1;       // unknown
            }
        }
        return data;
    }

    // 외부(RosBridge)에서 맵 원점(좌하단) 구하기: 맵 중심을 기준으로 -크기/2 만큼 이동한 점.
    // Unity 좌표 (x=오른쪽, z=위) ↔ ROS 좌표 (x=오른쪽, y=위) 대응.
    public Vector3 GetMapOrigin()
    {
        float half = mapWorldSize * 0.5f;
        return transform.position + new Vector3(-half, 0f, -half);
    }

    void Redraw()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                float p = occupancy[x, y];
                // RViz Map 색상 규칙과 동일:
                //   점유(occupied)  = 검정(black)
                //   자유(free)      = 흰색(white)
                //   미탐색(unknown) = 회색(gray)
                if (p >= occupiedThreshold)
                    mapTexture.SetPixel(x, y, Color.black);      // 장애물
                else if (p <= freeThreshold)
                    mapTexture.SetPixel(x, y, Color.white);      // 빈 공간
                else
                    mapTexture.SetPixel(x, y, Color.gray);       // 미탐색/불확실
            }
        }
        mapTexture.Apply();
    }

    // RViz처럼 화면 코너에 별도 2D 맵 패널을 그립니다.
    // - 흰 테두리(프레임) 안에 맵 텍스처가 표시되어, 3D 환경과 분리되어 보입니다.
    void OnGUI()
    {
        if (!showOnScreen || mapTexture == null) return;

        // 외곽 검은 테두리 + 안쪽 흰 배경(패널 프레임)
        GUI.DrawTexture(panelRect, Texture2D.blackTexture);
        Rect inner = new Rect(panelRect.x + 2, panelRect.y + 2,
                              panelRect.width - 4, panelRect.height - 4);
        GUI.DrawTexture(inner, Texture2D.whiteTexture);

        // 맵 텍스처를 패널 중앙에 정사각형으로 그리기 (비율 유지)
        float size = Mathf.Min(panelRect.width - 10, panelRect.height - 10);
        Rect mapRect = new Rect(panelRect.x + (panelRect.width - size) / 2f,
                                panelRect.y + (panelRect.height - size) / 2f,
                                size, size);
        GUI.DrawTexture(mapRect, mapTexture);
    }
}