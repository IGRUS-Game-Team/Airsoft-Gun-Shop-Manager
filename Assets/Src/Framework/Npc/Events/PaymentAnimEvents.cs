using UnityEngine;

[RequireComponent(typeof(NpcController))]
[RequireComponent(typeof(CarriedItemHandler))]
public class PaymentAnimEvents : MonoBehaviour
{
    CarriedItemHandler carrier;

    void Awake()
    {
        carrier = GetComponent<CarriedItemHandler>();
    }

    /* 손이 완전히 뻗은 프레임 */
    public void OnOfferReached()
    {
        // carrier.PlaceToCounter();                           // 상품 내려놓기
    }
}