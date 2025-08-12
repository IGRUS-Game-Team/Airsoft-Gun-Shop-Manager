using UnityEngine;
using UnityEngine.AI;

public class NpcState_QueueWait : IState
{
    private readonly NpcController npcController;
    private readonly QueueManager  queueManager;
    private Transform myNode;
    private bool itemPlacedOnCounter = false;

    private const string StandingAnim = "Standing";
    private const string WalkingAnim    = "Walking";

    public NpcState_QueueWait(NpcController npcController, QueueManager queueManager, Transform node)
    {
        this.npcController = npcController;
        this.queueManager  = queueManager;
        myNode = node;
    }

    public void Enter()
    {
        npcController.Agent.ResetPath();
        npcController.Agent.isStopped      = true;
        npcController.Agent.updateRotation = false; // 수동 회전

        // ★ 맨 앞이면 카운터, 아니면 "맨 앞자리(counterNode)" 방향으로 바라보기
        FaceToPoint(npcController.transform, queueManager.IsFront(npcController) ? queueManager.CounterTransform.position : queueManager.CounterNode.position);

        npcController.Animator.Play(StandingAnim);
    }

    public void Tick()
    {
        if (npcController.QueueTarget == null)
            return;

        // 자리 바뀌면 이동 시작
        if (npcController.QueueTarget != myNode)
        {
            npcController.Agent.ResetPath();
            npcController.Agent.isStopped      = false;
            npcController.Agent.updateRotation = true;
            npcController.Agent.SetDestination(npcController.QueueTarget.position);
            npcController.Animator.Play(WalkingAnim);
            myNode = npcController.QueueTarget;
            return;
        }

        // 도착 판정(간단)
        NavMeshAgent agent = npcController.Agent;
        
        if (agent.pathPending) return;
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid) return;

        // ── 속도→버킷→동적 여유 거리 계산 ─────────────────────────
        float speed = agent.velocity.magnitude;                      // m/s
        int bucket  = Mathf.Clamp(Mathf.CeilToInt(speed), 0, 12);    // 1~2 => 2
        float dynTol = 0.06f + (0.02f * bucket);                     // 기본 0.06 + 버킷당 0.02
        if (dynTol > 0.32f) dynTol = 0.32f;                          // 상한
        // ──────────────────────────────────────────────────────────

        // 2) 남은 거리 ≤ (stoppingDistance + 속도기반 여유)
        if (agent.remainingDistance <= agent.stoppingDistance + dynTol)
        {
            // 3) 거의 멈췄는지(완전 0 대신 작은 값)
            if (!agent.hasPath || agent.velocity.sqrMagnitude <= 0.002f)
            {
                npcController.Agent.isStopped = true;
                npcController.Agent.updateRotation = false;

                // ★ 서 있는 동안에도 같은 원칙으로 바라보기 유지
                FaceToPoint(npcController.transform, queueManager.IsFront(npcController) ? queueManager.CounterTransform.position : queueManager.CounterNode.position
            );

                npcController.Animator.Play(StandingAnim);
            // ★ 맨 앞이면: 상품을 카운터에 '한 번만' 올려두고(스캔 대기)
            if (queueManager.IsFront(npcController) && !itemPlacedOnCounter)
            {
                npcController.GetComponent<CarriedItemHandler>()?.PlaceToCounter();
                itemPlacedOnCounter = true;
            }

            // ★ 스캔/봉투 끝났는지 확인 → 끝났으면 이제 Offer 상태로 전환
            if (queueManager.IsFront(npcController)
                && CounterManager.Instance.IsReadyToPay(npcController))
                {
                    npcController.stateMachine.SetState(new NpcState_OfferPayment(npcController, queueManager.CounterTransform));
                    return;
                }
            }

        }
        
        // ★ 스캔/봉투 끝났는지 확인 → 끝났으면 이제 Offer 상태로 전환
        if (queueManager.IsFront(npcController)
        && CounterManager.Instance.IsReadyToPay(npcController))
        {
            npcController.stateMachine.SetState(
            new NpcState_OfferPayment(npcController, queueManager.CounterTransform));
            return;
        }
    }

    public void Exit()
    {
        npcController.Agent.updateRotation = true;
    }

    // ── 로컬 유틸: 특정 지점(월드 좌표)을 향해 부드럽게 회전 ──
    private static void FaceToPoint(Transform me, Vector3 worldPoint, float degPerSec = 540f)
    {
        Vector3 dir = worldPoint - me.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion target = Quaternion.LookRotation(dir);
        me.rotation = Quaternion.RotateTowards(me.rotation, target, degPerSec * Time.deltaTime);
    }
}