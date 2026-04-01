using System.Collections.Generic;
using UnityEngine;

public class WallGunSaveHandler : MonoBehaviour, ISaveable
{
    [SerializeField] private ItemDatabase database;

    public object CaptureData()
    {
        var data = new WallGunSaveData();

        foreach (var slot in FindObjectsOfType<WallGunSlot>(true))
        {
            var shelfSlot = slot.GetComponent<ShelfSlot>();
            if (shelfSlot == null || !shelfSlot.HasItem) continue;

            // 슬롯의 자식(snap point 이하)에서 GunInteraction 찾기
            var gun = slot.GetComponentInChildren<GunInteraction>(true);
            if (gun == null) continue;

            string prefabName = gun.gameObject.name.Replace("(Clone)", "").Trim();

            data.entries.Add(new WallGunEntry
            {
                slotPath = GetHierarchyPath(slot.transform),
                gunPrefabName = prefabName
            });
        }

        return data;
    }

    public void RestoreData(object data)
    {
        if (data == null) return;
        var loaded = data as WallGunSaveData;
        if (loaded == null) return;
        if (database == null)
        {
            Debug.LogWarning("[WallGunSaveHandler] database 참조 없음");
            return;
        }

        // 슬롯 맵핑
        var slotMap = new Dictionary<string, WallGunSlot>();
        foreach (var slot in FindObjectsOfType<WallGunSlot>(true))
            slotMap[GetHierarchyPath(slot.transform)] = slot;

        // 총기 프리팹 이름 → GameObject 프리팹 매핑 빌드
        var prefabLookup = BuildGunPrefabLookup();

        foreach (var entry in loaded.entries)
        {
            if (!slotMap.TryGetValue(entry.slotPath, out var slot)) continue;

            if (!prefabLookup.TryGetValue(entry.gunPrefabName, out var gunPrefab))
            {
                Debug.LogWarning($"[WallGunSaveHandler] 총기 프리팹 못 찾음: {entry.gunPrefabName}");
                continue;
            }

            // 총기 인스턴스 생성
            var gunObj = Instantiate(gunPrefab);
            gunObj.name = entry.gunPrefabName;

            // WallGunSlot.HangGunDirect 로 슬롯에 배치
            if (!slot.HangGunDirect(gunObj))
            {
                Debug.LogWarning($"[WallGunSaveHandler] HangGunDirect 실패: {entry.gunPrefabName} → {entry.slotPath}");
                Destroy(gunObj);
            }
        }

        Debug.Log($"[Load] WallGuns ← {loaded.entries.Count} entries");
    }

    /// <summary>
    /// ItemDatabase의 모든 아이템에서 displayPrefab → SmallBoxInteraction → gunPrefab 을 추출하여
    /// prefabName → gunPrefab 딕셔너리를 만든다.
    /// </summary>
    private Dictionary<string, GameObject> BuildGunPrefabLookup()
    {
        var lookup = new Dictionary<string, GameObject>();

        foreach (var item in database.items)
        {
            if (item == null || item.displayPrefab == null) continue;

            var smallBox = item.displayPrefab.GetComponent<SmallBoxInteraction>();
            if (smallBox == null) continue;

            // SmallBoxInteraction의 gunPrefab 필드 (private serialized)
            var gunPrefab = GetGunPrefabFromSmallBox(smallBox);
            if (gunPrefab == null) continue;

            string key = gunPrefab.name;
            if (!lookup.ContainsKey(key))
                lookup[key] = gunPrefab;
        }

        return lookup;
    }

    /// <summary>리플렉션으로 SmallBoxInteraction.gunPrefab (private) 접근</summary>
    private static GameObject GetGunPrefabFromSmallBox(SmallBoxInteraction smallBox)
    {
        var field = typeof(SmallBoxInteraction).GetField("gunPrefab",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(smallBox) as GameObject;
    }

    private static string GetHierarchyPath(Transform t)
    {
        var parts = new List<string>();
        while (t != null)
        {
            parts.Add(t.name);
            t = t.parent;
        }
        parts.Reverse();
        return string.Join("/", parts);
    }
}
