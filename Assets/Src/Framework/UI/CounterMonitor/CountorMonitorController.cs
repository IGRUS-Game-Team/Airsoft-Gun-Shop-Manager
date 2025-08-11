using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountorMonitorController : MonoBehaviour
{
    [SerializeField] Transform contentRoot;           // ScrollView의 Content 오브젝트
    [SerializeField] CheckoutCardView cardPrefab;      // 위 프리팹
    [SerializeField] TextMeshProUGUI totalPriceText;
    [SerializeField] CalculatorOk calculatorOk;

public void Show(CounterSlotData items)
{
    // 0) null 검사
    if (items == null)
    {
        Debug.LogError("[Show] items == null");
        return;
    }
    if (items.itemData == null)
    {
        Debug.LogError("[Show] items.itemData == null");
        return;
    }
    if (cardPrefab == null)
    {
        Debug.LogError("[Show] cardPrefab not assigned");
        return;
    }
    if (contentRoot == null)
    {
        Debug.LogError("[Show] contentRoot not assigned");
        return;
    }
    if (totalPriceText == null)
    {
        Debug.LogError("[Show] totalPriceText not assigned");
        return;
    }
    if (calculatorOk == null)
    {
        Debug.LogError("[Show] calculatorOk not assigned");
        return;
    }

    // 1) 안전하게 값 복사 (코루틴 중 원본 파괴 방지)
    var data   = items.itemData;
    var amount = items.amount;
    var unit   = data.baseCost;

    // 2) UI 갱신
    Clear();

    float total = unit * amount;

    var card = Instantiate(cardPrefab, contentRoot);
    card.Setup(data, amount);

    totalPriceText.text = $"${total:F2}";
    calculatorOk.SetTotalPrice(total);
}


    public void Clear()
    {
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        totalPriceText.text = "$0.00";
    }
}
