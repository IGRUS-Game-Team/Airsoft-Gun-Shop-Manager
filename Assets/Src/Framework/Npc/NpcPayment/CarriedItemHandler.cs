using System.Collections.Generic;
using UnityEngine;

public class CarriedItemHandler : MonoBehaviour
{
    [SerializeField] Transform handSocket;

    [Header("여러 개 담기")]
    [SerializeField] int desiredMin;
    [SerializeField] int desiredMax;

    NpcController npc;
    GameObject heldItem;
    Transform counterSlot;

    public int DesiredCount { get; private set; }
    public int PlacedCount  { get; private set; }

    private struct Picked
    {
        public GameObject go;
        public Vector3 worldScale;
    }

    private readonly Queue<Picked> carried = new();
    private readonly HashSet<int> carriedIds = new();     // ★추가: 중복 방지
    public  int  CarriedCount  => carried.Count;
    public  bool HasAllDesired => CarriedCount >= DesiredCount;

    public  bool PickingClosed { get; private set; }      // ★추가: 이후 Attach 차단
    public  void ClosePicking() => PickingClosed = true;  // ★추가

    void Awake()
    {
        npc = GetComponent<NpcController>();

        // 먼저 개수 정하고 로그 찍자
        DesiredCount = Mathf.Clamp(Random.Range(desiredMin, desiredMax + 1), 1, 9999);
    }

    /* ───────── 물건 집기(애니메이션 이벤트가 여기로 옴) ───────── */
    public void Attach(GameObject item)
    {
        // ★하드캡 1: 상태 전환 이후/목표 초과 시 전부 무시
        if (PickingClosed || CarriedCount >= DesiredCount)
        {
            if (item) item.SetActive(false);
            return;
        }

        // ★하드캡 2: 같은 오브젝트가 두 번 들어오는 거 방지
        int id = item.GetInstanceID();
        if (!carriedIds.Add(id))
        {
            // 이미 담았던 애
            return;
        }

        heldItem = item;

        // 붙이기 전 월드 스케일 저장
        var originalWorldScale = item.transform.lossyScale;

        item.transform.SetParent(handSocket, true);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.SetActive(true);

        carried.Enqueue(new Picked { go = item, worldScale = originalWorldScale });

        // ★하드캡 3: 방금으로 목표를 채웠다면 더 이상 못 담게 스위치 내림
        if (CarriedCount >= DesiredCount)
            PickingClosed = true;
    }

    public void Hide()
    {
        if (heldItem) heldItem.SetActive(false);
    }

    public void PlaceToCounter()
    {
        if (carried.Count == 0) return;

        var pick = carried.Dequeue();

        foreach (var col in pick.go.GetComponentsInChildren<Collider>(true)) col.enabled = true;
        var rb = pick.go.GetComponent<Rigidbody>(); if (rb) rb.isKinematic = true;

        var assigned = CounterManager.Instance.PlaceItem(npc, pick.go, pick.worldScale);
        if (assigned == null)
        {
            // 슬롯 부족 → 다시 대기열로
            pick.go.SetActive(false);
            carried.Enqueue(pick);
            return;
        }

        // 큐에서 빠졌으니 id도 정리
        carriedIds.Remove(pick.go.GetInstanceID());

        if (counterSlot == null) counterSlot = assigned;
        PlacedCount++;
    }

    public void ReleaseSlot()
    {
        if (counterSlot != null)
            CounterManager.Instance.ReturnSlot(counterSlot);
        counterSlot = null;
    }

    public void FillCounterNow()
    {
        PlacedCount = 0;
        int guard = 64;
        while (carried.Count > 0 && CounterManager.Instance.FreeSlotCount > 0 && guard-- > 0)
            PlaceToCounter();
    }
}