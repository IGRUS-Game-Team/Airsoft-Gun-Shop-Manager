// // 수정일: 2025-08-06, 작성자: 이준서
// 설명: 줄 맨 앞이 된 손님이 계산대 바로 앞 자리로 이동한 뒤, 결제 제안 상태로 전환하는 상태
using UnityEngine;
using UnityEngine.AI;

public class NpcState_ToCounter : IState
{
    private readonly NpcController npcController;
    private readonly Transform counterNode;
    private const string WalkAnim = "Walking";

    public NpcState_ToCounter(NpcController npcController, Transform counterNode)
    {
        this.npcController = npcController;
        this.counterNode = counterNode;
    }

    public void Enter()
    {
        // 이동 재개 및 목적지 설정 (기존 로직 유지)
        npcController.Agent.updateRotation = true;
        this.npcController.Agent.isStopped = false;
        this.npcController.Agent.SetDestination(this.counterNode.position);
        this.npcController.Animator.Play(WalkAnim);
    }

    public void Tick()
    {
        // 경로 계산 중이면 대기 (기존 유지)
        if (this.npcController.Agent.pathPending)
            return;

        // ===== 도착 체크(허용 반경) + 스냅만 변경 =====
        const float ARRIVE_EPS = 0.35f; // 씬 보며 0.25~0.4 사이 조정

        Vector3 targetPos = this.counterNode.position;
        float sqrDist = (this.npcController.transform.position - targetPos).sqrMagnitude;

        // 위치 거리 OR remainingDistance 둘 중 하나로 도착 인정
        bool inRange =
            sqrDist <= ARRIVE_EPS * ARRIVE_EPS ||
            this.npcController.Agent.remainingDistance <=
                Mathf.Max(this.npcController.Agent.stoppingDistance, ARRIVE_EPS);

        if (!inRange)
            return;

        // NavMesh 위 안전 스냅(Warp) + 경로 정지
        var ag = this.npcController.Agent;
        ag.isStopped = true;
        ag.ResetPath();

        Vector3 snapPos = targetPos;
        if (NavMesh.SamplePosition(targetPos, out var hit, 0.5f, NavMesh.AllAreas))
            snapPos = hit.position;
        ag.Warp(snapPos);

        // ===== 아래는 기존 로직 그대로 =====
        this.npcController.Agent.updateRotation = false;
        this.npcController.transform.LookAt(this.counterNode);

        // 결제 제안 상태로 전환 (기존 그대로)
        this.npcController.stateMachine.SetState(
            new NpcState_OfferPayment(
                this.npcController,
                Object.FindFirstObjectByType<QueueManager>(),
                this.npcController.CashPrefab,
                this.npcController.CardPrefab,
                this.counterNode));
    }

    public void Exit() { }
}