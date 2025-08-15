// 2025-08-14 이준서
// AdjustmentUIValue
// - SettlementManager 스냅샷을 정산 UI(TextMeshPro)에 바인딩
// - 이벤트 기반 갱신(OnChanged) + 캔버스 활성화 시 즉시 1회 렌더
// - 금액 포맷/색 표시(초록 +, 빨강 -)

using TMPro;
using UnityEngine;

public class AdjustmentUIValue : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] SettlementManager settlement;   // 비워두면 런타임에 Instance로 채움

    [Header("정산UI 값 텍스트")]
    [SerializeField] TextMeshProUGUI SCustomersText;
    [SerializeField] TextMeshProUGUI DSCustomersText;
    [SerializeField] TextMeshProUGUI TotalCustomersText;

    [SerializeField] TextMeshProUGUI ReputationText;
    [SerializeField] TextMeshProUGUI StoreLevelText;

    [SerializeField] TextMeshProUGUI ProfitsText;     // grossProfit(매출)
    [SerializeField] TextMeshProUGUI SupplyCostText;  // purchaseCost(매입/비용)
    [SerializeField] TextMeshProUGUI NetProfitText;   // netProfit(순이익)

    void Awake()
    {
        if (settlement == null)
            settlement = SettlementManager.Instance;
    }

    void OnEnable()
    {
        if (settlement != null)
        {
            settlement.OnChanged += HandleSnapshot;
            // 캔버스 켜질 때 즉시 1회 그리기
            HandleSnapshot(settlement.GetSnapshot());
        }
    }

    void OnDisable()
    {
        if (settlement != null)
            settlement.OnChanged -= HandleSnapshot;
    }

    void HandleSnapshot(SettlementManager.Snapshot s)
    {
        if (SCustomersText)      SCustomersText.text      = $"Satisfied Customers : {s.satisfied}";
        if (DSCustomersText)     DSCustomersText.text     = $"Dissatisfied Customers : {s.dissatisfied}";
        if (TotalCustomersText)  TotalCustomersText.text  = $"Total number of customers : {s.totalCustomers}";

        if (ReputationText)      ReputationText.text      = $"Reputation : {s.reputation}";
        if (StoreLevelText)      StoreLevelText.text      = $"Store Level : {s.shopLevel}";

        // 금액 포맷/색상
        if (ProfitsText)     ProfitsText.text    = $"Profits : {FormatSignedColored(s.grossProfit)}";
        if (SupplyCostText)  SupplyCostText.text = $"Cost of Supply : {FormatSignedColored(-s.purchaseCost)}";
        if (NetProfitText)   NetProfitText.text  = $"Net Profit : {FormatPlainColored(s.netProfit)}";
    }

    // +값은 초록, -값은 빨강으로 표시. (요청 색상: 초록 6BD95B, 빨강 DB4B47)
    string FormatSignedColored(float value)
    {
        string color = value >= 0f ? "#6BD95B" : "#DB4B47";
        string sign  = value >= 0f ? "+" : ""; // 양수면 + 붙이기
        return $"<color={color}>{sign}{FormatMoney(value)}</color>";
    }

    // 순이익은 부호대로 색만 입힘(양수 초록, 음수 빨강, 0은 기본)
    string FormatPlainColored(float value)
    {
        if (Mathf.Approximately(value, 0f))
            return FormatMoney(0f);

        string color = value > 0f ? "#6BD95B" : "#DB4B47";
        return $"<color={color}>{FormatMoney(value)}</color>";
    }

    // 천 단위 콤마, 소수 2자리 고정
    string FormatMoney(float v)
    {
        // 1234.5 -> 1,234.50
        return v.ToString("#,0.00");
    }
}