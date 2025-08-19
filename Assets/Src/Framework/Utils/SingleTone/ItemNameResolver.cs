// ItemNameResolver.cs
using UnityEngine;

/// <summary>
/// 8/14 박정민
/// 이름 변경 기능 만듬에 따라 리펙토링이 막막하다 느껴 추가함
/// 기존 코드의 item.itemName 부분을 ItemNameResolver.Get(item) 으로 한 줄 교체
/// </summary>
public static class ItemNameResolver
{
    // ItemData 기반
    public static string Get(ItemData def)
    {
        if (def == null) return string.Empty;
        var store = ItemOverrideStore.Instance;
        return (store != null) ? store.GetDisplayName(def) : def.itemName;
    }

    // itemId + fallback 기반 (세이브/네트워크에서 원본명만 있을 때)
    public static string Get(int itemId, string fallback)
    {
        var store = ItemOverrideStore.Instance;
        if (store == null) return fallback ?? string.Empty;

        // ItemData를 DB에서 찾아서 넘겨줌(있으면 오버라이드 반영)
        var db = Object.FindFirstObjectByType<ItemDatabase>();
        if (db != null && db.TryGet(itemId, out var def))
            return store.GetDisplayName(def);

        return fallback ?? string.Empty;
    }
}
