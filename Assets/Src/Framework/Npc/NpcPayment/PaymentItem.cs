using UnityEngine;

// NPC 손에 붙는 결제 오브젝트(현금·카드)
public class PaymentItem : MonoBehaviour
{
    private PaymentContext payContext;           // 결제 정보
    private System.Action  onPaidCallback;       // 결제 완료 후 실행할 콜백

    // 외부에서 초기화할 때 호출
    public void Init(PaymentContext context, System.Action onPaid)
    {
        this.payContext     = context;           // 결제 정보 저장
        this.onPaidCallback = onPaid;            // 콜백 저장
    }

    // 기존 IPointerClickHandler 경로는 유지하되,
    // 마우스나 UI 이벤트 없이도 호출할 수 있게 별도 공개 메서드 추가
    public void PayByPlayer()
    {
        // 1) 콜백 실행 → 줄 이동·퇴장 로직
        if (onPaidCallback != null)
        {
            onPaidCallback.Invoke();
        }
        // 2) UI에 결제 패널 보였다면 숨기기
        if (UIManager.uiManager != null)
        {
            UIManager.uiManager.HidePayPrompt();
        }
        // 3) 이 오브젝트 제거
        Destroy(this.gameObject);
    }
}