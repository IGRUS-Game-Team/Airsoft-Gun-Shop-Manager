using TMPro;
using UnityEngine;

/// <summary>
/// 25/8/3 박정민
/// 돈관리용 클래스입니다
/// </summary>

//TODO : 돈 표시하는 UI에 Text 만들고 연결하기
//       그 후 주석 풀기
using UnityEngine.Events;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    [Header("돈 상태")]
    [SerializeField] private float money = 0;
    [SerializeField] private TextMeshProUGUI moneyText;

    // 잔액 변경(모든 돈 변화 공통)
    public UnityEvent<float> OnMoneyChanged = new();
    // ★ 매출(경험치 포함) 전용 이벤트
    public UnityEvent<float> OnRevenueAdded = new();

    public float Money => money;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        UpdateMoneyUI();
        OnMoneyChanged.Invoke(money);
    }

    public void SetMoney(float value)
    {
        money = value;
        UpdateMoneyUI();
        OnMoneyChanged.Invoke(money);
    }

    // 일반 입금(대출/환급 등 포함) → 경험치 미포함
    public void AddMoney(float amount)
    {
        money += amount;
        UpdateMoneyUI();
        OnMoneyChanged.Invoke(money);
    }

    // ★ 매출 전용: 잔액 증가 + 경험치 반영
    public void AddRevenue(float amount)
    {
        if (amount <= 0f) return; // 음수 매출 방지
        money += amount;
        UpdateMoneyUI();
        OnMoneyChanged.Invoke(money);
        OnRevenueAdded.Invoke(amount); // 경험치로 처리됨
    }

    public bool SpendMoney(float amount)
    {
        if (money < amount) { Debug.LogWarning("돈 부족!"); return false; }
        money -= amount;
        UpdateMoneyUI();
        OnMoneyChanged.Invoke(money);
        return true;
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null) moneyText.text = $"${money:0}";
    }
}
