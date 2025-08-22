using UnityEngine;

public class XPDebugHotkeys : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
            RevenueXPTracker.Instance?.ForceSetXP(25_000f); // 레벨 2 임계값
    }
}