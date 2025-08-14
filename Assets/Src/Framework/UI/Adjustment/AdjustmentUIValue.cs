using TMPro;
using UnityEngine;

public class AdjustmentUIValue : MonoBehaviour
{
    int SCustomers = 100;
    int DSCustomers = 1;
    int TotalCustomers = 101;
    float Reputation = 900;
    int StoreLevel = 4;
    float Profits= 800.0f;
    float SupplyCost = 20.0f;
    float NetProfit = 780.0f;

    [Header("정산UI 값 텍스트")]
    [SerializeField] TextMeshProUGUI SCustomersText;
    [SerializeField] TextMeshProUGUI DSCustomersText;
    [SerializeField] TextMeshProUGUI TotalCustomersText;

    [SerializeField] TextMeshProUGUI ReputationText;
    [SerializeField] TextMeshProUGUI StoreLevelText;

    [SerializeField] TextMeshProUGUI ProfitsText;
    [SerializeField] TextMeshProUGUI SupplyCostText;
    [SerializeField] TextMeshProUGUI NetProfitText;

    void Update()
    {
        SCustomersText.text = $"Satisfied Customers : {SCustomers}";
        DSCustomersText.text = $"Dissatisfied Customers : {DSCustomers}";
        TotalCustomersText.text = $"Total number of cumtomers : {TotalCustomers}";

        ReputationText.text = $"Reputation : {Reputation}";
        StoreLevelText.text = $"Store Level : {StoreLevel}";

        ProfitsText.text = $"Profits : <color=#6BD95B>+{Profits}</color>";
        SupplyCostText.text = $"Cost of Supply : <color=#DB4B47>-{SupplyCost}</color>";
        NetProfitText.text = $"Net Profit : {NetProfit}";
    }

    // 초록색 6BD95B
    // 빨간색 DB4B47
}