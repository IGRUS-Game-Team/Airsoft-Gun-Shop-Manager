using UnityEngine;

public class CashRegisterEnterHandler : MonoBehaviour
{
    [SerializeField] private Transform cashBasket; // 현금 바구니 오브젝트
    [SerializeField] private Vector3 openOffset = new Vector3(0.8f, 0f, 0f);
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private CashRegisterUI cashUI;

    private Vector3 closedPos;
    private Vector3 openPos;
    private bool canPressEnter = false;

    private void Start()
    {
        closedPos = cashBasket.localPosition;
        openPos = closedPos + openOffset;
        InteractionController.Instance.OnCashRegister += HandleExitKeyPressed;
    }

    private void OnDisable()
    {
        if (InteractionController.Instance != null)
            InteractionController.Instance.OnCashRegister -= HandleExitKeyPressed;

        CashRegisterUI.SuccessCompare -= HandlePaymentSuccess;
        CashRegisterUI.FailedCompare -= HandlePaymentFail;
    }

    public void OpenBasket()
    {
        canPressEnter = true;
        StopAllCoroutines();
        StartCoroutine(MoveBasket(openPos));
    }

    public void CloseBasket()
    {
        canPressEnter = false;
        StopAllCoroutines();
        StartCoroutine(MoveBasket(closedPos));
    }

    private void HandleExitKeyPressed()
    {
        if (!canPressEnter) return;

        // 금액 비교는 CashRegisterUI에서 처리
        cashUI.TryConfirm();
    }

    private System.Collections.IEnumerator MoveBasket(Vector3 targetPos)
    {
        while (Vector3.Distance(cashBasket.localPosition, targetPos) > 0.01f)
        {
            cashBasket.localPosition = Vector3.MoveTowards(
                cashBasket.localPosition,
                targetPos,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }
        cashBasket.localPosition = targetPos;
    }

    private void OnEnable()
    {
        CashRegisterUI.SuccessCompare += HandlePaymentSuccess;
        CashRegisterUI.FailedCompare += HandlePaymentFail;
    }



    private void HandlePaymentSuccess()
    {
        // 결제 성공 시 바구니 닫기
        CloseBasket();
    }

    private void HandlePaymentFail()
    {
        // 필요하면 경고 UI 띄우기
    }
}
