using TMPro;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Header("NPC 퇴장 위치")]
    [SerializeField] private Transform exitPoint;

    [Header("매장 최대 인원")]
    [SerializeField] private int maxInStore = 10;

    [Header("매장 상태 (OPEN / CLOSED)")]
    [SerializeField] private bool isOpen = true;
    public bool IsOpen => isOpen;

    [Header("입장 기본 확률 (%) + 평판 보정")]
    [Range(0, 100)] [SerializeField] private int baseEntryChancePercent = 70;

    [Header("평판 보정(선형)")]
    [SerializeField] private int repAtLow = 0;
    [SerializeField] private int repAtHigh = 100;
    [Range(-100, 100)] [SerializeField] private int bonusAtLowRep = -30;
    [Range(-100, 100)] [SerializeField] private int bonusAtHighRep = +20;

    [Header("최종 확률 클램프")]
    [Range(0, 100)] [SerializeField] private int minFinalChance = 5;
    [Range(0, 100)] [SerializeField] private int maxFinalChance = 95;

    [Header("입장 목적지 분배(%)")]
    [Tooltip("선반으로 보낼 확률(사격장은 100 - 이 값)")]
    [Range(0, 100)] [SerializeField] private int percentToShelves = 80;

    [Tooltip("선택한 목적지가 꽉 찼으면 다른 쪽으로 보낼지?")]
    [SerializeField] private bool fallbackToOtherIfFull = true;

    [Header("현재 매장 인원 텍스트 (선택)")]
    [SerializeField] private TextMeshProUGUI customerText;

    private const string TagNpc = "Npc";
    private int insideCount;

    // =========================================================
    // Unity Events
    // =========================================================
    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidNpc(other, out var npc)) return;

        // 한 번만 처리
        if (npc.DoorProcessed) return;
        npc.DoorProcessed = true;

        // 1) 매장 입장 가능 여부 체크 (열려 있음? 인원? 확률?)
        if (!CanEnterStore(npc))
        {
            SendOut(npc);
            return;
        }

        // 2) 선반 / 사격장 등 목적지 할당
        if (!TryAssignDestination(npc))
        {
            SendOut(npc);
            return;
        }

        // 3) 실제 입장 처리
        RegisterEnter(npc);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(TagNpc)) return;

        var npc = other.GetComponent<NpcController>();
        if (npc != null && npc.inStore && npc.isLeaving)
        {
            insideCount = Mathf.Max(0, insideCount - 1);
            UpdateCustomerUI();
            npc.inStore = false;
        }
    }

    // =========================================================
    // Public API (문 열고 닫기)
    // =========================================================
    public void OpenStore()  => isOpen = true;
    public void CloseStore() => isOpen = false;
    public void ToggleOpen() => isOpen = !isOpen;

    // =========================================================
    // Private helpers
    // =========================================================

    /// <summary>콜라이더가 유효한 NPC인지 확인하고 NpcController를 꺼내온다.</summary>
    private bool IsValidNpc(Collider other, out NpcController npc)
    {
        npc = null;
        if (!other.CompareTag(TagNpc)) return false;

        npc = other.GetComponent<NpcController>();
        if (npc == null) return false;
        if (npc.isLeaving) return false;

        return true;
    }

    /// <summary>매장이 열려 있고, 인원/확률 조건을 만족하면 true.</summary>
    private bool CanEnterStore(NpcController npc)
    {
        // 매장 닫혀있으면 입장 불가
        if (!isOpen) return false;

        // 인원 제한
        if (insideCount >= maxInStore) return false;

        // 입장 확률
        int effectiveChance = ComputeEffectiveChance();
        return RollEntry(effectiveChance);
    }

    /// <summary>NPC를 어느 목적지로 보낼지 결정하고 실제로 보낸다.</summary>
    private bool TryAssignDestination(NpcController npc)
    {
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

        return sent;
    }

    /// <summary>입장을 확정하고 카운트 / UI / 정산에 반영.</summary>
    private void RegisterEnter(NpcController npc)
    {
        insideCount++;
        UpdateCustomerUI();

        SettlementManager.Instance?.RegisterCustomerEnter(npc);
        npc.SetDoor(this);
        npc.inStore = true;
    }

    /// <summary>NPC를 출구 방향으로 돌려보낸다.</summary>
    private void SendOut(NpcController npc)
    {
        npc.StartLeaving(exitPoint);
    }

    private void UpdateCustomerUI()
    {
        if (customerText != null)
            customerText.text = "Customer : " + insideCount.ToString();
    }

    // ─────────────────────────────────────────────
    // 목적지 분기
    // ─────────────────────────────────────────────
    private bool TrySendToShelf(NpcController npc)
    {
        if (ShelfManager.Instance != null &&
            ShelfManager.Instance.TryGetAvailableSlot(out ShelfSlot slot))
        {
            npc.AllowEntry(slot.transform, exitPoint);
            Debug.Log("TrySendToShelf result = true");
            return true;
        }

        Debug.Log("TrySendToShelf result = false");
        return false;
    }

    private bool TrySendToRange(NpcController npc)
    {
        if (ShootingRangeManager.Instance != null &&
            ShootingRangeManager.Instance.TryGetAvailableLane(out ShootingLane lane))
        {
            npc.AllowRange(lane, exitPoint);
            Debug.Log("TrySendToRange result = true");
            return true;
        }

        Debug.Log("TrySendToRange result = false");
        return false;
    }

    // ─────────────────────────────────────────────
    // 확률 계산
    // ─────────────────────────────────────────────
    private int ComputeEffectiveChance()
    {
        int rep = SettlementManager.Instance != null
            ? SettlementManager.Instance.Reputation
            : 0;

        float minRep = Mathf.Min(repAtLow, repAtHigh);
        float maxRep = Mathf.Max(repAtLow, repAtHigh);

        float t = (Mathf.Approximately(minRep, maxRep))
            ? 1f
            : Mathf.InverseLerp(minRep, maxRep, rep);

        float bonus  = Mathf.Lerp(bonusAtLowRep, bonusAtHighRep, t);
        float chance = baseEntryChancePercent + bonus;
        chance       = Mathf.Clamp(chance, minFinalChance, maxFinalChance);

        return Mathf.RoundToInt(chance);
    }

    private bool RollEntry(int chancePercent)
    {
        chancePercent = Mathf.Clamp(chancePercent, 0, 100);
        int dice = Random.Range(0, 100);
        return dice < chancePercent;
    }
}