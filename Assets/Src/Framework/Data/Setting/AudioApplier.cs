using UnityEngine;
using UnityEngine.Audio;

public class AudioApplier : MonoBehaviour
{
    [Header("AudioMixer & exposed params")]
    public AudioMixer mixer;
    public string masterParam = "MasterVol"; // dB
    public string bgmParam = "BGMVol";
    public string sfxParam = "SFXVol";

    public void Apply(SettingsData d)
    {
        if (!mixer) return;
        mixer.SetFloat(masterParam, d.masterDb);
        mixer.SetFloat(bgmParam, d.bgmDb);
        mixer.SetFloat(sfxParam, d.sfxDb);
    }

    // UI 슬라이더(0~1)를 dB(-80~0)로 맵핑하고 싶다면:
    public static float Linear01ToDb(float x)
    {
        // 0 → -80dB, 1 → 0dB (간단 버전)
        return Mathf.Approximately(x, 0f) ? -80f : Mathf.Log10(Mathf.Clamp01(x)) * 20f;
    }
}
