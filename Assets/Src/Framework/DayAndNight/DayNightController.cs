using UnityEngine;

public class DayNightController : MonoBehaviour
{
    [Header("연동할 TimeUI")]
    public TimeUI timeUI;
    
    [Header("하위 시스템들")]
    public SunController sunController;
    public MoonController moonController;
    public SkyboxController skyboxController;
    public EnvironmentLightingController environmentController;
    public FogController fogController;
    
    [Header("디버그")]
    public bool showDebugInfo = true;
    
    private void Update()
    {
        if (timeUI == null) return;
        
        float normalizedTime = GetNormalizedTimeFromTimeUI();
        UpdateAllSystems(normalizedTime);
        
        if (showDebugInfo)
        {
            DebugTimeInfo(normalizedTime);
        }
    }
    
    void UpdateAllSystems(float normalizedTime)
    {
        // 안전한 시간 값 보장
        normalizedTime = Mathf.Clamp01(normalizedTime);
        if (float.IsNaN(normalizedTime) || float.IsInfinity(normalizedTime))
        {
            normalizedTime = 0.5f; // 기본값: 정오
        }
        
        // 각 시스템 업데이트
        if (sunController != null) sunController.UpdateSun(normalizedTime);
        if (moonController != null) moonController.UpdateMoon(normalizedTime);
        if (skyboxController != null) skyboxController.UpdateSkybox(normalizedTime);
        if (environmentController != null) environmentController.UpdateEnvironment(normalizedTime);
        if (fogController != null) fogController.UpdateFog(normalizedTime);
    }
    
    /// <summary>
    /// TimeUI로부터 0-1 범위의 정규화된 시간을 계산
    /// 0 = 자정, 0.5 = 정오, 1 = 다시 자정
    /// </summary>
    float GetNormalizedTimeFromTimeUI()
    {
        if (timeUI == null) return 0.5f;
        
        int totalMinutes = timeUI.totalGameMinutes;
        int hours = (totalMinutes / 60) % 24;
        int minutes = totalMinutes % 60;
        
        float timeInHours = hours + (minutes / 60f);
        return timeInHours / 24f;
    }
    
    void DebugTimeInfo(float normalizedTime)
    {
        string currentTime = timeUI.GetFormattedTime();
        Debug.Log($"현재 시간: {currentTime}, 정규화된 시간: {normalizedTime:F3}");
    }
    
    // 유틸리티 메서드들
    public bool IsDayTime()
    {
        float normalizedTime = GetNormalizedTimeFromTimeUI();
        return normalizedTime > 0.25f && normalizedTime < 0.75f;
    }
    
    public bool IsNightTime() => !IsDayTime();
    
    public bool IsSunrise()
    {
        float normalizedTime = GetNormalizedTimeFromTimeUI();
        return normalizedTime > 0.2f && normalizedTime < 0.3f;
    }
    
    public bool IsSunset()
    {
        float normalizedTime = GetNormalizedTimeFromTimeUI();
        return normalizedTime > 0.7f && normalizedTime < 0.8f;
    }
    
    public float GetNormalizedTime() => GetNormalizedTimeFromTimeUI();
}