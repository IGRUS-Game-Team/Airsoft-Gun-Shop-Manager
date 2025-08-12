// 2025-08-12 이준서
// 변경 요약:
// - 기존: PayByPlayer()에서 즉시 CompletePayment 호출 → 카드/현금 분기 무시
// - 변경: 같은 오브젝트(또는 부모)에 붙은 IInteractable(CardPaymentInteraction 등)로 위임.
//         실제 결제 완료는 Calculator/단말 성공 콜백에서 수행.


using UnityEngine;

public class PaymentItem : MonoBehaviour
{
    PaymentContext context;
    System.Action  onPaid;

    public void Init(PaymentContext ctx, System.Action paidCallback)
    { context = ctx; onPaid = paidCallback; }
}