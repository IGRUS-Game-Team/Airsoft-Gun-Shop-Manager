// 2025-08-12 이준서
// <변경 요약>
// 카드 결제 클릭 시, 기존 프로젝트의 카드 결제 진입점인 
// CounterManager.StartCalculatorPayment(npc)를 스크립트로 나눠서 호출하도록 변경함.



using UnityEngine;

public class CardPaymentInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] string standingStateName = "Standing";

    public void Interact()
    {
        // 1) 카드 들고 있는 NPC 찾기
        var npc = GetComponentInParent<NpcController>();
        if (npc == null || CounterManager.Instance == null) return;

        // 2) 스캔 목표 달성한 손님만 처리(기존 가드 유지)
        if (!CounterManager.Instance.IsReadyToPay(npc)) return;

        // 3) 시각/이동 처리(기존과 동일)
        gameObject.SetActive(false);
        var agent = npc.Agent;
        if (agent != null) { agent.isStopped = true; agent.updateRotation = false; }
        npc.Animator?.Play(standingStateName);

        // 4) 기존 카드 결제 진입점 그대로 호출
        CounterManager.Instance.StartCalculatorPayment(npc);
    }
}