using TMPro;
using UnityEngine;

/// <summary>
/// 장지원 8.3 가격 세팅 화면 전반적 로직
/// </summary>
public class PriceSettingController : MonoBehaviour, IPriceChangeable
{
    // 추가: 이 세팅창을 띄운 가격표 컨트롤러
    [SerializeField] private PriceCardController priceCardController;

    [Header("텍스트 연결 요소")]
    [SerializeField] private TextMeshProUGUI productName; // 상품 이름
    [SerializeField] private TextMeshProUGUI costAVG;      // 원가
    [SerializeField] private TextMeshProUGUI profit;       // 이익

    // so에서 가져올 요소 (임시 작성)
    string displayName; //상품 이름
    float baseCost;     //원가
    int currentItemId;

    private bool isSuccess; //float 바꾸는거 성공여부
    private float price;
    private PriceObserver priceObserver;
    private PriceInputHandler priceInputHandler;

    void Awake()
    {
        priceObserver = FindFirstObjectByType<PriceObserver>();
        priceInputHandler = GetComponentInChildren<PriceInputHandler>();

        // 인스펙터에서 안 넣어줬으면 부모에서 자동으로 찾아오기
        if (priceCardController == null)
        {
            priceCardController = GetComponentInParent<PriceCardController>();
            if (priceCardController == null)
            {
                Debug.LogWarning("[PriceSetting] 부모 PriceCardController 를 찾지 못했습니다.");
            }
        }
    }

    void OnEnable()
    {
        if (!string.IsNullOrEmpty(displayName) && priceObserver != null)
        {
            SetText();
        }
    }

    //변수에 저장(so 받기)
    public void GetScriptableObject(ItemData itemData)
    {
        displayName  = ItemNameResolver.Get(itemData);
        baseCost     = itemData.baseCost;
        currentItemId = itemData.itemId;

        if (priceObserver == null)
        {
            priceObserver = FindFirstObjectByType<PriceObserver>();
            Debug.Log("없길래 제가 만들었습니다.");
        }

        Debug.Log($"세팅창이 받은 값 {displayName} {baseCost}");

        if (gameObject.activeInHierarchy)
        {
            SetText();
        }
    }

    private void SetText()
    {
        Debug.Log("setting받은 id : " + currentItemId);
        price = priceObserver.GetPrice(currentItemId);
        if (price == 0) price = baseCost;

        productName.text = displayName;
        costAVG.text     = "Cost.AVG  $: " + baseCost.ToString();
        profit.text      = "Profit  $: " + (price - baseCost);
    }

    // Okay 버튼과 연결
    public void Exit()
    {
        priceInputHandler.SendFloatPrice();

        // 기존 닫기 로직
        gameObject.SetActive(false);
        ClickObjectUIManager.Instance.CloseUI(this.gameObject);

        // 가격표 쪽 플래그/상태도 같이 닫기
        if (priceCardController != null)
        {
            priceCardController.ClosePriceUI();
        }
        else
        {
            Debug.LogWarning("[PriceSetting] priceCardController 가 null 이라 ClosePriceUI 를 호출할 수 없습니다.");
        }
    }

    public void OnPriceChanged(int itemId, float newPrice, float oldPrice)
    {
        SetText();
    }

    public int SendItemId()
    {
        return currentItemId;
    }

    public void ReceiveId(int itemDataId)
    {
        currentItemId = itemDataId;
    }
}