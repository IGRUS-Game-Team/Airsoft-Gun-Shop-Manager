using UnityEngine;

using TMPro;

public class CartItemCardView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI quantityText;

    public void Setup(ItemData data, int quantity)
    {
        nameText.text = data.displayName;
        quantityText.text = $"x{quantity}";
    }
}