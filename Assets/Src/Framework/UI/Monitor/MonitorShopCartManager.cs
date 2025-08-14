using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MonitorShopCartManager : MonoBehaviour
{
    [SerializeField] GameObject shoppingCart;
    [SerializeField] Transform cartContentRoot;
    [SerializeField] CartItemCardView cartItemCardPrefab;
    [SerializeField] TextMeshProUGUI totalAmountText;
    [SerializeField] TextMeshProUGUI totalValueText;
    [SerializeField] private ItemDatabase itemDatabase;

    private Dictionary<ItemData, int> cart = new(); // ItemData → 수량
    private Dictionary<ItemData, CartItemCardView> spawnedViews = new();
    private float totalValue = 0;
    private float totalAmount = 0;
    public void Start()
    {
        ShowShoppingCart(false);
    }
    public void ShowShoppingCart(bool active)
    {
        if (shoppingCart != null)
            shoppingCart.SetActive(active);
        else
            Debug.LogWarning("ShoppingCart가 연결되어 있지 않음!");
    }

    public void AddItem(ItemData item, int amount)
    {
        if (amount <= 0) return;

        if (cart.ContainsKey(item))
            cart[item] += amount;
        else
            cart[item] = amount;

        Debug.Log($"장바구니에 추가됨: {item.itemName} x{amount} (총 수량: {cart[item]})");

        UpdateCartUI();
    }

    public void ClearCart()
    {
        cart.Clear();
        UpdateCartUI();
        Debug.Log("장바구니 비움");
    }

    private void UpdateCartUI()
    {
        foreach (Transform child in cartContentRoot)
            Destroy(child.gameObject);

        spawnedViews.Clear();

        // 누적값 초기화
        totalAmount = 0;
        totalValue = 0;

        foreach (var kvp in cart)
        {
            var view = Instantiate(cartItemCardPrefab, cartContentRoot);
            float partTotalCost = kvp.Key.baseCost * kvp.Value;
            view.Setup(kvp.Key, kvp.Value, partTotalCost);

            view.OnPlusClicked = OnPlusClicked;
            view.OnMinusClicked = OnMinusClicked;
            view.OnDeleteClicked = OnDeleteClicked;

            spawnedViews[kvp.Key] = view;

            totalAmount += kvp.Value;
            totalValue += kvp.Key.baseCost * kvp.Value; // 총 가격 계산
        }

        totalAmountText.text = totalAmount.ToString();
        totalValueText.text = $"${totalValue:F2}";
    }

    public List<CartSaveData> GetCartData()
    {
        List<CartSaveData> list = new List<CartSaveData>();

        foreach (var kvp in cart)
        {
            var data = new CartSaveData
            {
                itemId = kvp.Key.itemId,
                amount = kvp.Value,
                unitPrice = kvp.Key.baseCost,
                itemName = ItemNameResolver.Get(kvp.Key),
                category = kvp.Key.category
            };

            list.Add(data);
        }

        return list;
    }
    public void LoadCartData(List<CartSaveData> cartSaveDatas)
    {
        cart.Clear();

        foreach (var saveData in cartSaveDatas)
        {
            var item = itemDatabase.items.Find(i => i.itemId == saveData.itemId);
            if (item != null)
                cart[item] = saveData.amount;
            else
                Debug.LogWarning($"itemId {saveData.itemId} 없음");
        }

        UpdateCartUI();
    }

    private void OnPlusClicked(ItemData item)
    {
        if (!cart.ContainsKey(item)) return;
        if (cart[item] >= 99) return;
        cart[item]++;

        UpdateCartUI();
    }

    private void OnMinusClicked(ItemData item)
    {
        if (!cart.ContainsKey(item)) return;

        cart[item]--;
        if (cart[item] <= 0)
            cart.Remove(item);

        UpdateCartUI();
    }

    private void OnDeleteClicked(ItemData item)
    {
        if (!cart.ContainsKey(item)) return;

        cart.Remove(item);
        UpdateCartUI();
    }
}
