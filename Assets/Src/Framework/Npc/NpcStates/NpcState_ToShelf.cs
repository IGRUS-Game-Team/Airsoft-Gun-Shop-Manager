using UnityEngine;

// NPC가 선반 위치로 이동했다가 도착하면 물건 집기 대기 상태로 전환
public class NpcState_ToShelf : IState
{
    private readonly NpcController npcController;   // 이 상태를 수행할 NPC
    private const string WalkingAnim = "Walking";   // 걷기 애니메이션 이름

    // 생성자: NPC 참조를 받아 필드에 할당
    public NpcState_ToShelf(NpcController npcController)
    {
        this.npcController = npcController;         // 필드 초기화
    }

    // 상태 진입 시 호출: 이동 재개, 목적지 설정, 애니메이션 실행
    public void Enter()
    {
        npcController.agent.isStopped = false;                                              // 이동 정지 해제
        npcController.agent.SetDestination(npcController.targetShelfSlot.position);         // 목적지: 선반 슬롯 위치
        npcController.animator.Play(WalkingAnim);                                           // 걷기 애니메이션 재생
    }

    // 매 프레임 호출: 도착 여부 검사 후 회전 보정 및 다음 상태 전환
    public void Tick()
    {
        // 경로 계산 중이면 도착 검사하지 않고 대기
        if (npcController.agent.pathPending == true)
        {
            return;
        }

        // 남은 거리와 속도를 기반으로 도착 여부 판단
        bool arrived = 
            npcController.agent.remainingDistance <= 0.1f &&
            npcController.agent.velocity.sqrMagnitude < 0.01f;
        if (arrived == false)
        {
            return;  
        }

        // 자동 회전 끄기 (직접 회전 처리할 것)
        npcController.agent.updateRotation = false;

        // 선반의 전방 벡터 가져오기
        Vector3 shelfForward = npcController.targetShelfSlot.forward;
        // 수평 반대 방향 계산 (NPC가 선반을 바라보도록)
        Vector3 lookDirection = new Vector3(-shelfForward.x, 0f, -shelfForward.z);
        // 유효한 벡터인지 확인 후 회전 적용
        if (lookDirection.sqrMagnitude >= 0.0001f)
        {
            npcController.transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        // 이동 중지 후 물건 집기 대기 상태로 전환
        npcController.agent.isStopped = true;
        npcController.stateMachine.SetState(
            new NpcState_PickWait(npcController));
    }

    // 상태 종료 시 호출: 별도 복원 작업 없음
    public void Exit()
    {
    }
}