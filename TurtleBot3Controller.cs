using UnityEngine;

// ############################################################
// # TurtleBot3Controller
// # 역할: 키보드(WASD) 입력을 받아 Rigidbody 기반으로 로봇을 이동/회전시킵니다.
// #       이동은 MovePosition, 회전은 MoveRotation으로 FixedUpdate에서 처리합니다.
// ############################################################
public class TurtleBot3Controller : MonoBehaviour
{
    // ---------- [이동 설정] ----------
    // 전진/후진 속도(m/s). TurtleBot3 Burger 실측 최대 선속도 0.22 m/s.
    // (MoveSpeed = 초당 이동 거리: 1이면 1m/s로 전진)
    public float moveSpeed = 0.22f;

    // 회전 속도(deg/s). TurtleBot3 Burger 실측 최대 각속도 2.84 rad/s ≈ 162.7 deg/s를
    // 각도 기준으로 환산한 값입니다. (RotationSpeed = 초당 회전 각도)
    public float rotationSpeed = 2.84f;

    private Rigidbody rb; // 물리 이동을 제어할 Rigidbody 참조

    // Start: Play 시작 시 한 번 실행. 이 오브젝트의 Rigidbody를 가져옵니다.
    // (TurtleBot3Setup이 Awake에서 Rigidbody를 추가하므로 반드시 그 이후에 실행됨)
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // FixedUpdate: 물리 엔진 스텝마다(기본 0.02초 간격) 호출.
    //              MovePosition/MoveRotation은 물리 스텝 안에서만 동작하므로 여기서 처리합니다.
    void FixedUpdate()
    {
        if (rb == null) return; // Rigidbody가 없으면 동작하지 않음 (설정 오류 방지)

        // 입력값 받기
        // Vertical   : W(앞)=+1, S(뒤)=-1  → 전진/후진
        // Horizontal : D(오른쪽)=+1, A(왼쪽)=-1 → 좌/우 회전
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        // 이동 벡터 계산: 로봇이 향하는 방향(transform.forward) × 입력 × 속도
        // -1과 1 사이의 moveInput 덕분에 속도를 그대로 곱해 부드럽게 가감속됩니다.
        Vector3 moveDirection = transform.forward * moveInput * moveSpeed;

        // 물리 이동: 현재 위치에서 이동량(속도 × 물리 스텝시간)만큼 다음 위치로 이동.
        // Rigidbody 이동이므로 중력/충돌 영향은 물리엔진이 유지한 채 위치만 이동합니다.
        rb.MovePosition(rb.position + moveDirection * Time.fixedDeltaTime);

        // 회전 각도 계산: 입력(turnInput) × 회전속도(deg/s) × 스텝시간(초) → 스텝당 회전 각도
        float rotation = turnInput * rotationSpeed * Mathf.Rad2Deg * Time.fixedDeltaTime;

        // 물리 회전: 현재 회전에 Y축(좌우) 회전을 곱해 로봇을 돌립니다.
        // Euler(0, rotation, 0) = X·Z축은 0, Y축(방위각)만 회전 = 욜로 회전(터틀봇처럼 제자리 회전)
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, rotation, 0f));
    }
}
