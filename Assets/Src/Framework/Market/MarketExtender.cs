using System.Collections;
using UnityEngine;

public class MarketExtender : MonoBehaviour
{
    [SerializeField] GameObject market;
    [SerializeField] GameObject building;
    [SerializeField] GameObject middleWall;
    [SerializeField] GameObject marketWindow;
    [SerializeField] GameObject eraseWindow;
    [SerializeField] int unlockLevel = 2; // 레벨 2에서 해제

    void OnEnable()
    {
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
        if (RevenueXPTracker.Instance)
            OnLevelChanged(RevenueXPTracker.Instance.CurrentLevel); // 현재 레벨 즉시 반영
        else
            Apply(unlocked:false); // 트래커 없으면 기본 잠금 상태
    }

    void OnLevelChanged(int level)
    {
        Apply(level >= unlockLevel);
    }

    void Apply(bool unlocked)
    {
        if (eraseWindow) eraseWindow.SetActive(!unlocked);
        if (marketWindow) marketWindow.SetActive(unlocked);
        if (middleWall) middleWall.SetActive(!unlocked);
        if (market) market.SetActive(unlocked);
        if (building) building.SetActive(!unlocked);
    }
}