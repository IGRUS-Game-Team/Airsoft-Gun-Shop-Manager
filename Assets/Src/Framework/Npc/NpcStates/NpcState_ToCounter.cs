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
        npcController.Agent.isStopped = false;
        npcController.Agent.SetDestination(counterNode.position);
        npcController.Animator.Play(WalkAnim);
    }

    public void Tick()
    {
        // 경로 계산 중이면 대기 (기존 유지)
        if (npcController.Agent.pathPending)
            return;

        // ===== 도착 체크(허용 반경) + 스냅만 변경 =====
        const float ARRIVE_EPS = 0.35f; // 씬 보며 0.25~0.4 사이 조정

        Vector3 targetPos = counterNode.position;
        float sqrDist = (npcController.transform.position - targetPos).sqrMagnitude;

        // 위치 거리 OR remainingDistance 둘 중 하나로 도착 인정
        bool inRange =
            sqrDist <= ARRIVE_EPS * ARRIVE_EPS ||
            npcController.Agent.remainingDistance <=
                Mathf.Max(npcController.Agent.stoppingDistance, ARRIVE_EPS);

        if (!inRange)
            return;

        // NavMesh 위 안전 스냅(Warp) + 경로 정지
        NavMeshAgent agent = npcController.Agent;
        agent.isStopped = true;
        agent.ResetPath();

        Vector3 snapPos = targetPos;
        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 0.5f, NavMesh.AllAreas))
            snapPos = hit.position;
            agent.Warp(snapPos);

        // ===== 아래는 기존 로직 그대로 =====
        npcController.Agent.updateRotation = false;
        npcController.transform.LookAt(counterNode);

        // 결제 제안 상태로 전환 (기존 그대로)
        npcController.stateMachine.SetState(new NpcState_OfferPayment(npcController, counterNode));
    }

    public void Exit() { }
}