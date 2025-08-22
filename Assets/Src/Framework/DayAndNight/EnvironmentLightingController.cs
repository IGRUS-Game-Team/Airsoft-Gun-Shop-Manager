using UnityEngine;

public class EnvironmentLightingController : MonoBehaviour
{
    [Header("환경광 색상")]
    public Gradient ambientLightColor;
    
    [Header("조명 강도")]
    public AnimationCurve ambientIntensity;
    
    [Header("반사광 설정")]
    public AnimationCurve reflectionIntensity;
    
    [Header("그림자 설정")]
    public AnimationCurve shadowStrength;
    
    public void UpdateEnvironment(float normalizedTime)
    {
        UpdateAmbientLighting(normalizedTime);
        UpdateReflections(normalizedTime);
        UpdateShadows(normalizedTime);
    }
    
    void UpdateAmbientLighting(float normalizedTime)
    {
        // 환경광 색상
        if (ambientLightColor != null)
        {
            RenderSettings.ambientLight = ambientLightColor.Evaluate(normalizedTime);
        }
        
        // 환경광 강도 (최소값 보장)
        if (ambientIntensity != null)
        {
            float intensity = ambientIntensity.Evaluate(normalizedTime);
            // 낮 시간대에는 최소 0.8 보장
            if (normalizedTime > 0.25f && normalizedTime < 0.75f)
            {
                intensity = Mathf.Max(intensity, 0.8f);
            }
            RenderSettings.ambientIntensity = Mathf.Clamp01(intensity);
        }
}
    
    void UpdateReflections(float normalizedTime)
    {
        // 반사광 강도 조절
        if (reflectionIntensity != null)
        {
            float intensity = reflectionIntensity.Evaluate(normalizedTime);
            RenderSettings.reflectionIntensity = Mathf.Clamp01(intensity);
        }
    }
    
    void UpdateShadows(float normalizedTime)
    {
        // 그림자 강도 조절 (모든 라이트에 적용)
        if (shadowStrength != null)
        {
            float strength = shadowStrength.Evaluate(normalizedTime);
            // 개별 라이트의 shadow strength는 각 컨트롤러에서 처리
        }
    }
}