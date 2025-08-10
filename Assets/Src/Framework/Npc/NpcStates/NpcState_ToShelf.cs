using UnityEngine;
using UnityEngine.AI;

// NPC가 선반 위치로 이동했다가 도착하면 물건 집기 대기 상태로 전환
public class NpcState_ToShelf : IState
{
    private readonly NpcController npcController;    // 이 상태를 수행할 NPC
    private const string WalkingAnim = "Walking";    // 걷기 애니메이션 이름

    public NpcState_ToShelf(NpcController npcController)
    {
        this.npcController = npcController;
    }

    public void Enter()
    {
        NavMeshAgent agent = npcController.Agent;

        // StandPoint을 목적지로 설정
        ShelfSlot slotComp = npcController.targetShelfSlot.GetComponent<ShelfSlot>();
        Transform stand = slotComp.StandPoint;
        Vector3 dest = stand.position;

        agent.SetDestination(dest);
        npcController.Animator.Play(WalkingAnim);
    }

    public void Tick()
{

    NavMeshAgent agent = npcController.Agent;

    if (agent.pathPending) return;
    if (agent.pathStatus == NavMeshPathStatus.PathInvalid) return;

    // ── 속도→버킷→동적 여유 거리 계산 ─────────────────────────
    float speed = agent.velocity.magnitude;                      // m/s
    int bucket  = Mathf.Clamp(Mathf.CeilToInt(speed), 0, 12);    // 1~2 => 2
    float dynTol = 0.06f + (0.02f * bucket);                     // 기본 0.06 + 버킷당 0.02
    if (dynTol > 0.32f) dynTol = 0.32f;                          // 상한
    // ──────────────────────────────────────────────────────────

    // 경로가 유효하고, 남은 거리 ≤ 정지 기준(+속도 기반 여유)
    if (agent.remainingDistance <= agent.stoppingDistance + dynTol)
    {
        // 거의 멈췄으면 도착으로 인정
        if (!agent.hasPath || agent.velocity.sqrMagnitude <= 0.002f)
        {
            Debug.Log("선반 도착 했음");

            // 자동 회전 끄고, 선반 쪽을 바라보게
            agent.updateRotation = false;

            Vector3 shelfForward = npcController.targetShelfSlot.forward;
            Vector3 lookDirection = new Vector3(-shelfForward.x, 0f, -shelfForward.z);

            if (lookDirection.sqrMagnitude >= 0.0001f)
            {
                npcController.transform.rotation = Quaternion.LookRotation(lookDirection);
            }

            // 다음 상태 전환
            agent.updateRotation = true;
            npcController.stateMachine.SetState(new NpcState_PickItem(npcController));
        }
    }
}
    public void Exit() {}
}