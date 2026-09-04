using UnityEngine;

public class OdometrySensor : MonoBehaviour
{
    [Header("바퀴 반경 (인치→m)")]
    public float wheelRadius = 0.033f;     // 33mm
    public float wheelBase = 0.160f;       // 바퀴 간 거리 (m), 좌우 0.08 + 0.08

    [Header("기준 Rigidbody (물리 보정용)")]
    public Rigidbody targetRb;

    [Header("출력")]
    // 실제 로봇이 발행하는 nav_msgs/Odometry 필드
    public Vector3 position;      // x, z 평면 위치 (y=0)
    public float yaw;             // 방향 (라디안)
    public Vector2 linearVel;     // 선속도 (전진 v, 후진)
    public float angularVel;      // 각속도 (rady/s)

    // 좌우 바퀴 회전 관찰용
    private Transform wheelLeft;
    private Transform wheelRight;
    private float lastLeftAngle;
    private float lastRightAngle;
    private Vector3 lastPos;

    void Start()
    {
        FindWheels();
        if (targetRb != null)
        {
            position = targetRb.position;
            lastPos = targetRb.position;
        }
    }

    void FindWheels()
    {
        // 05단계에서 만든 바퀴 transform
        wheelLeft = FindChildRecursive(transform, "wheel_left_link");
        wheelRight = FindChildRecursive(transform, "wheel_right_link");
        if (wheelLeft != null) lastLeftAngle = GetWheelAngle(wheelLeft);
        if (wheelRight != null) lastRightAngle = GetWheelAngle(wheelRight);
    }

    float GetWheelAngle(Transform wheel)
    {
        return wheel.localEulerAngles.x * Mathf.Deg2Rad;
    }

    void FixedUpdate()
    {
        if (targetRb == null) return;

        // --- 실시간 위치는 Rigidbody로부터 (정확) ---
        Vector3 rbPos = targetRb.position;
        position = new Vector3(rbPos.x, 0, rbPos.z);
        yaw = targetRb.rotation.eulerAngles.y * Mathf.Deg2Rad;

        // --- 속도는 Rigidbody 속도로 ---
        Vector3 vel = targetRb.velocity;
        // 로컬 전진 방향 성분
        Vector3 localVel = targetRb.transform.InverseTransformDirection(vel);
        linearVel = new Vector2(localVel.z, localVel.x); // 전진=z
        angularVel = targetRb.angularVelocity.y;

        // --- 바퀴 회전 관찰 (엔코더 개념) ---
        // 참고용: 실제 바퀴 회전량을 거리로 환산
        if (wheelLeft != null && wheelRight != null)
        {
            float dLeft = (GetWheelAngle(wheelLeft) - lastLeftAngle) * wheelRadius;
            float dRight = (GetWheelAngle(wheelRight) - lastRightAngle) * wheelRadius;
            // (여기선 물리 보정을 쓰므로 엔코더 거리는 표시만)
            lastLeftAngle = GetWheelAngle(wheelLeft);
            lastRightAngle = GetWheelAngle(wheelRight);
        }

        lastPos = position;
    }

    Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform found = FindChildRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }
}