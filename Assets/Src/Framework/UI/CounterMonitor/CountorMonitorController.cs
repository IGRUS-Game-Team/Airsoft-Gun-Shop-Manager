using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountorMonitorController : MonoBehaviour
{
    [SerializeField] Transform contentRoot;           // ScrollView의 Content 오브젝트
    [SerializeField] CheckoutCardView cardPrefab;      // 위 프리팹
    [SerializeField] TextMeshProUGUI totalPriceText;
    [SerializeField] CalculatorOk calculatorOk;

    [Header("PriceObserver")]
    [SerializeField] PriceObserver priceObserver; // 가격 옵저버 

    private float total = 0f;
public void Show(CounterSlotData items)//items = 게임 오브젝트, itemdata(원가, id), 수량
{
    if (items == null || items.itemData == null) return;

    var data = items.itemData;
    var amount = items.amount;
    var unit = data.baseCost;//unit = 원가 

    // 기존 카드 있는지 찾기
    CheckoutCardView existingCard = null;
    foreach (Transform child in contentRoot)
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
            card.Setup(data, amount, priceObserver); //옵저버 추가
            //새 카드 생성시 priceobserver에게 참조 전달
    }

    // 전체 합계 갱신
    float total = 0f;
    foreach (Transform child in contentRoot)
    {
        var cardView = child.GetComponent<CheckoutCardView>();
        if (cardView != null)
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
    
    public float GetCurrentTotalAmount()
{
    float sum = 0f;

    foreach (Transform child in contentRoot)
    {
        var cardView = child.GetComponent<CheckoutCardView>();
        if (cardView != null)
        {
            sum += cardView.TotalPrice; // 각 카드의 총 가격 합산
        }
    }

    return sum;
}

}
