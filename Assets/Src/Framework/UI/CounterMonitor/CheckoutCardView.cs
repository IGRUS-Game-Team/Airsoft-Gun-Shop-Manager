using TMPro;
using UnityEngine;

public class CheckoutCardView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI unitText;
    [SerializeField] TextMeshProUGUI priceText;
    [SerializeField] TextMeshProUGUI totalText;

    public void Setup(ItemData itemData, int amount) // 지금은 price가 정적으로 저장된 데이터인데
                                                    // 나중에 상품 값 변경하는 기능 만들면 그거 뽑아와서 바꾸세요
    {
        nameText.text = itemData.itemName;
        unitText.text = amount.ToString();
        priceText.text = $"${itemData.baseCost:F2}";
        totalText.text = $"${itemData.baseCost * amount:F2}";
    }
}
