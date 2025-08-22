using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI_InGame : MonoBehaviour
{
    [Header("Safe Graphics")]
    public Toggle vSync;
    public TMP_InputField targetFps;  // 숫자 입력

    public Slider fovSlider;          // 40~110
    public Toggle motionBlurToggle;

    [Header("Dangerous Graphics (disabled in-game)")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown fullscreenDropdown;
    public TMP_Dropdown qualityDropdown;
    public TMP_Text disabledHint;

    void OnEnable()
    {
        SettingsManager.Instance.BeginSnapshot();

        var d = SettingsManager.Instance.Data;
        vSync.SetIsOnWithoutNotify(d.vSync);
        targetFps.text = d.targetFps.ToString();
        fovSlider.SetValueWithoutNotify(d.fov);
        motionBlurToggle.SetIsOnWithoutNotify(d.motionBlur);

        // 위험 옵션 비활성화 + 안내문
        if (resolutionDropdown) resolutionDropdown.interactable = false;
        if (fullscreenDropdown) fullscreenDropdown.interactable = false;
        if (qualityDropdown) qualityDropdown.interactable = false;
        if (disabledHint) disabledHint.text = "Change resolution/window mode/quality presets in the main menu.";
    }

    public void OnVSync(bool on)
    {
        SettingsManager.Instance.Data.vSync = on;
        SettingsManager.Instance.graphicsApplier.ApplySafe(SettingsManager.Instance.Data);
        SettingsManager.Instance.Save();
    }

    public void OnTargetFpsChanged(string s)
    {
        if (int.TryParse(s, out int fps))
        {
            SettingsManager.Instance.Data.targetFps = Mathf.Clamp(fps, 30, 1000);
            SettingsManager.Instance.graphicsApplier.ApplySafe(SettingsManager.Instance.Data);
            SettingsManager.Instance.Save();
        }
    }

    public void OnFovChanged(float v)
    {
        SettingsManager.Instance.Data.fov = v;
        SettingsManager.Instance.graphicsApplier.ApplySafe(SettingsManager.Instance.Data);
        SettingsManager.Instance.Save();
    }

    public void OnMotionBlur(bool on)
    {
        SettingsManager.Instance.Data.motionBlur = on;
        SettingsManager.Instance.graphicsApplier.ApplySafe(SettingsManager.Instance.Data);
        SettingsManager.Instance.Save();
    }

public void OnClickClose()
{
    // UI → Data 반영
    var d = SettingsManager.Instance.Data;
    d.vSync = vSync.isOn;
    if (int.TryParse(targetFps.text, out var fps))
        d.targetFps = Mathf.Clamp(fps, 30, 1000);
    d.fov = fovSlider.value;
    d.motionBlur = motionBlurToggle.isOn;

    SettingsManager.Instance.graphicsApplier.ApplySafe(d);
    SettingsManager.Instance.Save();

    gameObject.SetActive(false);
    Time.timeScale = 1f; // 일시정지 해제
}


    public void OnClickCancel()
    {
        SettingsManager.Instance.RevertSnapshot();
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}
