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
        Clear();

        float total = 0f;

        var card = Instantiate(cardPrefab, contentRoot);
        card.Setup(items.itemData, items.amount);
        total += items.itemData.baseCost * items.amount;

        totalPriceText.text = $"${total:F2}";

        calculatorOk.SetTotalPrice(total);
        //일단 하나 받아옴
        //준서님이 여러개 받는걸로 로직 고치면
        //FIXME : 로직 여러개 받은 후에 그 값들(total) 다 더해서 한번에 settotalprice하는걸로 로직 변경해야함.
    }

    public void Clear()
    {
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        totalPriceText.text = "$0.00";
    }
}
