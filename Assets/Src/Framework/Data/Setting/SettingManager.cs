using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("References")]
    public AudioApplier audioApplier;
    public GraphicsApplier graphicsApplier;
    public InputApplier inputApplier;

    public SettingsData Data { get; private set; }
    private SettingsData snapshot; // UI 오픈 시점 스냅샷

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Data = SettingsStore.LoadOrCreate();
        ApplyAllImmediate();
    }

    // UI 열 때 호출: 스냅샷 생성 (취소/롤백용)
    public void BeginSnapshot()
    {
        snapshot = JsonUtility.FromJson<SettingsData>(JsonUtility.ToJson(Data));
    }

    // 취소/닫기
    public void RevertSnapshot()
    {
        if (snapshot != null)
        {
            Data = snapshot;
            ApplyAllImmediate();
            snapshot = null;
        }
    }

    // 저장 & 즉시 적용(안전 옵션은 런타임 적용, 위험 옵션은 상황에 따라 제한)
    public void Save()
    {
        SettingsStore.Save(Data);
    }

    public void ApplyAllImmediate()
    {
        graphicsApplier.ApplySafe(Data); // VSync / FPS / FOV / 모션블러 등
        audioApplier.Apply(Data);
        inputApplier.Apply(Data);
    }

    // 메인메뉴에서만 호출할 위험 옵션
    public void ApplyHeavy(GraphicsApplier.HeavyOptions option)
    {
        graphicsApplier.ApplyHeavyOption(Data, option);
    }
}
