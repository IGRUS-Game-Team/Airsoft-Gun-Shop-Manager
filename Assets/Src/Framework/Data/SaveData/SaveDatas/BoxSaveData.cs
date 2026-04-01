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
    public bool isDeliveryBox;  // true = BoxContainer (배달 박스), false = BoxItemContainer (기존)
}
