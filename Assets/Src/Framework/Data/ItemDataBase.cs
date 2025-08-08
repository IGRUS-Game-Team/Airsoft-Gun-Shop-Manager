using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Item Database", fileName = "ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> items = new List<ItemData>();

    // 빠른 조회용 인덱스
    [System.NonSerialized] private Dictionary<int, ItemData> _byId;
    [System.NonSerialized] private Dictionary<string, ItemData> _byName; // 소문자 키

    void OnEnable()  => RebuildIndex();
#if UNITY_EDITOR
    void OnValidate() => RebuildIndex(); // 에디터에서 리스트 수정 시 자동 갱신
#endif

    public void RebuildIndex()
    {
        _byId   = new Dictionary<int, ItemData>();
        _byName = new Dictionary<string, ItemData>();

        foreach (var it in items)
        {
            if (!it) continue;

            // ID 인덱스
            if (it.itemId != 0)
            {
                if (_byId.ContainsKey(it.itemId))
                    Debug.LogWarning($"[ItemDatabase] 중복 itemId 발견: {it.itemId} ({_byId[it.itemId].name} / {it.name})");
                else
                    _byId.Add(it.itemId, it);
            }

            // 이름 인덱스: ItemData.itemName 우선, 없으면 SO의 name 사용
            var key = NormalizeName(!string.IsNullOrEmpty(it.itemName) ? it.itemName : it.name);
            if (!string.IsNullOrEmpty(key))
            {
                if (_byName.ContainsKey(key))
                    Debug.LogWarning($"[ItemDatabase] 중복 itemName 발견: {key} ({_byName[key].name} / {it.name})");
                else
                    _byName.Add(key, it);
            }
        }
    }

    static string NormalizeName(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim().ToLowerInvariant();

    // ===== 조회 API =====
    public ItemData GetById(int id)
    {
        if (_byId != null && _byId.TryGetValue(id, out var it)) return it;
        // 인덱스가 없으면 느리지만 폴백
        foreach (var x in items) if (x && x.itemId == id) return x;
        return null;
    }

    public ItemData GetByName(string name)
    {
        var key = NormalizeName(name);
        if (string.IsNullOrEmpty(key)) return null;

        if (_byName != null && _byName.TryGetValue(key, out var it)) return it;
        foreach (var x in items)
        {
            if (!x) continue;
            if (NormalizeName(x.itemName) == key) return x;
            if (NormalizeName(x.name)     == key) return x; // SO 파일명으로도 폴백
        }
        return null;
    }

    // 편의: id 우선 → name 보조 → 카테고리 일치 확인(선택)
    public ItemData GetByComposite(int id, ItemCategory cat, string name)
    {
        var it = GetById(id);
        if (it != null) return it;

        it = GetByName(name);
        if (it != null && (cat == 0 || it.category == cat)) return it;

        return it; // cat이 다르더라도 일단 반환(원하면 여기서 null로)
    }
}
