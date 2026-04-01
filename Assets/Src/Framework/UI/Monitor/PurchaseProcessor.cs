using UnityEngine;

public class PurchaseProcessor : MonoBehaviour
{
    [SerializeField] private MonitorShopCartManager monitorShopCartManager;
    [SerializeField] private GameState gameState;
    [SerializeField] private BoxSpawner boxSpawner;
    [SerializeField] private ItemDatabase itemDatabase;
    public void ProcessPurchase()
    {
        var cartItems = monitorShopCartManager.GetCartData();
        Debug.Log($"<color=cyan>[Purchase] ▶ ProcessPurchase 시작 — 카트 아이템 {cartItems.Count}개</color>");

        float totalCost = 0f;
        foreach (var item in cartItems)
            totalCost += item.unitPrice * item.amount;

        if (gameState.Money < totalCost)
        {
            Debug.LogWarning("[Purchase] 돈이 부족합니다!");
            return;
        }

        // 1) 돈 차감 + 정산 반영
        gameState.SpendMoney(totalCost);
        SettlementManager.Instance?.RegisterPurchaseCost(totalCost);
        Debug.Log($"<color=cyan>[Purchase] 돈 차감 완료: ${totalCost:F2}</color>");

        // 2) 카트 아이템 분기 처리
        bool hasPlaceable = false;

        foreach (var item in cartItems)
        {
            Debug.Log($"<color=cyan>[Purchase] 아이템: {item.itemName} | category={item.category} | id={item.itemId} | amount={item.amount}</color>");

            // 장식품·가구(선반)는 배치 모드로 설치
            if (item.category == ItemCategory.Decoration
                || item.category == ItemCategory.Furniture)
            {
                hasPlaceable = true;
                bool decoMode = item.category == ItemCategory.Decoration;
                Debug.Log($"<color=yellow>[Purchase]   → 설치형 아이템 감지 (decoMode={decoMode})</color>");

                if (itemDatabase == null)
                {
                    Debug.LogError("[Purchase]   ✗ itemDatabase == null! Inspector에서 연결하세요.");
                    continue;
                }

                bool tryGetOk = itemDatabase.TryGet(item.itemId, out ItemData data);
                Debug.Log($"<color=yellow>[Purchase]   TryGet(id={item.itemId}): {tryGetOk}" +
                          $" | worldPrefab={(data != null && data.worldPrefab != null ? data.worldPrefab.name : "NULL")}" +
                          $" | DecoPlacementMgr={(DecorationPlacementManager.Instance != null ? "OK" : "NULL")}</color>");

                if (tryGetOk && data.worldPrefab != null && DecorationPlacementManager.Instance != null)
                {
                    DecorationPlacementManager.Instance
                        .EnqueuePlacement(data.worldPrefab, item.amount, decoMode, item.unitPrice);
                    Debug.Log($"<color=green>[Purchase]   ✓ EnqueuePlacement 호출 완료</color>");
                }
                else
                {
                    Debug.LogError($"[Purchase]   ✗ 배치 큐 등록 실패! TryGet={tryGetOk}, worldPrefab={data?.worldPrefab}, Instance={DecorationPlacementManager.Instance}");
                }
            }
            else
            {
                boxSpawner.BoxDrop(item.itemId, item.amount);
                Debug.Log($"<color=cyan>[Purchase]   → 일반 아이템 BoxDrop</color>");
            }
        }

        TutorialEvents.RaisePurchased();
        monitorShopCartManager.ClearCart();

        // 4) 설치형 아이템이 있었으면: 모니터 종료 → 배치 모드 진입
        Debug.Log($"<color=cyan>[Purchase] hasPlaceable={hasPlaceable}" +
                  $" | MonitorUIModeManager={(MonitorUIModeManager.Instance != null ? "OK" : "NULL")}" +
                  $" | DecoPlacementMgr={(DecorationPlacementManager.Instance != null ? "OK" : "NULL")}" +
                  $" | HasPending={(DecorationPlacementManager.Instance != null ? DecorationPlacementManager.Instance.HasPending.ToString() : "N/A")}</color>");

        if (hasPlaceable)
        {
            if (MonitorUIModeManager.Instance != null)
            {
                MonitorUIModeManager.Instance.ExitUIMode();
                Debug.Log("<color=green>[Purchase] ✓ ExitUIMode 호출 완료</color>");
            }
            else
            {
                Debug.LogError("[Purchase] ✗ MonitorUIModeManager.Instance == null!");
            }

            if (DecorationPlacementManager.Instance != null
                && DecorationPlacementManager.Instance.HasPending)
            {
                DecorationPlacementManager.Instance.StartPlacing();
                Debug.Log("<color=green>[Purchase] ✓ StartPlacing 호출 완료</color>");
            }
            else
            {
                Debug.LogError("[Purchase] ✗ StartPlacing 호출 못 함! Instance=" +
                    (DecorationPlacementManager.Instance != null ? "OK" : "NULL") +
                    " HasPending=" + (DecorationPlacementManager.Instance?.HasPending));
            }
        }

        Debug.Log("<color=cyan>[Purchase] ▶ ProcessPurchase 종료</color>");
    }
    
}
