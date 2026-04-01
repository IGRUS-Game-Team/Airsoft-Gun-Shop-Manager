using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임의 모든 ItemData를 등록하고 빠르게 조회하는 중앙 데이터베이스 ScriptableObject.
///
/// [사용법]
/// 1. Create > Shop > Item Database 로 에셋 생성 (프로젝트에 1개만 존재해야 함)
/// 2. items 리스트에 모든 ItemData SO를 드래그하여 등록
/// 3. OnEnable/OnValidate 시 자동으로 _byId, _byName 인덱스 재구성
///
/// [조회 우선순위]
/// - GetById(id): O(1) 해시맵 조회 (가장 권장)
/// - GetByName(name): 대소문자 무시 O(1) 조회
/// - GetByComposite(id, cat, name): id 우선 → name 보조
///
/// [위치] Assets/Src/ScriptableObject/DataBase/ItemDatabase.asset
/// </summary>
[CreateAssetMenu(menuName = "Shop/Item Database", fileName = "ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> items = new List<ItemData>();

    // 빠른 조회용 인덱스 (런타임에만 존재, 직렬화 안 됨)
    [System.NonSerialized] private Dictionary<int, ItemData> _byId;
    [System.NonSerialized] private Dictionary<string, ItemData> _byName; // 소문자 키

    void OnEnable() => RebuildIndex();
#if UNITY_EDITOR
    void OnValidate() => RebuildIndex(); // 에디터에서 리스트 수정 시 자동 갱신
#endif

    public void RebuildIndex()
    {
        _byId = new Dictionary<int, ItemData>();
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
            if (NormalizeName(x.name) == key) return x; // SO 파일명으로도 폴백
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
    

    //랜덤 아이템 전달
    public ItemData GetRandomItemData()
    {
        if (_byId == null || _byId.Count == 0) 
        {
            Debug.LogWarning("[ItemDatabase] 유효한 아이템이 없습니다.");
            return null; 
        }
        
        // Dictionary의 실제 키들을 배열로 변환
        var keys = new List<int>(_byId.Keys);
        
        // 랜덤 인덱스로 키 선택
        int randomIndex = Random.Range(0, keys.Count);
        int randomId = keys[randomIndex];
        
        return _byId[randomId];

    }
    /// <summary>
    /// 특정 카테고리의 모든 아이템을 반환한다.
    /// </summary>
    public List<ItemData> GetItemsByCategory(ItemCategory category)
    {
        var result = new List<ItemData>();
        foreach (var it in items)
        {
            if (it != null && it.category == category)
                result.Add(it);
        }
        return result;
    }

    /// <summary>
    /// 특정 카테고리에서 랜덤으로 count개를 뽑아 반환한다 (중복 없음).
    /// </summary>
    public List<ItemData> GetRandomItems(ItemCategory category, int count)
    {
        var pool = GetItemsByCategory(category);
        var result = new List<ItemData>();
        if (pool.Count == 0) return result;

        // Fisher-Yates shuffle
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        int pick = Mathf.Min(count, pool.Count);
        for (int i = 0; i < pick; i++)
            result.Add(pool[i]);

        return result;
    }

    public bool TryGet(int itemId, out ItemData result)
    {
        result = null;
        foreach (var item in items)
        {
            if (item != null && item.itemId == itemId)
            {
                result = item;
                return true;
            }
        }
        return false;
}
}
