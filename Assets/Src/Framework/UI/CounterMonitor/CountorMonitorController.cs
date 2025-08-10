using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountorMonitorController : MonoBehaviour
{
    [SerializeField] Transform contentRoot;           // ScrollView의 Content 오브젝트
    [SerializeField] CheckoutCardView cardPrefab;      // 위 프리팹

    [SerializeField] TextMeshProUGUI totalPriceText;

    public void Show(CounterSlotData items)
    {
        Clear();

        float total = 0f;

        var card = Instantiate(cardPrefab, contentRoot);
        card.Setup(items.itemData, items.amount);
        total += items.itemData.baseCost * items.amount;
        
        totalPriceText.text = $"${total:F2}";
    }

    public void Clear()
    {
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        totalPriceText.text = "$0.00";
    }
}
