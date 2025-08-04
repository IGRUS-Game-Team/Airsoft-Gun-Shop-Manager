using UnityEngine;

public class PaymentAnimEvents : MonoBehaviour
{
    NpcController npc;

    void Awake() => npc = GetComponentInParent<NpcController>();

    /* 애니메이션 이벤트에서 호출 (손이 완전히 뻗은 프레임) */
    public void OnOfferReached()
    {
        Debug.Log("[AnimEvent] OfferReached");
        npc.ShowMoneyOrCard();          // ← 항상 호출, 중복은 ShowMoneyOrCard 내부에서만 판단
    }
}