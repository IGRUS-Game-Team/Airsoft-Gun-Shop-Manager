using UnityEngine;
using System.Collections.Generic;
using StarterAssets;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    GameState gameState;
    FirstPersonController playerController;
    MonitorShopCartManager cartManager;
    //RoomManager roomManager;

    private void Awake() //싱글톤
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        gameState = FindFirstObjectByType<GameState>();
        playerController = FindFirstObjectByType<FirstPersonController>();
        cartManager = FindFirstObjectByType<MonitorShopCartManager>();
        //roomManager = FindObjectOfType<RoomManager>();
    }

    public void SaveGame()
    {
        Debug.Log("저장시작");

        GameSaveData data = new GameSaveData();

        data.playerMoney = gameState.Money;
        data.playerPosition = playerController.transform.position;
        data.cartData = cartManager.GetCartData();
        // data.unlockedRooms = roomManager.GetUnlockedRoomIds();

        ES3.Save("GameSave", data);
        ES3.Save("GameSave", data);
        Debug.Log("게임 저장 완료");

    }

    public void LoadGame()
    {
        Debug.Log("불러오기 시작");

        if (!ES3.KeyExists("GameSave"))
        {
           Debug.LogWarning("저장된 데이터 없음");
            return;
        }

        GameSaveData data = ES3.Load<GameSaveData>("GameSave");

        gameState.SetMoney(data.playerMoney);
        playerController.transform.position = data.playerPosition;
        cartManager.LoadCartData(data.cartData);
        // roomManager.LoadUnlockedRooms(data.unlockedRooms);

        Debug.Log("게임 불러오기 완료");
    }

}
