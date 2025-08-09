using UnityEngine;

public class MoneySaveHandler : MonoBehaviour, ISaveable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private GameState gameState;

    private void Awake()
    {
        gameState = FindFirstObjectByType<GameState>();
    }
    public object CaptureData()
    {
        return new MoneySaveData
        {
            money = gameState.Money
        };
    }
    public void RestoreData(object data)
    {
        MoneySaveData moneyData = data as MoneySaveData;
        if (moneyData == null) return;

        gameState.SetMoney(moneyData.money);
    }
}
