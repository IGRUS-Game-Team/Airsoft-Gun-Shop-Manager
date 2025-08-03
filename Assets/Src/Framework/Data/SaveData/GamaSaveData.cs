using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public float playerMoney;
    public Vector3 playerPosition;
    public List<CartSaveData> cartData = new();
    //public List<string> unlockedRooms = new(); //해금되는 방
}