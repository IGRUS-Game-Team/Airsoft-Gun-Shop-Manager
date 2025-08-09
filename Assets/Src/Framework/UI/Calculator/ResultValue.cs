using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 장지원 8.3 계산기 결괏값 화면
/// 버튼이 입력, 삭제하는 이벤트를 주면 그에 맞춰 text를 변경해준다.
/// 추후 책임 분산을 위한 리펙토링 예정
/// </summary>

public class ResultValue : MonoBehaviour
{
    public static event Action<string> OnInputValueChanged;
    private string currentValue = "0"; // 현재 result값을 string으로 관리
    TextMeshProUGUI resultText; // 계산기 결과값 text
    private bool hasDot = false; // 소수점 존재 여부
    private bool isInitialState = true; // 현재 아무것도 입력하지 않은 상태인지 여부


    //프로퍼티 currentValue get
    public string CurrentValue => currentValue; 


    void Start()
    {
        resultText = GetComponentInChildren<TextMeshProUGUI>();
        UpdateDisplay();
    }

    void OnEnable() {
        CalculatorButton.OnCalculatorInput += OnCalculatorInput;
        CalculatorClear.ClearDisplay += ClearDisplay;
    }
    void OnDisable() {
        CalculatorButton.OnCalculatorInput -= OnCalculatorInput;
        CalculatorClear.ClearDisplay -= ClearDisplay;
    } 

    // 숫자버튼, 소수점 버튼 눌렀을 때 호출되는 이벤트
    public void OnCalculatorInput(string buttonText) //호출한 버튼의 기호
    {

        if (IsValidInput(buttonText)==false) return;//숫자 입력 제약사항
        ProcessButton(buttonText);//버튼 상호작용
        UpdateDisplay(); //화면 갱신

    }


    // 숫자 입력시 제약사항
    private bool IsValidInput(string buttonText)
    {
        Debug.Log("검수 시작");
        if (isInitialState && buttonText == "0")//0 시작 불가
        {
            return false;
        }
        
        if (buttonText == ".")//소수점 제약
        {
            if (isInitialState)//소수점 시작 불가
            {
                Debug.Log("소수점 시작");
                return false;
            }

            if (hasDot) //이미 소수점 존재x
            {
                Debug.Log("이미 소수점 존재");
                return false; 
            }
            
        }
        //문제없으면
        return true;
    }

    //숫자, 소수점 버튼을 눌렀을 때
    //숫자, 소수점 버튼을 눌렀을 때
private void ProcessButton(string buttonText)
{
    Debug.Log("버튼 클릭 시");
    
    if (buttonText == "·")//소수점 표시를 .으로 변환
    {
        buttonText = ".";
    }
    
    if (isInitialState) //초기 상태 -> 변경 상태
    {
        currentValue = buttonText;
        isInitialState = false;
    }
    else
    {
        currentValue += buttonText;
    }
    
    // 소수점이 추가된 경우에만 hasDot을 true로 설정
    if (buttonText == ".")
    {
        hasDot = true;
        Debug.Log(". 입력 및 hasDot true");
    }
}


    //result화면에 업데이트
    private void UpdateDisplay()
    {
        resultText.text = currentValue;
        Debug.Log($"화면 업데이트 및 이벤트 발생: '{currentValue}'");
        OnInputValueChanged?.Invoke(currentValue);
    } 


    // Clear 버튼이 눌렸을 때 호출되는 이벤트
    public void ClearDisplay()
    {
        Debug.Log("clear 호출");
        currentValue = "0"; //result 리스트 초기화
        hasDot = false; //소수점 없음
        isInitialState = true; // 숫자 입력없는 초기 상태
        UpdateDisplay();// 화면 업데이트
    }

    
}