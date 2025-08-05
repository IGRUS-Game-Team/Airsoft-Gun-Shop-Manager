using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 장지원 8.3 계산기 결괏값 화면
/// 버튼이 입력, 삭제하는 이벤트를 주면 그에 맞춰 text를 변경해준다.
/// </summary>

public class ResultValue : MonoBehaviour
{
    private string currentValue = "0"; // 현재 result값을 string으로 관리
    private TextMeshProUGUI resultText; // 계산기 결과값 text
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

        if (!IsValidInput(buttonText)) return;//숫자 입력 제약사항
        ProcessButton(buttonText);//버튼 상호작용
        UpdateDisplay(); //화면 갱신

    }


    // 숫자 입력시 제약사항
    private bool IsValidInput(string buttonText)
    {
        if (isInitialState && buttonText == "0") return false; // 0이 처음에 오면 안됨
        if (buttonText == "·")
        {
            if (isInitialState) return false; // 맨앞 소수점 불가
            if (hasDot) return false; // 이미 소수점 존재한다면 불가
        }
        //문제없으면
        return true;
    }

    //숫자, 소수점 버튼을 눌렀을 때
    private void ProcessButton(string buttonText)
    {
        if (isInitialState) //초기 상태 -> 변경 상태
        {
            currentValue = buttonText;
            isInitialState = false;
        }
        else { currentValue += buttonText; } //string + string = string?

        if (buttonText == "·") hasDot = true; //소수점 입력시 bool 수정
    }


    //result화면에 업데이트
    private void UpdateDisplay() => resultText.text = currentValue;


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

