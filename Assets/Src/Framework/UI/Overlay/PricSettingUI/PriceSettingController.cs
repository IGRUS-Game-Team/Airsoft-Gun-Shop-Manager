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
    string displayName = "pistol"; //상품 이름
    float baseCost = 10f; //원가 


    private bool isSuccess; //float 바꾸는거 성공여부
    private float price;


//아근데 스크립터블 오브젝트 가져오는 컨트롤러는 따로 있어야할듯?
    private void GetScriptableObject()
    {
        //so에서 값 가져와서 이름, 원가 변수에 넣기
    }

    //텍스트에 받은 so넣기
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
