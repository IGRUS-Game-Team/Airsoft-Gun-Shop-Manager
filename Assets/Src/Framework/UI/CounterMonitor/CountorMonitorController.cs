using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountorMonitorController : MonoBehaviour
{
    [SerializeField] Transform contentRoot;           // ScrollView의 Content 오브젝트
    [SerializeField] CheckoutCardView cardPrefab;      // 위 프리팹
    [SerializeField] TextMeshProUGUI totalPriceText;
    [SerializeField] CalculatorOk calculatorOk;

    private float total = 0f;
public void Show(CounterSlotData items)
{
    if (items == null || items.itemData == null) return;

    var data = items.itemData;
    var amount = items.amount;
    var unit = data.baseCost;

    // 기존 카드 있는지 찾기
    CheckoutCardView existingCard = null;
    foreach (Transform child in contentRoot)
    {
        var cardView = child.GetComponent<CheckoutCardView>();
        if (cardView != null && cardView.ItemData == data)
        {
            existingCard = cardView;
            break;
        }
    }

    if (existingCard != null)
    {
        // 기존 수량 증가
        existingCard.AddAmount(amount);
    }
    else
    {
        // 새 카드 생성
        var card = Instantiate(cardPrefab, contentRoot);
        card.Setup(data, amount);
    }

    // 전체 합계 갱신
    float total = 0f;
    foreach (Transform child in contentRoot)
    {
        var cardView = child.GetComponent<CheckoutCardView>();
        if (cardView != null)
        {
            total += cardView.TotalPrice;
        }
    }

    totalPriceText.text = $"${total:F2}";
    calculatorOk.SetTotalPrice(total);
}

    public void Clear()
    {
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        total = 0f;
        totalPriceText.text = "$0.00";
    }

}
