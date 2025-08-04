// NPC가 쇼핑을 마치고 출구로 이동하는 상태
public class NpcState_Leave : IState
{
    /* ---------- 의존성 ---------- */

    private readonly NpcController npcController;           // 상태를 수행할 NPC
    private readonly ShelfSlot    reservedSlot;            // NPC가 점유했던 선반 슬롯

    /* ---------- 상수 ---------- */

    private const string WalkingAnim = "Walking";           // 걷기 애니메이션 클립 이름

    /* ---------- 생성자 ---------- */

    // 다른 스크립트에서 new NpcState_Leave(npc)로 호출
    public NpcState_Leave(NpcController npcController)
    {
        this.npcController = npcController;                 // 필드에 NPC 저장
        // NPC가 바라보던 선반 슬롯 참고(있을 수도, 없을 수도)
        reservedSlot = this.npcController.targetShelfSlot != null
                     ? this.npcController.targetShelfSlot.GetComponent<ShelfSlot>()
                     : null;
    }

    /* ---------- IState 구현 ---------- */

    // 상태 진입: 슬롯 해제 → NavMeshAgent 설정 → 출구로 이동
    public void Enter()
    {
        if (reservedSlot != null)                           // 1) 슬롯이 있으면
        {
            reservedSlot.Release();                         //    예약 해제
        }

        npcController.isLeaving = true;                     // 2) NPC를 '퇴장 중'으로 표시

        /* ---------- NavMeshAgent 재설정 ---------- */

        npcController.agent.ResetPath();                    // 3) 이전 경로 초기화
        npcController.agent.updateRotation = true;          // 4) 이동 중 자동 회전 켬
        npcController.agent.isStopped      = false;         // 5) 이동 가능 상태로 전환

        npcController.agent.SetDestination(                 // 6) 목적지: 출구
            npcController.exitPoint.position);

        /* ---------- 애니메이션 ---------- */

        npcController.animator.Play(WalkingAnim);           // 7) 걷기 애니메이션 시작
    }

    // 퇴장 상태는 이동만 하면 되므로 매 프레임 로직 없음
    public void Tick() { }

    // 특별한 정리 작업 필요 없음
    public void Exit() { }
}