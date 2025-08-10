using System.Collections.Generic;
using UnityEngine;

public class BlockOutLiner : MonoBehaviour
{
    [SerializeField] private bool selected = false;
    [SerializeField] Material sharedOutlineMaterial;
    Renderer[] renderers;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    public void SetSelected(bool state)
    {
        if (selected == state) return;
        selected = state;
        foreach (var r in renderers)
        {
            var mats = new List<Material>(r.sharedMaterials);
            if (selected && !mats.Contains(sharedOutlineMaterial))
                mats.Add(sharedOutlineMaterial);
            else if (!selected)
                mats.Remove(sharedOutlineMaterial);

            r.materials = mats.ToArray();
        }
    }
}
