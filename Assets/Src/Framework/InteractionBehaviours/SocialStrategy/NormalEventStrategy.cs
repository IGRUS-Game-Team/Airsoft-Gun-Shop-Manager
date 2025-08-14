using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 장지원 8.13
/// 
/// 평상시 상태
/// 랜덤값은 필요 없고.. 이름만 ui에 전달하면 된다
/// </summary>
public class NormalEventStrategy : ISocialEventStrategy
{
    private ItemDatabase itemDatabase;
    private List<int> cachedItemIds; // 아이템 id만
    private List<string> EventNames = new List<string>() {
        "평소와 다름없는 하루",
        "평온한 일상",
        "활기찬 하루",
        "모처럼의 여유"
    }; //이벤트 이름 리스트 


    // 현재 이벤트 정보를 지정할 필드
    private string currentEventName;
    //private int currentDuration;

    // 생성자 -> 호출시 랜덤값 생성
    public void GetEventStrategyData()
    {
        GenerateRandomEventData();
    }

    private void GenerateRandomEventData()
    {
        currentEventName = EventNames[UnityEngine.Random.Range(0, EventNames.Count)]; //이벤트이름 고르기
        
        //currentDuration = UnityEngine.Random.Range(1, 4); // 1~3일

        Debug.Log($"normal 전략 : {currentEventName}");
    }


    public string EventName => currentEventName;
    public string StatusText => "공백";
    public float MarketModifier => 0;
    //public int Duration => currentDuration;
}