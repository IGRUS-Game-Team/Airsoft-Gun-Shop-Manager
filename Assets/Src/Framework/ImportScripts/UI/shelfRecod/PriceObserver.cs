using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// PriceObserver.UpdatePrice() 호출하면 구독한 모든 관찰자들에게 알림이간다.
/// 값 동기화 가능(설정창, 가격표, 모니터 3개다 동일한 값 적용)
/// </summary>
public class PriceObserver : MonoBehaviour
{
    // 전역 가격 관리자

    private Dictionary<int, float> currentPrices = new();//각 상품의 현재 가격을 저장하는 딕셔너리<상품id,현재 가격>
    private Dictionary<int, List<IPriceChangeable>> observers = new();//상품 구독중인 관찰자 목록<상품id,>

    //관찰자 등록
    public void Subscribe(int itemId, IPriceChangeable observer)
    {
        if (!observers.ContainsKey(itemId))//해당 상품id의 관찰자 목록이 없다면 생성
            observers[itemId] = new List<IPriceChangeable>();
        observers[itemId].Add(observer);
    }

    //등록 해제(할필요..는 없을것같아 구현안함)


    //특정 상품 가격(정가)조회
    public float GetPrice(int itemId)
    {
        return currentPrices.TryGetValue(itemId, out float price) ? price : 0f;
    }


    //가격 업데이트
    //가격 변경시 모든 구독자에게 자동 알림이 전송
    public void UpdatePrice(int itemId, float newPrice)//가격 변경할 상품, 새로운 가격
    {
        float oldPrice = GetPrice(itemId);
        currentPrices[itemId] = newPrice;//상품 가격 변경

        // 모든 구독자에게 알림
        if (observers.ContainsKey(itemId))
            foreach (var observer in observers[itemId])
                observer.OnPriceChanged(itemId, newPrice, oldPrice);//옵저버들은 OnPriceChanged를 구현해서 본인의 로직에 맞게 가격을 수정해야함
    }
    

}
