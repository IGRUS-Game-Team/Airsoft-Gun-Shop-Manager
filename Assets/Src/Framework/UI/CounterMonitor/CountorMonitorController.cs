using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountorMonitorController : MonoBehaviour
{
    [SerializeField] Transform contentRoot;           // ScrollView의 Content 오브젝트
    [SerializeField] CheckoutCardView cardPrefab;      // 위 프리팹

    [SerializeField] TextMeshProUGUI totalPriceText;

    public void Show(List<CounterSlotData> items)
    {
        Clear();

        float total = 0f;

        foreach (var slot in items)
        {
            var card = Instantiate(cardPrefab, contentRoot);
            card.Setup(slot.itemData, slot.amount);
            total += slot.itemData.baseCost * slot.amount;
        }

        totalPriceText.text = $"${total:F2}";
    }

    public void Clear()
    {
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        totalPriceText.text = "$0.00";
    }
}
