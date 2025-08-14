using UnityEngine;

using TMPro;
using UnityEngine.UI;
using System;

public class CartItemCardView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI quantityText;
    [SerializeField] TextMeshProUGUI totalCost;

    [SerializeField] Button plusButton;
    [SerializeField] Button minusButton;
    [SerializeField] Button deleteButton;

    private ItemData currentItem;

    public Action<ItemData> OnPlusClicked;
    public Action<ItemData> OnMinusClicked;
    public Action<ItemData> OnDeleteClicked;

    public void Setup(ItemData data, int quantity, float cost)
    {
        currentItem = data;

        nameText.text = ItemNameResolver.Get(data);
        quantityText.text = $"x{quantity}";
        totalCost.text = cost.ToString("F2") + "$";

        plusButton.onClick.AddListener(() => OnPlusClicked?.Invoke(currentItem));
        minusButton.onClick.AddListener(() => OnMinusClicked?.Invoke(currentItem));
        deleteButton.onClick.AddListener(() => OnDeleteClicked?.Invoke(currentItem));
        
        
    }
}