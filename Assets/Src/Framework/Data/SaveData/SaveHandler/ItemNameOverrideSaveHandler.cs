// ItemNameOverrideSaveHandler.cs
using System.Collections.Generic;
using UnityEngine;

public class ItemNameOverrideSaveHandler : MonoBehaviour, ISaveable
{
    public object CaptureData()
    {
        var list = new List<ItemNameOverrideEntry>();
        foreach (var (itemId, name) in ItemOverrideStore.Instance.GetAllOverrides())
            list.Add(new ItemNameOverrideEntry { itemId = itemId, customName = name });
        return list;
    }

    public void RestoreData(object data)
    {
        var loaded = data as List<ItemNameOverrideEntry>;
        if (loaded == null) return;

        ItemOverrideStore.Instance.ClearAllOverrides();
        foreach (var e in loaded)
            ItemOverrideStore.Instance.SetCustomName(e.itemId, e.customName);
    }
}
