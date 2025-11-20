using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// 장지원 8.7 가격표 컨트롤러
/// 가격표 클릭시 UI 호출
/// </summary>
public class PriceCardController : MonoBehaviour, IInteractable, IPriceChangeable
{
    // 어떤 가격표 UI라도 열려 있으면 true
    public static bool IsAnyPriceUIOpen { get; private set; }

    [Header("가격 세팅창")]
    [SerializeField] GameObject setting;

    [Header("텍스트 요소들")]
    [SerializeField] TextMeshProUGUI price;
    [SerializeField] TextMeshProUGUI productname;
    [SerializeField] TextMeshProUGUI left;
    private ItemData currentItemData;
    private PriceObserver priceObserver;
    private PriceSettingController priceSettingController;
    private ShelfSlot shelfSlot;

    void Start()
    {
        shelfSlot = GetComponentInParent<ShelfSlot>();
        priceObserver = FindFirstObjectByType<PriceObserver>();
        priceSettingController = GetComponentInChildren<PriceSettingController>(true);
        
    }
    public void Interact()
    {
        // 0) RaycastDetector 방어
        if (RaycastDetector.Instance == null)
        {
            Debug.LogWarning("[PriceCard] RaycastDetector.Instance 가 null");
            return;
        }

        // 이 카드에 레이가 안 맞았으면 무시
        if (RaycastDetector.Instance.HitObject != this.gameObject)
            return;

        // 1) ShelfSlot 다시 한 번 안전하게 가져오기
        if (shelfSlot == null)
        {
            shelfSlot = GetComponentInParent<ShelfSlot>();
            if (shelfSlot == null)
            {
                Debug.LogWarning("[PriceCard] 상위에서 ShelfSlot 을 찾지 못했습니다. 이 가격표 프리팹이 ShelfSlot 자식에 있는지 확인하세요.");
                return;
            }
        }

        // 슬롯에 아이템이 없으면 가격표 삭제
        if (!shelfSlot.HasItem)
        {
            Debug.Log("[PriceCard] 슬롯에 아이템이 없어 가격표 제거");
            Destroy(this.gameObject);
            return;
        }

        // 2) ItemData 세팅 여부 확인
        if (currentItemData == null)
        {
            Debug.LogWarning("[PriceCard] currentItemData 가 null 입니다. PriceCardFactory 에서 UpdatePrice(ItemData)를 호출하고 있는지, 그리고 그 안에서 currentItemData를 세팅하는지 확인하세요.");
            return;
        }

        // 3) PriceSettingController 체크
        if (priceSettingController == null)
        {
            priceSettingController = GetComponentInChildren<PriceSettingController>(true);
            if (priceSettingController == null)
            {
                Debug.LogWarning("[PriceCard] PriceSettingController 를 찾지 못했습니다. 가격표 프리팹 자식에 이 컴포넌트가 붙어 있는지 확인하세요.");
                return;
            }
        }

        // 4) setting GameObject 검사
        if (setting == null)
        {
            Debug.LogWarning("[PriceCard] setting GameObject 가 null 입니다. PriceCardController 인스펙터에서 setting 참조를 할당하세요.");
            return;
        }

        // 5) UI 매니저 검사
        if (ClickObjectUIManager.Instance == null)
        {
            Debug.LogWarning("[PriceCard] ClickObjectUIManager.Instance 가 null 입니다. 씬에 ClickObjectUIManager 가 있는지 확인하세요.");
            return;
        }

        // 여기까지 오면 모든 참조가 유효
        Debug.Log($"[PriceCard] 가격 설정 UI 오픈. itemId={currentItemData.itemId}");

        priceSettingController.ReceiveId(currentItemData.itemId);

        setting.SetActive(true);
        ClickObjectUIManager.Instance.OpenUI(setting);

        // 가격 수정 모드 ON
        IsAnyPriceUIOpen = true;

        priceSettingController.GetScriptableObject(currentItemData);
    }

    //사람이 input변경하면 가격이 변한다
    //Okay버튼과 이어주기
    public void UpdatePrice()
    {
        Debug.Log("현재 옵저버 price " + priceObserver.GetPrice(currentItemData.itemId).ToString());
        string priceString = priceObserver.GetPrice(currentItemData.itemId).ToString();
        //옵저버 값 가져오기
        price.text = "$  " + (priceString == "" ? "0.00" : priceString);
    }

    public void UpdatePrice(ItemData itemData)//팩토리랑 연결
    {
        price.text = "$  " + itemData.baseCost.ToString();
        currentItemData = itemData;
        Debug.Log("updatePrice : "+currentItemData.itemId);
    }
    public void UpdateName(ItemData itemData)
    {
        productname.text = ItemNameResolver.Get(itemData);
    }


    //옵저버에서 값 받음
    public void OnPriceChanged(int itemId, float newPrice, float oldPrice)
    {
        price.text = "$  " + newPrice.ToString();
    }

    // OK / 닫기 버튼에서 호출할 함수
    public void ClosePriceUI()
    {
        if (setting != null)
            setting.SetActive(false);

        Debug.Log("잘 닫아짐");
        IsAnyPriceUIOpen = false;
    }
    
}
