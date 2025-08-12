using System;
using TMPro;
using UnityEngine;

public class CashRegisterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI receivedText;
    [SerializeField] private TextMeshProUGUI totalText;
    [SerializeField] private TextMeshProUGUI changeText;
    [SerializeField] private TextMeshProUGUI givingText;

    private float currentGiven = 0f;
    [SerializeField] private float targetChange = 0f;
    private float receivedCash = 0f; // UI표시용, 선택
    private float totalCost = 0f;    // 확인 성공 시 매출 반영 기준

    public static event Action FailedCompare;  // 비교 실패(아직 모자람)
    public static event Action SuccessCompare; // 비교 성공(같거나 초과)


    public float GetCurrentGiven()
    {
        return currentGiven;
    }
    public void SetValues(float received, float total)
    {
        receivedCash = received;
        totalCost = total;

        // 손님에게 거슬러줘야 할 돈(많이 내면 양수, 딱 맞으면 0, 적게 내면 0으로 처리)
        float change = Mathf.Max(0f, received - total);

        receivedText.text = $"${received:0.00}";
        totalText.text = $"${total:0.00}";
        changeText.text = $"${change:0.00}";
        givingText.text = "$0.00";

        currentGiven = 0f;
        targetChange = change;
    }

    // 돈/동전 클릭으로 증가 — “초과 가능”으로 변경
    public void AddGivenAmount(float amount)
    {
        currentGiven += amount;
        givingText.text = $"${currentGiven:0.00}";
    }

    // Enter 로 호출: 충분하면 성공, 아니면 실패 이벤트
    public void TryConfirm()
    {
        if (currentGiven + 1e-4f >= targetChange) // float 오차 여유
            SuccessCompare?.Invoke();
        else
            FailedCompare?.Invoke();
    }

    public void Clear()
    {
        receivedText.text = totalText.text = changeText.text = givingText.text = "";
        currentGiven = 0f;
        targetChange = 0f;
        receivedCash = 0f;
        totalCost = 0f;
    }
}
