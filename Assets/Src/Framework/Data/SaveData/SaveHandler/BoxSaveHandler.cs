using System.Collections.Generic;
using UnityEngine;

public class BoxSaveHandler : MonoBehaviour, ISaveable
{
    public object CaptureData()
    {
        List<BoxSaveData> boxes = new();
        foreach (BlockIsHolding box in FindObjectsOfType<BlockIsHolding>())
        {
            boxes.Add(new BoxSaveData
            {
                position = box.transform.position,
                rotation = box.transform.rotation,
                prefabName = box.name.Replace("(Clone)", "")
            });
        }
        return boxes;
    }

    public void RestoreData(object data)
    {
        List<BoxSaveData> boxes = data as List<BoxSaveData>;
        if (boxes == null) return;

        foreach (BoxSaveData b in boxes)
        {
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/{b.prefabName}");
            Instantiate(prefab, b.position, b.rotation);
        }
    }
    
}
