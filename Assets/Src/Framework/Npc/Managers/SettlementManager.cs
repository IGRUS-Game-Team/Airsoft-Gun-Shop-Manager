// 2025-08-13 이준서
// 목적: 최종 정산 UI용 집계/수정 전담 매니저(카운터/결제 로직과 분리)
// 사용법:
//  - 불평 모션 재생 시: SettlementManager.Instance.MarkNpcComplained(npc);
//    (애니메이션 이벤트로 호출 권장, 아래 NpcComplainAlerter 참고)
//  - 결제 완료 시: SettlementManager.Instance.OnPaymentCompleted(npc);
//    (NpcController.OnPaymentCompleted()에서 한 줄 호출 권장)

using System;
using System.Collections.Generic;
using UnityEngine;

public class SettlementManager : MonoBehaviour
{
    public static SettlementManager Instance { get; private set; }

    [Header("정상 결제 보상(Inspector)")]
    [SerializeField] int happyLevelGain = 1;       // 정상 결제 시 상점 레벨 증가치
    [SerializeField] int happyReputationGain = 3;  // 정상 결제 시 평판 증가치

    // =================== Debug Logging (on/off) ===================
    [Header("Debug Logging")]
    [SerializeField] bool debugLog = true;        // 로그 켜기/끄기
    [SerializeField] bool logEveryEvent = false;   // 매 이벤트 전체 스냅샷 로그? (false면 변경만)


    // 집계 값
    int satisfiedCustomers;     // 만족(불평 없이 정상 결제)
    int dissatisfiedCustomers;  // 불만족(불평 이후 결제)
    int shopLevel;              // 상점 레벨
    int reputation;             // 평판
    SettlementManager.Snapshot _prevSnap;
    bool _hasPrevSnap = false;

    // 불평 여부 추적 (결제 완료 시 분류 기준)
    readonly HashSet<NpcController> complained = new();

    // 정산 UI 바인딩용 읽기 전용 프로퍼티
    public int SatisfiedCustomers => satisfiedCustomers;
    public int DissatisfiedCustomers => dissatisfiedCustomers;
    public int ShopLevel => shopLevel;
    public int Reputation => reputation;

    // UI 실시간 갱신이 필요하면 구독
    public event Action<Snapshot> OnChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // === 외부 API =========================================================

    // 불평 발생 표시(중복 호출 무해)
    public void MarkNpcComplained(NpcController npc)
    {
        if (npc == null) return;
        complained.Add(npc);
    }

    // 결제 완료 시 호출: 만족/불만족 분류 + 레벨/평판 반영
    public void OnPaymentCompleted(NpcController npc)
    {
        if (npc == null) return;

        bool wasUnhappy = complained.Remove(npc); // 있었으면 true + 동시에 정리
        if (wasUnhappy)
        {
            dissatisfiedCustomers += 1;
        }
        else
        {
            satisfiedCustomers += 1;
            shopLevel += happyLevelGain;
            reputation += happyReputationGain;
        }

        OnChanged?.Invoke(GetSnapshot());
        LogChange("PaymentCompleted");      // ★ 추가
    }

    // === 정산 UI에서 값 수정/리셋이 필요할 때 =============================

    public void AddSatisfied(int v)
    {
        satisfiedCustomers = Mathf.Max(0, satisfiedCustomers + v); OnChanged?.Invoke(GetSnapshot());
    }
    public void AddDissatisfied(int v)
    {
        dissatisfiedCustomers = Mathf.Max(0, dissatisfiedCustomers + v); OnChanged?.Invoke(GetSnapshot());
    }
    public void AddShopLevel(int v)
    {
        shopLevel = Mathf.Max(0, shopLevel + v); OnChanged?.Invoke(GetSnapshot());
    }
    public void AddReputation(int v)
    {
        reputation = Mathf.Max(0, reputation + v); OnChanged?.Invoke(GetSnapshot());
        LogChange("AddReputation");         // ★ 추가
    }

    public void SetSatisfied(int v)
    {
        satisfiedCustomers = Mathf.Max(0, v); OnChanged?.Invoke(GetSnapshot());
    }
    public void SetDissatisfied(int v)
    {
        dissatisfiedCustomers = Mathf.Max(0, v); OnChanged?.Invoke(GetSnapshot());
    }
    public void SetShopLevel(int v)
    {
        shopLevel = Mathf.Max(0, v); OnChanged?.Invoke(GetSnapshot());
        LogChange("SetShopLevel");          // ★ 추가
    }
    public void SetReputation(int v)
    {
        reputation = Mathf.Max(0, v); OnChanged?.Invoke(GetSnapshot());
    }

    // 라운드/하루 리셋(레벨/평판은 유지하고 싶으면 keepFlags로 선택)
    [Flags] public enum KeepFlags { None = 0, KeepLevel = 1, KeepReputation = 2, KeepBoth = KeepLevel | KeepReputation }
    public void ResetForNewDay(KeepFlags keep = KeepFlags.KeepBoth)
    {
        satisfiedCustomers = 0;
        dissatisfiedCustomers = 0;
        if ((keep & KeepFlags.KeepLevel) == 0) shopLevel = 0;
        if ((keep & KeepFlags.KeepReputation) == 0) reputation = 0;
        complained.Clear();
        OnChanged?.Invoke(GetSnapshot());
        LogChange("ResetForNewDay");        // ★ 추가
    }

    // 스냅샷: UI에 한 번에 바인딩할 때 편함
    public Snapshot GetSnapshot() => new Snapshot
    {
        satisfied = satisfiedCustomers,
        dissatisfied = dissatisfiedCustomers,
        shopLevel = shopLevel,
        reputation = reputation
    };

    public struct Snapshot
    {
        public int satisfied;
        public int dissatisfied;
        public int shopLevel;
        public int reputation;
    }

    void LogChange(string reason)
    {
        if (!debugLog) return;

        var s = GetSnapshot();

        if (logEveryEvent || !_hasPrevSnap)
        {
            Debug.Log($"[Settlement:{reason}] " +
                      $"Satisfied={s.satisfied}, Dissatisfied={s.dissatisfied}, " +
                      $"ShopLevel={s.shopLevel}, Reputation={s.reputation}");
        }
        else
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (s.satisfied != _prevSnap.satisfied) sb.AppendLine($"  Satisfied    : {_prevSnap.satisfied} -> {s.satisfied}");
            if (s.dissatisfied != _prevSnap.dissatisfied) sb.AppendLine($"  Dissatisfied : {_prevSnap.dissatisfied} -> {s.dissatisfied}");
            if (s.shopLevel != _prevSnap.shopLevel) sb.AppendLine($"  ShopLevel    : {_prevSnap.shopLevel} -> {s.shopLevel}");
            if (s.reputation != _prevSnap.reputation) sb.AppendLine($"  Reputation   : {_prevSnap.reputation} -> {s.reputation}");

            if (sb.Length > 0)
                Debug.Log($"[Settlement:{reason}] Changed:\n{sb}");
        }

        _prevSnap = s;
        _hasPrevSnap = true;
    }

// ==============================================================
}