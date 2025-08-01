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



    // public class AddToCartEventArgs : EventArgs //커스텀 이벤트 아규먼트 정의
    // {
    //     public ItemData itemData;
    //     public int amount;

    //     public AddToCartEventArgs(ItemData itemData, int amount)
    //     {
    //         this.itemData = itemData;
    //         this.amount = amount;
    //     }
    // }
    //public event EventHandler<AddToCartEventArgs> onAddToCart;
    //public Action<ItemData, int> onAddToCart;
    public UnityEvent<ItemData, int> onAddToCart;

    void Awake()
    {
        //onAddToCart += Test_OnSpacePressed; <-  잘못된 방식
        
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
        Debug.Log("구독자의 스페이스바!");
    }
}
