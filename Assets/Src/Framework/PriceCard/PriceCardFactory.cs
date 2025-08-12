using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 가격표 생성
/// so 데이터 받아서 가격표에게 넘기기 
/// </summary>
public class PriceCardFactory : MonoBehaviour
{
    public static PriceCardFactory Instance { get; private set; }
    [Header("가격표 프리팹 & 위치 슬롯")]
    [SerializeField] GameObject priceCardWithSetting;

    

    [Header("PriceObserver")]
    [SerializeField] private PriceObserver priceObserver;


    // 현재 생성된 가격표 추적 (딕셔너리로 변경)
    private Dictionary<Vector3, GameObject> priceCardDictionary = new Dictionary<Vector3, GameObject>();


    void Awake()
    {
        // 만약 다른 PriceCardFactory 인스턴스가 이미 존재한다면
        if (Instance != null && Instance != this)
        {
            // 이 오브젝트는 파괴하고 로직을 중단
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }
    void OnEnable()
    {
        // 람다식이 아닌, 메소드 이름을 직접 등록합니다.
        ShelfSlot.OnProductPlacedToFactory += SendItemData;
    }

    void OnDisable()
    {
        // 등록했던 메소드 이름으로 정확하게 해지합니다.
        ShelfSlot.OnProductPlacedToFactory -= SendItemData;
    }

    //so 데이터 가격표에게 전달하기
    void SendItemData(ItemData itemData, Vector3 priceCardPosition, Transform parentTransform)
    {
        CreatePriceCard(itemData, priceCardPosition, parentTransform);
    }

    //가격표 생성하기
    private void CreatePriceCard(ItemData itemData,Vector3 position, Transform parent)
    {
        if (priceCardWithSetting == null)
        {
            Debug.LogError("PriceCard 프리팹 또는 PriceCardSlot이 설정되지 않았습니다!");
            return;
        }

        // 이 위치에 이미 가격표 있는지 확인하고 있다면 제거
        if (priceCardDictionary.ContainsKey(position))
        {
            Destroy(priceCardDictionary[position]);
            priceCardDictionary.Remove(position);
        }


        // 새 가격표 생성
        Quaternion rotation = Quaternion.Euler(0, 0, 0);
        GameObject newPriceCard = Instantiate(priceCardWithSetting, position, rotation, parent);
        priceCardDictionary.Add(position, newPriceCard);



        // 가격표 및 세팅창 item데이터 전송
        PriceCardController priceCardController = newPriceCard.GetComponent<PriceCardController>();
        
        if (priceCardController)
        {
            priceCardController.UpdateName(itemData);
            priceCardController.UpdatePrice(itemData);
            priceObserver.Subscribe(itemData.itemId, priceCardController);//옵저버 구독
        }

        PriceSettingController priceSettingController = newPriceCard.GetComponentInChildren<PriceSettingController>(true);
        if (priceSettingController)
        {
            priceSettingController.GetScriptableObject(itemData);
            priceObserver.Subscribe(itemData.itemId, priceSettingController);//옵저버 구독
        }
        
        
    }
}
