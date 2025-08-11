// NpcDebugHotkey_Queue.cs
// L키: NPC를 카운터 맨 앞자리로 즉시 이동 → OfferPayment 상태 진입(이후 로직은 기존 흐름)
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NpcController))]
public class NpcDebugHotkey_Queue : MonoBehaviour
{
    [SerializeField] float frontOffset = 0.6f; // 카운터 앞 거리

    NpcController npc;

    void Awake() => npc = GetComponent<NpcController>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            JumpToCounterAndOffer();
    }

    void JumpToCounterAndOffer()
    {
        // 1) QueueManager에서 카운터 Transform 얻기
        var qm = QueueManager.Instance ?? Object.FindFirstObjectByType<QueueManager>();
        if (qm == null) { Debug.LogWarning("[L] QueueManager 없음"); return; }

        Transform counter = null;
        // CounterTransform 프로퍼티가 있으면 사용
        var prop = qm.GetType().GetProperty("CounterTransform");
        if (prop != null) counter = prop.GetValue(qm) as Transform;
        if (counter == null) counter = qm.transform; // fallback

        // 2) 카운터 앞 ‘맨 앞자리’ 좌표 계산
        Vector3 standPos = counter.position - counter.forward * frontOffset;

        // 3) 에이전트 정지 → NavMesh 위로 워프 → 카운터 바라보기
        var ag = npc.Agent;
        ag.isStopped = true;
        ag.ResetPath();
        if (NavMesh.SamplePosition(standPos, out var hit, 0.3f, NavMesh.AllAreas))
            standPos = hit.position;
        ag.Warp(standPos);
        ag.updateRotation = false;
        npc.transform.LookAt(counter);
        npc.Animator.Play("Standing");

        // 4) 기존 상태머신으로 결제 대기 상태 진입(★OfferPayment는 수정 안 함)
        npc.hasItemInHand = true; // ToCounter/OfferPayment 가드 통과용
npc.stateMachine.SetState(
    new NpcState_OfferPayment(npc, counter)
);
        Debug.Log("[L] 카운터 앞 워프 → OfferPayment 진입");
    }
}