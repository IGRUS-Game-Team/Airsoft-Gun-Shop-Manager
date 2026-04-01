using System;
using System.Collections.Generic;
using UnityEngine;

public class RecessionEventStrategy : ISocialEventStrategy
{

    private List<string> EventNames = new List<string>()
    {
        "[ Gun control following a shooting incident ]", // 총기 난사 사건에 따른 규제
        "[ Economic recession ]", // 경제 악화
        "[ Gun control protest ]"// 총기 규제 시위
    };

    //현재 이벤트 저장할 필드
    private string currentEventName;
    private float currentMarketModifier;

// -> 호출시 랜덤값 생성
    public void GetEventStrategyData()
    {
        GenerateRandomEventData();
    }

    private void GenerateRandomEventData()
    {
        currentEventName = EventNames[UnityEngine.Random.Range(0, EventNames.Count)]; //이벤트이름 고르기
        currentMarketModifier = UnityEngine.Random.Range(-0.3f,-0.1f); // -10%~-30% 하락
    }

    public string EventName => currentEventName;
    public string StatusText => "Decrease in demand";
    public float MarketModifier => currentMarketModifier;
    // "shooting incident", "Gun control protest"가 규제 이벤트 (특정 1~2개 총기)
    // "Economic recession"은 전체 총기
    public bool IsGunRegulation => currentEventName != null
        && !currentEventName.Contains("Economic", System.StringComparison.OrdinalIgnoreCase);

}
