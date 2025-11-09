using System.Collections.Generic;
using UnityEngine;

public class BlockOutLiner : MonoBehaviour
{
    [SerializeField] Color highlight = new(0.3f, 1f, 0.4f, 1f);
    [Range(0f, 1f)] public float intensity = 0.35f;

    Renderer[] renderers;
    MaterialPropertyBlock mpb;

    void Awake()
    {
        var list = new List<Renderer>(GetComponentsInChildren<Renderer>(true));
        renderers = list.ToArray();
        mpb = new MaterialPropertyBlock();
    }

    public void SetSelected(bool state)
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;

            if (state)
            {
                var sm = r.sharedMaterial;
                if (sm == null) continue;

                mpb.Clear();
                if (sm.HasProperty("_BaseColor")) // URP/Lit
                {
                    var baseCol = sm.GetColor("_BaseColor");
                    mpb.SetColor("_BaseColor", Color.Lerp(baseCol, highlight, intensity));
                }
                else if (sm.HasProperty("_Color")) // Standard/Unlit 등
                {
                    var baseCol = sm.GetColor("_Color");
                    mpb.SetColor("_Color", Color.Lerp(baseCol, highlight, intensity));
                }
                r.SetPropertyBlock(mpb);
            }
            else
            {
                // 선택 해제 → 원래 색으로 복귀
                r.SetPropertyBlock(null);
            }
        }
    }
}