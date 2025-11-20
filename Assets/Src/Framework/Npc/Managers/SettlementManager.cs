using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SettlementManager : MonoBehaviour
{
    public static SettlementManager Instance { get; private set; }

    [Header("사격장 수입(Inspector)")]
    [SerializeField] float shootingRangeIncome = 20f;

    [Header("정상 결제 보상(Inspector)")]
    [SerializeField] int happyLevelGain = 1;

    [Header("영업 시간(게임 시각)")]
    [SerializeField] int openHour  = 8;
    [SerializeField] int closeHour = 20;

    [Header("Refs")]
    [SerializeField] TimeUI timeUI;

    [SerializeField] int   satisfiedCustomers;
    [SerializeField] int   dissatisfiedCustomers;
    [SerializeField] int   shopLevel;
    readonly HashSet<NpcController> complained = new();

    [Header("평판 변화량(Inspector)")]
    [SerializeField] float happyReputationDelta   = +3f;
    [SerializeField] float unhappyReputationDelta = -3f;

    [Header("오늘 집계(런타임)")]
    [SerializeField] int   totalCustomersToday;
    [SerializeField] float grossProfitToday;
    [SerializeField] float purchaseCostToday;
    [SerializeField] float netProfitToday;

    // ✅ 중복 결제 방지용: 오늘 처리된 영수증 ID들
    readonly HashSet<string> processedReceiptIds = new();

    // 총 고객 수 중복 방지
    readonly HashSet<int> countedNpcIdsToday = new();

    // 디버그(원인 추적에 유용)
    [Header("Debug")]
    public bool debugSalesLog = false;

    // UI 바인딩
    public event Action<Snapshot> OnChanged;
    public UnityEvent OnComplainInvoked = new();

    public int  SatisfiedCustomers   => satisfiedCustomers;
    public int  DissatisfiedCustomers=> dissatisfiedCustomers;
    public int  ShopLevel            => shopLevel;
    public int  TotalCustomersToday  => totalCustomersToday;
    public float GrossProfitToday    => grossProfitToday;
    public float PurchaseCostToday   => purchaseCostToday;
    public float CogsToday           => purchaseCostToday;
    public float NetProfitToday      => netProfitToday;
    public int Reputation            => Mathf.RoundToInt(ReputationState.CurrentGlobal);
    public int OpenHour              => openHour;
    public int CloseHour             => closeHour;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (ReputationState.Instance != null)
            ReputationState.Instance.OnChangedRaw.AddListener(OnRepChangedRelay);
    }

    // ───────── 외부 API ─────────
    public void MarkNpcComplained(NpcController npc)
    {
        if (npc == null) return;
        complained.Add(npc);
        OnComplainInvoked?.Invoke();
    }

    public void OnPaymentCompleted(NpcController npc)
    {
        if (npc == null) return;

        bool had = complained.Remove(npc);
        if (had)
        {
            dissatisfiedCustomers++;
            if (unhappyReputationDelta != 0f)
                ReputationState.Instance?.Add(unhappyReputationDelta);
        }
        else
        {
            satisfiedCustomers++;
            shopLevel += happyLevelGain;

            if (happyReputationDelta != 0f)
                ReputationState.Instance?.Add(happyReputationDelta);
        }

        OnChanged?.Invoke(GetSnapshot());
    }

    public void RegisterCustomerEnter(NpcController npc)
    {
        if (npc == null || timeUI == null) return;

        int hours = (timeUI.totalGameMinutes / 60) % 24;
        if (hours < openHour || hours >= closeHour) return;

        int id = npc.GetInstanceID();
        if (!countedNpcIdsToday.Add(id)) return;

        totalCustomersToday++;
        OnChanged?.Invoke(GetSnapshot());
    }

    public void RegisterPurchaseCost(float totalCost)
    {
        totalCost = Mathf.Max(0f, totalCost);
        purchaseCostToday += totalCost;
        netProfitToday = grossProfitToday - purchaseCostToday;
        OnChanged?.Invoke(GetSnapshot());
    }

    public void ExcessChangeCost(float totalCost)
    {
        totalCost = Mathf.Max(0f, totalCost);
        purchaseCostToday += totalCost;
        netProfitToday = grossProfitToday - purchaseCostToday;
        OnChanged?.Invoke(GetSnapshot());
    }

    /// <summary>
    /// ✅ 결제 금액 반영 (같은 영수증 ID는 한 번만 반영)
    /// </summary>
    public void RegisterSaleAmount(float saleAmount, string receiptId = null)
    {
        if (!string.IsNullOrEmpty(receiptId))
        {
            // 이미 처리한 영수증이면 무시
            if (!processedReceiptIds.Add(receiptId))
            {
                if (debugSalesLog) Debug.Log($"[Sale] Skip duplicate receipt {receiptId}");
                return;
            }
        }

        saleAmount = Mathf.Max(0f, saleAmount);
        if (debugSalesLog) Debug.Log($"[Sale] +{saleAmount:0.00} (id={receiptId})");

        grossProfitToday += saleAmount;
        netProfitToday = grossProfitToday - purchaseCostToday;

        OnChanged?.Invoke(GetSnapshot());
    }

    public void RegisterShootingRangeUse(string sessionId = null)
    {
        // 사격장도 세션/라운드별로 한 번만 반영하고 싶다면 ID 넘겨주세요
        RegisterSaleAmount(shootingRangeIncome, sessionId);

        GameState.Instance?.AddRevenue(shootingRangeIncome);
    }

    public void ResetToday()
    {
        totalCustomersToday = 0;
        grossProfitToday    = 0f;
        purchaseCostToday   = 0f;
        netProfitToday      = 0f;
        countedNpcIdsToday.Clear();
        processedReceiptIds.Clear(); // ✅ 영수증 가드 초기화

        OnChanged?.Invoke(GetSnapshot());
    }

    public void OnCustomerLeftUnhappy(NpcController npc)
    {
        if (npc == null) return;

        bool had = complained.Remove(npc);
        if (had)
        {
            dissatisfiedCustomers++;
            if (unhappyReputationDelta != 0f)
                ReputationState.Instance?.Add(unhappyReputationDelta);
            OnChanged?.Invoke(GetSnapshot());
        }
    }

    void OnEnable()
    {
        if (ReputationState.Instance != null)
            ReputationState.Instance.OnChangedRaw.AddListener(OnRepChangedRelay);
    }

    void OnDisable()
    {
        if (ReputationState.Instance != null)
            ReputationState.Instance.OnChangedRaw.RemoveListener(OnRepChangedRelay);
    }

    private void OnRepChangedRelay(float _) => OnChanged?.Invoke(GetSnapshot());

    [Flags] public enum KeepFlags { None = 0, KeepLevel = 1, KeepReputation = 2, KeepBoth = KeepLevel | KeepReputation }
    public void ResetForNewDay(KeepFlags keep = KeepFlags.KeepBoth)
    {
        satisfiedCustomers = 0;
        dissatisfiedCustomers = 0;

        if ((keep & KeepFlags.KeepLevel) == 0)
            shopLevel = 0;

        if ((keep & KeepFlags.KeepReputation) == 0)
            ReputationState.Instance?.SetRaw(0f);

        complained.Clear();
        OnChanged?.Invoke(GetSnapshot());
    }

    public Snapshot GetSnapshot() => new Snapshot
    {
        satisfied      = satisfiedCustomers,
        dissatisfied   = dissatisfiedCustomers,
        shopLevel      = shopLevel,
        reputation     = Mathf.RoundToInt(ReputationState.CurrentGlobal),
        totalCustomers = totalCustomersToday,
        grossProfit    = grossProfitToday,
        purchaseCost   = purchaseCostToday,
        netProfit      = netProfitToday
    };

    public struct Snapshot
    {
        public int   satisfied;
        public int   dissatisfied;
        public int   shopLevel;
        public int   reputation;
        public int   totalCustomers;
        public float grossProfit;
        public float purchaseCost;
        public float netProfit;
    }

    // 테스트용 수동 조정
    public void AddSatisfied(int v)    { satisfiedCustomers    = Mathf.Max(0, satisfiedCustomers + v);    OnChanged?.Invoke(GetSnapshot()); }
    public void AddDissatisfied(int v) { dissatisfiedCustomers = Mathf.Max(0, dissatisfiedCustomers + v); OnChanged?.Invoke(GetSnapshot()); }
    public void AddShopLevel(int v)    { shopLevel             = Mathf.Max(0, shopLevel + v);             OnChanged?.Invoke(GetSnapshot()); }
    public void SetSatisfied(int v)    { satisfiedCustomers    = Mathf.Max(0, v);                          OnChanged?.Invoke(GetSnapshot()); }
    public void SetDissatisfied(int v) { dissatisfiedCustomers = Mathf.Max(0, v);                          OnChanged?.Invoke(GetSnapshot()); }
    public void SetShopLevel(int v)    { shopLevel             = Mathf.Max(0, v);                          OnChanged?.Invoke(GetSnapshot()); }
}
