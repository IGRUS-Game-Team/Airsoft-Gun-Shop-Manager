using UnityEngine;

public class MarketExpansionSaveHandler : MonoBehaviour, ISaveable
{
    public object CaptureData()
    {
        var extender = FindFirstObjectByType<MarketExtender>();
        if (extender == null)
        {
            Debug.LogWarning("[MarketExpansionSaveHandler] MarketExtender 없음");
            return new MarketExpansionSaveData();
        }

        return new MarketExpansionSaveData
        {
            marketPurchased = extender.MarketPurchased,
            rangePurchased  = extender.RangePurchased
        };
    }

    public void RestoreData(object data)
    {
        if (data == null) return;
        var loaded = data as MarketExpansionSaveData;
        if (loaded == null) return;

        var extender = FindFirstObjectByType<MarketExtender>();
        if (extender == null)
        {
            Debug.LogWarning("[MarketExpansionSaveHandler] MarketExtender 없음");
            return;
        }

        extender.RestoreState(loaded.marketPurchased, loaded.rangePurchased);
        Debug.Log($"[Load] MarketExpansion ← market={loaded.marketPurchased}, range={loaded.rangePurchased}");
    }
}
