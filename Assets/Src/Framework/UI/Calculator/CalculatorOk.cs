using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 장지원 8.3 Ok 계산기 버튼
/// 
/// OK 를 누르면 계산이 완료
/// CalculatorManager에 입력한 결괏값을 보내기
/// </summary>
public class CalculatorOk : MonoBehaviour
{
    static string paymentAmount; //...에서 결제할 금액 받아오기
    ResultValue resultScript; //ResultValue에서 입력값 받아오기
    Button OkButton;

    void Start()
    {
        OkButton = GetComponent<Button>();
        OkButton.onClick.AddListener(OnButtonClick);
        //버튼 클릭시 자동 onClick 이벤트 메서드 호출

        resultScript = FindObjectOfType<ResultValue>();

    }


    //비교 메서드
    private void OnButtonClick()
    {   

        //Amount : 비교할 string값  Script : 스크립트
        string resultAmount = resultScript.CurrentValue;
        //paymentAmonut도 받아오기
        
        if (paymentAmount != resultAmount)
        {
            PaymentManager.PaymentFailed();
        }
        else
        {
            PaymentManager.PaymentSuccess();
        }


    }
}
