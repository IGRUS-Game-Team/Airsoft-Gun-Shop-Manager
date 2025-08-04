using UnityEngine;
using System;
using StarterAssets;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private GameState gameState;
    private FirstPersonController playerController;
    private MonitorShopCartManager cartManager;
    // private RoomManager roomManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 이동 시 유지
    }

    private void Start()
    {

        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene != "MainMenu") // ← 실제 인게임 씬 이름으로 교체
        {
            Debug.Log("현재 씬이 인게임이 아님. Save/Load 생략.");
            return;
        }
        // 게임 오브젝트들이 다 로드된 이후에 참조 시도
        gameState = FindFirstObjectByType<GameState>();
        playerController = FindFirstObjectByType<FirstPersonController>();
        cartManager = FindFirstObjectByType<MonitorShopCartManager>();
        // roomManager = FindFirstObjectByType<RoomManager>();

        // // 인게임 진입 후 슬롯 선택돼 있으면 자동 저장
        // if (!string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath))
        // {
        //     Debug.Log("▶ 인게임 씬 시작됨 - 자동 저장 실행");
        //     SaveGame();
        // }
        if (string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath))
        {
            Debug.LogWarning("슬롯 없음 - 세이브/로드 생략");
            return;
        }

        if (ES3.KeyExists("GameSave", ES3SlotManager.selectedSlotPath))
        {
            LoadGame();
        }
        else
        {
            SaveGame(); // 신규 슬롯이면 저장
        }
    }

    /// <summary>
    /// 메인 메뉴에서 슬롯 생성 + 씬 전환
    /// </summary>
    public void NewGame()
    {
        string newSlotName = "slot_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string slotPath = "slots/" + newSlotName + ".es3";

        if (!ES3.FileExists(slotPath))
        {
            ES3.Save("Created", true, slotPath); // 더미 키로 파일 생성
            Debug.Log("슬롯 파일 생성됨: " + slotPath);
        }

        ES3SlotManager.selectedSlotPath = slotPath;
        Debug.Log("선택된 슬롯 경로: " + slotPath);

        
    }

    public void SaveGame()
    {
        if (gameState == null || playerController == null || cartManager == null)
        {
            Debug.LogWarning("SaveGame 실패: 게임 오브젝트 참조 누락");
            return;
        }

        if (string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath))
        {
            Debug.LogError("SaveGame 실패: 슬롯 경로가 설정되지 않음");
            return;
        }

        Debug.Log("저장 시작");

        GameSaveData data = new GameSaveData
        {
            playerMoney = gameState.Money,
            playerPosition = playerController.transform.position,
            cartData = cartManager.GetCartData()
            // unlockedRooms = roomManager.GetUnlockedRoomIds()
        };

        ES3.Save("GameSave", data, ES3SlotManager.selectedSlotPath);

        Debug.Log("게임 저장 완료");
    }

    public void LoadGame()
    {
        Debug.Log("불러오기 시작");

        if (gameState == null || playerController == null || cartManager == null)
        {
            Debug.LogWarning("LoadGame 실패: 게임 오브젝트 참조 누락");
            return;
        }

        if (string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath))
        {
            Debug.LogError("LoadGame 실패: 선택된 슬롯 없음");
            return;
        }

        if (!ES3.KeyExists("GameSave", ES3SlotManager.selectedSlotPath))
        {
            Debug.LogWarning("저장된 데이터 없음");
            return;
        }

        GameSaveData data = ES3.Load<GameSaveData>("GameSave", ES3SlotManager.selectedSlotPath);

        gameState.SetMoney(data.playerMoney);
        playerController.transform.position = data.playerPosition;
        cartManager.LoadCartData(data.cartData);
        // roomManager.LoadUnlockedRooms(data.unlockedRooms);

        Debug.Log("✅ 게임 불러오기 완료");
    }
}
