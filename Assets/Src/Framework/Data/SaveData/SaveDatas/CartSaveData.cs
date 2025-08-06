[System.Serializable]
public class CartSaveData
{
    public string itemId;
    public int amount;
    public float unitPrice; // 추가
    public string itemName; // 박스 prefab 이름 연결에 활용
    public ItemCategory category;
}