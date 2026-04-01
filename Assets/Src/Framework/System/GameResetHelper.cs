using UnityEngine;

/// <summary>
/// 새 게임 시작 시 모든 DontDestroyOnLoad 싱글톤과 PlayerPrefs를 초기 상태로 되돌린다.
/// MainMenuUIManager에서 "새 게임" 버튼 클릭 시 호출됨.
///
/// [초기화 대상]
/// - ES3SlotManager.selectedSlotPath → null (새 슬롯은 첫 저장 시 자동 생성)
/// - GameState.Money → InitialMoney (500)
/// - ReputationState → 0
/// - RevenueXPTracker → 0
/// - SettlementManager → day 1, 모든 통계 0
/// - PlayerPrefs: 매장 확장, 튜토리얼, 소셜이벤트 플래그 삭제
///
/// [주의]
/// 새 기능 추가 시 PlayerPrefs 키를 사용하면 여기에도 DeleteKey 추가 필요.
/// 향후 PlayerPrefs를 ES3 슬롯으로 통합하면 이 부분 정리 가능.
/// </summary>
public static class GameResetHelper
{
    /// <summary>
    /// 기본 초기 자금 (프로젝트 설정에 맞게 조정)
    /// </summary>
    private const float InitialMoney = 500f;

    public static void ResetAll()
    {
        // 슬롯 선택 해제 (새 게임이므로 로드 안 함)
        ES3SlotManager.selectedSlotPath = null;

        // 돈 초기화
        if (GameState.Instance != null)
            GameState.Instance.SetMoney(InitialMoney);

        // 평판 초기화
        if (ReputationState.Instance != null)
            ReputationState.Instance.SetRaw(0f);

        // 매출 XP 초기화
        if (RevenueXPTracker.Instance != null)
            RevenueXPTracker.Instance.ForceSetXP(0f);

        // 정산(일수) 초기화
        if (SettlementManager.Instance != null)
        {
            SettlementManager.Instance.RestoreState(new SettlementSaveData
            {
                dayNumber = 1,
                satisfiedCustomers = 0,
                dissatisfiedCustomers = 0,
                shopLevel = 0,
                expensiveComplaints = 0,
                totalCustomersToday = 0,
                grossProfitToday = 0f,
                purchaseCostToday = 0f,
                netProfitToday = 0f
            });
        }

        // 매장 확장 초기화
        PlayerPrefs.DeleteKey("StoreExpansionPurchased");
        PlayerPrefs.DeleteKey("ShootingRangePurchased");

        // 소셜 이벤트 첫날 플래그 리셋
        PlayerPrefs.DeleteKey("SocialEvent_FirstDayDone");

        // 튜토리얼 플래그 리셋
        PlayerPrefs.DeleteKey("TutorialWelcomeShown");
        PlayerPrefs.DeleteKey("ExpansionTutorialDone");
        PlayerPrefs.DeleteKey("ShootingRangeTutorialDone");
        PlayerPrefs.DeleteKey("BouncerTutorialDone");
        PlayerPrefs.DeleteKey("SocialEventTutorialDone");

        PlayerPrefs.Save();

        Debug.Log("[GameResetHelper] 모든 상태 초기화 완료");
    }
}
