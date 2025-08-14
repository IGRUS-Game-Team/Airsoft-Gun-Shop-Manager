using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 장지원 8.12 
/// 
/// 호황 상태
/// 전략에서 랜덤값을 정하여 SocialEventManager에게 전달한다
/// </summary>
public class BoomEventStrategy : ISocialEventStrategy
{
    private ItemDatabase itemDatabase;
    private List<int> cachedItemIds; // 아이템 id만
    private List<string> EventNames = new List<string>() {
        "총기 규제 완화",
        "슈팅 게임 히트",
        "건 액션 영화 히트",
        "밀리터리 페스티벌 개최"
    }; //이벤트 이름 리스트 


    // 현재 이벤트 정보를 지정할 필드
    private string currentEventName;
    private float currentMarketModifier;
    //private int currentDuration;

    // 생성자 -> 호출시 랜덤값 생성
    public void GetEventStrategyData()
    {
        GenerateRandomEventData();
    }

    private void GenerateRandomEventData()
    {
        currentEventName = EventNames[UnityEngine.Random.Range(0, EventNames.Count)]; //이벤트이름 고르기
        currentMarketModifier = UnityEngine.Random.Range(0.1f, 0.3f); // 10%~30% 상승
        //currentDuration = UnityEngine.Random.Range(1, 4); // 1~3일

        Debug.Log($"Boom 전략 : {currentEventName}, {currentMarketModifier}");
    }

    public string EventName => currentEventName;        
    public string StatusText => "수요 증가";             
    public float MarketModifier => currentMarketModifier; 
    // public int Duration => currentDuration;             
}