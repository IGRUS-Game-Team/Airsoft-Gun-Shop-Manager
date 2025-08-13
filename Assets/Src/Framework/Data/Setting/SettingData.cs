using System;
using UnityEngine;

[Serializable]
public class SettingsData
{
    // — 그래픽(안전 옵션: 인게임 허용)
    public bool vSync = true;
    public int targetFps = 120;
    public float fov = 60f;                 // 카메라 FOV
    public bool motionBlur = false;         // 포스트 효과 on/off

    // — 그래픽(위험 옵션: 메인메뉴 전용)
    public int width = 1920;
    public int height = 1080;
    public int refreshRate = 60;
    public FullScreenMode fullscreenMode = FullScreenMode.FullScreenWindow;
    public int qualityLevel = 2;            // QualitySettings 인덱스

    // — 오디오 (AudioMixer 파라미터)
    public float masterDb = 0f;             // dB
    public float bgmDb = -5f;
    public float sfxDb = -5f;

    // — 조작
    public float mouseSensitivity = 1.0f;
    public bool invertY = false;
    public bool gamepadVibration = true;

    // — UI/접근성
    public bool subtitles = true;
    public float subtitleScale = 1.0f;      // 0.8~1.5
    public float uiScale = 1.0f;            // 0.8~1.5
}
