using UnityEngine;

public class MoonController : MonoBehaviour
{
    [Header("달 설정")]
    public Light moonLight;
    
    [Header("달 색상")]
    public Gradient moonColor;
    
    [Header("달 강도")]
    public AnimationCurve moonIntensity;
    
    [Header("달 회전 설정")]
    [Range(-180f, 180f)]
    public float moonRotationOffset = 30f; // Y축 회전 오프셋
    
    public void UpdateMoon(float normalizedTime)
    {
        if (moonLight == null) return;
        
        // 달 색상 업데이트
        if (moonColor != null)
        {
            moonLight.color = moonColor.Evaluate(normalizedTime);
        }
        
        // 달 강도 업데이트
        if (moonIntensity != null)
        {
            float intensity = moonIntensity.Evaluate(normalizedTime);
            moonLight.intensity = Mathf.Max(0f, intensity);
        }
        
        // 달 회전 (태양과 반대 방향)
        float moonAngle = (normalizedTime * 360f) + 90f;
        moonLight.transform.rotation = Quaternion.Euler(moonAngle, moonRotationOffset, 0f);
    }
}