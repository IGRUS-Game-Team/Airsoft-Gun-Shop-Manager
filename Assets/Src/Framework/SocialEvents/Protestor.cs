using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Protestor : MonoBehaviour
{
    [Header("애니 이름")]
    [SerializeField] string walkAnim   = "Walking"; // 프리팹의 걷기 클립 이름
    [SerializeField] string picketAnim = "Picket";  // 피켓 흔드는 클립 이름(프로젝트에 맞춰 변경)

    [Header("도착 판정")]
    [SerializeField] float stopDistance = 0.35f;

    [Header("피켓(선택)")]
    [SerializeField] Transform handSocket;     // 손 본에 넣어둔 소켓
    [SerializeField] GameObject picketPrefab;  // 있으면 손에 붙여줌

    NavMeshAgent agent;
    Animator animator;

    Vector3 rallyPos;
    Transform exitPoint;
    float protestEndTime; // 0이면 무한
    bool atRally;
    bool leaving;

    public void Init(Vector3 rallyPos, Transform exitPoint, float protestDurationSeconds)
    {
        this.rallyPos  = rallyPos;
        this.exitPoint = exitPoint;
        this.protestEndTime = (protestDurationSeconds <= 0f)
            ? 0f
            : Time.time + protestDurationSeconds;

        agent  = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        // 피켓 장착
        if (picketPrefab != null && handSocket != null)
            Instantiate(picketPrefab, handSocket, false);

        // 이동 시작
        agent.isStopped = false;
        agent.updateRotation = true;
        agent.SetDestination(rallyPos);
        if (animator != null && !string.IsNullOrEmpty(walkAnim))
            animator.Play(walkAnim);
    }

    void Update()
    {
        if (leaving) return;

        // 랠리 도착 → 멈추고 피켓 루프
        if (!atRally)
        {
            if (!agent.pathPending && agent.remainingDistance <= stopDistance)
            {
                agent.isStopped = true;
                atRally = true;
                if (animator != null && !string.IsNullOrEmpty(picketAnim))
                    animator.Play(picketAnim);
            }
            return;
        }

        // 시간형: 끝나면 떠남 / 무한형: 외부에서 Dismiss 호출될 때까지
        if (protestEndTime > 0f && Time.time >= protestEndTime)
            Leave();
    }

    public void Dismiss() => Leave();

    void Leave()
    {
        if (leaving) return;
        leaving = true;

        if (exitPoint != null)
        {
            agent.isStopped = false;
            agent.SetDestination(exitPoint.position);
            if (animator != null && !string.IsNullOrEmpty(walkAnim))
                animator.Play(walkAnim);
            Destroy(gameObject, 20f); // 안전 파괴
        }
        else
        {
            Destroy(gameObject);
        }
    }
}