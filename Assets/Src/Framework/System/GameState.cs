using TMPro;
using UnityEngine;

/// <summary>
/// 25/8/3 박정민
/// 돈관리용 클래스입니다
/// </summary>

//TODO : 돈 표시하는 UI에 Text 만들고 연결하기
//       그 후 주석 풀기
public class GameState : MonoBehaviour
{
    
    public static GameState Instance { get; private set; }

    [Header("돈 상태")]
    [SerializeField] private float money = 0;
    [SerializeField] private TextMeshProUGUI moneyText;


    public float Money => money;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        UpdateMoneyUI();
    }

    public void SetMoney(float value)
    {
        money = value;
        UpdateMoneyUI();
    }
    public void AddMoney(float amount)
    {
        money += amount;
        UpdateMoneyUI();
    }

    public bool SpendMoney(float amount)
    {
        if (money >= amount)
        {
            money -= amount;
            UpdateMoneyUI();
            return true;
        }
        else
        {
            Debug.LogWarning("돈 부족!");
            return false;
        }
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = $"${money}";
    }
}