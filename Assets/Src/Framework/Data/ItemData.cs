using UnityEngine;

[CreateAssetMenu(menuName="Shop/Item")]
public class ItemData : ScriptableObject
{
    public string ItemId;
    public string displayName;
    public ItemCategory category;
    public float baseCost;
    public float rarity;
    public int regulationLevel;
    public int BaseDemand;
    public Sprite icon;
    public DisplayType displayType;
}