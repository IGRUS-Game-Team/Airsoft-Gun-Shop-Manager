using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 25/8/3 박정민
/// 돈관리용 클래스입니다
/// </summary>
public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    [Header("돈 상태")]
    [SerializeField] private float money = 0;
    [SerializeField] private TextMeshProUGUI moneyText;

    // 기존: 잔액만 알림
    public UnityEvent<float> OnMoneyChanged = new();

    // 새로 추가: 변화량 + 현재 잔액
    [System.Serializable] public class MoneyDeltaEvent : UnityEvent<float, float> { }
    public MoneyDeltaEvent OnMoneyDelta = new();

    // 매출 전용 이벤트
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
        OnMoneyDelta.Invoke(0f, money); // 시작 시 delta=0
    }

    public void SetMoney(float value)
    {
        float delta = value - money;
        money = value;
        UpdateMoneyUI();
        OnMoneyChanged.Invoke(money);
        OnMoneyDelta.Invoke(delta, money);
    }

    public void AddMoney(float amount)
    {
        money += amount;
        UpdateMoneyUI();
        OnMoneyChanged.Invoke(money);
        OnMoneyDelta.Invoke(amount, money);
    }

    public void AddRevenue(float amount)
    {
        if (amount <= 0f) return;
        money += amount;
        UpdateMoneyUI();
        OnMoneyChanged.Invoke(money);
        OnMoneyDelta.Invoke(amount, money);
        OnRevenueAdded.Invoke(amount);
    }

    public bool SpendMoney(float amount)
    {
        if (money < amount) { Debug.LogWarning("돈 부족!"); return false; }
        money -= amount;
        UpdateMoneyUI();
        OnMoneyChanged.Invoke(money);
        OnMoneyDelta.Invoke(-amount, money);
        return true;
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null) moneyText.text = $"${money:0}";
    }
}
