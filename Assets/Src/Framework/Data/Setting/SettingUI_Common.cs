using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI_Common : MonoBehaviour
{
    [Header("Audio")]
    public Slider masterSlider; // 0~1 권장
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Controls")]
    public Slider mouseSensitivity; // 0.1~5.0
    public Toggle invertY;
    //public Toggle vibration;

    [Header("Accessibility/UI")]
    public Toggle subtitles;
    public Slider subtitleScale; // 0.8~1.5
    public Slider uiScale;       // 0.8~1.5

    void OnEnable()
    {
        var d = SettingsManager.Instance.Data;
        // Sliders (오디오: 0~1 ↔ dB 변환)
        masterSlider.SetValueWithoutNotify(AudioApplierDbTo01(d.masterDb));
        bgmSlider.SetValueWithoutNotify(AudioApplierDbTo01(d.bgmDb));
        sfxSlider.SetValueWithoutNotify(AudioApplierDbTo01(d.sfxDb));

        mouseSensitivity.SetValueWithoutNotify(d.mouseSensitivity);
        invertY.SetIsOnWithoutNotify(d.invertY);
       // vibration.SetIsOnWithoutNotify(d.gamepadVibration);

        subtitles.SetIsOnWithoutNotify(d.subtitles);
        subtitleScale.SetValueWithoutNotify(d.subtitleScale);
        uiScale.SetValueWithoutNotify(d.uiScale);
    }

    public void OnMasterChanged(float v)
    {
        SettingsManager.Instance.Data.masterDb = AudioApplier.Linear01ToDb(v);
        SettingsManager.Instance.audioApplier.Apply(SettingsManager.Instance.Data);
        SettingsManager.Instance.Save();
    }
    public void OnBGMChanged(float v)
    {
        SettingsManager.Instance.Data.bgmDb = AudioApplier.Linear01ToDb(v);
        SettingsManager.Instance.audioApplier.Apply(SettingsManager.Instance.Data);
        SettingsManager.Instance.Save();
    }
    public void OnSFXChanged(float v)
    {
        SettingsManager.Instance.Data.sfxDb = AudioApplier.Linear01ToDb(v);
        SettingsManager.Instance.audioApplier.Apply(SettingsManager.Instance.Data);
        SettingsManager.Instance.Save();
    }

    public void OnMouseSensitivity(float v)
    {
        SettingsManager.Instance.Data.mouseSensitivity = v;
        SettingsManager.Instance.inputApplier.Apply(SettingsManager.Instance.Data);
        SettingsManager.Instance.Save();
    }
    public void OnInvertY(bool on)
    {
        SettingsManager.Instance.Data.invertY = on;
        SettingsManager.Instance.inputApplier.Apply(SettingsManager.Instance.Data);
        SettingsManager.Instance.Save();
    }
    // public void OnVibration(bool on)
    // {
    //     SettingsManager.Instance.Data.gamepadVibration = on;
    //     SettingsManager.Instance.Save();
    // }

    public void OnSubtitles(bool on)
    {
        SettingsManager.Instance.Data.subtitles = on;
        SettingsManager.Instance.Save();
    }
    public void OnSubtitleScale(float v)
    {
        SettingsManager.Instance.Data.subtitleScale = v;
        SettingsManager.Instance.Save();
    }
    public void OnUIScale(float v)
    {
        SettingsManager.Instance.Data.uiScale = v;
        SettingsManager.Instance.Save();
    }

    private static float AudioApplierDbTo01(float db)
    {
        // 단순 역함수: -80~0 dB → 0~1 근사
        if (db <= -80f) return 0f;
        return Mathf.Clamp01(Mathf.Pow(10f, db / 20f));
    }
}
