using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using StarterAssets;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private GameState gameState;
    private FirstPersonController playerController;
    private MonitorShopCartManager cartManager;
    private TimeUI timeUI;
    
    private bool isInitialized = false;
    private List<ISaveable> saveables = new();

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

                if (ES3.KeyExists("SaveMarker", ES3SlotManager.selectedSlotPath))
                {
                    LoadGame();
                }
                else
                {
                    SaveGame(); // 초기 세이브
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
        timeUI = FindFirstObjectByType<TimeUI>();

        saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>().ToList();

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
        if (!isInitialized || string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath))
        {
            Debug.LogWarning("SaveGame 실패: 초기화되지 않았거나 슬롯 경로 없음");
            return;
        }

        foreach (var s in saveables)
        {
            string key = s.GetType().Name;
            object data = s.CaptureData();
            ES3.Save(key, data, ES3SlotManager.selectedSlotPath);
        }

        ES3.Save("SaveMarker", true, ES3SlotManager.selectedSlotPath); // 세이브 여부 마커
        Debug.Log("게임 저장 완료");
    }

    public void LoadGame()
    {
        if (!isInitialized || string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath))
        {
            Debug.LogWarning("LoadGame 실패: 초기화되지 않았거나 슬롯 경로 없음");
            return;
        }

        if (!ES3.KeyExists("SaveMarker", ES3SlotManager.selectedSlotPath))
        {
            Debug.LogWarning("저장된 데이터 없음");
            return;
        }

        foreach (var s in saveables)
        {
            string key = s.GetType().Name;

            if (ES3.KeyExists(key, ES3SlotManager.selectedSlotPath))
            {
                object data = ES3.Load<object>(key, ES3SlotManager.selectedSlotPath);
                s.RestoreData(data);
            }
            else
            {
                Debug.LogWarning($"불러올 키 없음: {key}");
            }
        }

        Debug.Log("게임 불러오기 완료");
    }
}
