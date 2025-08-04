using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 장지원 8.3 계산기 구현
/// 숫자 버튼 및 소수점 버튼에 붙일 스크립트 - ok,c를 제외한 모든 버튼이 공유한다
/// </summary>

public class CalculatorButton : MonoBehaviour 
{
    string buttonText;
    
    public static event Action<string> OnCalculatorInput;// 이벤트 선언 추가!
    void Awake()  
    {
        
        TextMeshProUGUI tmpText = GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
        {
            buttonText = tmpText.text;
        }
    }

    void Start()
    {
        // 런타임에 onClick 이벤트 강제 연결
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    public void OnButtonClicked()
    {
        Debug.Log($"버튼 클릭됨: {buttonText}");
        
        if (buttonText == "·")
        {
            OnCalculatorInput?.Invoke(".");
        }
        else
        {
            OnCalculatorInput?.Invoke(buttonText);
        }
    }
}
