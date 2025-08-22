using UnityEngine;

public class XPDebugHotkeys : MonoBehaviour
{
    [Header("디버그용 레벨 임계 XP (인스펙터에서 조절)")]
    [SerializeField] float level2XP = 25_000f; // 레벨 2 임계값
    [SerializeField] float level3XP = 55_000f; // 레벨 3 임계값(프로젝트 값으로 맞추기)

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
            RevenueXPTracker.Instance?.ForceSetXP(level2XP); // 레벨 2로 점프

        if (Input.GetKeyDown(KeyCode.F3))
            RevenueXPTracker.Instance?.ForceSetXP(level3XP); // 레벨 3로 점프
    }
}