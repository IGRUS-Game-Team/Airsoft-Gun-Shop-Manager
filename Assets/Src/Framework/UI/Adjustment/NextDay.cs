using UnityEngine;
using UnityEngine.UI;

public class NextDay : MonoBehaviour
{
    [Header("UI / 시스템")]
    [SerializeField] DateUI  dateUI;
    [SerializeField] TimeUI  timeUI;
    [SerializeField] OnDayEnd onDayEnd;
    [SerializeField] Button  nextDayButton;

    [Header("플레이어")]
    [SerializeField] PlayerSpawn playerSpawn;   // <-- PlayerCapsule에 붙인 그 컴포넌트

    public void ChangeNextDay()
    {
        Debug.Log("NextDay.ChangeNextDay() 호출");

        // 1) 날짜 +1
        if (dateUI != null)
            dateUI.UpdateDate();

        // 2) 일수 카운터 증가 + 오늘 데이터 리셋
        if (SettlementManager.Instance != null)
        {
            SettlementManager.Instance.AdvanceDay();
            SettlementManager.Instance.ResetToday();
        }

        // 3) 시간 08:00으로 리셋
        if (timeUI != null)
            timeUI.ResetForNewDay();

        // 4) 플레이어를 시작 위치로 리셋
        if (playerSpawn != null)
            playerSpawn.ResetToSpawn();
        else
            Debug.LogWarning("[NextDay] playerSpawn이 비어 있음!");

        // 5) 데일리 리포트 닫고 다음날 시작
        if (onDayEnd != null)
            onDayEnd.StartNextDay();
    }
}
