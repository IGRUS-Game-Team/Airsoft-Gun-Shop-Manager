using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 가격표 생성
/// SO 데이터 받아서 가격표에게 넘기기 
/// </summary>
public class PriceCardFactory : MonoBehaviour
{
    public static PriceCardFactory Instance { get; private set; }

    [Header("가격표 프리팹 & 위치 슬롯")]
    [SerializeField] GameObject priceCardWithSetting;
    
    // 현재 생성된 가격표 추적 (딕셔너리로 유지)
    private Dictionary<Vector3, GameObject> priceCardDictionary = new Dictionary<Vector3, GameObject>();


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void OnEnable()
    {
        // 시그니처 변경: Quaternion rotation 추가됨
        ShelfSlot.OnProductPlacedToFactory += SendItemData;
    }

    void OnDisable()
    {
        ShelfSlot.OnProductPlacedToFactory -= SendItemData;
    }

    // 슬롯에서 위치 + 회전 + 부모까지 받아옴
    void SendItemData(ItemData itemData, Vector3 priceCardPosition, Quaternion rotation, Transform parentTransform)
    {
        CreatePriceCard(itemData, priceCardPosition, rotation, parentTransform);
    }

    // 가격표 생성하기
    private void CreatePriceCard(ItemData itemData, Vector3 position, Quaternion rotation, Transform parent)
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

        // 새 가격표 생성 – 슬롯에서 넘겨준 회전 그대로 사용
        GameObject newPriceCard = Instantiate(priceCardWithSetting, position, rotation, parent);
        priceCardDictionary.Add(position, newPriceCard);

        // 가격표 및 세팅창 item데이터 전송
        PriceCardController priceCardController = newPriceCard.GetComponent<PriceCardController>();
        
        if (priceCardController)
        {
            priceCardController.UpdateName(itemData);
            priceCardController.UpdatePrice(itemData);
            PriceObserver.Instance.Subscribe(itemData.itemId, priceCardController); // 옵저버 구독
        }

        PriceSettingController priceSettingController = newPriceCard.GetComponentInChildren<PriceSettingController>(true);
        if (priceSettingController)
        {
            priceSettingController.GetScriptableObject(itemData);
            PriceObserver.Instance.Subscribe(itemData.itemId, priceSettingController); // 옵저버 구독
        }
    }
}