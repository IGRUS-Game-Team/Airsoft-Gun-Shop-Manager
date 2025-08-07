using TMPro;
using UnityEngine;

public class ProductAVGSetting : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private TMP_InputField inputFieldPrice; // 가격 입력 필드
    [SerializeField] private TextMeshProUGUI profitText;     // Profit Text
    
    [Header("설정값")]
    [SerializeField] private float avgCost = 8f; // 원가 (나중에 동적으로 변경 ㅍㅣㄹ요)
    
    private float profit = 0f;
    
    void Start()
    {
        // Profit Text 컴포넌트 가져오기
        profitText = GetComponent<TextMeshProUGUI>();
    }
    
    void Update()
    {
        
        CalculateProfit();
    }
    

    //수익 계산 및 표시
    void CalculateProfit()
    {
        if (inputFieldPrice != null && profitText != null)
        {
            // InputField의 텍스트를 float로 변환
            if (float.TryParse(inputFieldPrice.text, out float inputPrice))
            {
                // 수익 = 입력가격 - 원가
                profit = avgCost- inputPrice;
                if (profit < 0)
                {
                    profitText.text = "Profit    $ 손해!";
                }
                else
                {
                    // Profit Text에 표시 (소수점 2자리)
                    profitText.text = "Profit    $ " + profit.ToString("F2");
                }
            }
            else
            {
                // 잘못된 입력이거나 빈 값
                profitText.text = "Profit    $ 0.00";
            }
        }
    }
}