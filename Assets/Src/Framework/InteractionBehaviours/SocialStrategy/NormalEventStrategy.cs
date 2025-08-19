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
    


    // 현재 이벤트 정보를 지정할 필드
    private string currentEventName;
    

    // 생성자 -> 호출시 랜덤값 생성
    public void GetEventStrategyData()
    {
        //얘는 랜덤값 생성할게 없음
    }

    

    public string EventName => "Day";
    public string StatusText => "공백";
    public float MarketModifier => 0;
    //public int Duration => currentDuration;
}