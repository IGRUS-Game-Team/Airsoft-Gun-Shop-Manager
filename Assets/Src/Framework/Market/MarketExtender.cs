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

    [Header("Shooting Range 문(단일 오브젝트)")]
    [FormerlySerializedAs("rangeDoorClosed")]
    [SerializeField] GameObject rangeDoor;         // 잠금 상태에서 켜짐, 오픈 시 끔

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
            Apply(0); // 트래커 없으면 기본 잠금 상태(레벨 0 가정)
    }

    void OnLevelChanged(int level)
    {
        Apply(level);
    }

    void Apply(int level)
    {
        bool marketUnlocked = level >= marketUnlockLevel;
        bool rangeUnlocked  = level >= rangeUnlockLevel;

        // ─ Market / Building
        if (eraseWindow)  eraseWindow.SetActive(!marketUnlocked);
        if (marketWindow) marketWindow.SetActive(marketUnlocked);
        if (middleWall)   middleWall.SetActive(!marketUnlocked);
        if (market)       market.SetActive(marketUnlocked);
        if (building)     building.SetActive(!marketUnlocked);

        // ─ Shooting Range 본체
        if (shootingRange) shootingRange.SetActive(rangeUnlocked);

        // ─ Shooting Range 문: 레벨 도달 시 문 오브젝트 하나만 끄기
        if (rangeDoor) rangeDoor.SetActive(!rangeUnlocked);
    }
}