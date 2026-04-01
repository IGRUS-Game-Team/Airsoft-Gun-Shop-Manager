using System.Collections.Generic;
using UnityEngine;

public class MarketPriceSaveHandler : MonoBehaviour, ISaveable
{
    private MarketPriceDataManager priceManager;

    private void Awake()
    {
        priceManager = MarketPriceDataManager.Instance ?? FindFirstObjectByType<MarketPriceDataManager>();
    }

    private void EnsureRefs()
    {
        if (priceManager == null)
            priceManager = MarketPriceDataManager.Instance ?? FindFirstObjectByType<MarketPriceDataManager>(FindObjectsInactive.Include);
    }

    public object CaptureData()
    {
        EnsureRefs();
        if (priceManager == null) return null;

        var allPrices = priceManager.GetAllPrices();
        var saveData = new MarketPriceSaveData();

        foreach (var kvp in allPrices)
        {
            saveData.entries.Add(new MarketPriceEntry
            {
                itemId = kvp.Key,
                price  = kvp.Value
            });
        }
        return saveData;
    }

    public void RestoreData(object data)
    {
        EnsureRefs();
        var loaded = data as MarketPriceSaveData;
        if (loaded == null || priceManager == null) return;

        var dict = new Dictionary<int, float>();
        foreach (var e in loaded.entries)
            dict[e.itemId] = e.price;

        priceManager.SetAllPrices(dict);
    }
}
