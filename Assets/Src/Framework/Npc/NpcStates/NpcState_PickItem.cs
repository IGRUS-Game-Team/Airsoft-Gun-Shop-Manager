using UnityEngine;

// NPC가 선반에서 물건을 집고, 완료되면 줄서기 또는 퇴장 상태로 전환한다.
public class NpcState_PickItem : IState
{
    private readonly NpcController npcController;   // 이 상태를 수행할 NPC
    private QueueManager queueManager;              // 줄 관리 매니저

    private const string ReachHighAnim = "ReachHigh";  // 높은 위치 아이템 집기 애니메이션
    private const string ReachMidAnim = "ReachMid";   // 중간 위치 아이템 집기 애니메이션
    private const string ReachLowAnim = "ReachLow";   // 낮은 위치 아이템 집기 애니메이션

    // 생성자: NPC 참조를 받아 내부 필드에 할당하고, 줄 관리 매니저를 찾는다.
    public NpcState_PickItem(NpcController npcController)
    {
        this.npcController = npcController;
        queueManager = Object.FindFirstObjectByType<QueueManager>();
    }

    // 상태 진입 시 호출: 이동을 멈추고, 대상 슬롯 높이에 맞는 애니메이션을 재생한다.
    public void Enter()
    {
        // 이동 중지
        npcController.Agent.isStopped = true;
        // 기존 경로 초기화
        npcController.Agent.ResetPath();

        // ★ 현재 슬롯에서 살 의사가 없는 경우 → 다른 슬롯으로 이동 시도 또는 퇴장
        if (!WantsToBuyCurrentSlot())
        {
            if (!TryMoveToAnotherSlot())
            {
                // 다른 슬롯도 없으면 퇴장
                var carrier = npcController.GetComponent<CarriedItemHandler>();
                carrier?.ClosePicking();
                npcController.stateMachine.SetState(new NpcState_Leave(npcController));
            }
            return; // 집기 애니메이션 재생 안 함
        }

        // ── 여기부터는 "살 의사 있음"일 때만 집기 애니메이션 선택 ──
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
        var carrier = npcController.GetComponent<CarriedItemHandler>();
        if (carrier == null) return;

        // 목표 개수 채움 → 줄 이동
        if (carrier.CarriedCount >= carrier.DesiredCount)
        {
            carrier.ClosePicking();
            npcController.hasItemInHand = false;

            if (queueManager == null)
                queueManager = Object.FindFirstObjectByType<QueueManager>();

            if (queueManager != null)
                npcController.stateMachine.SetState(new NpcState_ToQueue(npcController, queueManager));
            else
                npcController.stateMachine.SetState(new NpcState_Leave(npcController));
            return;
        }

        // ★ 아직 집기 전인데(손에 없음), 가격이 안 맞게 바뀐 경우 대비 (가끔 이벤트로 시세 변동 등)
        if (!npcController.hasItemInHand && !WantsToBuyCurrentSlot())
        {
            if (!TryMoveToAnotherSlot())
            {
                var c = npcController.GetComponent<CarriedItemHandler>();
                c?.ClosePicking();
                npcController.stateMachine.SetState(new NpcState_Leave(npcController));
            }
            return;
        }

        if (!npcController.hasItemInHand)
            return;

        if (npcController.heldItem != null && npcController.heldItem.activeSelf)
            npcController.heldItem.SetActive(false);

        // 아직 목표 못 채웠으면 다음 슬롯 탐색...
        if (!carrier.HasAllDesired)
        {
            ShelfSlot cur = npcController.targetShelfSlot ? npcController.targetShelfSlot.GetComponent<ShelfSlot>() : null;
            ShelfSlot next = null;

            if (cur != null && cur.HasItem)
                next = cur; // 같은 슬롯에서 한 번 더
            else
            {
                if (ShelfManager.Instance != null && ShelfManager.Instance.TryGetAvailableSlot(out var other))
                    next = other;
            }

            if (next != null)
            {
                npcController.hasItemInHand = false;
                npcController.heldItem = null;
                npcController.targetShelfSlot = next.transform;
                npcController.targetShelfGroup = next.ParentGroup;
                npcController.stateMachine.SetState(new NpcState_ToShelf(npcController));
                return;
            }
        }

        // 슬롯 없으면(재고 없음) → 더 못 채워도 줄로
        carrier.ClosePicking();
        if (queueManager == null)
            queueManager = Object.FindFirstObjectByType<QueueManager>();

        if (queueManager != null)
            npcController.stateMachine.SetState(new NpcState_ToQueue(npcController, queueManager));
        else
            npcController.stateMachine.SetState(new NpcState_Leave(npcController));
    }

    // 상태 종료 시 호출: 별도 정리할 작업 없음
    public void Exit()
    {
    }
    
    // ★ 현재 타깃 슬롯의 아이템을 이 NPC가 "시세 + N%" 기준으로 살 의사가 있는지 판단
    private bool WantsToBuyCurrentSlot()
    {
        if (npcController.targetShelfSlot == null) return false;

        var slot = npcController.targetShelfSlot.GetComponent<ShelfSlot>();
        if (slot == null || !slot.HasItem) return false;

        var profile = npcController.GetComponent<NpcProfile>();
        if (profile == null) return true; // 프로필 없으면 막지 않음(안전)

        int itemId = 0;
        float offerPrice = 0f;

        if (!slot.TryGetPricing(out itemId, out offerPrice))
            return false;

        if (itemId == 0 || offerPrice <= 0f) return true; // 정보 없으면 막지 않음(안전)

        // 시세 조회(안전 버전)
        if (!MarketPriceDataManager.Instance.TryGetMarketPrice(itemId, out var market))
            return false; // 시세 없으면 구매 안 함

        // 전략 판단: offerPrice ≤ market×(1+N%)
        return profile.WillBuyWithMarket(offerPrice, market);
    }

    // ★ 다른 슬롯으로 이동 시도 (있으면 이동, 없으면 false)
    private bool TryMoveToAnotherSlot()
    {
        // 현재 슬롯 그룹 예약 해제(있으면)
        npcController.targetShelfGroup?.Release();
        npcController.targetShelfGroup = null;

        if (ShelfManager.Instance != null && ShelfManager.Instance.TryGetAvailableSlot(out var other))
        {
            npcController.targetShelfSlot  = other.transform;
            npcController.targetShelfGroup = other.ParentGroup;
            npcController.stateMachine.SetState(new NpcState_ToShelf(npcController));
            return true;
        }
        return false;
    }
}
