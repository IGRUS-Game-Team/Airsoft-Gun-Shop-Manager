
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 박정민 8/14
/// </summary>

[Serializable]
public class ItemOverride
{
    public string customName; // null/빈문자면 미사용
}

public class ItemOverrideStore : MonoBehaviour
{
    public static ItemOverrideStore Instance { get; private set; }

    // 핵심: 정수 itemId -> 오버라이드
    private readonly Dictionary<int, ItemOverride> map = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ===== API =====
    public string GetDisplayName(ItemData def)
    {
        if (def == null) return string.Empty;

        if (def.itemId != 0 &&
            map.TryGetValue(def.itemId, out var ov) &&
            !string.IsNullOrWhiteSpace(ov.customName))
        {
            return ov.customName;
        }
        return def.itemName; // 원본
    }

    public void SetCustomName(int itemId, string newName)
    {
        if (itemId == 0) return;

        if (!map.TryGetValue(itemId, out var ov))
        {
            ov = new ItemOverride();
            map[itemId] = ov;
        }
        // 빈 문자열은 오버라이드 해제
        ov.customName = string.IsNullOrWhiteSpace(newName) ? null : newName.Trim();
    }

    public void ClearName(int itemId)
    {
        if (map.TryGetValue(itemId, out var ov))
            ov.customName = null;
    }

    public void ClearAllOverrides() => map.Clear();

    // 세이브 핸들러용으로 현재 모든 오버라이드 나열
    public IEnumerable<(int itemId, string name)> GetAllOverrides()
    {
        foreach (var kv in map)
        {
            var name = kv.Value?.customName;
            if (!string.IsNullOrWhiteSpace(name))
                yield return (kv.Key, name);
        }
    }
}
