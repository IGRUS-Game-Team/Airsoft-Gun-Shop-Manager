using UnityEngine;

public class MoneySaveHandler : MonoBehaviour, ISaveable
{
    private GameState gameState;

    private void Awake()
    {
        gameState = FindFirstObjectByType<GameState>();
    }

    private void EnsureRefs()
    {
        if (gameState == null)
            gameState = FindFirstObjectByType<GameState>(FindObjectsInactive.Include);
    }

    public object CaptureData()
    {
        EnsureRefs();
        return new MoneySaveData { money = gameState ? gameState.Money : 0 };
    }

    public void RestoreData(object data)
    {
        EnsureRefs();
        var moneyData = data as MoneySaveData;
        if (moneyData == null || gameState == null) return;

        gameState.SetMoney(moneyData.money);
        Debug.Log($"[Load] Money ← {moneyData.money}");
    }
}
