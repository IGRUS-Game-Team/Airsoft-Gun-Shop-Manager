using System.Collections.Generic;
using UnityEngine;

public class FurnitureSaveHandler : MonoBehaviour, ISaveable
{
    [SerializeField] private ItemDatabase database;

    public object CaptureData()
    {
        var saveData = new FurnitureSaveData();

        foreach (var fp in FindObjectsOfType<FurniturePlaceable>(true))
        {
            // 프리뷰 중인 것은 저장하지 않음
            if (fp.IsPreviewing) continue;

            // 씬에 처음부터 배치된 오브젝트는 저장하지 않음 (PrefabName 미설정)
            if (string.IsNullOrEmpty(fp.PrefabName)) continue;

            saveData.items.Add(new FurnitureEntry
            {
                prefabName  = fp.PrefabName,
                position    = fp.transform.position,
                rotation    = fp.transform.rotation,
                isDecoration= fp.IsDecoration,
                unitPrice   = fp.UnitPrice
            });
        }

        return saveData;
    }

    public void RestoreData(object data)
    {
        if (data == null) { Debug.LogWarning("[FurnitureSaveHandler] data is null"); return; }
        var loaded = data as FurnitureSaveData;
        if (loaded == null)
        {
            Debug.LogWarning($"[FurnitureSaveHandler] 타입 캐스트 실패: {data.GetType().Name}");
            return;
        }
        if (database == null)
        {
            Debug.LogWarning("[FurnitureSaveHandler] database 참조 없음");
            return;
        }

        // 플레이어가 배치한 가구/장식만 제거 (씬 기본 오브젝트 제외)
        foreach (var fp in FindObjectsOfType<FurniturePlaceable>(true))
        {
            if (!fp.IsPreviewing && !string.IsNullOrEmpty(fp.PrefabName))
                Destroy(fp.gameObject);
        }

        // 복원
        foreach (var entry in loaded.items)
        {
            // ItemDatabase에서 worldPrefab 이름으로 매칭
            GameObject prefab = FindWorldPrefab(entry.prefabName);
            if (prefab == null)
            {
                Debug.LogWarning($"[Load] Furniture prefab '{entry.prefabName}' 을 찾을 수 없음");
                continue;
            }

            var go = Instantiate(prefab, entry.position, entry.rotation);

            var fp = go.GetComponent<FurniturePlaceable>();
            if (fp != null)
            {
                fp.PrefabName = entry.prefabName;
                fp.SetIsDecoration(entry.isDecoration);
                fp.SetUnitPrice(entry.unitPrice);
            }
        }

        Debug.Log($"[Load] Furniture ← {loaded.items.Count}개");
    }

    private GameObject FindWorldPrefab(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName) || database == null) return null;

        foreach (var item in database.items)
        {
            if (item == null || item.worldPrefab == null) continue;
            if (item.worldPrefab.name == prefabName)
                return item.worldPrefab;
        }

        // displayPrefab 이름으로도 탐색
        foreach (var item in database.items)
        {
            if (item == null || item.displayPrefab == null) continue;
            if (item.displayPrefab.name == prefabName)
                return item.displayPrefab;
        }

        return null;
    }
}
