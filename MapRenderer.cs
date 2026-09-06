using UnityEngine;

public class MapRenderer : MonoBehaviour
{
    [Header("맵 설정")]
    public int gridSize = 200;         // 200x200 픽셀
    public float resolution = 0.05f;   // 5cm/픽셀
    public Transform robotTransform;
    public LidarSensor lidar;

    [Header("연결")]
    public bool autoUpdate = true;

    private Texture2D mapTexture;
    private int[,] occupancy; // 0 free, 100 occupied, -1 unknown
    private float mapWorldSize;

    void Start()
    {
        mapWorldSize = gridSize * resolution; // 200 * 0.05 = 10m

        occupancy = new int[gridSize, gridSize];
        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
                occupancy[x, y] = -1; // 초기 미탐색

        mapTexture = new Texture2D(gridSize, gridSize);
        mapTexture.filterMode = FilterMode.Point;
        GetComponent<Renderer>().material.mainTexture = mapTexture;
        Redraw();

        // 자식 Quad 크기를 맵 크기에 맞춤
        transform.localScale = new Vector3(mapWorldSize, mapWorldSize, 1);
    }

    void Update()
    {
        if (autoUpdate && lidar != null)
            DrawLidarScan();
    }

    // LiDAR 스캔을 격자에 반영
    void DrawLidarScan()
    {
        for (int i = 0; i < lidar.rayCount; i++)
        {
            float range = lidar.GetRange(i);
            if (float.IsInfinity(range)) continue;

            float angleDeg = i; // 0~359
            float angleRad = angleDeg * Mathf.Deg2Rad;

            // 로봇의 월드 회전 고려 (로봇이 돌면 레이저도 같이 돔)
            // 주의: robotTransform의 Y 회전을 써야 로봇 방향이 반영됨. Quad(자기 자신) 회전이 아님!
            float worldAngle = robotTransform.eulerAngles.y * Mathf.Deg2Rad + angleRad;
            Vector3 dir = new Vector3(Mathf.Sin(worldAngle), 0, Mathf.Cos(worldAngle));

            // 로봇 위치 (맵 중심 기준 월드 좌표)
            Vector3 robotPos = robotTransform.position;

            // 장애물 끝점
            Vector3 hitPoint = robotPos + dir * range;

            // 로봇~장애물 라인 사이의 칸을 free(0)로, 장애물 칸을 occupied(100)로
            int steps = Mathf.CeilToInt(range / resolution);
            for (int s = 1; s <= steps; s++)
            {
                Vector3 point = Vector3.Lerp(robotPos, hitPoint, (float)s / steps);
                SetOccupancy(point, s == steps ? 100 : 0);
            }
        }
        Redraw();
    }

    void SetOccupancy(Vector3 worldPos, int value)
    {
        // 월드 좌표 → 격자 좌표 (맵 중심 = robotTransform 초기 위치)
        Vector3 mapCenter = transform.position;
        float localX = worldPos.x - mapCenter.x;
        float localZ = worldPos.z - mapCenter.z;

        int gx = Mathf.RoundToInt((localX / mapWorldSize + 0.5f) * gridSize);
        int gz = Mathf.RoundToInt((localZ / mapWorldSize + 0.5f) * gridSize);

        if (gx < 0 || gx >= gridSize || gz < 0 || gz >= gridSize) return;

        // occupied를 free로 덮어쓰지 않게 보존
        if (value == 100 || occupancy[gx, gz] == -1)
            occupancy[gx, gz] = value;
    }

    void Redraw()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                int v = occupancy[x, y];
                if (v == -1)
                    mapTexture.SetPixel(x, y, Color.gray);
                else if (v == 100)
                    mapTexture.SetPixel(x, y, Color.white);
                else
                    mapTexture.SetPixel(x, y, Color.black);
            }
        }
        mapTexture.Apply();
    }
}