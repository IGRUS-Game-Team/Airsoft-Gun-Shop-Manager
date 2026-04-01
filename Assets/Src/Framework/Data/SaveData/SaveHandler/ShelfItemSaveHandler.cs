using System.Collections.Generic;
using UnityEngine;

public class ShelfItemSaveHandler : MonoBehaviour, ISaveable
{
    [SerializeField] private ItemDatabase database;

    public object CaptureData()
    {
        var saveData = new ShelfItemSaveData();

        foreach (var slot in FindObjectsOfType<ShelfSlot>(true))
        {
            if (slot.ItemCount == 0) continue;

            var entry = new ShelfSlotEntry
            {
                slotPath = GetHierarchyPath(slot.transform)
            };

            // 슬롯의 자식 오브젝트에서 ItemDataManager 로 아이템 ID 추출
            foreach (Transform child in slot.transform)
            {
                // 스냅 포인트 자식 안의 아이템도 탐색
                var idm = child.GetComponentInChildren<ItemDataManager>();
                if (idm != null && idm.HasValidData)
                {
                    entry.itemIds.Add(idm.ItemId);
                    continue;
                }

                // CounterSlotData 로도 확인
                var csd = child.GetComponentInChildren<CounterSlotData>();
                if (csd != null && csd.itemData != null)
                    entry.itemIds.Add(csd.itemData.itemId);
            }

            if (entry.itemIds.Count > 0)
                saveData.slots.Add(entry);
        }

        return saveData;
    }

    public void RestoreData(object data)
    {
        if (data == null) { Debug.LogWarning("[ShelfItemSaveHandler] data is null"); return; }
        var loaded = data as ShelfItemSaveData;
        if (loaded == null)
        {
            Debug.LogWarning($"[ShelfItemSaveHandler] 타입 캐스트 실패: {data.GetType().Name}");
            return;
        }
        if (database == null)
        {
            Debug.LogWarning("[ShelfItemSaveHandler] database 참조 없음");
            return;
        }

        // 기존 선반 위 아이템 제거
        foreach (var slot in FindObjectsOfType<ShelfSlot>(true))
        {
            while (slot.HasItem)
            {
                var go = slot.PopItem();
                if (go != null) Destroy(go);
            }
        }

        // 경로→ShelfSlot 맵핑
        var slotMap = new Dictionary<string, ShelfSlot>();
        foreach (var slot in FindObjectsOfType<ShelfSlot>(true))
            slotMap[GetHierarchyPath(slot.transform)] = slot;

        // 복원
        foreach (var entry in loaded.slots)
        {
            if (!slotMap.TryGetValue(entry.slotPath, out var slot)) continue;

            for (int i = 0; i < entry.itemIds.Count && !slot.IsFull; i++)
            {
                var itemData = database.GetById(entry.itemIds[i]);
                if (itemData == null || itemData.displayPrefab == null) continue;

                int idx = slot.ItemCount;
                Transform snap = slot.GetSnapPoint(idx);

                var go = Instantiate(itemData.displayPrefab, snap.position, snap.rotation, slot.transform);
                go.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);

                SetLayerRecursively(go, LayerMask.NameToLayer("Box"));

                // IPickable 제거 (SmallBoxInteraction 제외)
                foreach (var existing in go.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (existing is IPickable && !(existing is SmallBoxInteraction))
                        DestroyImmediate(existing);
                }

                // CounterSlotData 연결
                var csd = go.GetComponent<CounterSlotData>();
                if (csd == null) csd = go.AddComponent<CounterSlotData>();
                csd.itemObject = go;
                csd.itemData = itemData;
                csd.amount = 1;

                // BlockIsHolding
                if (go.GetComponent<BlockIsHolding>() == null)
                    go.AddComponent<BlockIsHolding>();

                // 물리 끄기
                var rb = go.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                    rb.detectCollisions = true;
                }

                // SmallBoxInteraction
                if (go.GetComponent<SmallBoxInteraction>() == null)
                    go.AddComponent<SmallBoxInteraction>();

                // 바운딩 박스 콜라이더
                AddBoundsCollider(go);

                // 하이라이트용 BlockOutLiner
                if (go.GetComponent<BlockOutLiner>() == null)
                    go.AddComponent<BlockOutLiner>();

                slot.ReRegisterItem(go);

                // 슬롯의 첫 번째 아이템일 때 가격표 재생성
                if (idx == 0)
                    slot.FirePriceCardEvent(itemData);
            }
        }
        Debug.Log($"[Load] ShelfItems ← {loaded.slots.Count} slots");
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

    private static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private static void AddBoundsCollider(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds worldBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            worldBounds.Encapsulate(renderers[i].bounds);

        var box = go.AddComponent<BoxCollider>();
        box.center = go.transform.InverseTransformPoint(worldBounds.center);
        Vector3 ls = go.transform.lossyScale;
        box.size = new Vector3(
            worldBounds.size.x / Mathf.Abs(ls.x),
            worldBounds.size.y / Mathf.Abs(ls.y),
            worldBounds.size.z / Mathf.Abs(ls.z)
        );
    }
}
