using UnityEngine;

public class FogController : MonoBehaviour
{
    [Header("안개 활성화")]
    public bool enableFog = true;
    
    [Header("안개 색상")]
    public Gradient fogColor;
    
    [Header("안개 밀도")]
    public AnimationCurve fogDensity;
    
    [Header("안개 거리 설정")]
    public AnimationCurve fogStartDistance;
    public AnimationCurve fogEndDistance;
    
    public void UpdateFog(float normalizedTime)
    {
        RenderSettings.fog = enableFog;
        
        if (!enableFog) return;
        
        UpdateFogColor(normalizedTime);
        UpdateFogDensity(normalizedTime);
        UpdateFogDistance(normalizedTime);
    }
    
    void UpdateFogColor(float normalizedTime)
    {
        // 안개 색상 변경
        if (fogColor != null)
        {
            RenderSettings.fogColor = fogColor.Evaluate(normalizedTime);
        }
    }
    
    void UpdateFogDensity(float normalizedTime)
    {
        // 안개 밀도 조정
        RenderSettings.fogDensity = 0.0005f;
    }
    
    void UpdateFogDistance(float normalizedTime)
    {
        // 안개 시작 거리
        if (fogStartDistance != null)
        {
            float startDist = fogStartDistance.Evaluate(normalizedTime);
            RenderSettings.fogStartDistance = Mathf.Max(0f, startDist);
        }
        
        // 안개 끝 거리
        if (fogEndDistance != null)
        {
            float endDist = fogEndDistance.Evaluate(normalizedTime);
            RenderSettings.fogEndDistance = Mathf.Max(RenderSettings.fogStartDistance + 1f, endDist);
        }
    }
}