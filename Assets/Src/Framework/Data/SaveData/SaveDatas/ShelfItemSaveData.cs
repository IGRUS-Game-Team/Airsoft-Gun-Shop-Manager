using System.Collections.Generic;

[System.Serializable]
public class ShelfItemSaveData
{
    public List<ShelfSlotEntry> slots = new();
}

[System.Serializable]
public class ShelfSlotEntry
{
    public string slotPath;             // 계층 구조 경로 (이름 기반)
    public List<int> itemIds = new();   // 슬롯에 진열된 아이템 ID 목록
}
