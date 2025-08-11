using UnityEngine;
/// <summary>
/// 가격표 생성
/// so 데이터 받아서 가격표에게 넘기기 
/// </summary>
public class PriceCardFactory : MonoBehaviour
{
    [Header("가격표 프리팹 & 위치 슬롯")]
    [SerializeField] GameObject priceCardWithSetting;
    [SerializeField] Transform priceCardSlot;

    [Header("PriceObserver")]
    [SerializeField] private PriceObserver priceObserver;

    void Start()
    {
        ShelfSlot.OnProductPlacedToFactory += SendItemData;
    }
    void OnDestroy()
    {
        ShelfSlot.OnProductPlacedToFactory -= SendItemData;
    }

    //so 데이터 가격표에게 전달하기
    void SendItemData(ItemData itemData)
    {
        CreatePriceCard(itemData);
        //세팅창에도,,,!
    }

    //가격표 생성하기
    private void CreatePriceCard(ItemData itemData)
    {
        if (priceCardWithSetting)
        {
            Debug.LogError("PriceCard 프리팹 또는 PriceCardSlot이 설정되지 않았습니다!");
            return;
        }
        GameObject newPriceCard = Instantiate(priceCardWithSetting, priceCardSlot);
        newPriceCard.transform.localPosition = Vector3.zero;


    }

}
