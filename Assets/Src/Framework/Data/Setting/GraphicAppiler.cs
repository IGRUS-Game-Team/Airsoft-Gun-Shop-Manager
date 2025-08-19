using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GraphicsApplier : MonoBehaviour
{
    [Header("Optional URP refs")]
    public Volume postProcessVolume; // 메인 메뉴에선 비어 있어도 OK
    private MotionBlur motionBlur;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryBindVolumeAndComponents();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        TryBindVolumeAndComponents();
        if (SettingsManager.Instance != null)
            ApplySafe(SettingsManager.Instance.Data);
    }

    private void TryBindVolumeAndComponents()
    {
        if (postProcessVolume == null)
            postProcessVolume = FindFirstObjectByType<Volume>();

        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet(out motionBlur);
        }
        else
        {
            motionBlur = null; // Volume 없으면 모션블러 스킵
        }
    }

    public void ApplySafe(SettingsData d)
    {
        QualitySettings.vSyncCount = d.vSync ? 1 : 0;
        Application.targetFrameRate = Mathf.Clamp(d.targetFps, 30, 1000);

        var cam = Camera.main ?? FindFirstObjectByType<Camera>();
        if (cam) cam.fieldOfView = Mathf.Clamp(d.fov, 40f, 110f);

        if (motionBlur != null)
            motionBlur.active = d.motionBlur;
    }

    // 해상도/품질/창모드 적용은 이전 코드 그대로 사용

        public enum HeavyOptions { Resolution, QualityLevel, FullscreenMode }

    // 메인메뉴 전용: 큰 리소스 변동(해상도/창모드/품질)

    public void ApplyHeavyOption(SettingsData d, HeavyOptions what)
    {
        switch (what)
        {
            case HeavyOptions.Resolution:
                var rr = new RefreshRate 
                { 
                    numerator = (uint)d.refreshRate, 
                    denominator = 1u 
                };
                Screen.SetResolution(d.width, d.height, d.fullscreenMode, rr);
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
