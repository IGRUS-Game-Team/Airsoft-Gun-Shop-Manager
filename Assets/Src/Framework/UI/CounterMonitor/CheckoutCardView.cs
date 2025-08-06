using TMPro;
using UnityEngine;

public class CheckoutCardView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI amountText;
    [SerializeField] TextMeshProUGUI totalPriceText;

    public void Setup(CartSaveData item)
    {
        nameText.text       = item.itemName;
        amountText.text     = $"x{item.amount}";
        totalPriceText.text = $"${item.unitPrice * item.amount:F2}";
    }
}
