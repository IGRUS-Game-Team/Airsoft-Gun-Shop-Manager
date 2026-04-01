using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FurnitureSaveData
{
    public List<FurnitureEntry> items = new();
}

[System.Serializable]
public class FurnitureEntry
{
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
    public bool isDecoration;
    public float unitPrice;
}
