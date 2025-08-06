using UnityEngine;

public class BoxItemContainer : MonoBehaviour
{
    public int itemId;
    public int amount;
    public ItemCategory category;

    public string displayName;
    public void Setup(int itemId, ItemCategory category, string itemName)
    {
        this.itemId = itemId;
        this.category = category;
        this.displayName = itemName;
    }
}