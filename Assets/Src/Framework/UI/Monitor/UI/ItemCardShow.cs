using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ItemCardView : MonoBehaviour
{
    // [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI unitPriceText;
    [SerializeField] TextMeshProUGUI amountText;
    [SerializeField] TextMeshProUGUI totalText;
    [SerializeField] Button plusBtn, minusBtn, addBtn;

    private ItemData data;
    private int amount = 0;
    public UnityEvent<ItemData, int> onAddToCart;

    void Awake()
    {        
        onAddToCart.AddListener(Test_OnSpacePressed);
    }
    public void Setup(ItemData item)
    {
        data = item;
        //  icon.sprite = data.icon;
        nameText.text = data.displayName;
        unitPriceText.text = $"${data.baseCost:F2}";
        UpdateAmount(amount);

        plusBtn.onClick.AddListener(() => ChangeAmount(1));
        minusBtn.onClick.AddListener(() => ChangeAmount(-1));
        //addBtn.onClick.AddListener(() => onAddToCart?.Invoke(this, new AddToCartEventArgs(data, amount)));
        addBtn.onClick.AddListener(() => onAddToCart?.Invoke(data, amount));
    }

    void ChangeAmount(int delta)
    {
        amount = Mathf.Clamp(amount + delta, 0, 999);
        UpdateAmount(amount);
    }

    void UpdateAmount(int a)
    {
        amountText.text = a.ToString();
        totalText.text = $"${(data.baseCost * a):F2}";
    }

    private void Test_OnSpacePressed(ItemData itemData, int a)
    {
        UpdateAmount(0);
        amount = 0;
    }
}
