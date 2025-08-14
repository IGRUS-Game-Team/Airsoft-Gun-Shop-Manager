using UnityEngine;
using UnityEngine.SceneManagement;

public class InputApplier : MonoBehaviour
{
    public Camera playerCamera; // 비워도 자동 탐색
    private LookBinding binding;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryBind();
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        TryBind();
        if (SettingsManager.Instance) Apply(SettingsManager.Instance.Data);
    }

    private void TryBind()
    {
        if (!playerCamera) playerCamera = Camera.main ?? FindFirstObjectByType<Camera>();
        binding = binding ?? FindFirstObjectByType<LookBinding>();
    }

    public void Apply(SettingsData d)
    {
        if (!binding) binding = FindFirstObjectByType<LookBinding>();
        if (binding) binding.Apply(d.mouseSensitivity, d.invertY);
        // 없으면(메인 메뉴) 조용히 스킵
    }
}
