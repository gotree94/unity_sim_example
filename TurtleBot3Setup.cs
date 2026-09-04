using UnityEngine;

// ############################################################
// # TurtleBot3Setup
// # 역할: URDF Importer가 만든 ArticulationBody(관절 물리) 체계를
// #       일반 Rigidbody(단일 물리) 체계로 일괄 전환합니다.
// #       Play 시작 시 Awake()에서 한 번 자동 실행됩니다.
// ############################################################
public class TurtleBot3Setup : MonoBehaviour
{
    // ---------- [물리 설정] ----------
    // 로봇 전체 질량(kg). TurtleBot3 Burger 기본값은 0.826kg.
    // 가속/관성/바닥 마찰 반응에 영향을 줍니다. 가벼울수록 관성 영향이 작아 잘 미끄러집니다.
    public float robotMass = 0.826f;

    // 중력 적용 여부(true면 플레이 시 바닥으로 떨어짐). 바닥(Ground)이 없으면추락하므로
    // 반드시 씬에 "Ground" 오브젝트가 있어야 합니다.
    public bool useGravity = true;

    // ---------- [바퀴 설정] ----------
    // 바퀴 반지름(m). TurtleBot3 Burger 기본 휠 반지름 0.033m.
    // Capsule Collider의 radius 값으로 사용됩니다.
    public float wheelRadius = 0.033f;

    // 바퀴 폭/두께(m). Capsule Collider의 height(길이)로 사용됩니다.
    public float wheelWidth = 0.018f;

    // Awake: 오브젝트가 활성화되는 최초 시점(Play 직전)에 한 번 실행됩니다.
    // 전환 순서가 중요합니다: ArticulationBody를 먼저 제거해야 Rigidbody가 충돌 없이 적용됩니다.
    void Awake()
    {
        RemoveArticulationBodies(); // 1) URDF 관절 물리(ArticulationBody) 제거
        SetupRigidbody();           // 2) 단일 Rigidbody 추가/설정
        ReplaceWheelColliders();    // 3) 바퀴/Caster/LiDAR 충돌체 정리
        SetupGround();              // 4) 바닥 물리 재질 적용
        AutoAttachLidarSensor();    // 5) base_scan에 LidarSensor 없으면 자동 부착
        Debug.Log("TurtleBot3 Setup 완료! Rigidbody 방식으로 전환됨.");
    }

    // 함수: ArticulationBody(관절 물리)를 전부 제거
    // 이유: ArticulationBody는 다중 링크(관절) 기반 물리로 바퀴 구동/Ground 충돌이
    //       불안정합니다. Rigidbody 단일 물리로 바꿔야 MovePosition 조작이 안정적입니다.
    // 중요: UrdfJoint/UrdfInertial/Controller/JointControl 같은 URDF Importer 컴포넌트가
    //       ArticulationBody를 "참조"하고 있어, ArticulationBody만 먼저 지우면
    //       "depends on it" 오류(또는 Start()에서 IndexOutOfRange)가 발생합니다.
    //       그래서 ① URDF Importer 스크립트를 먼저 제거 → ② ArticulationBody를 제거 순서로 진행합니다.
    void RemoveArticulationBodies()
    {
        // ① URDF Importer 스크립트를 먼저 제거:
        //    - 클래스 이름에 "Urdf"가 포함된 컴포넌트 (UrdfJoint, UrdfInertial, UrdfVisual ...)
        //    - 네임스페이스가 "Unity.Robotics.UrdfImporter"로 시작하는 컴포넌트
        //      (Controller, FKRobot 등. 네임스페이스에 Controller가 있어 이름 검사로는 못 걸러냄)
        //    - JointControl (네임스페이스가 없어 이름으로만 판별)
        //    위 컴포넌트들은 ArticulationBody를 참조하므로 남겨두면 에러가 납니다.
        //    컴포넌트 간 상호 참조가 있을 수 있어, 남는 것이 없을 때까지 반복 제거.
        bool found = true;
        while (found)
        {
            found = false;
            foreach (var comp in GetComponentsInChildren<Component>(true))
            {
                if (comp == null) continue;
                System.Type t = comp.GetType();
                string typeName = t.Name;
                string ns = (t.Namespace != null) ? t.Namespace : "";

                // 사용자가 만든 TurtleBot3Controller/TurtleBot3Setup은 네임스페이스가 없고
                // 이름도 위 패턴과 겹치지 않으므로 지워지지 않습니다 (안전).
                bool isUrdfImporter =
                    typeName.Contains("Urdf") ||
                    ns.StartsWith("Unity.Robotics.UrdfImporter") ||
                    typeName == "JointControl";

                if (isUrdfImporter)
                {
                    DestroyImmediate(comp);
                    found = true;
                    break;
                }
            }
        }

        // ② 이제 참조 컴포넌트가 없으므로 ArticulationBody를 안전하게 제거.
        ArticulationBody[] abs = GetComponentsInChildren<ArticulationBody>(true);
        foreach (var ab in abs)
            DestroyImmediate(ab);

        Debug.Log($"ArticulationBody/UrdfJoint 제거 완료: {abs.Length}개");
    }

    // 함수: 루트 오브젝트(turtlebot3_burger)에 Rigidbody를 추가하고 물리 값을 설정
    void SetupRigidbody()
    {
        // 이미 Rigidbody가 있으면 재사용, 없으면 새로 추가
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.mass = robotMass;         // 질량(kg) → 가속/충돌 반응 결정
        rb.useGravity = useGravity;  // 중력 적용 여부 (true면 추락 → Ground 필수)
        rb.isKinematic = false;      // false = 물리엔진이 추진/충돌을 처리.
                                     // true로 하면 외부 힘/중력이 무시되고 MovePosition만으로 이동하게 됨.
        rb.drag = 0f;                // 선형 마찰(공기저항). 0 = 감속 없음(정확한 이동)
        rb.angularDrag = 0.05f;      // 회전 마찰(각속도 감쇠). 살짝 둬서 회전이 과도하게 이어지지 않게 함
    }

    // 함수: 바퀴/Caster/LiDAR의 충돌체를 정리하고 바퀴에 회전용 Collider를 새로 배치
    // 이유: URDF 임포트 시 Mesh Collider가 붙는데, 복잡한 메시는 물리 연산이 무겁고
    //       바퀴 굴림이 안정적으로 표현되지 않습니다. 단순한 Capsule Collider로 교체합니다.
    void ReplaceWheelColliders()
    {
        // 교체 대상 바퀴 링크 이름 (URDF의 wheel_left_link / wheel_right_link)
        string[] wheelNames = { "wheel_left_link", "wheel_right_link" };

        foreach (string wheelName in wheelNames)
        {
            // 자식 계층에서 해당 이름의 링크를 재귀 탐색
            Transform wheelTransform = FindChildRecursive(transform, wheelName);
            if (wheelTransform == null) continue; // 링크가 없으면 건너뜀

            // 기존 Mesh Collider 자식들을 제거
            foreach (Transform child in wheelTransform)
            {
                Collider col = child.GetComponent<Collider>();
                if (col != null)
                    DestroyImmediate(col);
            }

            // 바퀴 링크에 Capsule Collider 추가 (원기둥형 충돌체)
            CapsuleCollider capsule = wheelTransform.gameObject.AddComponent<CapsuleCollider>();
            capsule.radius = wheelRadius;   // 반지름 = wheelRadius (0.033m)
            capsule.height = wheelWidth;    // 길이 = wheelWidth (0.018m)
            capsule.direction = 2;          // 2 = Z축 방향으로 세움 (바퀴 굴림 축)
            capsule.center = Vector3.zero;  // 링크 중심에 배치

            // 바퀴용 마찰 재질: 구르면서 미끄러지지 않도록 마찰을 높게 설정
            PhysicMaterial mat = new PhysicMaterial("WheelPhysMat");
            mat.dynamicFriction = 0.8f;  // 움직일 때(미끄러질 때) 마찰 계수
            mat.staticFriction = 0.8f;   // 정지 상태에서 움직이기 시작할 때 마찰 계수
            mat.bounciness = 0f;         // 반발력 0 (바닥에 튀지 않음)
            mat.frictionCombine = PhysicMaterialCombine.Maximum; // 바닥과의 마찰은 둘 중 큰 값을 사용
            capsule.material = mat;
        }

        // 캐스터(뒤쪽 보조바퀴)의 Mesh Collider 제거 — 불필요한 충돌면이 굴림을 방해하지 않도록
        Transform casterTransform = FindChildRecursive(transform, "caster_back_link");
        if (casterTransform != null)
            foreach (Transform child in casterTransform)
            { Collider c = child.GetComponent<Collider>(); if (c != null) DestroyImmediate(c); }

        // LiDAR(base_scan)의 Mesh Collider 제거 — 장애물 감지용이므로 충돌체가 필요 없음
        Transform baseScanTransform = FindChildRecursive(transform, "base_scan");
        if (baseScanTransform != null)
            foreach (Transform child in baseScanTransform)
            { Collider c = child.GetComponent<Collider>(); if (c != null) DestroyImmediate(c); }
    }

    // 함수: 씬의 "Ground"(이름이 정확히 Ground인 Plane)에 물리 재질을 적용
    // 이유: 바퀴와 바닥 사이의 마찰을 조절해 로봇이 미끄러지지 않고 안정적으로 굴러가게 함.
    //       이름이 "Ground"가 아니거나 오브젝트가 없으면 아무 일도 하지 않고 종료합니다.
    void SetupGround()
    {
        // 이름이 정확히 "Ground"인 오브젝트를 찾습니다.
        GameObject ground = GameObject.Find("Ground");
        if (ground == null) return; // 없으면(바닥 미생성) 함수 종료 → 이 경우 로봇이 추락합니다!

        // 바닥용 마찰 재질: 바퀴보다 낮은 마찰로 미끄러짐을 줄이고 굴림을 돕습니다.
        PhysicMaterial groundMat = new PhysicMaterial("GroundPhysMat");
        groundMat.dynamicFriction = 0.4f;  // 움직일 때 마찰
        groundMat.staticFriction = 0.4f;   // 정지 마찰
        groundMat.bounciness = 0f;         // 반발력 0
        groundMat.frictionCombine = PhysicMaterialCombine.Average; // 바퀴와의 마찰은 평균값 사용

        // Ground가 Mesh Collider면 그 재질을, 아니면 Box Collider의 재질을 교체
        MeshCollider mc = ground.GetComponent<MeshCollider>();
        if (mc != null)
        {
            mc.material = groundMat;
        }
        else
        {
            BoxCollider bc = ground.GetComponent<BoxCollider>();
            if (bc != null)
                bc.material = groundMat;
        }
    }

    // 함수: base_scan에 LidarSensor(2D LiDAR)를 아직 부착하지 않았다면 자동으로 추가
    // 이유: 06단계에서 LiDAR 시각화(녹색 레이저 링)를 보려면 base_scan 오브젝트에
    //       LidarSensor 컴포넌트가 있어야 합니다. 수동으로 Add Component를 빼먹어도
    //       Play 시 자동으로 붙도록 해, 항상 레이저가 동작하게 합니다.
    // 참고: 이미 직접 추가해 둔 경우 중복하지 않도록 GetComponent로 먼저 확인합니다.
    void AutoAttachLidarSensor()
    {
        Transform baseScanTransform = FindChildRecursive(transform, "base_scan");
        if (baseScanTransform == null) // base_scan 링크가 없으면 부착 불가 → 종료
        {
            Debug.LogWarning("[TurtleBot3Setup] base_scan 링크를 찾지 못해 LidarSensor를 부착하지 않았습니다.");
            return;
        }

        // 이미 LidarSensor가 있으면 중복 부착하지 않음
        if (baseScanTransform.GetComponent<LidarSensor>() != null)
            return;

        // base_scan에 LidarSensor 컴포넌트 자동 추가
        baseScanTransform.gameObject.AddComponent<LidarSensor>();
        Debug.Log($"[TurtleBot3Setup] base_scan에 LidarSensor 자동 부착 완료.");
    }

    // 보조 함수: 오브젝트 계층을 재귀적으로 탐색해 주어진 이름의 Transform을 찾아 반환.
    //            찾지 못하면 null을 반환. (바퀴/캐스터/LiDAR 링크를 이름으로 찾는 데 사용)
    Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
}
