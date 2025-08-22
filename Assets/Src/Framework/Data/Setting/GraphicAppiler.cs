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
        Debug.Log($"[GraphicsApplier] SceneLoaded: {s.name}");
        TryBindVolumeAndComponents();

        if (SettingsManager.Instance != null)
        {
            Debug.Log("[GraphicsApplier] SettingsManager Instance OK, ApplySafe 호출");
            ApplySafe(SettingsManager.Instance.Data);
        }
        else
        {
            Debug.LogWarning("[GraphicsApplier] SettingsManager.Instance 가 null임");
        }
    }

    private void TryBindVolumeAndComponents()
    {
        if (postProcessVolume == null)
        {
            postProcessVolume = FindFirstObjectByType<Volume>();
            Debug.Log($"[GraphicsApplier] postProcessVolume FindFirstObjectByType → {(postProcessVolume ? postProcessVolume.name : "NULL")}");
        }

        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            bool found = postProcessVolume.profile.TryGet(out motionBlur);
            Debug.Log($"[GraphicsApplier] MotionBlur TryGet → found={found}, motionBlur={(motionBlur != null)}");
        }
        else
        {
            Debug.LogWarning("[GraphicsApplier] Volume or Profile is NULL → MotionBlur 스킵");
            motionBlur = null; // Volume 없으면 모션블러 스킵
        }
    }

    public void ApplySafe(SettingsData d)
    {
        Debug.Log($"[GraphicsApplier] ApplySafe 호출: vSync={d.vSync}, fps={d.targetFps}, fov={d.fov}, motionBlur={d.motionBlur}");

        QualitySettings.vSyncCount = d.vSync ? 1 : 0;
        Application.targetFrameRate = Mathf.Clamp(d.targetFps, 30, 1000);

        var cam = Camera.main ?? FindFirstObjectByType<Camera>();
        if (cam)
        {
            cam.fieldOfView = Mathf.Clamp(d.fov, 40f, 110f);
            Debug.Log($"[GraphicsApplier] 카메라 FOV 적용: {cam.fieldOfView}");
        }
        else
        {
            Debug.LogWarning("[GraphicsApplier] Camera.main 이 없음");
        }

        if (motionBlur != null)
        {
            motionBlur.active = d.motionBlur;
            Debug.Log($"[GraphicsApplier] MotionBlur 적용됨: {motionBlur.active}, Intensity={(motionBlur.intensity != null ? motionBlur.intensity.value : -1f)}");
        }
        else
        {
            Debug.LogWarning("[GraphicsApplier] motionBlur NULL, 적용 실패");
        }
    }

    // 해상도/품질/창모드 적용은 이전 코드 그대로 사용
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
                Debug.Log($"[GraphicsApplier] 해상도 적용: {d.width}x{d.height}@{d.refreshRate}Hz");
                break;

            case HeavyOptions.QualityLevel:
                d.qualityLevel = Mathf.Clamp(d.qualityLevel, 0, QualitySettings.names.Length - 1);
                QualitySettings.SetQualityLevel(d.qualityLevel, true);
                Debug.Log($"[GraphicsApplier] 퀄리티 적용: {d.qualityLevel} ({QualitySettings.names[d.qualityLevel]})");
                break;

            case HeavyOptions.FullscreenMode:
                Screen.fullScreenMode = d.fullscreenMode;
                Debug.Log($"[GraphicsApplier] 창모드 적용: {d.fullscreenMode}");
                break;
        }
    }
}
