using System.Collections.Generic;
using UnityEngine;

public class ShopCartSaveHandler : MonoBehaviour, ISaveable
{
    private MonitorShopCartManager cartManager;

    private void Awake()
    {
        cartManager = FindFirstObjectByType<MonitorShopCartManager>();
    }
    public object CaptureData()
    {
        return cartManager.GetCartData(); //리스트 타입
    }         
    public void RestoreData(object data)
    {
        List<CartSaveData> loadedCart = data as List<CartSaveData>;
        if (loadedCart == null)
        {
            Debug.LogWarning("ShopCartSaveHandler: 데이터 캐스팅 실패");
            return;
        }

        cartManager.LoadCartData(loadedCart);
    }
}
