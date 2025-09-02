using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BouncerEscort : MonoBehaviour
{
    [SerializeField] string walkingState = "Walking"; // 네 컨트롤러의 걷기 상태 이름
    
    enum State { ToStance, Talking, EscortToExit, Done }

    NavMeshAgent _agent;
    Animator _anim;

    ProtestDirector _director;
    Transform _exitT;
    Transform _faceLookTarget;     // 시위대가 바라보는 기준(간판/매장 정면)
    Vector3 _rallyCenter;
    Vector3 _stancePoint;          // 바운서가 설 지점(군중 앞)

    float _orderRadius;

    [Header("애니메이션")]
    [SerializeField] string talkTrigger = "Talk";
    [SerializeField, Min(0.2f)] float minTalkSeconds = 1.2f;

    [Header("스탠스(정지 지점) 설정")]
    [SerializeField, Min(0.5f)] float standAhead = 2.0f; // 군중이 보는 방향으로 얼마나 앞에 설지
    [SerializeField] float lateralOffset = 0f;           // 좌우로 약간 치우치고 싶으면(+오, -왼)

    State _state;
    bool _ordered;

    public void Setup(
        ProtestDirector director,
        Vector3 rallyCenter,
        Transform exitT,
        float orderRadius,
        float escortSpeed,
        Transform faceLookTarget,   // ★ 추가: 시위대가 바라보는 기준
        float standAhead = 2f,
        float lateralOffset = 0f
    )
    {
        _director        = director;
        _rallyCenter     = rallyCenter;
        _exitT           = exitT;
        _orderRadius     = Mathf.Max(0f, orderRadius);
        _faceLookTarget  = faceLookTarget;
        this.standAhead  = Mathf.Max(0.5f, standAhead);
        this.lateralOffset = lateralOffset;

        _agent = GetComponent<NavMeshAgent>();
        _agent.stoppingDistance = 0.4f;
        _agent.updateRotation = true;

        _anim = GetComponent<Animator>();
        if (!_anim) _anim = GetComponentInChildren<Animator>();
        if (_anim) _anim.applyRootMotion = false;

        // 스탠스 지점 계산: rallyCenter에서 "시위대가 보는 방향"으로 standAhead만큼 전진 + 좌우 오프셋
        var forward = GetCrowdForwardXZ(_rallyCenter, _faceLookTarget);
        var right   = new Vector3(forward.z, 0f, -forward.x);
        var rawPos  = _rallyCenter + forward * this.standAhead + right * this.lateralOffset;

        _stancePoint = SampleOnNavmesh(rawPos, 2.5f); // NavMesh 위로 스냅

        GoTo(_stancePoint);
        _state = State.ToStance;
    }

    void Update()
    {
        switch (_state)
        {
            case State.ToStance:
                if (Reached(_stancePoint, 0.7f))
                {
                    BeginTalk();
                }
                break;

            case State.EscortToExit:
                if (_exitT && Reached(_exitT.position, 1.0f))
                {
                    _state = State.Done;
                    Destroy(gameObject, 0.4f);
                }
                break;
        }
    }

    void BeginTalk()
    {
        // 이동 멈추고, 군중을 바라보도록 회전(수평만)
        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;
        _agent.updateRotation = false;

        FacePointHorizontally(_rallyCenter);

        if (_anim && !string.IsNullOrEmpty(talkTrigger))
            _anim.SetTrigger(talkTrigger);

        StartCoroutine(TalkThenOrderAndEscort());
        _state = State.Talking;
    }

    IEnumerator TalkThenOrderAndEscort()
    {
        yield return new WaitForSeconds(minTalkSeconds);

        if (!_ordered) DoOrder();
        yield return new WaitForSeconds(0.5f);

        // 다시 이동/회전 활성화 후 출구로
        _agent.isStopped = false;
        _agent.updateRotation = true;

        if (_exitT) GoTo(_exitT.position);

        if (_anim)
        {
            _anim.ResetTrigger(talkTrigger);                 // 혹시 잔여 트리거 클린업
            if (!string.IsNullOrEmpty(walkingState))
            _anim.CrossFade(walkingState, 0.1f);         // 걷기로 전환
        }

        _state = State.EscortToExit;
    }

    public void AnimEvent_OrderNow()
    {
        if (_state != State.Talking || _ordered) return;
        DoOrder();
    }

    void DoOrder()
    {
        _ordered = true;

        var list = _director?.GetSpawned();
        if (list == null) return;

        var my = transform.position;
        foreach (var p in list)
        {
            if (!p) continue;
            if (_orderRadius <= 0f || Vector3.Distance(my, p.transform.position) <= _orderRadius)
                p.Dismiss();
        }
    }

    // ── 유틸 ──────────────────────────────────────────────────────────
    void GoTo(Vector3 pos)
    {
        if (_agent && _agent.isOnNavMesh) _agent.SetDestination(pos);
        else transform.position = pos;
    }

    bool Reached(Vector3 pos, float stopDist)
    {
        if (!_agent || !_agent.isOnNavMesh) return Vector3.Distance(transform.position, pos) <= stopDist;
        if (_agent.pathPending) return false;
        return _agent.remainingDistance <= Mathf.Max(stopDist, _agent.stoppingDistance + 0.05f);
    }

    static Vector3 GetCrowdForwardXZ(Vector3 rallyCenter, Transform faceLookTarget)
    {
        Vector3 fwd;
        if (faceLookTarget)
            fwd = (faceLookTarget.position - rallyCenter);
        else
            fwd = Vector3.forward;

        fwd.y = 0f;
        return fwd.sqrMagnitude > 0.0001f ? fwd.normalized : Vector3.forward;
    }

    static Vector3 SampleOnNavmesh(Vector3 pos, float maxDist)
    {
        return NavMesh.SamplePosition(pos, out var hit, maxDist, NavMesh.AllAreas) ? hit.position : pos;
    }

    void FacePointHorizontally(Vector3 target)
    {
        var dir = target - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        var rot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = rot;
    }
}