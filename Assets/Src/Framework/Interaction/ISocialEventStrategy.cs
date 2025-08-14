using UnityEngine;
/// <summary>
/// 장지원 8.12
/// 사회 이벤트 (호황,불황, 폐쇄, 평상시) 틀
/// </summary>
public interface ISocialEventStrategy
{
    string EventName { get; } //사회 이벤트 이름 : 총기 규제 완화
    string StatusText { get; } // 사회 이벤트 카테고리 : 수요 상승(호황)


    //대상 아이템들
    float MarketModifier { get; } // 시세 변동률
    int Duration { get; } //지속시간 (일)
    

}
