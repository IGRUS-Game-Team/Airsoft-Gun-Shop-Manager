using UnityEngine;

public class BoxItemContainer : MonoBehaviour
{
    public string itemId;
    public int amount;
    public ItemCategory category;

    public string displayName;
    public void Setup(string itemId, ItemCategory category, string itemName)
    {
        this.itemId = itemId;
        this.category = category;
        this.displayName = itemName;
    }
}