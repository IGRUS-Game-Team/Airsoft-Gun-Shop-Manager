using UnityEngine;

public class SunController : MonoBehaviour
{
    [Header("태양 설정")]
    public Light sunLight;
    
    [Header("태양 색상")]
    public Gradient sunColor;
    
    [Header("태양 강도")]
    public AnimationCurve sunIntensity;
    
    [Header("태양 회전 설정")]
    [Range(-180f, 180f)]
    public float sunRotationOffset = 30f; // Y축 회전 오프셋
    
    public void UpdateSun(float normalizedTime)
    {
        if (sunLight == null) return;
        
        // 태양 색상 업데이트
        if (sunColor != null)
        {
            sunLight.color = sunColor.Evaluate(normalizedTime);
        }
        
        // 태양 강도 업데이트
        if (sunIntensity != null)
        {
            float intensity = sunIntensity.Evaluate(normalizedTime);
            sunLight.intensity = Mathf.Max(0f, intensity);
        }
        
        // 태양 회전 (동쪽에서 서쪽으로)
        float sunAngle = (normalizedTime * 360f) - 90f;
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, sunRotationOffset, 0f);
    }
}