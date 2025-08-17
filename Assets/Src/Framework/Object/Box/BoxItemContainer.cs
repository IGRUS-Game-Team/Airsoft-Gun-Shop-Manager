using UnityEngine;

public class BoxItemContainer : MonoBehaviour
{
    [Header("Runtime data (source of truth = ItemData)")]
    [SerializeField] private ItemData item;   // SO 참조
    [SerializeField] private int amount;

    // ----- 읽기 전용 프로퍼티 -----
    public ItemData Item => item;
    public int Amount => amount;

    public int ItemId => item != null ? item.itemId : 0;
    public ItemCategory Category => item != null ? item.category : default;

    /// 표시 이름은 항상 오버라이드 스토어를 통해 가져오기
    public string DisplayName
    {
        get
        {
            if (item == null) return string.Empty;
            var store = ItemOverrideStore.Instance;
            return (store != null) ? store.GetDisplayName(item) : item.itemName;
        }
    }

    // ----- 세팅 API -----
    public void Setup(ItemData data, int amount)
    {
        this.item = data;
        this.amount = Mathf.Max(0, amount);
    }

    public void SetupById(int itemId, ItemDatabase db, int amount)
    {
        var data = db != null ? db.GetById(itemId) : null;
        if (data == null)
        {
            Debug.LogError($"[BoxItemContainer] Item id {itemId}를 DB에서 찾을 수 없음");
            this.item = null;
            this.amount = 0;
            return;
        }
        Setup(data, amount);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (amount < 0) amount = 0;
    }
#endif
}
