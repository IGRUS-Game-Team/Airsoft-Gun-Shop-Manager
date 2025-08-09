using UnityEngine;

public class PurchaseProcessor : MonoBehaviour
{
    [SerializeField] private MonitorShopCartManager monitorShopCartManager;
    [SerializeField] private GameState gameState;
    [SerializeField] private BoxSpawner boxSpawner;
    public void ProcessPurchase()
    {
        var cartItems = monitorShopCartManager.GetCartData();
        float totalCost = 0f;

        foreach (var item in cartItems)
        {
            totalCost += item.unitPrice * item.amount;
        }
        if (gameState.Money < totalCost)
        {
            Debug.LogWarning("돈이 부족합니다!");
            return;
        }

        gameState.SpendMoney(totalCost);
        Debug.Log($"결제 성공! {totalCost}원 사용됨");

        foreach (var item in cartItems)
        {
            string prefabName = $"Box_{item.itemName}";
            boxSpawner.BoxDrop(item.itemId, item.amount, item.category, item.itemName);
        }

        monitorShopCartManager.ClearCart();
    }
    
}
