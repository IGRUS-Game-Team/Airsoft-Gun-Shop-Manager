using UnityEngine;

// NPC가 물건을 집은 후 줄에 합류하거나 배회 상태로 전환하는 상태
public class NpcState_ToQueue : IState
{
    private readonly NpcController npcController;  // 대상 NPC
    private readonly QueueManager queueManager;    // 줄 관리 매니저
    private Transform queueSpot;                   // NPC가 서야 할 자리
    

    private const string WalkingAnim = "Walking";  // 걷기 애니메이션 이름

    public NpcState_ToQueue(NpcController npcController, QueueManager queueManager)
    {
        this.npcController = npcController;
        this.queueManager  = queueManager;
    }

    public void Enter()
    {
        Debug.Log("[ToQueue] 상태 진입");

        // ✅ 선반에서 떠나는 시점에 예약 즉시 해제 (대기/방황/퇴장으로 가더라도 안전)
        npcController.ReleaseShelfReservation("leaving shelf to queue");


        // 1) 빈 자리 확보 시도
        Transform assignedSpot;
        bool joined = queueManager.TryEnqueue(npcController, out assignedSpot);

        // 2) 줄이 가득 찼으면 배회 또는 퇴장 상태로 전환
        if (!joined)
        {
            Debug.Log("[ToQueue] 줄이 가득 참 → 배회 또는 퇴장");
            npcController.Agent.updateRotation = true;  // 이동은 자동 회전 모드
            npcController.Agent.isStopped      = false; // 혹시 멈춰있던 거 풀기

            Transform[] wanderPoints = queueManager.WanderPoints;
            if (wanderPoints == null || wanderPoints.Length == 0)
            {
                npcController.stateMachine.SetState(
                    new NpcState_Leave(npcController));
            }
            else
            {
                npcController.stateMachine.SetState(
                    new NpcState_Wander(npcController, wanderPoints, 20f));
            }
            return;
        }

        // 3) 자리 확보 성공: 목표 노드 설정 및 애니메이션 실행
        this.queueSpot = assignedSpot;
        npcController.SetQueueTarget(assignedSpot);   
        npcController.Animator.Play(WalkingAnim);
    }

    public void Tick()
    {
        // 2) 아직 경로 계산 중이면 대기
        if (npcController.Agent.pathPending)
            return;

        // ===== 도착 체크(허용 반경) + 스냅 =====
        const float ARRIVE_EPS = 0.35f; // 허용 반경(씬 보고 0.25~0.4로 조절)

        Vector3 targetPos = queueSpot.position;
        float sqrDist = (npcController.transform.position - targetPos).sqrMagnitude;

        // 위치 거리 or remainingDistance 둘 중 하나만 만족해도 도착 인정
        bool inRange =
            sqrDist <= ARRIVE_EPS * ARRIVE_EPS ||
            npcController.Agent.remainingDistance <= Mathf.Max(npcController.Agent.stoppingDistance, ARRIVE_EPS);

        if (!inRange)
        {
            // 필요하면 로그 유지
            // Debug.Log($"[ToQueue] 이동 중: 남은거리={npcController.Agent.remainingDistance:F3}");
            return;
        }

        // NavMesh 위로 안전 스냅(Warp) + 이동/경로 정지
        var ag = npcController.Agent;
        ag.isStopped = true;
        ag.ResetPath();

        Vector3 snapPos = targetPos;
        if (UnityEngine.AI.NavMesh.SamplePosition(targetPos, out var hit, 0.5f, UnityEngine.AI.NavMesh.AllAreas))
            snapPos = hit.position;
        ag.Warp(snapPos);

        Debug.Log("[ToQueue] 도착 완료 → QueueWait 상태로 전환");

        // 다음 상태로 전환(회전은 QueueWait에서 처리)
        npcController.stateMachine.SetState(
            new NpcState_QueueWait(npcController, queueManager, queueSpot));
    }

    public void Exit()
    {
        // 상태 종료 시 특별한 작업은 없음
    }
}