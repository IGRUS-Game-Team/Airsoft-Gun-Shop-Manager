using UnityEngine;
using UnityEngine.Events;

public class FeatureUnlockable : MonoBehaviour
{
    [Min(0)]
    [SerializeField] private int requiredLevel = 1;

    [Header("연동(선택)")]
    public UnityEvent OnUnlocked = new(); // 처음 해금될 때 1회
    public UnityEvent OnLocked = new();   // 레벨 하향 시(거의 없겠지만)

    private bool wasUnlocked = false;

    private void OnEnable()
    {
        var t = RevenueXPTracker.Instance;
        if (!t)
        {
            Debug.LogWarning("[FeatureUnlockable] ProfitTracker가 없습니다.");
            return;
        }

        t.OnLevelChanged.AddListener(ApplyLevel);
        ApplyLevel(t.CurrentLevel); // 초기 적용
    }

    private void OnDisable()
    {
        var t = RevenueXPTracker.Instance;
        if (!t) return;
        t.OnLevelChanged.RemoveListener(ApplyLevel);
    }

    private void ApplyLevel(int lvl)
    {
        bool unlocked = (lvl >= requiredLevel);
        if (unlocked != wasUnlocked)
        {
            if (unlocked) OnUnlocked.Invoke(); else OnLocked.Invoke();
            wasUnlocked = unlocked;
        }
        gameObject.SetActive(unlocked); // 단순히 오브젝트 활성/비활성
    }
}
