using UnityEngine;

public class SkyboxController : MonoBehaviour
{
    [Header("스카이박스 재질")]
    public Material daySkybox;
    public Material nightSkybox;
    
    [Header("스카이박스 색조")]
    public Gradient skyTintColor;
    
    [Header("스카이박스 노출도 (밝기)")]
    public AnimationCurve skyExposure;
    
    [Header("전환 설정")]
    [Range(0f, 1f)]
    public float dayNightTransitionPoint = 0.25f; // 0.25 = 6시, 0.75 = 18시
    
    public void UpdateSkybox(float normalizedTime)
    {
        UpdateSkyboxMaterial(normalizedTime);
        UpdateSkyboxProperties(normalizedTime);
        
        // 스카이박스 업데이트 강제 적용
        DynamicGI.UpdateEnvironment();
    }
    
    void UpdateSkyboxMaterial(float normalizedTime)
    {
        // 낮/밤 스카이박스 전환
        if (daySkybox != null && nightSkybox != null)
        {
            bool isDayTime = normalizedTime > dayNightTransitionPoint && 
                           normalizedTime < (1f - dayNightTransitionPoint);
            RenderSettings.skybox = isDayTime ? daySkybox : nightSkybox;
        }
    }
    
    void UpdateSkyboxProperties(float normalizedTime)
    {
        if (RenderSettings.skybox == null) return;
        
        // 스카이박스 색조 조정
        if (skyTintColor != null && RenderSettings.skybox.HasProperty("_Tint"))
        {
            Color tint = skyTintColor.Evaluate(normalizedTime);
            RenderSettings.skybox.SetColor("_Tint", tint);
        }
        
        // 스카이박스 노출도 조정 (밝기)
        if (skyExposure != null)
        {
            float exposure = skyExposure.Evaluate(normalizedTime);
            
            // 다양한 스카이박스 shader 속성명 지원
            if (RenderSettings.skybox.HasProperty("_Exposure"))
                RenderSettings.skybox.SetFloat("_Exposure", exposure);
            else if (RenderSettings.skybox.HasProperty("_SunSize"))
                RenderSettings.skybox.SetFloat("_SunSize", exposure);
        }
    }
}