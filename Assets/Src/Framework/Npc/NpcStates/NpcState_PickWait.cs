using UnityEngine;

// NPC가 물건 집기 전 잠시 대기하며 선반 방향을 유지하는 상태
public class NpcState_PickWait : IState
{
    private readonly NpcController npcController;  // 상태를 수행할 대상 NPC
    private float waitEndTime;                     // 대기 종료 시각(초 단위)

    private const string StandingAnim = "Standing"; // 대기 애니메이션 클립 이름

    // 생성자: NPC 컨트롤러를 받아 필드를 초기화
    public NpcState_PickWait(NpcController npcController)
    {
        this.npcController = npcController;         // 필드에 NPC 저장
    }

    // 상태 진입 시 호출: 대기 종료 시간 설정, 애니메이션·회전·이동 제어
    public void Enter()
    {
        // 1) 랜덤 시간(1~3초) 만큼 대기하도록 종료 시각 계산
        waitEndTime = Time.time + Random.Range(1f, 3f);
        // 2) 이동 중지
        npcController.Agent.isStopped = true;
        // 3) 서 있는 애니메이션 재생
        npcController.Animator.Play(StandingAnim);
        // 4) NavMeshAgent 자동 회전 해제
        npcController.Agent.updateRotation = false;
    }

    // 매 프레임 호출: 선반 바라보기 유지, 대기 종료 후 다음 상태로 전환
    public void Tick()
    {
        // 1) 매 프레임 선반 쪽으로 회전 보정
        KeepFacingShelf();

        // 2) 아직 대기 시간이 남았다면 아무 작업도 하지 않음
        if (Time.time < waitEndTime)
        {
            return;
        }

        // 3) 대기 완료 시 물건 집기 상태로 전환
        npcController.stateMachine.SetState(new NpcState_PickItem(npcController));
    }

    // 상태 종료 시 호출: 자동 회전 복원
    public void Exit()
    {
        npcController.Agent.updateRotation = true;
    }

    // 손쉽게 선반(또는 슬롯 부모)을 바라보도록 회전을 보정
    private void KeepFacingShelf()
    {
        // 1) 타겟 슬롯의 Transform 가져오기
        Transform slot = npcController.targetShelfSlot;
        // 2) 슬롯의 부모가 있으면 부모(선반)를, 없으면 슬롯 자신을 사용
        Transform shelf = (slot.parent != null ? slot.parent : slot);

        // 3) 수평 방향 벡터 계산
        Vector3 forward = -shelf.forward;
        forward.y = 0f;

        // 4) 유효한 벡터인지 확인
        if (forward.sqrMagnitude < 0.0001f)
        {
            return;
        }

        // 5) 현재 회전을 선반 방향으로 부드럽게 변경
        Quaternion to = Quaternion.LookRotation(forward);
        npcController.transform.rotation =
    Quaternion.RotateTowards(npcController.transform.rotation, to, 360f * Time.deltaTime);
    }
}