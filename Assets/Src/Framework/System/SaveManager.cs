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

    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // 아직 초기화되지 않았고, 인게임 씬일 때만 시도
        if (!isInitialized && IsInGameScene())
        {
            TryInitialize();

            if (isInitialized)
            {
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
                    SaveGame();
                }
            }
        }
    }

    private bool IsInGameScene()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return currentScene != "MainMenu";
    }

    private void TryInitialize()
    {
        gameState = FindFirstObjectByType<GameState>();
        playerController = FindFirstObjectByType<FirstPersonController>();
        cartManager = FindFirstObjectByType<MonitorShopCartManager>();

        if (gameState != null && playerController != null && cartManager != null)
        {
            isInitialized = true;
            Debug.Log("SaveManager 초기화 완료");
        }
        else
        {
            Debug.LogWarning("SaveManager 초기화 대기 중...");
                    if (gameState == null) Debug.LogWarning("→ GameState가 null입니다");
        if (playerController == null) Debug.LogWarning("→ FirstPersonController가 null입니다");
        if (cartManager == null) Debug.LogWarning("→ MonitorShopCartManager가 null입니다");
        }
    }

    public void NewGame()
    {
        string newSlotName = "slot_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string slotPath = "slots/" + newSlotName + ".es3";

        if (!ES3.FileExists(slotPath))
        {
            ES3.Save("Created", true, slotPath);
            Debug.Log("슬롯 파일 생성됨: " + slotPath);
        }

        ES3SlotManager.selectedSlotPath = slotPath;
        Debug.Log("선택된 슬롯 경로: " + slotPath);
    }

    public void SaveGame()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("SaveGame 실패: 아직 초기화되지 않음");
            return;
        }

        if (string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath))
        {
            Debug.LogError("SaveGame 실패: 슬롯 경로 없음");
            return;
        }

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
        if (!isInitialized)
        {
            Debug.LogWarning("LoadGame 실패: 아직 초기화되지 않음");
            return;
        }

        if (string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath))
        {
            Debug.LogError("LoadGame 실패: 슬롯 경로 없음");
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

        Debug.Log("게임 불러오기 완료");
    }
}
