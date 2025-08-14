using UnityEngine;
using UnityEngine.AI;

// NPC가 물건을 집은 후 줄에 합류하거나 배회 상태로 전환하는 상태
public class NpcState_ToQueue : IState
{
    private readonly NpcController npcController;  // 대상 NPC
    private readonly QueueManager queueManager;    // 줄 관리 매니저
    private Vector3 queueSpot;                   // NPC가 서야 할 자리
    private Transform destination; // 최종 목적지
    private const string WalkingAnim = "Walking";  // 걷기 애니메이션 이름

    public NpcState_ToQueue(NpcController npcController, QueueManager queueManager)
    {
        this.npcController = npcController;
        this.queueManager = queueManager;
    }

    public void Enter()
    {
        npcController.targetShelfGroup?.Release();
        NavMeshAgent agent = npcController.Agent;

        if (!queueManager.TryEnqueue(npcController, out Transform assignedSpot) || assignedSpot == null)
        {
            npcController.Agent.updateRotation = true;
            npcController.Agent.isStopped = false;

            Transform[] wanderPoints = queueManager.WanderPoints;
            if (wanderPoints == null || wanderPoints.Length == 0)
            {
                npcController.stateMachine.SetState(new NpcState_Leave(npcController));
            }
            else
            {
                npcController.stateMachine.SetState(new NpcState_Wander(npcController, wanderPoints, 20f));
            }
            return;
        }

        destination = assignedSpot;
        queueSpot = assignedSpot.position;
        agent.SetDestination(queueSpot);
        npcController.Animator.Play(WalkingAnim);

    }

    public void Tick()
    {
        NavMeshAgent agent = npcController.Agent;

        if (agent.pathPending) return;
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid) return;

        // ── 속도→버킷→동적 여유 거리 계산 ─────────────────────────
        float speed = agent.velocity.magnitude;                      // m/s
        int bucket = Mathf.Clamp(Mathf.CeilToInt(speed), 0, 12);    // 1~2 => 2
        float dynTol = 0.06f + (0.02f * bucket);                     // 기본 0.06 + 버킷당 0.02
        if (dynTol > 0.32f) dynTol = 0.32f;                          // 상한
        // 추가 start
        Vector3 me = npcController.transform.position;
        float planarDist = Vector2.Distance(
        new Vector2(me.x, me.z),
        new Vector2(queueSpot.x, queueSpot.z)
        );

        bool nearByPath = agent.remainingDistance <= agent.stoppingDistance + dynTol;
        bool partialOK = agent.pathStatus == NavMeshPathStatus.PathPartial && planarDist <= dynTol + 0.05f;
      
        if (nearByPath || partialOK)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude <= 0.002f)
            {
                npcController.stateMachine.SetState(
                new NpcState_QueueWait(npcController, queueManager, destination));
            }
        }
    }

    public void Exit()
    {
        // 상태 종료 시 특별한 작업은 없음
    }
}