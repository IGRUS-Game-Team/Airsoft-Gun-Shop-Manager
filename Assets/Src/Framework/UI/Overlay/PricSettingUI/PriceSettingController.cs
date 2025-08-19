using TMPro;
using UnityEngine;

/// <summary>
/// 장지원 8.3 가격 세팅 화면 전반적 로직
/// </summary>
public class PriceSettingController : MonoBehaviour, IPriceChangeable

{
    private PriceCardController priceCardController;

    [Header("텍스트 연결 요소")]
    [SerializeField] private TextMeshProUGUI productName; // 상품 이름
    [SerializeField] private TextMeshProUGUI costAVG; // 원가
    [SerializeField] private TextMeshProUGUI profit;//이익


    //so에서 가져올 요소 (임시 작성)
    string displayName; //상품 이름
    float baseCost; //원가
    int currentItemId;


    private bool isSuccess; //float 바꾸는거 성공여부
    private float price;
    private PriceObserver priceObserver;
    private PriceInputHandler priceInputHandler;

    void Awake()
    {
        priceObserver = FindFirstObjectByType<PriceObserver>();
        priceInputHandler = GetComponentInChildren<PriceInputHandler>();
    }
    void OnEnable() //데이터가 있을때만
    {
        if (!string.IsNullOrEmpty(displayName) && priceObserver != null)
        {
            SetText();
        }
    }

    //변수에 저장(so 받기)
    public void GetScriptableObject(ItemData itemData)
    {
        displayName = ItemNameResolver.Get(itemData);
        baseCost = itemData.baseCost;
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

    //텍스트에 받은 so넣기 -> 이걸 언제 시작하느냐가 관건
    private void SetText()
    {
        Debug.Log("setting받은 id : " + currentItemId);
        price = priceObserver.GetPrice(currentItemId);//또여기야?
        if (price == 0) price = baseCost; //여기

        //출력
        productName.text = displayName;
        costAVG.text = "Cost.AVG  $: " + baseCost.ToString();
        profit.text = "Profit  $: " + (price - baseCost);
    }

    //Okay버튼과 이어주기
    public void Exit()
    {
        priceInputHandler.SendFloatPrice();
        gameObject.SetActive(false);
        ClickObjectUIManager.Instance.CloseUI(this.gameObject);
        
    }

    //업데이트 되면 호출될 녀석
    public void OnPriceChanged(int itemId, float newPrice, float oldPrice)
    {
        SetText();
    }



    //input에게 보낼거임
    public int SendItemId()
    {
        return currentItemId;
    }

    public void ReceiveId(int itemDataId)
    {
        currentItemId = itemDataId;
    }
}
