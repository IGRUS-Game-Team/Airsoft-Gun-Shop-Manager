using System.Collections.Generic;
using UnityEngine;

public class BoxSaveHandler : MonoBehaviour, ISaveable
{
    [SerializeField] private BoxSpawner spawner;

    public object CaptureData()
    {
        List<BoxSaveData> boxes = new();

        foreach (BlockIsHolding box in FindObjectsOfType<BlockIsHolding>())
        {
            var container = box.GetComponent<BoxItemContainer>();
            if (container == null) continue;

            boxes.Add(new BoxSaveData
            {
                position = box.transform.position,
                rotation = box.transform.rotation,
                itemId = container.itemId,
                itemName = container.displayName,
                category = container.category
            });
        }

        return boxes;
    }

    public void RestoreData(object data)
    {
        List<BoxSaveData> boxes = data as List<BoxSaveData>;
        if (boxes == null || spawner == null) return;

        foreach (BoxSaveData b in boxes)
        {
            spawner.RestoreBox(b);
        }
    }
}