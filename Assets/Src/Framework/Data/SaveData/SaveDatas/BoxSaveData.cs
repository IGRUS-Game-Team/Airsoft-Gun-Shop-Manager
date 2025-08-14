using UnityEngine;

[System.Serializable]
public class BoxSaveData
{
    public string prefabName; //식별용
    public Vector3 position;
    public Quaternion rotation;
   // public string prefabName; // 나중에 어떤 프리팹인지 구분할 때 사용
    public int itemId;
    public string itemName;
   // public ItemCategory category;
    //public int perBoxCount;        // 박스당 수량
    public int amount; // 얼마나 남았는지
    //public bool isHeld; // 플레이어가 들고 있는지
    public bool isOpen;
    // 필요하면 isHeld, 상태, 카테고리 등 추가
}
