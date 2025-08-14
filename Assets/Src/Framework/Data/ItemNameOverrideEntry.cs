// ItemNameOverrideEntry.cs
[System.Serializable]
public class ItemNameOverrideEntry
{
    public int itemId;
    public string customName; // null/빈문자면 저장 안 해도 되지만 호환 위해 남김
}
