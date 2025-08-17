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

    [Header("기본 입장 확률 (%)")]
    [Tooltip("평판 보정 빼고, 기본 확률")]

    [Range(0, 100)]
    [SerializeField] int baseEntryChancePercent = 70;

    // 평판이 낮을수록 깎고, 높을수록 올리는 정비례 그래프의 기울기와 끝값 조
    [Header("평판 보정(선형)")]
    [Tooltip("이 구간 안에서 평판을 0 ~ 1 슬라이더로 환산")]

    [SerializeField] int repAtLow  = 0;    // 낮은 평판 기준(예: 0)
    [SerializeField] int repAtHigh = 100;  // 높은 평판 기준(예: 100)

    [Tooltip("평판이 최저일 때 얼마나 깎을지")]
    [Range(-100, 100)]
    [SerializeField] int bonusAtLowRep = -30;

    [Tooltip("평판이 최고일 때 얼마나 올릴지")]
    [Range(-100, 100)]
    [SerializeField] int bonusAtHighRep = +20;

    [Header("최종 확률 클램프")]
    [Tooltip("최종 확률이 너무 0%/100%로 고정되지 않게 안전 범위 설정(원하면 0/100으로 바꿔도 됨)")]
    [Range(0, 100)] [SerializeField] int minFinalChance = 5;
    [Range(0, 100)] [SerializeField] int maxFinalChance = 95;

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

        /* ── 평판 보정 포함 최종 확률 계산 + 주사위 ── */
        int effectiveChance = ComputeEffectiveChance();
        if (!RollEntry(effectiveChance))
        { npc.StartLeaving(exitPoint); return; }

        /* ── 선반 빈 칸 예약 ── */
        if (!ShelfManager.Instance.TryGetAvailableSlot(out ShelfSlot slot))
        { npc.StartLeaving(exitPoint); return; }

        /* ── 입장 확정 ── */
        insideCount++;
        SettlementManager.Instance?.RegisterCustomerEnter(npc);
        npc.SetDoor(this);
        npc.inStore = true;
        npc.AllowEntry(slot.transform, exitPoint);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(TagNpc)) return;

        var npc = other.GetComponent<NpcController>();
        if (npc != null && npc.inStore && npc.isLeaving)
        {
            insideCount = Mathf.Max(0, insideCount - 1);
            npc.inStore = false;
        }
    }

    // 평판 기반 최종 확률 계산
    int ComputeEffectiveChance()
    {
        int rep = SettlementManager.Instance != null ? SettlementManager.Instance.Reputation : 0;

        // repAtLow~repAtHigh 구간으로 정규화 (범위 뒤집혀 있어도 안전)
        float minRep = Mathf.Min(repAtLow, repAtHigh);
        float maxRep = Mathf.Max(repAtLow, repAtHigh);
        float t = (Mathf.Approximately(minRep, maxRep)) ? 1f : Mathf.InverseLerp(minRep, maxRep, rep);

        // 낮은 평판 보정 → 높은 평판 보정으로 선형 보간
        float bonus = Mathf.Lerp(bonusAtLowRep, bonusAtHighRep, t);

        // 기본 확률 + 보정
        float chance = baseEntryChancePercent + bonus;

        // 최종 확률 클램프
        chance = Mathf.Clamp(chance, minFinalChance, maxFinalChance);

        return Mathf.RoundToInt(chance);
    }

    // 0~99 주사위 < chancePercent
    bool RollEntry(int chancePercent)
    {
        chancePercent = Mathf.Clamp(chancePercent, 0, 100);
        int dice = Random.Range(0, 100);
        return dice < chancePercent;
    }
}