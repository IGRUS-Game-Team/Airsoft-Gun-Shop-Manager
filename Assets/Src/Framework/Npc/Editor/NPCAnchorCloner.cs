// Assets/Editor/NPCAnchorCloner.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class NPCAnchorCloner : EditorWindow
{
    private GameObject reference;        // 기준 NPC (AgentRoot)
    private bool copyNavMeshAgent = true;
    private bool alignFeetToZero  = true;
    private bool alignForwardYaw  = true;

    [MenuItem("Tools/NPC/Anchor Cloner")]
    public static void Open() => GetWindow<NPCAnchorCloner>("NPC Anchor Cloner");

    void OnGUI()
    {
        EditorGUILayout.LabelField("1) 기준 NPC(AgentRoot) 지정", EditorStyles.boldLabel);
        reference = (GameObject)EditorGUILayout.ObjectField("Reference", reference, typeof(GameObject), true);
        copyNavMeshAgent = EditorGUILayout.Toggle("Copy NavMeshAgent settings", copyNavMeshAgent);
        alignFeetToZero  = EditorGUILayout.Toggle("Move feet to (0,0,0)", alignFeetToZero);
        alignForwardYaw  = EditorGUILayout.Toggle("Match forward (+Z yaw)", alignForwardYaw);

        EditorGUILayout.Space(8);

        using (new EditorGUI.DisabledScope(reference == null))
        {
            if (GUILayout.Button("Set Reference = Selected"))
                reference = Selection.activeGameObject;

            if (GUILayout.Button("Apply To Selection"))
                ApplyToSelection();
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox("선택 요령:\n- 기준 NPC = NavMeshAgent가 붙어 있는 루트(AgentRoot)\n- 대상 NPC들도 같은 구조(AgentRoot/Model)면 가장 깔끔함.\n- 구조가 달라도 자동으로 Animator/Renderer가 달린 모델 자식을 찾아서 맞춰줍니다.", MessageType.Info);
    }

    void ApplyToSelection()
    {
        if (reference == null) { Debug.LogWarning("Reference가 비어있음"); return; }

        var refRoot  = FindAgentRoot(reference);
        if (refRoot == null) { Debug.LogError("Reference에서 NavMeshAgent 루트를 못 찾음"); return; }

        var refModel = FindModel(refRoot);
        if (refModel == null) { Debug.LogError("Reference에서 Model(Animator/Renderer)이 없음"); return; }

        // 기준 값들
        Vector3 refFwd = refModel.forward; refFwd.y = 0f; refFwd = refFwd.sqrMagnitude > 1e-4f ? refFwd.normalized : Vector3.forward;
        var refAgent = refRoot.GetComponent<NavMeshAgent>();

        foreach (var go in Selection.gameObjects)
        {
            if (go == reference) continue;

            var tgtRoot  = FindAgentRoot(go);
            if (tgtRoot == null) { Debug.LogWarning($"Skip: {go.name} (NavMeshAgent 루트 없음)"); continue; }

            var tgtModel = FindModel(tgtRoot);
            if (tgtModel == null) { Debug.LogWarning($"Skip: {go.name} (Model 못 찾음)"); continue; }

            Undo.RecordObject(tgtModel, "Align Model");
            Undo.RecordObject(tgtRoot,  "Align Root");

            // 1) 발 중앙을 루트 원점으로
            if (alignFeetToZero)
            {
                Vector3 feet = GetFeetMidWorld(tgtModel);
                Vector3 delta = feet - tgtRoot.position;
                tgtModel.position -= delta; // 발을 (root.position)으로 이동
            }

            // 2) yaw 정렬(+Z를 기준 NPC와 동일 yaw로)
            if (alignForwardYaw)
            {
                Vector3 curr = tgtModel.forward; curr.y = 0f;
                if (curr.sqrMagnitude > 1e-4f)
                {
                    Quaternion yaw = Quaternion.FromToRotation(curr.normalized, refFwd);
                    tgtModel.rotation = yaw * tgtModel.rotation;
                }
            }

            // 3) NavMeshAgent 세팅 복사
            var tgtAgent = tgtRoot.GetComponent<NavMeshAgent>() ?? Undo.AddComponent<NavMeshAgent>(tgtRoot.gameObject);
            if (copyNavMeshAgent && refAgent != null)
            {
                Undo.RecordObject(tgtAgent, "Copy NavMeshAgent");
                tgtAgent.baseOffset = 0f;                                // 표준: 발이 원점이므로 0
                tgtAgent.radius     = refAgent.radius;
                tgtAgent.height     = refAgent.height;
                tgtAgent.speed      = refAgent.speed;
                tgtAgent.angularSpeed = refAgent.angularSpeed;
                tgtAgent.acceleration = refAgent.acceleration;
                tgtAgent.stoppingDistance = refAgent.stoppingDistance;
                tgtAgent.obstacleAvoidanceType = refAgent.obstacleAvoidanceType;
                tgtAgent.areaMask   = refAgent.areaMask;
                tgtAgent.avoidancePriority = refAgent.avoidancePriority;
                tgtAgent.autoRepath = refAgent.autoRepath;
                // 필요하면 더 복사
            }

            // 4) Animator 옵션 통일
            var anim = tgtModel.GetComponentInChildren<Animator>();
            if (anim) { Undo.RecordObject(anim, "Animator root motion off"); anim.applyRootMotion = false; }

            Debug.Log($"[AnchorCloner] Applied to {go.name}", go);
        }
    }

    // ---- 유틸 ----
    static Transform FindAgentRoot(GameObject go)
    {
        var tr = go.transform;
        if (tr.GetComponent<NavMeshAgent>()) return tr;

        var ag = go.GetComponentInChildren<NavMeshAgent>(true);
        return ag ? ag.transform : tr;
    }

    static Transform FindModel(Transform root)
    {
        // 우선순위: 이름 'Model' → Animator 포함 → Renderer 포함 → 첫 자식
        var model = root.Find("Model");
        if (model) return model;

        var anim = root.GetComponentInChildren<Animator>(true);
        if (anim) return anim.transform;

        var rend = root.GetComponentInChildren<Renderer>(true);
        if (rend) return rend.transform;

        return root.childCount > 0 ? root.GetChild(0) : null;
    }

    static Vector3 GetFeetMidWorld(Transform model)
    {
        var anim = model.GetComponentInChildren<Animator>();
        if (anim && anim.isHuman)
        {
            var lf = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
            var rf = anim.GetBoneTransform(HumanBodyBones.RightFoot);
            if (lf && rf) return (lf.position + rf.position) * 0.5f;
        }
        // 휴머노이드가 아니면 렌더 바운드 하단을 사용
        Bounds b = GetRenderBounds(model);
        return new Vector3(b.center.x, b.min.y, b.center.z);
    }

    static Bounds GetRenderBounds(Transform t)
    {
        var rends = t.GetComponentsInChildren<Renderer>(true);
        Bounds b = new Bounds(t.position, Vector3.zero);
        bool has = false;
        foreach (var r in rends)
        {
            if (!has) { b = r.bounds; has = true; }
            else b.Encapsulate(r.bounds);
        }
        if (!has) b = new Bounds(t.position, Vector3.one);
        return b;
    }
}
#endif