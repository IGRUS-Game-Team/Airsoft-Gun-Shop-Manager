using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class Protestor : MonoBehaviour
{
    [Header("Sign(팻말) 세팅")]
    [SerializeField] GameObject signPrefab;          // ← 프리팹 Asset을 넣어줘
    [SerializeField] Transform  signSocket;          // ← 비워두면 자동으로 오른손 뼈/소켓 찾음
    [SerializeField] string     socketNameHint = "HandSocket"; // 있으면 우선 찾음
    [SerializeField] Vector3    signLocalPos;        // 손에 딱 맞게 로컬 오프셋
    [SerializeField] Vector3    signLocalEuler;
    [SerializeField] Vector3    signLocalScale = Vector3.one;
    [SerializeField] bool       disableSignPhysics = true;     // 리지드바디/콜라이더 꺼서 튀지 않게

    [Header("애니메이션 클립 이름")]
    [SerializeField] string walkAnim    = "Walking";
    [SerializeField] string protestAnim = "ProtestIdle";
    [SerializeField] string leaveAnim   = "Walking";

    [Header("도착/퇴장 판정")]
    [SerializeField, Min(0.1f)] float arriveDist = 0.35f;
    [SerializeField, Min(0.1f)] float exitDist   = 1.0f;

    NavMeshAgent agent;
    Animator     anim;

    Vector3   rallyPos;
    Transform exitT;
    Transform faceT;
    float     dwellSec;

    GameObject signInstance;    // ← 실제로 손에 붙여둘 인스턴스
    Coroutine  flow;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim  = GetComponent<Animator>();
    }

    public void Init(Vector3 rallyPos, Transform exitT, float dwellSec, Transform faceT = null)
    {
        this.rallyPos = rallyPos;
        this.exitT    = exitT;
        this.dwellSec = dwellSec;
        this.faceT    = faceT;

        if (flow != null) StopCoroutine(flow);
        flow = StartCoroutine(Flow());
    }

    IEnumerator Flow()
    {
        // 1) 매장 앞으로 이동(걷기, 팻말 OFF)
        SetSign(false);
        Play(walkAnim);
        MoveTo(rallyPos);
        while (!Reached(rallyPos, arriveDist)) yield return null;

        // 2) 도착(정면보기 + 팻말 ON + 시위 모션)
        agent.isStopped = true;
        Face(faceT ? faceT.position : transform.position + transform.forward);
        SetSign(true);
        Play(protestAnim);

        if (dwellSec > 0f) yield return new WaitForSeconds(dwellSec);
        else yield break; // 무한 대기 모드

        // 3) 떠나기(걷기, 팻말 OFF)
        Dismiss();
    }

    public void Dismiss()
    {
        if (!this || agent == null) return;
        StopAllCoroutines();
        flow = StartCoroutine(LeaveRoutine());
    }

    IEnumerator LeaveRoutine()
    {
        SetSign(false);
        agent.isStopped = false;
        Play(leaveAnim);
        if (exitT) MoveTo(exitT.position);
        while (exitT && !Reached(exitT.position, exitDist)) yield return null;
        Destroy(gameObject);
    }

    // ───── Sign 제어 ─────
    void EnsureSignReady()
    {
        if (signInstance) return;
        if (!signPrefab) return;

        var socket = ResolveSocket();
        if (!socket)
        {
            Debug.LogWarning("[Protestor] signSocket을 찾지 못했어. 루트에 붙일게(정렬 필요).", this);
            socket = transform;
        }

        signInstance = Instantiate(signPrefab, socket);
        var t = signInstance.transform;
        t.localPosition   = signLocalPos;
        t.localEulerAngles= signLocalEuler;
        t.localScale      = signLocalScale;

        if (disableSignPhysics)
        {
            foreach (var rb in signInstance.GetComponentsInChildren<Rigidbody>()) { rb.isKinematic = true; rb.detectCollisions = false; }
            foreach (var col in signInstance.GetComponentsInChildren<Collider>()) col.enabled = false;
        }

        signInstance.SetActive(false);
    }

    Transform ResolveSocket()
    {
        if (signSocket) return signSocket;

        // 1) 소켓 이름 힌트 우선
        if (!string.IsNullOrWhiteSpace(socketNameHint))
        {
            var s1 = transform.Find(socketNameHint);
            if (s1) return s1;
        }

        // 2) 휴머노이드면 오른손 뼈
        if (anim && anim.isHuman)
        {
            var rightHand = anim.GetBoneTransform(HumanBodyBones.RightHand);
            if (rightHand)
            {
                // 오른손 아래에 소켓 이름이 있으면 우선
                if (!string.IsNullOrWhiteSpace(socketNameHint))
                {
                    var child = rightHand.Find(socketNameHint);
                    if (child) return child;
                }
                return rightHand;
            }
        }

        // 3) 후보 이름들 대충 스캔
        string[] candidates = { "HandSocket", "RightHandSocket", "R_HandSocket", "RightHand", "mixamorig:RightHand" };
        foreach (var name in candidates)
        {
            var f = transform.Find(name);
            if (f) return f;
        }

        return null;
    }

    void SetSign(bool on)
    {
        EnsureSignReady();
        if (signInstance && signInstance.activeSelf != on)
            signInstance.SetActive(on);
    }

    // ───── 이동/공용 유틸 ─────
    void MoveTo(Vector3 dst)
    {
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
                transform.position = hit.position;
        }
        agent.isStopped = false;
        agent.SetDestination(dst);
    }

    bool Reached(Vector3 target, float dist)
    {
        if (agent.pathPending) return false;
        var d = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                 new Vector3(target.x, 0, target.z));
        return d <= dist && agent.remainingDistance <= dist + 0.1f;
    }

    void Face(Vector3 worldPos)
    {
        var dir = worldPos - transform.position; dir.y = 0;
        if (dir.sqrMagnitude > 1e-6f) transform.rotation = Quaternion.LookRotation(dir);
    }

    void Play(string clip)
    {
        if (!string.IsNullOrEmpty(clip)) anim.CrossFadeInFixedTime(clip, 0.15f);
    }
}