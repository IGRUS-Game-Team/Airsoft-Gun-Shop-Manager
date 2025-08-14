// 2025-08-14 이준서
// SettlementManager (최종 정산 전담 매니저)
// ─ 목적 ──────────────────────────────────────────────────────────────
// - 최종 정산 UI에 필요한 값만 "집계/수정" 담당(카운터/결제 로직과 분리)
// - 만족/불만족, 상점 레벨/평판 + "총 고객 수 / 이익 / 순이익"까지 한 곳에서 관리
//
// ─ 연결(훅) 가이드 ───────────────────────────────────────────────────
// [입장 시점] DoorTrigger 등에서   : SettlementManager.Instance.RegisterCustomerEnter(npc);
// [매입 완료] PurchaseProcessor     : SettlementManager.Instance.RegisterPurchaseCost(totalCost);
// [결제 완료] (카드/현금 성공 핸들러) : 
//              var sale = countorMonitorController.GetCurrentTotalAmount();
//              SettlementManager.Instance.RegisterSaleAmount(sale);
// [만족/불만족] NpcController.OnPaymentCompleted() 에서 그대로 유지:
//              SettlementManager.Instance.OnPaymentCompleted(this);

using System;
using System.Collections.Generic;
using UnityEngine;

public class SettlementManager : MonoBehaviour
{
    public static SettlementManager Instance { get; private set; }

    // ───────── 기본 보상(Inspector) ─────────
    [Header("정상 결제 보상(Inspector)")]
    [SerializeField] int happyLevelGain = 1;       // 정상 결제 시 상점 레벨 증가치
    [SerializeField] int happyReputationGain = 3;  // 정상 결제 시 평판 증가치

    // ───────── 영업시간 & 참조 ─────────
    [Header("영업 시간(게임 시각)")]
    [SerializeField] int openHour  = 8;   // 08:00 포함
    [SerializeField] int closeHour = 20;  // 20:00 제외

    [Header("Refs")]
    [SerializeField] TimeUI timeUI;       // 게임 시각: timeUI.totalGameMinutes

    // ───────── 만족/불만족/레벨/평판 ─────────
    [SerializeField] int satisfiedCustomers;     // 만족(불평 없이 정상 결제)
    [SerializeField] int dissatisfiedCustomers;  // 불만족(불평 이후 결제)
    [SerializeField] int shopLevel;              // 상점 레벨
    [SerializeField] int reputation;             // 평판
    readonly HashSet<NpcController> complained = new(); // 불평 여부 추적

    // ───────── 오늘 집계(총/이익/순이익) ─────────
    [Header("오늘 집계(런타임)")]
    [SerializeField] int   totalCustomersToday;   // 08~20 입장 고객
    [SerializeField] float grossProfitToday;      // 이익(매출 합) = 결제 완료 시 모니터 합계 누적
    [SerializeField] float purchaseCostToday;     // 매입가 합 = PurchaseProcessor.totalCost 누적
    [SerializeField] float netProfitToday;        // 순이익 = Gross - Purchase

    // 총 고객 수 중복 방지
    readonly HashSet<int> countedNpcIdsToday = new();

    // ───────── UI 바인딩 ─────────
    public event Action<Snapshot> OnChanged;

    // 읽기 전용 프로퍼티(필요 시 UI에서 직접 참조)
    public int   SatisfiedCustomers    => satisfiedCustomers;
    public int   DissatisfiedCustomers => dissatisfiedCustomers;
    public int   ShopLevel             => shopLevel;
    public int   Reputation            => reputation;

    public int   TotalCustomersToday   => totalCustomersToday;
    public float GrossProfitToday      => grossProfitToday;
    public float PurchaseCostToday     => purchaseCostToday;
    public float CogsToday             => purchaseCostToday; // ▼ 호환용 별칭(기존 cogsToday 참조 대비)
    public float NetProfitToday        => netProfitToday;

    public int OpenHour  => openHour;
    public int CloseHour => closeHour;
    
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ───────── 외부 API: 불평 기록 ─────────
    public void MarkNpcComplained(NpcController npc)
    {
        if (npc == null) return;
        complained.Add(npc);
    }

    // ───────── 외부 API: 결제 완료(만족/불만족 분류) ─────────
    // ※ 매출 집계는 성공 핸들러에서 RegisterSaleAmount(...)로 이미 처리함
    public void OnPaymentCompleted(NpcController npc)
    {
        if (npc == null) return;

        bool had = complained.Remove(npc);
        if (had) { dissatisfiedCustomers++; }
        else     { satisfiedCustomers++; shopLevel += happyLevelGain; reputation += happyReputationGain; }

        OnChanged?.Invoke(GetSnapshot());
    }

    // ───────── 외부 API: 총 고객 수(입장 시) ─────────
    public void RegisterCustomerEnter(NpcController npc)
    {
        if (npc == null || timeUI == null) return;

        int hours = (timeUI.totalGameMinutes / 60) % 24;
        if (hours < openHour || hours >= closeHour) return; // 영업시간 외 → 카운트 X

        int id = npc.GetInstanceID();
        if (countedNpcIdsToday.Contains(id)) return;

        countedNpcIdsToday.Add(id);
        totalCustomersToday++;
        OnChanged?.Invoke(GetSnapshot());
    }

    // ───────── 외부 API: 매입 비용 반영 (PurchaseProcessor) ─────────
    public void RegisterPurchaseCost(float totalCost)
    {
        totalCost = Mathf.Max(0f, totalCost);
        purchaseCostToday += totalCost;
        netProfitToday     = grossProfitToday - purchaseCostToday;

        OnChanged?.Invoke(GetSnapshot());
    }
    
    public void ExcessChangeCost(float totalCost)
    {
        totalCost = Mathf.Max(0f, totalCost);
        purchaseCostToday += totalCost;
        netProfitToday     = grossProfitToday - purchaseCostToday;

        OnChanged?.Invoke(GetSnapshot());
    }

    // ───────── 외부 API: 판매 금액 반영 (카드/현금 성공 핸들러) ─────────
    public void RegisterSaleAmount(float saleAmount)
    {
        saleAmount = Mathf.Max(0f, saleAmount);
        grossProfitToday += saleAmount;
        netProfitToday = grossProfitToday - purchaseCostToday;

        OnChanged?.Invoke(GetSnapshot());
    }

    // ───────── 하루 리셋(정산 UI 닫고 다음날 시작) ─────────
    public void ResetToday()
    {
        totalCustomersToday = 0;
        grossProfitToday    = 0f;
        purchaseCostToday   = 0f;
        netProfitToday      = 0f;
        countedNpcIdsToday.Clear();

        OnChanged?.Invoke(GetSnapshot());
    }

    // ───────── 라운드/하루 리셋(만족/불만족/레벨/평판 세트) ─────────
    [Flags] public enum KeepFlags { None = 0, KeepLevel = 1, KeepReputation = 2, KeepBoth = KeepLevel | KeepReputation }
    public void ResetForNewDay(KeepFlags keep = KeepFlags.KeepBoth)
    {
        satisfiedCustomers    = 0;
        dissatisfiedCustomers = 0;
        if ((keep & KeepFlags.KeepLevel) == 0)      shopLevel   = 0;
        if ((keep & KeepFlags.KeepReputation) == 0) reputation  = 0;
        complained.Clear();

        OnChanged?.Invoke(GetSnapshot());
    }

    // ───────── Snapshot (UI에 한 번에 바인딩) ─────────
    public Snapshot GetSnapshot() => new Snapshot
    {
        satisfied      = satisfiedCustomers,
        dissatisfied   = dissatisfiedCustomers,
        shopLevel      = shopLevel,
        reputation     = reputation,

        totalCustomers = totalCustomersToday,
        grossProfit    = grossProfitToday,
        purchaseCost   = purchaseCostToday, // ▼ cogs 대신 purchaseCost로 명확히 표기
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
        public float purchaseCost; // (구)cogs
        public float netProfit;
    }

    // ───────── 값 수동 보정/설정(테스트용) ─────────
    public void AddSatisfied(int v)        { satisfiedCustomers    = Mathf.Max(0, satisfiedCustomers + v);    OnChanged?.Invoke(GetSnapshot()); }
    public void AddDissatisfied(int v)     { dissatisfiedCustomers = Mathf.Max(0, dissatisfiedCustomers + v); OnChanged?.Invoke(GetSnapshot()); }
    public void AddShopLevel(int v)        { shopLevel             = Mathf.Max(0, shopLevel + v);             OnChanged?.Invoke(GetSnapshot()); }
    public void AddReputation(int v)       { reputation            = Mathf.Max(0, reputation + v);            OnChanged?.Invoke(GetSnapshot()); }

    public void SetSatisfied(int v)        { satisfiedCustomers    = Mathf.Max(0, v);                          OnChanged?.Invoke(GetSnapshot()); }
    public void SetDissatisfied(int v)     { dissatisfiedCustomers = Mathf.Max(0, v);                          OnChanged?.Invoke(GetSnapshot()); }
    public void SetShopLevel(int v)        { shopLevel             = Mathf.Max(0, v);                          OnChanged?.Invoke(GetSnapshot()); }
    public void SetReputation(int v)       { reputation            = Mathf.Max(0, v);                          OnChanged?.Invoke(GetSnapshot()); }
}