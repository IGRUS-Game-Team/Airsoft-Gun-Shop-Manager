using UnityEngine;
using UnityEngine.Events;

public class RevenueXPTracker : MonoBehaviour
{
    public static RevenueXPTracker Instance { get; private set; }

    [Header("레벨 임계값(오름차순)")]
    [SerializeField] private float[] levelThresholds = new float[] { 10_000f, 25_000f, 50_000f };

    [Header("누적 매출(경험치)")]
    [SerializeField] private float totalRevenueXP = 0f; // 감소하지 않음

    public float TotalRevenueXP => totalRevenueXP;
    public float FinalGoal => levelThresholds != null && levelThresholds.Length > 0 ? levelThresholds[^1] : 0f;

    // 이벤트
    public UnityEvent<float> OnXPChanged = new();        // 원시 XP(달러)
    public UnityEvent<float> OnProgress01 = new();       // 0~1 (FinalGoal 기준)
    public UnityEvent<int>   OnLevelChanged = new();     // 0~3

    public int CurrentLevel { get; private set; } = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
        BroadcastAll();
    }

    private void TrySubscribe()
    {
        if (!GameState.Instance) return;

        // 중복 구독 방지
        GameState.Instance.OnRevenueAdded.RemoveListener(AddXPFromRevenue);
        GameState.Instance.OnRevenueAdded.AddListener(AddXPFromRevenue);
    }

    // 매출 발생 → 경험치 증가(단조)
    private void AddXPFromRevenue(float amount)
    {
        if (amount <= 0f) return;
        totalRevenueXP += amount;
        RecalcAndBroadcast();
    }

    private void RecalcAndBroadcast()
    {
        float goal = Mathf.Max(1f, FinalGoal);
        float t01  = Mathf.Clamp01(totalRevenueXP / goal);

        // 레벨 계산
        int lvl = 0;
        if (levelThresholds != null)
            for (int i = 0; i < levelThresholds.Length; i++)
                if (totalRevenueXP >= levelThresholds[i]) lvl = i + 1;

        OnXPChanged.Invoke(totalRevenueXP);
        OnProgress01.Invoke(t01);

        if (lvl != CurrentLevel)
        {
            CurrentLevel = lvl;
            OnLevelChanged.Invoke(CurrentLevel);
        }
    }

    private void BroadcastAll()
    {
        float goal = Mathf.Max(1f, FinalGoal);
        float t01  = Mathf.Clamp01(totalRevenueXP / goal);

        OnXPChanged.Invoke(totalRevenueXP);
        OnProgress01.Invoke(t01);

        int lvl = 0;
        if (levelThresholds != null)
            for (int i = 0; i < levelThresholds.Length; i++)
                if (totalRevenueXP >= levelThresholds[i]) lvl = i + 1;

        CurrentLevel = lvl;
        OnLevelChanged.Invoke(CurrentLevel);
    }

    // (선택) ES3 저장/로드
    public void SaveES3()
    {
        ES3.Save("xp_totalRevenue", totalRevenueXP);
    }
    public void LoadES3()
    {
        totalRevenueXP = ES3.KeyExists("xp_totalRevenue") ? ES3.Load<float>("xp_totalRevenue") : 0f;
        BroadcastAll();
    }
}
