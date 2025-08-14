using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GraphicsApplier : MonoBehaviour
{
    [Header("Optional URP refs")]
    public Volume postProcessVolume;           // 모션블러 등
    private MotionBlur motionBlur;             // URP Volume Component (있으면 사용)

    void Start()
    {
        if (postProcessVolume && postProcessVolume.profile && 
            postProcessVolume.profile.TryGet(out motionBlur))
        {
            // ok
        }
    }

    // 인게임 즉시 적용 가능한 옵션
    public void ApplySafe(SettingsData d)
    {
        QualitySettings.vSyncCount = d.vSync ? 1 : 0;
        Application.targetFrameRate = Mathf.Clamp(d.targetFps, 30, 1000);

        // FOV 적용 (메인 카메라 기준)
        var cam = Camera.main;
        if (cam) cam.fieldOfView = Mathf.Clamp(d.fov, 40f, 110f);

        // 모션블러 토글 (있을 때만)
        if (motionBlur != null)
            motionBlur.active = d.motionBlur;
    }

    public enum HeavyOptions { Resolution, QualityLevel, FullscreenMode }

    // 메인메뉴 전용: 큰 리소스 변동
    public void ApplyHeavyOption(SettingsData d, HeavyOptions what)
    {
        switch (what)
        {
            case HeavyOptions.Resolution:
                Screen.SetResolution(d.width, d.height, d.fullscreenMode, d.refreshRate);
                break;
            case HeavyOptions.QualityLevel:
                d.qualityLevel = Mathf.Clamp(d.qualityLevel, 0, QualitySettings.names.Length - 1);
                QualitySettings.SetQualityLevel(d.qualityLevel, true);
                break;
            case HeavyOptions.FullscreenMode:
                Screen.fullScreenMode = d.fullscreenMode;
                break;
        }
    }
}
