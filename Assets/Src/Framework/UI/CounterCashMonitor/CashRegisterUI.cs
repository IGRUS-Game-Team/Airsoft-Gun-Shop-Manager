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

    public void SetValues(float received, float total)
    {
        float change = received - total;
        receivedText.text = $"${received:0.00}";
        totalText.text = $"${total:0.00}";
        changeText.text = $"${change:0.00}";
        givingText.text = "$0.00";

        currentGiven = 0f;
        targetChange = change;
    }

    public void AddGivenAmount(float amount)
    {
        if (currentGiven + amount > targetChange)
        {
            // TODO : 경고문 뜨게 하기
            return;
        }
        currentGiven += amount;
        givingText.text = $"${currentGiven:0.00}";
    }

    public void Clear()
    {
        receivedText.text = totalText.text = changeText.text = givingText.text = "";
        currentGiven = 0f;
        targetChange = 0f;
    }
}