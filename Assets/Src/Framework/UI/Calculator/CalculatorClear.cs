using System;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 장지원 8.3 계산기 clear 버튼
/// c 버튼을 누를 경우 ResultValue스크립트의 ClearDisplay 메서드를 호출한다.
/// </summary>

public class CalculatorClear : MonoBehaviour
{
    public static event Action ClearDisplay;

    void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClearButtonClicked);
        }
    }

    public void OnClearButtonClicked()
    {
        ClearDisplay?.Invoke();
    }
}
