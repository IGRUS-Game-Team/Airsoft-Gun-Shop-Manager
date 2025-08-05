using UnityEngine;

[RequireComponent(typeof(NpcController))]
[RequireComponent(typeof(CarriedItemHandler))]
public class PaymentAnimEvents : MonoBehaviour
{
    NpcController      npc;
    CarriedItemHandler carrier;

    void Awake()
    {
        npc     = GetComponent<NpcController>();
        carrier = GetComponent<CarriedItemHandler>();
    }

    /* 손이 완전히 뻗은 프레임 */
    public void OnOfferReached()
    {
        carrier.PlaceToCounter();                           // 상품 내려놓기
        CounterManager.I.ShowPaymentObject(npc, npc.HandTransform); // 돈/카드 생성
    }
}