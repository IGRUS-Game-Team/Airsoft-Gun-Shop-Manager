using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private static bool pendingLoad;          // ★ 슬롯 선택 후 로드 대기 플래그
    private bool isInitialized;
    private List<ISaveable> saveables = new();

    public static void QueueLoadAfterSceneChange()  // ★ 슬롯 이벤트에서 호출
    {
        pendingLoad = true;
        Debug.Log("[SaveManager] pendingLoad = true");
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // 게임씬만 제한하고 싶으면 이름 비교, 아니면 아래 if 삭제
        // if (s.name != "RealFinal") return;
        StartCoroutine(InitAndMaybeLoad());
    }

    private IEnumerator InitAndMaybeLoad()
    {
        yield return null; // 씬 오브젝트 올라올 때까지 1프레임 대기

        // ISaveable 재수집 (비활성 포함)
        saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToList();
        isInitialized = saveables.Count > 0;
        Debug.Log($"[SaveManager] ISaveable {saveables.Count}개");

        if (!pendingLoad) yield break;     // 슬롯에서 로드 요청 안 왔으면 끝
        pendingLoad = false;                // 1회성

        if (string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath))
        {
            Debug.LogWarning("[SaveManager] 슬롯 경로 없음");
            yield break;
        }

        if (ES3.KeyExists("SaveMarker", ES3SlotManager.selectedSlotPath)) LoadGame();
        else SaveGame();
    }

    public void SaveGame()
    {
        if (!isInitialized || string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath))
        { Debug.LogWarning("SaveGame 실패: 초기화/슬롯"); return; }

        // 최신 상태로 한 번 더 수집
        saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToList();

        foreach (var s in saveables)
        {
            var key = s.GetType().Name;
            ES3.Save(key, s.CaptureData(), ES3SlotManager.selectedSlotPath);
        }
        ES3.Save("SaveMarker", true, ES3SlotManager.selectedSlotPath);
        Debug.Log("게임 저장 완료");
    }

    public void LoadGame()
    {
        if (!isInitialized || string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath))
        { Debug.LogWarning("LoadGame 실패: 초기화/슬롯"); return; }

        if (!ES3.KeyExists("SaveMarker", ES3SlotManager.selectedSlotPath))
        { Debug.LogWarning("저장 없음"); return; }

        // 최신 상태로 한 번 더 수집
        saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToList();

        foreach (var s in saveables)
        {
            var key = s.GetType().Name;
            if (!ES3.KeyExists(key, ES3SlotManager.selectedSlotPath))
            { Debug.LogWarning($"키 없음: {key}"); continue; }

            var data = ES3.Load<object>(key, ES3SlotManager.selectedSlotPath);
            s.RestoreData(data);
        }
        Debug.Log("게임 불러오기 완료");
    }
}
