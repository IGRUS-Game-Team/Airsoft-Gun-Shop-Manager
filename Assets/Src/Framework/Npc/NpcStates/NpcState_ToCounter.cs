// 수정일: 2025-08-06, 작성자: 이준서
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
        // 이동 재개 및 목적지 설정
        this.npcController.Agent.isStopped = false;
        this.npcController.Agent.SetDestination(this.counterNode.position);
        this.npcController.Animator.Play(WalkAnim);
    }

    public void Tick()
    {
        // 경로 계산 중이면 대기
        if (this.npcController.Agent.pathPending)
        {
            return;
        }

        // stoppingDistance 이내면 도착으로 간주
        if (this.npcController.Agent.remainingDistance > this.npcController.Agent.stoppingDistance)
        {
            return;
        }

        // 도착 처리
        this.npcController.Agent.isStopped = true;
        this.npcController.Agent.updateRotation = false;
        this.npcController.transform.LookAt(this.counterNode);

        // 결제 제안 상태로 전환
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
