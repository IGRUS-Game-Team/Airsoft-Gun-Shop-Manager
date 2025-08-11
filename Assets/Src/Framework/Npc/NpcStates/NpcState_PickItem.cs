using UnityEngine;

// NPC가 선반에서 물건을 집고, 완료되면 줄서기 또는 퇴장 상태로 전환한다.
public class NpcState_PickItem : IState
{
    private readonly NpcController npcController;   // 이 상태를 수행할 NPC
    private QueueManager queueManager;              // 줄 관리 매니저

    private const string ReachHighAnim = "ReachHigh";  // 높은 위치 아이템 집기 애니메이션
    private const string ReachMidAnim  = "ReachMid";   // 중간 위치 아이템 집기 애니메이션
    private const string ReachLowAnim  = "ReachLow";   // 낮은 위치 아이템 집기 애니메이션

    // 생성자: NPC 참조를 받아 내부 필드에 할당하고, 줄 관리 매니저를 찾는다.
    public NpcState_PickItem(NpcController npcController)
    {
        this.npcController = npcController;
        queueManager  = Object.FindFirstObjectByType<QueueManager>();
    }

    // 상태 진입 시 호출: 이동을 멈추고, 대상 슬롯 높이에 맞는 애니메이션을 재생한다.
    public void Enter()
    {
        // 이동 중지
        npcController.Agent.isStopped = true;
        // 기존 경로 초기화
        npcController.Agent.ResetPath();

        // 대상 슬롯의 높이를 가져옴
        float itemHeight = npcController.targetShelfSlot.position.y;

        // 높이에 따라 적절한 애니메이션 선택
        string animationName;
        if (itemHeight > 1.4f)
        {
            animationName = ReachHighAnim;
        }
        else if (itemHeight < 0.6f)
        {
            animationName = ReachLowAnim;
        }
        else
        {
            animationName = ReachMidAnim;
        }

        // 애니메이션 재생
        npcController.Animator.Play(animationName);
    }

    // 매 프레임 호출: 손에 물건이 생기면 다음 상태로 전환
    public void Tick()
    {
        // 아직 물건을 집지 못했으면 대기
        if (!npcController.hasItemInHand)
            return;

        // ① 혹시 아이템이 아직 켜져 있으면 여기서도 한 번 더 숨김
        if (npcController.heldItem != null &&
            npcController.heldItem.activeSelf)
            npcController.heldItem.SetActive(false);


        // ★추가: 더 담아야 하면, 다음 슬롯으로 이동해서 계속 담기
        var carrier = npcController.GetComponent<CarriedItemHandler>();
        if (carrier != null && !carrier.HasAllDesired)
        {
            // 현재 슬롯에 아직 남아있으면 그 자리 다시 가기 (애니 한 번 더)
            ShelfSlot cur = npcController.targetShelfSlot ? npcController.targetShelfSlot.GetComponent<ShelfSlot>() : null;

            ShelfSlot next = null;
            if (cur != null && cur.HasItem)
            {
                next = cur; // 같은 슬롯에서 한 번 더
            }
            else
            {
                // 다른 슬롯 검색: ShelfManager 활용
                if (ShelfManager.Instance != null && ShelfManager.Instance.TryGetAvailableSlot(out var other))
                    next = other;
            }

            // 다음 목적지로 재이동
            if (next != null)
            {
                npcController.hasItemInHand = false;   // ★추가: 다음 픽업을 위해 리셋
                npcController.heldItem = null;    // ★추가
                npcController.targetShelfSlot = next.transform;
                npcController.targetShelfGroup = next.ParentGroup; // ★추가(Queue에서 Release용)
                npcController.stateMachine.SetState(new NpcState_ToShelf(npcController));
                return;
            }
            // 슬롯이 전혀 없다면 → 담은 만큼만 계산대로
        }


        if (queueManager == null)
            queueManager = Object.FindFirstObjectByType<QueueManager>();

        if (queueManager != null)
            npcController.stateMachine.SetState(
                new NpcState_ToQueue(npcController, queueManager));
        else
            npcController.stateMachine.SetState(
                new NpcState_Leave(npcController));
    }

    // 상태 종료 시 호출: 별도 정리할 작업 없음
    public void Exit()
    {
    }
}