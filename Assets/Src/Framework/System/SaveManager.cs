using UnityEngine;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    //GameState gameState;
    //PlayerController playerController;
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

        //gameState = FindObjectOfType<GameState>();
        //playerController = FindObjectOfType<PlayerController>();
        cartManager = FindAnyObjectByType<MonitorShopCartManager>();
        //roomManager = FindObjectOfType<RoomManager>();
    }

    public void SaveGame()
    {
        Debug.Log("저장시작");

        GameSaveData data = new GameSaveData();

        // data.playerMoney = gameState.Money;
        // data.playerPosition = playerController.transform.position;
        // data.cartData = cartManager.GetCartData();
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

        // gameState.Money = data.playerMoney;
        // playerController.transform.position = data.playerPosition;
        // cartManager.LoadCartData(data.cartData);
        // roomManager.LoadUnlockedRooms(data.unlockedRooms);

        Debug.Log("게임 불러오기 완료");
    }

}
