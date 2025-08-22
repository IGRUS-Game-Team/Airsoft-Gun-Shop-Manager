using UnityEngine;
using UnityEngine.AI;

public class NpcState_ToRange : IState
{
    private readonly NpcController npc;

    public NpcState_ToRange(NpcController npc) { this.npc = npc; }

    public void Enter()
    {
        var agent = npc.Agent;
        agent.updateRotation = true;
        agent.isStopped = false;
        agent.SetDestination(npc.TargetLane.StandPosition);
        npc.Animator.Play("Walking"); // 필요시 애니메이션 이름 바꿔
    }

    public void Tick()
    {
        var agent = npc.Agent;
        if (agent.pathPending) return;
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid) return;

        if (agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude <= 0.002f)
            {
                agent.updateRotation = false;

                // 레인지 방향을 정면으로
                Vector3 lookDir = npc.TargetLane.Forward; 
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.0001f)
                    npc.transform.rotation = Quaternion.LookRotation(lookDir);

                npc.stateMachine.SetState(new NpcState_Shoot(npc));
            }
        }
    }

    public void Exit() { }
}