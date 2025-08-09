using System;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 장지원 8.5 Ok 계산기 버튼
/// 
/// OK 를 누르면 계산이 완료
/// CalculatorManager에 입력한 결괏값을 보내기
/// </summary>
public class CalculatorOk : MonoBehaviour
{
    Button OkButton;
    public static event Action FailedCompare; //에러 메세지 이벤트 : 비교 실패
    public static event Action SuccessCompare; //다음 장면 이벤트 : 비교 성공

    private string totalPrice = "5";
    private string inputPrice = "0";

    //읽기 전용 프로퍼티
    public string TotalPrice => totalPrice;// 계산값
    public string InputPrice => inputPrice;// 입력값

    //set 함수 
    public void SetTotalPrice(string newPrice)
    {
        totalPrice = newPrice;
    }

    //set 함수 
    public void SetInputPrice(string newPrice)
    {
        Debug.Log("값 받기"); //왜 나오는거냐고
        inputPrice = newPrice;
    }


    void Start()
    {
        OkButton = GetComponent<Button>();
        OkButton.onClick.AddListener(ValueComparsion);
        //뭔가 클릭 한번만 되고 재사용이 안되는것같음
        //버튼 클릭시 자동 onClick 이벤트 메서드 호출
        Debug.Log($"초기 정답값: {totalPrice}");
    }

    void OnEnable()
    {
        ResultValue.OnInputValueChanged += SetInputPrice;
    }

    void OnDisable()
    {
        ResultValue.OnInputValueChanged -= SetInputPrice;
    }



    //비교 메서드
    public void ValueComparsion()
    {
        Debug.Log("비교 성공");
        if (totalPrice == inputPrice)
        {
            Debug.Log("맞았어");
            SuccessCompare?.Invoke(); // 다음 메커니즘을 위한 트리거
        }
        else
        {
            Debug.Log("틀렸어");
            FailedCompare?.Invoke(); // errorUI 발생
            //왜 이벤트가 실행이 안되는거지?
        }
    }



}