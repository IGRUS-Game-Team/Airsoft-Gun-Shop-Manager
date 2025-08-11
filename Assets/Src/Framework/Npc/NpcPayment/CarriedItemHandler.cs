using System.Collections.Generic; // ★추가
using UnityEngine;

public class CarriedItemHandler : MonoBehaviour
{
    [SerializeField] Transform handSocket;

    // ★추가: 랜덤 목표 개수 범위
    [Header("여러 개 담기")]
    [SerializeField] int desiredMin = 1;   // 최소 픽업 개수
    [SerializeField] int desiredMax = 3;   // 최대 픽업 개수

    NpcController npc;
    GameObject    heldItem;
    Vector3       worldSize;
    Vector3 lastWorldSize;
    Transform     counterSlot;          // 매니저에서 빌린 슬롯 저장
    public int DesiredCount { get; private set; } // ★추가

     // ★추가: 들고 있는 아이템 큐(계산대에 순차로 올림)
    private struct Picked
    {
        public GameObject go;
        public Vector3 worldScale;
    }

    private readonly Queue<Picked> carried = new(); // ★추가
    public int CarriedCount => carried.Count;       // ★추가
    public bool HasAllDesired => CarriedCount >= DesiredCount; // ★추가

    void Awake()
    {
        npc = GetComponent<NpcController>();
        DesiredCount = Mathf.Clamp(Random.Range(desiredMin, desiredMax + 1), 1, 9999); // ★fix: 랜덤 목표 개수 초기화
    }

    /* ───────────────── 물건 집기 ───────────────── */
    public void Attach(GameObject item)
    {
        heldItem = item;
        
        // ★ 1) 붙이기 전, 원래 월드 스케일 저장
        var originalWorldScale = item.transform.lossyScale;

        item.transform.SetParent(handSocket, true);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.SetActive(true);
        
        // ★추가: 리스트(큐)에 저장만 해두기
        carried.Enqueue(new Picked { go = item, worldScale = originalWorldScale });
    }

    /* ───────────────── 손에서 숨기기 ───────────────── */
    public void Hide()
    {
        if (heldItem) heldItem.SetActive(false);
    }

    /* ───────────────── 카운터로 이동 ───────────────── */
    public void PlaceToCounter()
    {

        // ✅ 큐가 비었으면 종료
        if (carried.Count == 0) return;

        // ✅ 큐에서 "실제" 아이템 꺼내기 (이게 핵심!)
        var pick = carried.Dequeue();

        // 계산대에서는 튀지 않게: 콜라이더 ON, RB는 kinematic 유지
        foreach (var col in pick.go.GetComponentsInChildren<Collider>(true)) col.enabled = true;
        var rb = pick.go.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        // ★ worldScale을 넘겨서 CounterManager가 로컬스케일 계산하게
        if (counterSlot == null)
            counterSlot = CounterManager.Instance.PlaceItem(npc, pick.go, pick.worldScale);
        else
            CounterManager.Instance.PlaceItem(npc, pick.go, pick.worldScale);
    }

    /* ───────────────── 계산대에 전부 올리기 ───────────────── */
    public void PlaceAllToCounter() // ★추가
    {
        // ✅ 초기 개수만큼만 반복 (혹시 중간에 뭔가 꼬여도 무한루프 방지)
        int count = carried.Count;
        for (int i = 0; i < count; i++)
            PlaceToCounter();

        heldItem = null; // 마지막 레퍼런스 정리
    }

    /* ───────────────── 결제 완료 후 반납 ───────────────── */
    public void ReleaseSlot()
    {
        if (counterSlot != null)
            CounterManager.Instance.ReturnSlot(counterSlot);   // 매니저에 돌려주기
        counterSlot = null;
    }
}