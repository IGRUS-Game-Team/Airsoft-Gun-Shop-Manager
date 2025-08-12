using UnityEngine;
using UnityEngine.AI;

// 이거 이제 안 씀 ㅇㅇ
// 이거 이제 안 씀 ㅇㅇ
// 이거 이제 안 씀 ㅇㅇ
// 이거 이제 안 씀 ㅇㅇ
// 이거 이제 안 씀 ㅇㅇ
// 이거 이제 안 씀 ㅇㅇ
// 이거 이제 안 씀 ㅇㅇ
// 이거 이제 안 씀 ㅇㅇ
// 이거 이제 안 씀 ㅇㅇ
// 이거 이제 안 씀 ㅇㅇ
// 이거 이제 안 씀 ㅇㅇ
// 이거 이제 안 씀 ㅇㅇ
// 이거 이제 안 씀 ㅇㅇ
//[RequireComponent(typeof(Collider))]        // Raycast에 맞도록 Collider 필수
public class PaymentInteractionBehaviour : MonoBehaviour, IInteractable
{
    [Tooltip("Standing 애니메이터 상태 이름")]
    [SerializeField] string standingStateName = "Standing";

    public void Interact()
    {
        // 1)  카드/현금을 손에 든 NPC 찾기
        NpcController npc = GetComponentInParent<NpcController>();
        if (npc == null) return; //----추가 : 장지원

        gameObject.SetActive(false);

        // 2) 이동 잠그기 + 회전 고정
        NavMeshAgent agent = npc.Agent;          // NpcController에 public 프로퍼티가 있다고 가정
        agent.isStopped = true;
        agent.updateRotation = false;

        // 3) Standing 애니메이션 재생
        Animator anim = npc.Animator;
        anim.Play(standingStateName);

        // 4) “결제 시도”만 기록해 두고 실제 결제는 계산 로직에서 호출 ------변경: 계산로직 : 장지원
        //CounterManager.I.MarkPendingPayment(npc);
        CounterManager.Instance.StartCalculatorPayment(npc);

    }
}