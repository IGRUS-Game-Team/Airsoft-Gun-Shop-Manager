using UnityEngine;
/// <summary>
/// 장지원 8.11
/// 모든 상품 프리팹에 붙여서 ItemData 정보를 이 스크립트를 통해 쉽게 가져온다
/// so는 inspector에서 할당하면 된다.
/// </summary>
public class ItemDataManager : MonoBehaviour
{
    [Header("상품 데이터")]
    [SerializeField] private ItemData itemData;
    
    public ItemData GetItemData() => itemData; //so 데이터 전부 제공
    
    
    public int ItemId => itemData != null ? itemData.itemId : -1;
    public string ItemName => itemData != null ? itemData.itemName : "Unknown";
    public float BaseCost => itemData != null ? itemData.baseCost : 0f;
    public int PerBoxCount => itemData != null ? itemData.perBoxCount : 0;
    //public ItemCategory Category => itemData != null ? itemData.category : ItemCategory.None;
    public Sprite Icon => itemData != null ? itemData.icon : null;
    //public DisplayType DisplayType => itemData != null ? itemData.displayType : DisplayType.None;
    
    
    public bool HasValidData => itemData != null;
    
}
