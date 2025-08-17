// RevenueXPSaveHandler.cs
using UnityEngine;

public class RevenueXPSaveHandler : MonoBehaviour, ISaveable
{
    [SerializeField] private RevenueXPTracker tracker;

    void Awake()
    {
        if (!tracker) tracker = RevenueXPTracker.Instance;
    }

    public object CaptureData()
    {
        if (!tracker)
        {
            Debug.LogWarning("[RevenueXPSaveHandler] Tracker 없음");
            return new RevenueXPSaveData { totalRevenueXP = 0f };
        }

        return new RevenueXPSaveData
        {
            totalRevenueXP = tracker.TotalRevenueXP
        };
    }

    public void RestoreData(object data)
    {
        var loaded = data as RevenueXPSaveData;
        if (!tracker || loaded == null) return;

        tracker.ForceSetXP(loaded.totalRevenueXP);
    }
}
