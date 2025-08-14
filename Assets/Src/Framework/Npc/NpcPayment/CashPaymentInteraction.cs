// 2025-08-12 이준서
// <변경 요약>
// 현금 결제 클릭 시, CounterManager.Instance.StartCashPayment(npc);로
// 넘어가도록 수정함.

using UnityEngine;

public class CashPaymentInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] string standingStateName = "Standing";

    public void Interact()
    {
        var npc = GetComponentInParent<NpcController>();
        if (npc == null || CounterManager.Instance == null) return;

        // 스캔 목표 달성한 손님만 처리 (카드와 동일 가드)
        if (!CounterManager.Instance.IsReadyToPay(npc)) return;

        // 시각 처리(선택): 손의 현금 오브젝트 숨김
        gameObject.SetActive(false);

        // 이동 정지 & 정면 포즈 (카드와 동일)
        var agent = npc.Agent;
        if (agent != null) { agent.isStopped = true; agent.updateRotation = false; }
        npc.Animator?.Play(standingStateName);

        // ★ 현금 결제 시작(아래 2번 함수 호출)
        CounterManager.Instance.StartCashPayment(npc);
    }
}