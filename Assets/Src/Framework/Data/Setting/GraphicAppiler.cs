using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Cinemachine;

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
            postProcessVolume.profile.TryGet(out motionBlur);
        else
            motionBlur = null;
    }

    public void ApplySafe(SettingsData d)
    {
        QualitySettings.vSyncCount = d.vSync ? 1 : 0;
        Application.targetFrameRate = Mathf.Clamp(d.targetFps, 30, 1000);

        float clampedFov = Mathf.Clamp(d.fov, 40f, 110f);

        Scene myScene = gameObject.scene;

        var cam = Camera.main ?? FindFirstObjectByType<Camera>();
        if (cam && cam.gameObject.scene == myScene)
            cam.fieldOfView = clampedFov;

        // Cinemachine 가상카메라 중 같은 씬에 있는 것만 FOV 적용 (Cinemachine이 매 프레임 Camera를 덮어쓰므로 필수)
        var allVcams = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);
        foreach (var vc in allVcams)
            if (vc.gameObject.scene == myScene)
                vc.m_Lens.FieldOfView = clampedFov;

        if (motionBlur != null)
            motionBlur.active = d.motionBlur;
    }

    public enum HeavyOptions { Resolution, QualityLevel, FullscreenMode }

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
