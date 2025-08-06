using UnityEngine;

[System.Serializable]
public class BoxSaveData
{
    public Vector3 position;
    public Quaternion rotation;
    public string prefabName; // 나중에 어떤 프리팹인지 구분할 때 사용

    public int itemId;
    public string itemName;
    public ItemCategory category;

    // 필요하면 isHeld, 상태, 카테고리 등 추가
}
