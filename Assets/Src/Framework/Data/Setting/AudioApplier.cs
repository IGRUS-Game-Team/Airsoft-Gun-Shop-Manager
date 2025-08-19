using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioApplier : MonoBehaviour
{
    [Header("AudioMixer & exposed params")]
    public AudioMixer mixer;         // 비어 있어도 됨. 자동 로드 시도함.
    public string mixerResourcePath = "Audio/MasterMixer"; // Resources/Audio/MasterMixer.mixer
    public string masterParam = "MasterVol";
    public string bgmParam = "BGMVol";
    public string sfxParam = "SFXVol";

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryLoadMixer();
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        TryLoadMixer();
        if (SettingsManager.Instance != null)
            Apply(SettingsManager.Instance.Data);
    }

    private void TryLoadMixer()
    {
        if (!mixer && !string.IsNullOrEmpty(mixerResourcePath))
            mixer = Resources.Load<AudioMixer>(mixerResourcePath);
    }

    public void Apply(SettingsData d)
    {
        if (!mixer) return; // 메인 메뉴에 믹서 없으면 조용히 패스
        mixer.SetFloat(masterParam, d.masterDb);
        mixer.SetFloat(bgmParam, d.bgmDb);
        mixer.SetFloat(sfxParam, d.sfxDb);
    }

    public static float Linear01ToDb(float x)
    {
        return Mathf.Approximately(x, 0f) ? -80f : Mathf.Log10(Mathf.Clamp01(x)) * 20f;
    }
}
