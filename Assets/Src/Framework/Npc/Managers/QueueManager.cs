using System.Collections.Generic;
using UnityEngine;

// 카운터 앞에 줄을 세우고 이동·시선을 관리한다.
public class QueueManager : MonoBehaviour
{
    public static QueueManager Instance { get; private set; }

    [Header("줄 서는 위치(노드)")]
    [SerializeField] private Transform[] spots;

    [Header("계산대 앞 NPC 자리")]
    [SerializeField] private Transform counterNode;

    [Header("줄이 꽉 찼을 때 NPC가 배회할 지점")]
    [SerializeField] private Transform[] wanderPoints;

    [Header("계산대 Transform")]
    [SerializeField] private Transform counter;

    private readonly List<NpcController> waitingLine = new List<NpcController>();

    // “누굴 바라볼지” 매니저가 관리
    private readonly Dictionary<NpcController, Transform> lookTargets = new();

    public Transform CounterTransform => counter;
    public Transform[] WanderPoints => wanderPoints;
    public Transform CounterNode => counterNode;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // NPC가 줄 서기 시도: “자리 배정만” (Agent는 상태가)
    public bool TryEnqueue(NpcController npcController, out Transform node)
    {
        int capacity = spots.Length + 1; // +1 = CounterNode
        if (waitingLine.Count >= capacity)
        {
            node = null;
            return false;
        }

        waitingLine.Add(npcController);

        node = (waitingLine.Count == 1) ? counterNode : spots[waitingLine.Count - 2];

        // 자리 “배정 기록만”
        npcController.AcceptQueueAssignment(node);

        // 바라볼 대상 갱신
        RefreshLookTargets();
        return true;
    }

    // 맨 앞 NPC 결제 완료 → 한 칸씩 앞으로 (배정만 + LookTarget 갱신)
    public void DequeueFront()
    {
        if (waitingLine.Count == 0) return;

        var removed = waitingLine[0];
        waitingLine.RemoveAt(0);
        lookTargets.Remove(removed);

        for (int i = 0; i < waitingLine.Count; i++)
            waitingLine[i].AcceptQueueAssignment(spots[i]);

        RefreshLookTargets();
    }

    // 특정 NPC가 맨 앞인지
    public bool IsFront(NpcController npcController)
    {
        return waitingLine.Count > 0 && waitingLine[0] == npcController;
    }

    // ── 내부: LookTarget 일괄 갱신(맨 앞은 카운터, 나머지는 앞사람) ──
    private void RefreshLookTargets()
    {
        for (int i = 0; i < waitingLine.Count; i++)
        {
            Transform look = (i == 0) ? counter : waitingLine[i - 1].transform;
            lookTargets[waitingLine[i]] = look;
        }
    }
}