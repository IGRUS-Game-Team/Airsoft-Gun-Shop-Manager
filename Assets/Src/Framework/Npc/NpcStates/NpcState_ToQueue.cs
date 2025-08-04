using UnityEngine;

// NPC가 물건을 집은 후 줄에 합류하거나 배회 상태로 전환하는 상태
public class NpcState_ToQueue : IState
{
    private readonly NpcController npcController;  // 대상 NPC
    private readonly QueueManager queueManager;    // 줄 관리 매니저
    private Transform queueSpot;                   // NPC가 서야 할 자리

    private const string WalkingAnim = "Walking";  // 걷기 애니메이션 이름

    // 생성자: NPC와 줄 관리 매니저를 받아 필드에 할당
    public NpcState_ToQueue(NpcController npcController, QueueManager queueManager)
    {
        this.npcController = npcController;
        this.queueManager  = queueManager;
    }

    // 상태 진입 시 호출: 줄 합류 시도, 실패 시 배회 또는 퇴장으로 전환
    public void Enter()
    {
        // 1) 빈 자리 확보 시도
        Transform assignedSpot;
        bool joined = queueManager.TryEnqueue(npcController, out assignedSpot);

        // 2) 줄이 가득 찼으면 배회 또는 퇴장 상태로 전환
        if (!joined)
        {
            Transform[] wanderPoints = queueManager.WanderPoints;
            if (wanderPoints == null || wanderPoints.Length == 0)
            {
                // 배회 포인트 없으면 퇴장
                npcController.stateMachine.SetState(
                    new NpcState_Leave(npcController));
            }
            else
            {
                // 배회 상태로 전환, 최대 20초 배회
                npcController.stateMachine.SetState(
                    new NpcState_Wander(npcController, wanderPoints, 20f));
            }
            return;  // 더 이상 진행 안 함
        }

        // 3) 자리 확보 성공: 목표 노드 설정 및 애니메이션 실행
        this.queueSpot = assignedSpot;                
        npcController.SetQueueTarget(assignedSpot);   
        npcController.Animator.Play(WalkingAnim);
    }

    // 매 프레임 호출: 이동 보정, 도착 시 대기 상태로 전환
    public void Tick()
    {
        // 1) 이동 중 시선은 앞사람이나 카운터로 부드럽게 조정
        npcController.FaceLookTarget(540f);

        // 2) 아직 경로 계산 중이면 대기
        if (npcController.Agent.pathPending)
        {
            return;
        }

        // 3) 도착 여부 체크: 남은 거리가 거의 0이고 속도도 거의 0
        bool arrived = 
            npcController.Agent.remainingDistance < 0.05f &&
            npcController.Agent.velocity.sqrMagnitude < 0.01f;
        if (!arrived)
        {
            return;
        }

        // 4) 노드 도착 시 이동 중지하고 줄 대기 상태로 전환
        npcController.Agent.isStopped = true;
        npcController.stateMachine.SetState(
            new NpcState_QueueWait(
                npcController, 
                queueManager, 
                queueSpot));
    }

    // 상태 종료 시 호출: 별도 복원 작업 없음
    public void Exit()
    {
    }
}