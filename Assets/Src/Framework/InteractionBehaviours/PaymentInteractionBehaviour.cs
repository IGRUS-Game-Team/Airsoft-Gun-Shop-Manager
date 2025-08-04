using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 플레이어가 카드·현금을 클릭했을 때 호출되는 스크립트.
/// 부모 체인에서 NpcController를 찾아 ‘결제 대기’ 동작을 실행합니다.
/// </summary>
[RequireComponent(typeof(Collider))]        // Raycast에 맞도록 Collider 필수
public class PaymentInteractionBehaviour : MonoBehaviour, IInteractable
{
    [Tooltip("Standing 애니메이터 상태 이름")]
    [SerializeField] string standingStateName = "Standing";

    public void Interact()
    {
        // 1)  카드/현금을 손에 든 NPC 찾기
        NpcController npc = GetComponentInParent<NpcController>();
        Debug.Log($"npc = {npc}");
        if (npc == null)
        {
            Debug.LogWarning("PaymentInteractionBehaviour: 부모 체인에 NpcController가 없습니다!");
            return;
        }

        // 2) 이동 잠그기 + 회전 고정
        NavMeshAgent agent = npc.Agent;          // NpcController에 public 프로퍼티가 있다고 가정
        Debug.Log($"agent = {agent}");
        agent.isStopped      = true;
        agent.updateRotation = false;

        // 3) Standing 애니메이션 재생
        Animator anim = npc.Animator;
        Debug.Log($"anim = {anim}");
        anim.Play(standingStateName);

        // 4) 로그(선택) + 이후 로직(타임아웃·상태머신 전환 등) 추가 가능
        Debug.Log($"[{npc.name}] 결제 대기(Standing) 상태 진입");
        
    }
}