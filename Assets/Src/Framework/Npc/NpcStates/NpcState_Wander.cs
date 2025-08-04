using UnityEngine;

// NPC가 매장 안을 배회하며 랜덤 지점으로 이동하는 상태
public class NpcState_Wander : IState
{
    private readonly NpcController npcController;   // 이 상태를 실행할 NPC
    private readonly Transform[] wanderPoints;      // 배회 지점 목록
    private readonly float minIdle = 2f;            // 도착 후 최소 대기 시간
    private readonly float maxIdle = 5f;            // 도착 후 최대 대기 시간
    private readonly float maxWanderTime;           // 배회 최대 지속 시간(초)

    private const string WalkingAnim = "Walking";   // 걷기 애니메이션 이름

    private float nextMoveTime;                     // 다음 지점 이동 시각
    private float wanderEndTime;                    // 배회 종료 시각

    // 생성자: NPC와 배회 지점, 최대 배회 시간을 받아 초기화
    public NpcState_Wander(NpcController npcController, Transform[] wanderPoints, float maxWanderTime = 20f)
    {
        this.npcController = npcController;         // 필드에 NPC 저장
        this.wanderPoints = wanderPoints;           // 필드에 지점 배열 저장
        this.maxWanderTime = maxWanderTime;         // 필드에 최대 지속 시간 저장
    }

    // 상태 진입 시 호출
    public void Enter()
    {
        npcController.Agent.isStopped = false;      // 이동 재개
        PickNextDestination();                       // 첫 배회 지점 선택 및 이동
        wanderEndTime = Time.time + maxWanderTime;  // 배회 종료 시각 계산
        npcController.Animator.Play(WalkingAnim);   // 걷기 애니메이션 실행
    }

    // 매 프레임 호출
    public void Tick()
    {
        // 배회 시간이 끝났으면 퇴장 상태로 전환
        if (Time.time >= wanderEndTime)
        {
            npcController.stateMachine.SetState(
                new NpcState_Leave(npcController));
            return;
        }

        // 이동 중이면 도착 검사 건너뜀
        if (npcController.Agent.pathPending)
        {
            return;
        }

        // 도착 여부 판단: 남은 거리와 속도 체크
        bool arrived = 
            npcController.Agent.remainingDistance <= 0.1f &&
            npcController.Agent.velocity.sqrMagnitude < 0.01f;
        if (!arrived)
        {
            return;
        }

        // 도착 후 아직 대기 시간이 남아 있으면 대기
        if (Time.time < nextMoveTime)
        {
            return;
        }

        // 대기 종료 후 다음 배회 지점으로 이동
        PickNextDestination();
    }

    // 상태 종료 시 호출: 별도 작업 없음
    public void Exit()
    {
    }

    // 다음 배회 지점을 선택하고 이동 목적지를 설정
    private void PickNextDestination()
    {
        // 배회 지점이 없으면 퇴장 상태로 전환
        if (wanderPoints == null || wanderPoints.Length == 0)
        {
            npcController.stateMachine.SetState(
                new NpcState_Leave(npcController));
            return;
        }

        // 랜덤 인덱스로 지점 선택
        int index = Random.Range(0, wanderPoints.Length);
        Transform targetPoint = wanderPoints[index];

        // NavMeshAgent에 목적지 지정
        npcController.Agent.SetDestination(targetPoint.position);

        // 다음 이동 전 대기 시간 설정
        nextMoveTime = Time.time + Random.Range(minIdle, maxIdle);
    }
}