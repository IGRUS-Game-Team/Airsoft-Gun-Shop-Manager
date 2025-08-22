using TMPro;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Header("NPC 퇴장 위치")]
    [SerializeField] Transform exitPoint;

    [Header("매장 최대 인원")]
    [SerializeField] int maxInStore = 10;

    // 바꿔야 하는 상황
    // 1. 평판이 0 ~ 중간임에도 너무 한산하다 -> 기본 확률 높이기
    // 2. 평판이 나쁜데도 계속 붐빈다 -> 기본 확률 낮추기
    // 3. 게임에서 평판이 100보다 넘게 크는 구조다 -> repAtHigh를 더 높이기
    // 4. 초반부터 평판 변화가 잘 느껴졌으면 좋겠다 -> repAtHigh를 더 낮추기
    // 5. 평판이 낮을 때도 너무 잘 들어온다 -> bonusAtLowRep을 더 음수로
    // 6. 평판이 높아도 사람이 별로 안 늘어난다 -> bonusAtLowRep을 더 양수로


    [Header("입장 기본 확률 (%) + 평판 보정")]
    [Range(0, 100)] [SerializeField] int baseEntryChancePercent = 70;

    [Header("평판 보정(선형)")]
    [SerializeField] int repAtLow  = 0;
    [SerializeField] int repAtHigh = 100;
    [Range(-100, 100)] [SerializeField] int bonusAtLowRep  = -30;
    [Range(-100, 100)] [SerializeField] int bonusAtHighRep = +20;

    [Header("최종 확률 클램프")]
    [Range(0, 100)] [SerializeField] int minFinalChance = 5;
    [Range(0, 100)] [SerializeField] int maxFinalChance = 95;

    [Header("입장 목적지 분배(%)")]
    [Tooltip("선반으로 보낼 확률(사격장은 100-이 값)")]
    [Range(0, 100)] [SerializeField] int percentToShelves = 80;

    [Tooltip("선택한 목적지가 꽉 찼으면 다른 쪽으로 보낼지?")]
    [SerializeField] bool fallbackToOtherIfFull = true;

    [SerializeField] TextMeshProUGUI Customer;

    const string TagNpc = "Npc";
    int insideCount;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(TagNpc)) return;

        var npc = other.GetComponent<NpcController>();
        if (npc == null || npc.isLeaving) return;

        // 중복 처리 방지
        if (npc.DoorProcessed) return;
        npc.DoorProcessed = true;

        /* ── 인원 제한 ── */
        if (insideCount >= maxInStore)
        { npc.StartLeaving(exitPoint); return; }

        /* ── 입장 허가 주사위 ── */
        int effectiveChance = ComputeEffectiveChance();
        if (!RollEntry(effectiveChance))
        { npc.StartLeaving(exitPoint); return; }

        /* ── 목적지 선택 ── */
        bool chooseShelf = Random.Range(0, 100) < percentToShelves;

        bool sent = false;

        if (chooseShelf)
        {
            sent = TrySendToShelf(npc);
            if (!sent && fallbackToOtherIfFull)
                sent = TrySendToRange(npc);
        }
        else
        {
            sent = TrySendToRange(npc);
            if (!sent && fallbackToOtherIfFull)
                sent = TrySendToShelf(npc);
        }

        if (!sent) { npc.StartLeaving(exitPoint); return; }

        // ── 입장 확정 ──
        insideCount++;
        if (Customer) Customer.text = "Customer : " + insideCount.ToString();
        SettlementManager.Instance?.RegisterCustomerEnter(npc);
        npc.SetDoor(this);
        npc.inStore = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(TagNpc)) return;

        var npc = other.GetComponent<NpcController>();
        if (npc != null && npc.inStore && npc.isLeaving)
        {
            insideCount = Mathf.Max(0, insideCount - 1);
            if (Customer) Customer.text = "Customer : " + insideCount.ToString();
            npc.inStore = false;
        }
    }

    // ─────────────────────────────────────────────
    // 목적지 분기 헬퍼
    // ─────────────────────────────────────────────
    bool TrySendToShelf(NpcController npc)
    {
        if (ShelfManager.Instance != null &&
            ShelfManager.Instance.TryGetAvailableSlot(out ShelfSlot slot))
        {
            npc.AllowEntry(slot.transform, exitPoint);
            return true;
        }
        return false;
    }

    bool TrySendToRange(NpcController npc)
    {
        if (ShootingRangeManager.Instance != null &&
            ShootingRangeManager.Instance.TryGetAvailableLane(out ShootingLane lane))
        {
            npc.AllowRange(lane, exitPoint);
            return true;
        }
        return false;
    }

    // ─────────────────────────────────────────────
    // 확률 계산
    // ─────────────────────────────────────────────
    int ComputeEffectiveChance()
    {
        int rep = SettlementManager.Instance != null ? SettlementManager.Instance.Reputation : 0;

        float minRep = Mathf.Min(repAtLow, repAtHigh);
        float maxRep = Mathf.Max(repAtLow, repAtHigh);
        float t = (Mathf.Approximately(minRep, maxRep)) ? 1f : Mathf.InverseLerp(minRep, maxRep, rep);

        float bonus = Mathf.Lerp(bonusAtLowRep, bonusAtHighRep, t);
        float chance = baseEntryChancePercent + bonus;
        chance = Mathf.Clamp(chance, minFinalChance, maxFinalChance);

        return Mathf.RoundToInt(chance);
    }

    bool RollEntry(int chancePercent)
    {
        chancePercent = Mathf.Clamp(chancePercent, 0, 100);
        int dice = Random.Range(0, 100);
        return dice < chancePercent;
    }
}