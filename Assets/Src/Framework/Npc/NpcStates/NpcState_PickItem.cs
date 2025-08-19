using UnityEngine;

// NPC가 선반에서 물건을 집고, 완료되면 줄서기 또는 퇴장 상태로 전환한다.
public class NpcState_PickItem : IState
{
    private readonly NpcController npcController;
    private QueueManager queueManager;

    private const string ReachHighAnim = "ReachHigh";
    private const string ReachMidAnim  = "ReachMid";
    private const string ReachLowAnim  = "ReachLow";

    // 불평 모션 (애니메이터 이름 확인!)
    private const string ComplainStateName = "complain";
    private const float  ComplainHoldSeconds = 1.0f;

    // 원샷 불평용 타이머
    private bool  isComplaining = false;
    private float complainRemain = 0f;

    public NpcState_PickItem(NpcController npcController)
    {
        this.npcController = npcController;
        queueManager = Object.FindFirstObjectByType<QueueManager>();
    }

    public void Enter()
    {
        npcController.Agent.isStopped = true;
        npcController.Agent.ResetPath();

        // 집기 전 가격 판정: 비싸면 즉시 불평 → 퇴장
        if (!WantsToBuyCurrentSlot())
        {
            StartComplainAndLeave();
            return;
        }

        // 집기 애니메이션
        float y = npcController.targetShelfSlot.position.y;
        npcController.Animator.Play(y > 1.4f ? ReachHighAnim : (y < 0.6f ? ReachLowAnim : ReachMidAnim));
    }

    public void Tick()
    {
        // 불평 모션 진행 중이면 타이머만
        if (isComplaining)
        {
            complainRemain -= Time.deltaTime;
            if (complainRemain <= 0f)
            {
                // ★ 여기 추가: 불평 후 결제 없이 떠남 → 불만족 집계
                SettlementManager.Instance?.OnCustomerLeftUnhappy(npcController);

                npcController.StartLeaving(npcController.exitPoint);
            }
            return;
        }

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

        // 손에 들기 전 재판정: 비싸면 즉시 불평 → 퇴장
        if (!npcController.hasItemInHand && !WantsToBuyCurrentSlot())
        {
            StartComplainAndLeave();
            return;
        }

        if (!npcController.hasItemInHand) return;

        if (npcController.heldItem != null && npcController.heldItem.activeSelf)
            npcController.heldItem.SetActive(false);

        // 재고 사유로만 다음 슬롯 탐색 (가격과 무관)
        if (!carrier.HasAllDesired)
        {
            ShelfSlot cur = npcController.targetShelfSlot ? npcController.targetShelfSlot.GetComponent<ShelfSlot>() : null;
            ShelfSlot next = null;

            if (cur != null && cur.HasItem) next = cur; // 같은 슬롯 반복 허용
            else if (ShelfManager.Instance != null && ShelfManager.Instance.TryGetAvailableSlot(out var other)) next = other;

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

        // 슬롯 자체가 없으면 → 줄로
        carrier.ClosePicking();
        if (queueManager == null)
            queueManager = Object.FindFirstObjectByType<QueueManager>();
        if (queueManager != null)
            npcController.stateMachine.SetState(new NpcState_ToQueue(npcController, queueManager));
        else
            npcController.stateMachine.SetState(new NpcState_Leave(npcController));
    }

    public void Exit() { }

    // ───────────── 헬퍼 ─────────────

    // 현재 슬롯이 “시세 + N%” 기준으로 살만한지
    private bool WantsToBuyCurrentSlot()
    {
        if (npcController == null || npcController.targetShelfSlot == null) return false;

        var slot = npcController.targetShelfSlot.GetComponent<ShelfSlot>();
        if (slot == null || !slot.HasItem) return false;

        var profile = npcController.GetComponent<NpcProfile>();
        if (profile == null) return false;

        if (!slot.TryGetPricing(out int itemId, out float offerPrice)) return false;
        if (itemId == 0 || offerPrice <= 0f) return false;

        if (MarketPriceDataManager.Instance == null) return false;
        if (!MarketPriceDataManager.Instance.TryGetMarketPrice(itemId, out var market)) return false;

        return profile.WillBuyWithMarket(offerPrice, market);
    }

    // 불평 + 집기 종료 + 평판 기록 + 잠깐 대기 후 퇴장
    private void StartComplainAndLeave()
    {
        // 중복 호출 방지
        if (isComplaining) return;

        // 집기 종료/정리
        npcController.GetComponent<CarriedItemHandler>()?.ClosePicking();
        npcController.hasItemInHand = false;
        npcController.heldItem = null;

        // 선반 예약 해제
        npcController.targetShelfGroup?.Release();
        npcController.targetShelfGroup = null;

        // 평판에 불평 기록
        SettlementManager.Instance?.MarkNpcComplained(npcController);

        // 이동 멈추고 불평 모션
        npcController.Agent.isStopped = true;
        PlayComplain();

        // 원샷 타이머 시작
        isComplaining  = true;
        complainRemain = ComplainHoldSeconds;
    }

    // 트리거/스테이트 둘 다 대응
    private void PlayComplain()
    {
        var anim = npcController.Animator;
        if (anim == null) return;

        foreach (var p in anim.parameters)
            if (p.type == AnimatorControllerParameterType.Trigger && p.name == ComplainStateName)
            { anim.SetTrigger(ComplainStateName); return; }

        anim.Play(ComplainStateName, 0, 0f);
    }
}