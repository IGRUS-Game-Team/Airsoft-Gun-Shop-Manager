using UnityEngine;
/// <summary>
/// 장지원 8.11
/// 가격 변동을 받는 (가격이 동기화 된, 모니터 가격표, 가격세팅창) 
/// 객체들이 구현해야할 인터페이스
/// </summary>
public interface IPriceChangeable
{
    //가격이 변경되었을 때 호출됨 -> 이전 가격을 새로운 가격으로 바꾸도록 각자 구현 필요
    void OnPriceChanged(int itemId, float newPrice, float oldPrice);
}
