using System.Collections.Generic;

[System.Serializable]
public class MarketPriceSaveData
{
    public List<MarketPriceEntry> entries = new();
}

[System.Serializable]
public class MarketPriceEntry
{
    public int itemId;
    public float price;
}
