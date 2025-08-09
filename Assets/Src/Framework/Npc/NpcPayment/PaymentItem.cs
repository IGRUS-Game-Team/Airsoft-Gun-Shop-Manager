using UnityEngine;

public class PaymentItem : MonoBehaviour
{
    PaymentContext context;
    System.Action  onPaid;

    public void Init(PaymentContext ctx, System.Action paidCallback)
    {
        context = ctx;
        onPaid  = paidCallback;
    }

    /* UI·마우스 클릭에서 호출 */
    public void PayByPlayer()
    {
        CounterManager.I.CompletePayment(context.payer);   // ★변경
        onPaid?.Invoke();
        Destroy(gameObject);
    }
}