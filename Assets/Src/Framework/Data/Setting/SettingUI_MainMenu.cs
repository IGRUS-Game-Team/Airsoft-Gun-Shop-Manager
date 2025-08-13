using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI_MainMenu : MonoBehaviour
{
    [Header("Graphics (Heavy)")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown fullscreenDropdown; // 0: FullScreenWindow, 1: MaximizedWindow, 2: Windowed
    public TMP_Dropdown qualityDropdown;
    public TMP_Text confirmTimerText;
    public GameObject confirmPanel;

    private Resolution[] resolutions;
    private (int w, int h, int rr, FullScreenMode mode, int ql) rollback;

    void OnEnable()
    {
        SettingsManager.Instance.BeginSnapshot();

        // 해상도 목록 채우기
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();
        for (int i = 0; i < resolutions.Length; i++)
            options.Add($"{resolutions[i].width}×{resolutions[i].height} @ {resolutions[i].refreshRateRatio.value}Hz");
        resolutionDropdown.AddOptions(options);

        // 현재값 반영
        var d = SettingsManager.Instance.Data;

        int currentIndex = FindClosestResolutionIndex(d.width, d.height, d.refreshRate);
        resolutionDropdown.SetValueWithoutNotify(currentIndex);

        fullscreenDropdown.ClearOptions();
        fullscreenDropdown.AddOptions(new System.Collections.Generic.List<string>{
            "Fullscreen", "Maximized", "Windowed"
        });
        fullscreenDropdown.SetValueWithoutNotify(FromMode(d.fullscreenMode));

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
        qualityDropdown.SetValueWithoutNotify(Mathf.Clamp(d.qualityLevel, 0, QualitySettings.names.Length - 1));

        confirmPanel.SetActive(false);
        confirmTimerText.text = "";
    }

    public void OnResolutionChanged(int idx)
    {
        var r = resolutions[Mathf.Clamp(idx, 0, resolutions.Length - 1)];
        var d = SettingsManager.Instance.Data;
        d.width = r.width; d.height = r.height; d.refreshRate = (int)r.refreshRateRatio.value;

        AskConfirmAndApply(GraphicsApplier.HeavyOptions.Resolution);
    }

    public void OnFullscreenChanged(int idx)
    {
        var d = SettingsManager.Instance.Data;
        d.fullscreenMode = ToMode(idx);
        AskConfirmAndApply(GraphicsApplier.HeavyOptions.FullscreenMode);
    }

    public void OnQualityChanged(int idx)
    {
        var d = SettingsManager.Instance.Data;
        d.qualityLevel = idx;
        SettingsManager.Instance.ApplyHeavy(GraphicsApplier.HeavyOptions.QualityLevel);
        SettingsManager.Instance.Save();
    }

    // 확인 타이머(15초) 후 롤백
    private void AskConfirmAndApply(GraphicsApplier.HeavyOptions opt)
    {
        var d = SettingsManager.Instance.Data;
        rollback = (Screen.width, Screen.height, Screen.currentResolution.refreshRate, Screen.fullScreenMode, QualitySettings.GetQualityLevel());

        SettingsManager.Instance.ApplyHeavy(opt);
        SettingsManager.Instance.Save();
        StopAllCoroutines();
        StartCoroutine(ConfirmRoutine());
    }

    private IEnumerator ConfirmRoutine()
    {
        confirmPanel.SetActive(true);
        float t = 15f;
        while (t > 0f)
        {
            confirmTimerText.text = $"설정 적용됨. {Mathf.CeilToInt(t)}초 내에 확인을 누르지 않으면 롤백됩니다.";
            t -= Time.unscaledDeltaTime;
            yield return null;
        }
        OnConfirmTimeout();
    }

    public void OnConfirmAccept()
    {
        StopAllCoroutines();
        confirmPanel.SetActive(false);
        confirmTimerText.text = "";
        // 현재를 확정 (아무 것도 안 함)
    }

    public void OnConfirmTimeout()
    {
        // 롤백
        Screen.SetResolution(rollback.w, rollback.h, rollback.mode, rollback.rr);
        QualitySettings.SetQualityLevel(rollback.ql, true);
        confirmPanel.SetActive(false);
        confirmTimerText.text = "이전 설정으로 복원했습니다.";
    }

    public void OnClickClose()
    {
        // 스냅샷 필요 없으면 생략 가능. 여기서는 유지.
        SettingsManager.Instance.Save();
        gameObject.SetActive(false);
    }

    public void OnClickCancel()
    {
        SettingsManager.Instance.RevertSnapshot();
        gameObject.SetActive(false);
    }

    private int FindClosestResolutionIndex(int w, int h, int rr)
    {
        int best = 0; int bestScore = int.MaxValue;
        for (int i = 0; i < resolutions.Length; i++)
        {
            int score = Mathf.Abs(resolutions[i].width - w) +
                        Mathf.Abs(resolutions[i].height - h) +
                        Mathf.Abs((int)resolutions[i].refreshRateRatio.value - rr);
            if (score < bestScore) { bestScore = score; best = i; }
        }
        return best;
    }

    private static int FromMode(FullScreenMode m)
    {
        return m switch
        {
            FullScreenMode.FullScreenWindow => 0,
            FullScreenMode.MaximizedWindow => 1,
            _ => 2,
        };
    }
    private static FullScreenMode ToMode(int i)
    {
        return i switch
        {
            0 => FullScreenMode.FullScreenWindow,
            1 => FullScreenMode.MaximizedWindow,
            _ => FullScreenMode.Windowed,
        };
    }
}
