using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class MarketExtender : MonoBehaviour
{
    [Header("Market / Building 토글")]
    [SerializeField] GameObject market;
    [SerializeField] GameObject building;
    [SerializeField] GameObject middleWall;
    [SerializeField] GameObject marketWindow;
    [SerializeField] GameObject eraseWindow;

    [FormerlySerializedAs("unlockLevel")]
    [SerializeField] int marketUnlockLevel = 2;   // 레벨 2에서 마켓 오픈

    [Header("Shooting Range 토글")]
    [SerializeField] GameObject shootingRange;     // 사격장 루트(레벨 3부터 활성화)
    [SerializeField] int rangeUnlockLevel = 3;     // 레벨 3에서 사격장 오픈
    [SerializeField] GameObject rangeDoor;         // 잠금 상태에서 켜짐, 오픈 시 끔

    [Header("가격(달러)")]
    [SerializeField] float marketPrice = 30000f;
    [SerializeField] float rangePrice  = 50000f;

    // 구매 상태 저장 키
    const string KeyMarketPurchased = "StoreExpansionPurchased";
    const string KeyRangePurchased  = "ShootingRangePurchased";

    // 외부에서 확인 가능
    public bool MarketPurchased { get; private set; }
    public bool RangePurchased  { get; private set; }

    void OnEnable()
    {
        // 저장된 구매 상태 로드
        MarketPurchased = PlayerPrefs.GetInt(KeyMarketPurchased, 0) == 1;
        RangePurchased  = PlayerPrefs.GetInt(KeyRangePurchased,  0) == 1;

        TrySubscribe();
        TryApplyImmediate();
    }

    void OnDisable()
    {
        Unsubscribe();
        StopAllCoroutines();
    }

    void TrySubscribe()
    {
        if (RevenueXPTracker.Instance)
        {
            RevenueXPTracker.Instance.OnLevelChanged.RemoveListener(OnLevelChanged);
            RevenueXPTracker.Instance.OnLevelChanged.AddListener(OnLevelChanged);
        }
        else
        {
            StartCoroutine(WaitAndSubscribe());
        }
    }

    IEnumerator WaitAndSubscribe()
    {
        while (RevenueXPTracker.Instance == null) yield return null;
        RevenueXPTracker.Instance.OnLevelChanged.AddListener(OnLevelChanged);
        TryApplyImmediate();
    }

    void Unsubscribe()
    {
        if (RevenueXPTracker.Instance)
            RevenueXPTracker.Instance.OnLevelChanged.RemoveListener(OnLevelChanged);
    }

    void TryApplyImmediate()
    {
        int level = RevenueXPTracker.Instance ? RevenueXPTracker.Instance.CurrentLevel : 0;
        Apply(level); // 현재 상태 즉시 반영
    }

    void OnLevelChanged(int level) => Apply(level);

    void Apply(int level)
    {
        // 레벨은 버튼 활성화 같은 UI 조건용으로만 쓰고
        // 실제 열림 여부는 '구매 여부'만 따진다.
        bool marketUnlocked = MarketPurchased;
        bool rangeUnlocked = RangePurchased;

        // ─ Market / Building
        if (eraseWindow) eraseWindow.SetActive(!marketUnlocked);
        if (marketWindow) marketWindow.SetActive(marketUnlocked);
        if (middleWall) middleWall.SetActive(!marketUnlocked);
        if (market) market.SetActive(marketUnlocked);
        if (building) building.SetActive(!marketUnlocked);

        // ─ Shooting Range 본체 + 문
        if (shootingRange) shootingRange.SetActive(rangeUnlocked);
        if (rangeDoor) rangeDoor.SetActive(!rangeUnlocked);
    }

    // ========== 구매 호출 (UI 버튼 OnClick에서 연결) ==========

    public void PurchaseMarketExpansion()
    {
        int level = RevenueXPTracker.Instance ? RevenueXPTracker.Instance.CurrentLevel : 0;

        if (MarketPurchased) { Debug.Log("이미 구입된 확장입니다."); return; }
        if (level < marketUnlockLevel) { Debug.Log("레벨이 부족합니다."); return; }

        if (GameState.Instance != null)
        {
            if (!GameState.Instance.SpendMoney(marketPrice))
            {
                Debug.Log("돈이 부족합니다.");
                return;
            }
        }
        // GameState가 없다면(테스트 씬) 돈 차감 없이 진행

        MarketPurchased = true;
        PlayerPrefs.SetInt(KeyMarketPurchased, 1);
        PlayerPrefs.Save();

        Apply(level);
    }

    public void PurchaseShootingRange()
    {
        int level = RevenueXPTracker.Instance ? RevenueXPTracker.Instance.CurrentLevel : 0;

        if (RangePurchased) { Debug.Log("이미 구입된 사격장입니다."); return; }
        if (level < rangeUnlockLevel) { Debug.Log("레벨이 부족합니다."); return; }

        if (GameState.Instance != null)
        {
            if (!GameState.Instance.SpendMoney(rangePrice))
            {
                Debug.Log("돈이 부족합니다.");
                return;
            }
        }

        RangePurchased = true;
        PlayerPrefs.SetInt(KeyRangePurchased, 1);
        PlayerPrefs.Save();

        Apply(level);
    }

    // 디버그 초기화(선택)
    [ContextMenu("DEBUG: Reset Purchases")]
    public void ResetPurchases()
    {
        MarketPurchased = false;
        RangePurchased  = false;
        PlayerPrefs.DeleteKey(KeyMarketPurchased);
        PlayerPrefs.DeleteKey(KeyRangePurchased);
        TryApplyImmediate();
    }
}