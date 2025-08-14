using TMPro;
using UnityEngine;

public class CheckoutCardView : MonoBehaviour,IPriceChangeable
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI unitText;
    [SerializeField] TextMeshProUGUI priceText;
    [SerializeField] TextMeshProUGUI totalText;

    public ItemData ItemData { get; private set; }
    public float TotalPrice => currentPrice * currentAmount; //총 금액
    private float currentPrice; //판매액<= 신호가 오면 얘가 바뀐다
    private int currentAmount; //현재 수량
    private float baseCost; //원가


    //처음 상품 바코드 찍을 때 가격 세팅
    public void Setup(ItemData itemData, int amount)
    {
        ItemData = itemData;
        currentAmount = amount;
        baseCost = itemData.baseCost;

        PriceObserver.Instance.Subscribe(itemData.itemId, this);

        currentPrice = PriceObserver.Instance.GetPrice(itemData.itemId);//현재 가격 저장
        Debug.Log($"아이템 {itemData.itemName} (ID: {itemData.itemId}) 초기 가격: {currentPrice}, 원가: {baseCost}");


        RefreshUI();
    }

    public void AddAmount(int amount)
    {
        currentAmount += amount;
        RefreshUI();
    }

    private void RefreshUI()
    {
        nameText.text = ItemNameResolver.Get(ItemData);
        unitText.text = currentAmount.ToString();
        priceText.text = $"${currentPrice:F2}";
        totalText.text = $"${currentPrice * currentAmount:F2}";
    }

    //input창에 의해 값이 변경되면 호출되는 메서드
    public void OnPriceChanged(int itemId, float newPrice, float oldPrice)
    {
        
        if (itemId == ItemData.itemId)
        {
            Debug.Log("변경된 price 모니터에 적용준비");
            currentPrice = newPrice; // 새 가격으로 변경
            RefreshUI(); // 화면에 반영
        }
    }
}