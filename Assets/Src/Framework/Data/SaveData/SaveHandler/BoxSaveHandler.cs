using System.Collections.Generic;
using UnityEngine;

public class BoxSaveHandler : MonoBehaviour, ISaveable
{
    [SerializeField] private BoxSpawner spawner;
    [SerializeField] private ItemDatabase database; // 복원 때 필요

    public object CaptureData()
    {
        var boxes = new List<BoxSaveData>();

        foreach (var box in FindObjectsOfType<BlockIsHolding>())
        {
            var container = box.GetComponent<BoxItemContainer>();
            if (container == null || container.ItemId == 0) continue;

            boxes.Add(new BoxSaveData
            {
                position = box.transform.position,
                rotation = box.transform.rotation,
                itemId   = container.ItemId,
                amount   = container.Amount
                // 이름/카테고리 저장 X (표시는 런타임에서 DB+Override로)
            });
        }
        return boxes;
    }

    public void RestoreData(object data)
    {
        var boxes = data as List<BoxSaveData>;
        if (boxes == null || spawner == null) return;

        foreach (var b in boxes)
        {
            // 스포너가 실제 프리팹을 만들고 BoxItemContainer에 세팅하도록
            var go = spawner.RestoreBoxTransform(b.position, b.rotation); // ← 스포너에 이런 메서드가 없다면 하나 만들어줘
            if (!go) continue;

            var container = go.GetComponent<BoxItemContainer>();
            if (container == null)
                container = go.AddComponent<BoxItemContainer>();

            container.SetupById(b.itemId, database, b.amount);
        }
    }
}
