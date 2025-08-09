using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Item")]
public class ItemData : ScriptableObject
{
    public int itemId; //아이템 고유 번호
    public string itemName; //표시 이름
    public ItemCategory category; //카테고리 (무기, 방어구, 소모품 등등)
    public float baseCost; //원가(재고관리모니터에서 뜨는 가격)
                           // TODO : 개별 가격도 뜨게 해야함
    public int perBoxCount; // 한 박스에 얼마나 있는지
    public float rarity; //희귀도
    public int regulationLevel; //규제 정도
    public int BaseDemand; // 수요 정도
    public Sprite icon; // 아이템 프사
    public DisplayType displayType; // 선반(유리창 안, 벽걸이 선반, 일반 선반들 등등 중 어디에 놓이는지)
    public GameObject displayPrefab; //제품 박스 프리팹
}