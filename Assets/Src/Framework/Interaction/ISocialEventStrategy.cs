using UnityEngine;

/// <summary>
/// 일일 경제 이벤트의 전략 인터페이스 (전략 패턴).
///
/// [구현체]
/// - NormalEventStrategy:     변동 없음 (1일차 강제)
/// - BoomEventStrategy:       호황 (+10~30% 가격 상승, 수요 증가)
/// - RecessionEventStrategy:  불황 (-20~30% 가격 하락, 수요 감소)
/// - RegulationEventStrategy: 규제 (특정 1~2종 총기 판매 제한)
///
/// [사용 흐름]
/// SocialEventManager.ExecuteStrategy()
///   → SelectRandomStrategy() → ISocialEventStrategy 선택
///   → GetEventStrategyData() → 랜덤 이벤트명/수치 생성
///   → MarketModifier를 MarketPriceDataManager에 전달 → 가격 변동
///
/// [새 전략 추가]
/// 1. SocialStrategy/ 폴더에 이 인터페이스 구현 클래스 생성
/// 2. SocialEventManager.CreateStrategy()에 case 추가
/// 3. strategyChances 배열에 확률 추가
/// </summary>
public interface ISocialEventStrategy
{
    string EventName { get; }       // 이벤트 이름 (예: "Relaxation of gun regulations")
    string StatusText { get; }      // 상태 카테고리 (예: "Increase in demand")
    float MarketModifier { get; }   // 가격 변동률 (-0.3 ~ +0.5, 0이면 변동 없음)
    bool IsGunRegulation { get; }   // true: 랜덤 1~2개 총기만 영향 / false: 전체 총기

    /// <summary>
    /// 랜덤으로 이벤트 데이터를 생성한다.
    /// EventName, MarketModifier 등의 값이 이 메서드 호출 후 설정됨.
    /// 세이브/로드 시에도 호출되어 전략 상태를 재생성.
    /// </summary>
    void GetEventStrategyData();
}
