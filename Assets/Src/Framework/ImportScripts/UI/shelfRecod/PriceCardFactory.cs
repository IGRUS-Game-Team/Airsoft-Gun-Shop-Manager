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
        if (priceCardWithSetting == null)
        {
            Debug.LogError("PriceCard 프리팹 또는 PriceCardSlot이 설정되지 않았습니다!");
            return;
        }
        GameObject newPriceCard = Instantiate(priceCardWithSetting, priceCardSlot);
        newPriceCard.transform.localPosition = Vector3.zero;

        //가격표의 상품 이름,값 설정
        PriceCardController priceCardCreate = newPriceCard.GetComponent<PriceCardController>();
        if (priceCardCreate)
        {
            priceCardCreate.UpdateName(itemData);
            priceCardCreate.UpdatePrice(itemData);
        }
        //가격표 세팅창 설정
        PriceSettingController priceSettingCreate = newPriceCard.GetComponentInChildren<PriceSettingController>(true);
        if (priceSettingCreate)
        {
            priceSettingCreate.GetScriptableObject(itemData);
        }
    }
}
