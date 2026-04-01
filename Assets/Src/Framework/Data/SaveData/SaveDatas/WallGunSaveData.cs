using System.Collections.Generic;

[System.Serializable]
public class WallGunSaveData
{
    public List<WallGunEntry> entries = new();
}

[System.Serializable]
public class WallGunEntry
{
    public string slotPath;      // WallGunSlot 계층경로
    public string gunPrefabName; // 총기 프리팹 이름 (Clone 제거)
}
