using TMPro;
using UnityEngine;

public class CheckoutCardView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI unitText;
    [SerializeField] TextMeshProUGUI priceText;
    [SerializeField] TextMeshProUGUI totalText;

    public ItemData ItemData { get; private set; }
    public float TotalPrice => baseCost * currentAmount;

    private int currentAmount;
    private float baseCost;

    public void Setup(ItemData itemData, int amount)
    {
        ItemData = itemData;
        currentAmount = amount;
        baseCost = itemData.baseCost;
        RefreshUI();
    }

    public void AddAmount(int amount)
    {
        currentAmount += amount;
        RefreshUI();
    }

    private void RefreshUI()
    {
        nameText.text = ItemData.itemName;
        unitText.text = currentAmount.ToString();
        priceText.text = $"${baseCost:F2}";
        totalText.text = $"${baseCost * currentAmount:F2}";
    }
}