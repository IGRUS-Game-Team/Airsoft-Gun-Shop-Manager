using TMPro;
using UnityEngine;

/// <summary>
/// 장지원 8.3 가격 세팅 화면 전반적 로직
/// </summary>
public class PriceSettingController : MonoBehaviour
{
    private PriceCardController priceCardController;

    [Header("텍스트 연결 요소")]
    [SerializeField] private TextMeshProUGUI productName; // 상품 이름
    [SerializeField] private TextMeshProUGUI costAVG; // 원가
    [SerializeField] private TextMeshProUGUI profit;//이익


    //so에서 가져올 요소 (임시 작성)
    string displayName; //상품 이름
    float baseCost; //원가


    private bool isSuccess; //float 바꾸는거 성공여부
    private float price;

    void Start()
    {
        SetText();   
    }
    void OnEnable() //활성화 될때마다 갱신 어때?
    {
        SetText();        
    }

    //변수에 저장(so 받기)
    public void GetScriptableObject(ItemData itemData)
    {
        displayName = itemData.name;
        baseCost = itemData.baseCost;
        Debug.Log($"세팅창이 받은 값 {displayName} {baseCost}");
    }

    //텍스트에 받은 so넣기 -> 이걸 언제 시작하느냐가 관건
    private void SetText()
    {
        // 이익 계산
        isSuccess = float.TryParse(PriceInputHandler.Instance.SendStirngPrice(), out price);

        //출력
        productName.text = displayName;
        costAVG.text = "Cost.AVG  $: " + baseCost.ToString();
        profit.text = "Profit  $: " + (price - baseCost);
    }

    //Okay버튼과 이어주기
    public void Exit()
    {
        gameObject.SetActive(false);
        ClickObjectUIManager.Instance.CloseUI(this.gameObject);
    }
}
