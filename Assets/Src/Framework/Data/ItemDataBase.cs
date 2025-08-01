using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Item Database", fileName = "ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> items = new List<ItemData>();
}