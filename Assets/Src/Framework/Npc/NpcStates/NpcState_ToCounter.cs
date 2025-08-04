using UnityEngine;

/// 줄 맨 앞이 된 손님이 계산대 바로 앞 자리로 전진하는 상태
public class NpcState_ToCounter : IState
{
    readonly NpcController npc;
    readonly Transform counterNode;
    const string Walk = "Walking";

    public NpcState_ToCounter(NpcController npc, Transform node)
    {
        this.npc = npc;
        counterNode = node;
    }

    public void Enter()
    {
        npc.Agent.isStopped = false;
        npc.Agent.SetDestination(counterNode.position);
        npc.Animator.Play(Walk);
    }

    public void Tick()
    {
        if (npc.Agent.pathPending) return;

        bool arrived =
            npc.Agent.remainingDistance <= 0.05f &&
            npc.Agent.velocity.sqrMagnitude < 0.01f;
        if (!arrived) return;

        npc.Agent.isStopped = true;

        // 계산대 바라보도록 회전
        npc.transform.LookAt(counterNode);

        // 결제 제안 상태로 전환
        npc.stateMachine.SetState(
            new NpcState_OfferPayment(
                npc,
                Object.FindFirstObjectByType<QueueManager>(),
                npc.CashPrefab,
                npc.CardPrefab,
                counterNode));
    }

    public void Exit() { }
}
