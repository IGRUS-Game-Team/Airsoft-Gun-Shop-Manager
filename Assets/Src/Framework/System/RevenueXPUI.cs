using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RevenueXPUI : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private RevenueXPTracker tracker;
    [SerializeField] private Slider progressSlider;          // min=0, max=1
    [SerializeField] private TextMeshProUGUI xpText;         // "$12,340 / $50,000"
    [SerializeField] private TextMeshProUGUI levelText;      // "Level 2"

    private void Awake()
    {
        if (!tracker) tracker = RevenueXPTracker.Instance;
        if (progressSlider) { progressSlider.minValue = 0f; progressSlider.maxValue = 1f; }
    }

    private void OnEnable()
    {
        if (!tracker) tracker = RevenueXPTracker.Instance;
        if (!tracker) { Debug.LogWarning("[RevenueXPUI] Tracker가 없습니다."); return; }

        tracker.OnProgress01.AddListener(OnProgress);
        tracker.OnXPChanged.AddListener(OnXP);
        tracker.OnLevelChanged.AddListener(OnLevel);

        // 초기 반영
        OnXP(tracker.TotalRevenueXP);
        float goal = Mathf.Max(1f, tracker.FinalGoal);
        OnProgress(Mathf.Clamp01(tracker.TotalRevenueXP / goal));
        OnLevel(tracker.CurrentLevel);
    }

    private void OnDisable()
    {
        if (!tracker) return;
        tracker.OnProgress01.RemoveListener(OnProgress);
        tracker.OnXPChanged.RemoveListener(OnXP);
        tracker.OnLevelChanged.RemoveListener(OnLevel);
    }

    private void OnProgress(float t01)
    {
        if (progressSlider) progressSlider.value = t01;
    }

    private void OnXP(float xp)
    {
        if (!xpText) return;
        float goal = Mathf.Max(1f, tracker.FinalGoal);
        xpText.text = $"${xp:0} / ${goal:0}";
    }

    private void OnLevel(int lvl)
    {
        if (levelText) levelText.text = $"Level {lvl}";
    }
}
