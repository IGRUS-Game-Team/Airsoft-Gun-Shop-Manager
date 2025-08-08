using UnityEngine;
using UnityEngine.AI;

// NPC가 선반 위치로 이동했다가 도착하면 물건 집기 대기 상태로 전환
public class NpcState_ToShelf : IState
{
    private const float ARRIVE_EPS = 0.35f;          // 허용 반경(씬에 맞게 0.25~0.4)
    private readonly NpcController npcController;    // 이 상태를 수행할 NPC
    private const string WalkingAnim = "Walking";    // 걷기 애니메이션 이름

    public NpcState_ToShelf(NpcController npcController)
    {
        this.npcController = npcController;
    }

    public void Enter()
    {
        var ag = npcController.Agent;
        ag.updateRotation = true;   // 이동 중엔 에이전트가 회전 맡음
        ag.isStopped      = false;

        // ✅ 목적지: StandPoint가 있으면 그걸 최우선 사용
        var slotComp = npcController.targetShelfSlot.GetComponent<ShelfSlot>();
        Transform stand = (slotComp != null) ? slotComp.StandPoint : null;
        Vector3 dest    = (stand != null) ? stand.position : npcController.targetShelfSlot.position;

        ag.SetDestination(dest);
        npcController.Animator.Play(WalkingAnim);
    }

    public void Tick()
    {
        var ag = npcController.Agent;
        if (ag.pathPending) return;

        // ✅ 도착 판정 대상도 StandPoint 우선
        var slotComp = npcController.targetShelfSlot.GetComponent<ShelfSlot>();
        Transform stand = (slotComp != null) ? slotComp.StandPoint : null;
        Vector3 targetPos = (stand != null) ? stand.position : npcController.targetShelfSlot.position;

        // 허용 반경(거리 or remainingDistance) 중 하나라도 통과하면 도착 인정
        float sqrDist = (npcController.transform.position - targetPos).sqrMagnitude;
        bool inRange =
            sqrDist <= ARRIVE_EPS * ARRIVE_EPS ||
            ag.remainingDistance <= Mathf.Max(ag.stoppingDistance, ARRIVE_EPS);

        if (!inRange) return;

        // NavMesh 위 안전 스냅(Warp) + 경로 정지
        ag.isStopped = true;
        ag.ResetPath();

        Vector3 snapPos = targetPos;
        // 반경을 작게(0.1~0.2) 잡아 옆으로 튀는 스냅 방지
        if (NavMesh.SamplePosition(targetPos, out var hit, 0.15f, NavMesh.AllAreas))
            snapPos = hit.position;

        ag.Warp(snapPos);

        // 기존: 자동 회전 끄고, 선반 쪽을 바라보게
        ag.updateRotation = false;

        Vector3 shelfForward  = npcController.targetShelfSlot.forward;
        Vector3 lookDirection = new Vector3(-shelfForward.x, 0f, -shelfForward.z);
        if (lookDirection.sqrMagnitude >= 0.0001f)
            npcController.transform.rotation = Quaternion.LookRotation(lookDirection);

        // 기존: 물건 집기 대기 상태로 전환
        npcController.stateMachine.SetState(new NpcState_PickWait(npcController));
    }

    public void Exit() { }
}