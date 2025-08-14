using UnityEngine;
using System.IO;

/// <summary>
/// Easy Save(ES3) 기반 Settings 저장/로드.
/// 파일: {persistentDataPath}/settings.es3 (기본)
/// </summary>
public static class SettingsStore
{
    private const string KEY = "settings";               // ES3 내 키 이름
    private const string FILE = "settings.es3";          // 파일명

    // 파일 경로 프로퍼티 (메서드 아님)
    private static string Es3Path => Path.Combine(Application.persistentDataPath, FILE);

    // (선택) JSON에서 ES3로 1회 마이그레이션
    private static string LegacyJson => Path.Combine(Application.persistentDataPath, "settings.json");

    public static void Save(SettingsData data)
    {
        // 클래스 통째로 저장 (Serializable 필요)
        ES3.Save(KEY, data, Es3Path);
#if UNITY_EDITOR
        Debug.Log($"[Settings/ES3] Saved to {Es3Path}");
#endif
    }

    public static SettingsData LoadOrCreate()
    {
        // 1) ES3에 있으면 로드
        if (ES3.FileExists(Es3Path) && ES3.KeyExists(KEY, Es3Path))
        {
            try
            {
                return ES3.Load<SettingsData>(KEY, Es3Path);
            }
            catch
            {
                Debug.LogWarning("[Settings/ES3] Load failed, creating defaults.");
                return new SettingsData();
            }
        }

        // 2) (선택) 이전 JSON이 있다면 한 번 읽어와 ES3로 저장 (마이그레이션)
        if (File.Exists(LegacyJson))
        {
            try
            {
                var legacyJson = File.ReadAllText(LegacyJson);
                var fromJson = JsonUtility.FromJson<SettingsData>(legacyJson) ?? new SettingsData();
                ES3.Save(KEY, fromJson, Es3Path);
                return fromJson;
            }
            catch
            {
                // 무시하고 기본값으로
            }
        }

        // 3) 없으면 기본값 새로 생성
        var fresh = new SettingsData();
        ES3.Save(KEY, fresh, Es3Path);
        return fresh;
    }

    // 필요 시 다른 슬롯 지원
    public static void SaveToSlot(SettingsData data, string slotName)
    {
        string path = Path.Combine(Application.persistentDataPath, $"settings_{slotName}.es3");
        ES3.Save(KEY, data, path);
    }

    public static SettingsData LoadFromSlot(string slotName)
    {
        string path = Path.Combine(Application.persistentDataPath, $"settings_{slotName}.es3");
        if (ES3.FileExists(path) && ES3.KeyExists(KEY, path))
            return ES3.Load<SettingsData>(KEY, path);
        return new SettingsData();
    }
}
