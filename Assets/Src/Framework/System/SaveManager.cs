using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// ES3 기반 저장/불러오기를 총괄하는 DontDestroyOnLoad 싱글톤.
///
/// [저장 흐름]
/// 1. SaveGame() 호출 (일일 정산 후 자동 or 수동)
/// 2. FindObjectsOfType로 모든 ISaveable 수집
/// 3. 각 핸들러의 CaptureData() → ES3.Save(클래스명, data, slotPath)
///
/// [불러오기 흐름]
/// 1. 메인 메뉴에서 슬롯 선택 → QueueLoadAfterSceneChange()
/// 2. 씬 전환 후 OnSceneLoaded → InitAndMaybeLoad()
/// 3. ISaveable 수집 → GetRestoreOrder()로 정렬 → 순서대로 RestoreData()
///
/// [복원 순서가 중요한 이유]
/// MarketExpansion(-2)이 매장 영역을 먼저 활성화해야
/// Furniture(-1)와 ShelfItem(1)이 해당 영역에 오브젝트를 복원할 수 있음.
///
/// [새 핸들러 추가 시]
/// 1. SaveDatas/에 DTO 클래스 생성 ([Serializable])
/// 2. SaveHandler/에 MonoBehaviour + ISaveable 구현
/// 3. 씬에 배치
/// 4. 필요 시 GetRestoreOrder()에 순서 추가
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private static bool pendingLoad;          // 슬롯 선택 후 씬 전환 시 자동 로드 실행 플래그
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
        // 메인 메뉴 씬에서는 ISaveable 수집 스킵
        if (s.name.StartsWith("MainMenu")) return;
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
        if (!isInitialized)
        { Debug.LogWarning("SaveGame 실패: 초기화 안 됨"); return; }

        // 슬롯이 없으면 (새 게임에서 직접 저장) 자동 생성
        if (string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath))
        {
            var slotName = "Save_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            ES3SlotManager.selectedSlotPath = "slots/" + slotName + ".es3";
            Debug.Log($"[SaveManager] 새 슬롯 자동 생성: {ES3SlotManager.selectedSlotPath}");
        }

        // 최신 상태로 한 번 더 수집
        saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToList();

        foreach (var s in saveables)
        {
            var key = s.GetType().Name;
            ES3.Save(key, s.CaptureData(), ES3SlotManager.selectedSlotPath);
        }
        ES3.Save("SaveMarker", true, ES3SlotManager.selectedSlotPath);
        Debug.Log($"[SaveManager] 게임 저장 완료 → {ES3SlotManager.selectedSlotPath} ({saveables.Count}개 핸들러)");
    }

    public void LoadGame()
    {
        if (!isInitialized || string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath))
        { Debug.LogWarning("LoadGame 실패: 초기화/슬롯"); return; }

        if (!ES3.KeyExists("SaveMarker", ES3SlotManager.selectedSlotPath))
        { Debug.LogWarning("저장 없음"); return; }

        // 최신 상태로 한 번 더 수집
        saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToList();

        // 복원 순서 보장: Furniture(-1) → 기본(0) → ShelfItem(1)
        saveables.Sort((a, b) => GetRestoreOrder(a).CompareTo(GetRestoreOrder(b)));

        foreach (var s in saveables)
        {
            var key = s.GetType().Name;
            if (!ES3.KeyExists(key, ES3SlotManager.selectedSlotPath))
            { Debug.LogWarning($"[Load] 키 없음: {key}"); continue; }

            try
            {
                var data = ES3.Load<object>(key, ES3SlotManager.selectedSlotPath);
                if (data == null)
                { Debug.LogWarning($"[Load] 데이터 null: {key}"); continue; }

                s.RestoreData(data);
                Debug.Log($"[Load] 복원 완료: {key}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Load] 복원 실패: {key} — {e.Message}");
            }
        }
        Debug.Log("게임 불러오기 완료");
    }

    private static int GetRestoreOrder(ISaveable s)
    {
        if (s is MarketExpansionSaveHandler) return -2;  // 매장 확장 먼저 (영역 활성화)
        if (s is FurnitureSaveHandler)       return -1;  // 가구
        if (s is ShelfItemSaveHandler)       return  1;  // 선반 아이템 + 가격표
        if (s is WallGunSaveHandler)         return  2;  // 벽 총기
        return 0;                                         // 기본 (박스, 돈, 문 상태 등)
    }
}