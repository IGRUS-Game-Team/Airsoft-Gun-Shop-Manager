using System.Collections.Generic;
using UnityEngine;

public class BlockOutLiner : MonoBehaviour
{
    [SerializeField] Color highlight = new(0.3f, 1f, 0.4f, 1f); // 연초록
    [Range(0f, 1f)] public float intensity = 0.35f;            // 섞는 정도(0=원래색, 1=완전 하이라이트)
    
    Renderer[] renderers;
    MaterialPropertyBlock mpb;

    void Start()
    {
        // 보조 오브젝트는 제외하고 싶으면 여기서 필터링 가능
        var list = new List<Renderer>(GetComponentsInChildren<Renderer>(true));
        renderers = list.ToArray();
        mpb = new MaterialPropertyBlock();
    }

    public void SetSelected(bool state)
    {
        foreach (var r in renderers)
        {
            if (state)
            {
                var sm = r.sharedMaterial;
                if (sm == null) continue;

                // 머티리얼을 복제하지 않고, 원래 색을 살짝 섞어서 올린다.
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

                // 🔴 여기는 키워드 호출 금지 (Renderer에는 없음, MPB로는 키워드 못 켬)
            }
            else
            {
                // 선택 해제 → 덮어쓴 값 제거(원본 색으로 복귀)
                r.SetPropertyBlock(null);
            }
        }
    }
}