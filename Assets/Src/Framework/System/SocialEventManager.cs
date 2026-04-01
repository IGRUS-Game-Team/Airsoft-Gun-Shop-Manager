using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 일일 경제 이벤트를 관리하는 중앙 제어 매니저.
/// 매일 아침 랜덤 전략을 선택하고, 해당 전략의 가격 변동률을 다른 시스템에 전파.
///
/// [하루 시작 시 호출 흐름]
/// ExecuteStrategy()
///   → 1일차: 강제 NormalEventStrategy
///   → 2일차~: SelectRandomStrategy() (확률: Normal 50%, Recession 30%, Boom 20%)
///   → GetEventStrategyData() → 이벤트명/변동률 랜덤 생성
///   → MarketPriceDataManager.ResetAllPrices() → 전날 변동 초기화
///   → SelectAffectedItems() → 영향받는 아이템 결정
///   → DeliverMarketPriceData() → OnMarketPriceChanged 이벤트 발사
///   → DeliverEventUIData() → 뉴스/UI 업데이트
///   → ShouldStartProtest() → 시위 이벤트 트리거
///
/// [이벤트 구독 (옵저버 패턴)]
/// - OnMarketPriceChanged(itemId, modifier) → MarketPriceDataManager가 구독
/// - OnEventUIUpdate(eventName, status, itemName) → UI가 구독
/// - OnNewsScreenUpdate(eventName) → 뉴스 화면이 구독
/// - OnProtestToggled(bool) → ProtestDirector가 구독
///
/// [새 전략 추가]
/// 1. ISocialEventStrategy 구현 클래스 생성
/// 2. CreateStrategy()에 case 추가
/// 3. strategyChances[] 배열에 확률 추가
/// </summary>
public class SocialEventManager : MonoBehaviour
{
    public static SocialEventManager Instance { get; private set; }//싱글톤

     // === (추가) 시위 신호 이벤트 ===
    public static event Action<bool> OnProtestToggled; // true=시작, false=종료

    //상품 목록 가져오기
    [Header("ItemDatabase 상품 데이터베이스")]
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("전략 확률 설정 (%)")]
    [SerializeField] private int[] strategyChances = { 50, 30, 20 }; // 배열로 관리

      // === (추가) 시위 트리거 옵션 ===
    [Header("시위 트리거(간단 키워드 매칭)")]
    [SerializeField] private bool enableProtestSignal = true;
    [SerializeField] private float protestAutoStopAfter = 0f;    // 0 이하면 자동 종료 안 함 (바운서로만 해산)
    [SerializeField] private string[] protestKeywords = { "shooting incident" };

    [Header("디버그")]
    [SerializeField] private KeyCode debugShootingIncidentKey = KeyCode.None;

    //전략들 저장
    private ISocialEventStrategy currentStrategy; //인스턴스 참조
    private int itemId;  //상품 id -> 시장 변동률 적용할 상품
    private string itemName; //상품 이름
    private ItemData selectedItemData; //아이템 so

    const string KeyFirstDay = "SocialEvent_FirstDayDone";

    // 옵저버 패턴을 위한 이벤트들
    public static event Action<int, float> OnMarketPriceChanged;  // id, 시장변동률
    public static event Action<string, string, string> OnEventUIUpdate; // 이벤트이름, 상태, 아이템이름
    public static event Action<string> OnNewsScreenUpdate; //뉴스 화면 업데이트
//------------------------------------------------------------------------------------------
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    // 이벤트에 영향받는 아이템 목록 (규제: 1~2개, 일반: 전체 총기)
    private List<ItemData> _affectedItems = new();

    //전략 실행 및 데이터 전달
    public void ExecuteStrategy()
    {
        // 첫날은 무조건 Normal (평범한 날)
        if (PlayerPrefs.GetInt(KeyFirstDay, 0) == 0)
        {
            PlayerPrefs.SetInt(KeyFirstDay, 1);
            PlayerPrefs.Save();
            currentStrategy = new NormalEventStrategy();
        }
        else
        {
            SelectRandomStrategy();//전략 랜덤 선택
        }
        if (currentStrategy == null) return;

        currentStrategy.GetEventStrategyData(); // 전략에서 랜덤 데이터 생성

        // 전날 변동분 리셋
        if (MarketPriceDataManager.Instance != null)
            MarketPriceDataManager.Instance.ResetAllPrices();

        SelectAffectedItems();     // 영향 대상 결정
        DeliverMarketPriceData();  // 시장 가격 업데이트
        DeliverEventUIData();      // UI 업데이트

        // 시위 신호
        if (enableProtestSignal)
        {
            if (ShouldStartProtest()) ToggleProtest(true);
            else                      ToggleProtest(false);
        }
    }

    /// <summary>
    /// 이벤트 종류에 따라 영향받는 아이템을 결정한다.
    /// 규제 이벤트: MainWeapon 중 랜덤 1~2개
    /// 일반 이벤트: MainWeapon 전체
    /// Normal: 없음
    /// </summary>
    private void SelectAffectedItems()
    {
        _affectedItems.Clear();

        // Normal이면 영향 아이템 없음
        if (Mathf.Approximately(currentStrategy.MarketModifier, 0f)) return;

        if (currentStrategy.IsGunRegulation)
        {
            // 규제 이벤트 → 랜덤 1~2개 총기
            int count = UnityEngine.Random.Range(1, 3); // 1 또는 2
            _affectedItems = itemDatabase.GetRandomItems(ItemCategory.MainWeapon, count);
        }
        else
        {
            // 일반 이벤트 → 전체 총기
            _affectedItems = itemDatabase.GetItemsByCategory(ItemCategory.MainWeapon);
        }

        // 하위 호환: selectedItemData에 첫 번째 아이템 저장
        if (_affectedItems.Count > 0)
        {
            selectedItemData = _affectedItems[0];
            itemName = ItemNameResolver.Get(selectedItemData);
            itemId = selectedItemData.itemId;
        }
    }

    // 시장 변동률 전달 — 영향 아이템 각각에 대해 이벤트 발사
    private void DeliverMarketPriceData()
    {
        foreach (var item in _affectedItems)
            OnMarketPriceChanged?.Invoke(item.itemId, currentStrategy.MarketModifier);
    }

    //이벤트 이름, 상태, 무기 이름 전달
    private void DeliverEventUIData()
    {
        if (currentStrategy == null) return;

        string eventName = currentStrategy.EventName ?? "알 수 없는 이벤트";
        string eventStatus = currentStrategy.StatusText ?? "사회 상태?";

        // 규제: 구체적 총기명 나열, 일반: "All Guns"
        string displayItemName;
        if (_affectedItems.Count == 0)
            displayItemName = "";
        else if (currentStrategy.IsGunRegulation)
        {
            var names = new List<string>();
            foreach (var it in _affectedItems)
                names.Add(ItemNameResolver.Get(it) ?? it.itemName);
            displayItemName = string.Join(", ", names);
        }
        else
            displayItemName = "All Guns";

        OnEventUIUpdate?.Invoke(eventName, eventStatus, displayItemName);
        OnNewsScreenUpdate?.Invoke(eventName);
    }

    //전략 선택적으로 바꾸기
    public void SetSocialEventStrategy(ISocialEventStrategy Isocialevent)
    {
        this.currentStrategy = Isocialevent; //가져온 전략을 본 ScialEventManager에 저장하는 것
    }


    //""랜덤"" 전략 선택
    private void SelectRandomStrategy()
    {
        // 누적 확률 자동 계산
        int[] cumulativeChances = new int[strategyChances.Length];
        int total = 0;

        for (int i = 0; i < strategyChances.Length; i++)
        {
            total += strategyChances[i];
            cumulativeChances[i] = total;
        }

        int randomValue = UnityEngine.Random.Range(1, total + 1); // 1 ~ 총합

        // 선택된 전략 찾기
        for (int i = 0; i < cumulativeChances.Length; i++)
        {
            if (randomValue <= cumulativeChances[i])
            {
                currentStrategy = CreateStrategy(i); //선택한 전략 저장
                Debug.Log($"{currentStrategy.StatusText} 선택됨 (확률: {strategyChances[i]}%)");
                return;
            }
        }
    }


    //전략들 고르기
    private ISocialEventStrategy CreateStrategy(int index)
    {
        switch (index)
        {
            case 0: return new NormalEventStrategy();
            case 1: return new RecessionEventStrategy();
            case 2: return new BoomEventStrategy();
            default: return new NormalEventStrategy();
        }
    }

     // === (추가) 시위 시작/종료 신호 쏘기 ===
    public void ToggleProtest(bool on)
    {
        OnProtestToggled?.Invoke(on);
    }

    // === (추가) 현재 전략이 시위 트리거인지 간단 판정 ===
    private bool ShouldStartProtest()
    {
        if (currentStrategy == null) return false;

        string name   = currentStrategy.EventName  ?? string.Empty;
        string status = currentStrategy.StatusText ?? string.Empty;

        foreach (var k in protestKeywords)
        {
            if (string.IsNullOrEmpty(k)) continue;
            if (name.Contains(k, StringComparison.OrdinalIgnoreCase))   return true;
            if (status.Contains(k, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    
    // 디버그 등 외부에서 뉴스 화면 갱신 이벤트를 발생시킬 때 사용
    public static void InvokeNewsScreenUpdate(string eventName)
    {
        OnNewsScreenUpdate?.Invoke(eventName);
    }

    void Update()
    {
        if (debugShootingIncidentKey != KeyCode.None && Input.GetKeyDown(debugShootingIncidentKey))
            DebugForceShootingIncident();
    }

    [ContextMenu("DEBUG: Force Shooting Incident")]
    public void DebugForceShootingIncident()
    {
        currentStrategy = new RecessionEventStrategy();

        // "shooting incident" 이벤트가 나올 때까지 재생성
        for (int i = 0; i < 100; i++)
        {
            currentStrategy.GetEventStrategyData();
            if (currentStrategy.EventName.Contains("shooting incident", StringComparison.OrdinalIgnoreCase))
                break;
        }

        if (MarketPriceDataManager.Instance != null)
            MarketPriceDataManager.Instance.ResetAllPrices();

        SelectAffectedItems();
        DeliverMarketPriceData();
        DeliverEventUIData();

        if (enableProtestSignal)
            ToggleProtest(true);

        Debug.Log("[SocialEventManager] 디버그: 총기 사건 강제 발생");
    }

    // ───────── 세이브/로드 ─────────
    public int GetStrategyIndex()
    {
        if (currentStrategy is NormalEventStrategy)    return 0;
        if (currentStrategy is RecessionEventStrategy) return 1;
        if (currentStrategy is BoomEventStrategy)      return 2;
        return 0;
    }

    public List<int> GetAffectedItemIds()
    {
        var ids = new List<int>();
        foreach (var it in _affectedItems)
            if (it != null) ids.Add(it.itemId);
        return ids;
    }

    public float GetMarketModifier()
    {
        return currentStrategy?.MarketModifier ?? 0f;
    }

    public string GetEventName()
    {
        return currentStrategy?.EventName ?? "";
    }

    public string GetStatusText()
    {
        return currentStrategy?.StatusText ?? "";
    }

    public bool GetFirstDayDone()
    {
        return PlayerPrefs.GetInt(KeyFirstDay, 0) == 1;
    }

    public void RestoreState(SocialEventSaveData d)
    {
        if (d == null) return;

        // firstDayDone 복원
        PlayerPrefs.SetInt(KeyFirstDay, d.firstDayDone ? 1 : 0);
        PlayerPrefs.Save();

        // 전략 복원
        currentStrategy = CreateStrategy(d.strategyIndex);
        currentStrategy.GetEventStrategyData();

        // 영향 아이템 복원
        _affectedItems.Clear();
        if (d.affectedItemIds != null && itemDatabase != null)
        {
            foreach (var id in d.affectedItemIds)
            {
                var it = itemDatabase.GetById(id);
                if (it != null) _affectedItems.Add(it);
            }
        }

        if (_affectedItems.Count > 0)
        {
            selectedItemData = _affectedItems[0];
            itemName = ItemNameResolver.Get(selectedItemData);
            itemId = selectedItemData.itemId;
        }

        // 시세 반영은 MarketPriceSaveHandler가 별도 복원하므로 여기서는 UI만 갱신
        // 뉴스 씬 로드 억제 — TV 화면 이미지만 갱신하고 뉴스 재생은 하지 않는다
        NewsDeskLoader.SuppressAutoLoad = true;
        DeliverEventUIData();
        NewsDeskLoader.SuppressAutoLoad = false;
        Debug.Log($"[Load] SocialEvent ← strategy {d.strategyIndex}");
    }

    // 외부에서 현재 선택된 아이템 정보를 가져올 수 있는 메서드들
    public ItemData GetSelectedItem() => selectedItemData;
    public int GetSelectedItemId() => selectedItemData?.itemId ?? 0;
    public string GetSelectedItemName() => ItemNameResolver.Get(selectedItemData) ?? "없음";

}
