using UnityEngine;

public class CarriedItemHandler : MonoBehaviour
{
    [SerializeField] Transform handSocket;

    NpcController npc;
    GameObject    heldItem;
    Vector3       worldSize;
    Transform     counterSlot;          // ★변경: 매니저에서 빌린 슬롯 저장

    void Awake() => npc = GetComponent<NpcController>();

    /* ───────────────── 물건 집기 ───────────────── */
    public void Attach(GameObject item)
    {
        heldItem  = item;
        worldSize = item.transform.lossyScale;

        item.transform.SetParent(handSocket, false);
        item.transform.localPosition = Vector3.zero;
        item.SetActive(true);
    }

    /* ───────────────── 손에서 숨기기 ───────────────── */
    public void Hide()
    {
        if (heldItem) heldItem.SetActive(false);
    }

    /* ───────────────── 카운터로 이동 ───────────────── */
    public void PlaceToCounter()
    {

        if (carried.Count == 0) return;

        var pick = carried.Dequeue();

        foreach (var col in pick.go.GetComponentsInChildren<Collider>(true)) col.enabled = true;
        var rb = pick.go.GetComponent<Rigidbody>(); if (rb) rb.isKinematic = true;

        // ★ 여기서 로그 + null 처리
        Debug.Log($"[PLACE] try npc={npc.name} item={pick.go.name} carriedBefore={carried.Count}");
        var assigned = CounterManager.Instance.PlaceItem(npc, pick.go, pick.worldScale);

        if (assigned == null)
        {
            // 슬롯이 없으면 다시 숨기고 큐로 되돌림 → 다음에 다시 시도
            pick.go.SetActive(false);
            carried.Enqueue(pick);                              // ★ 되돌려두기
            Debug.Log($"[PLACE] NO SLOT. requeue npc={npc.name} pending={carried.Count}");
            return;
        }

        if (counterSlot == null) counterSlot = assigned;
    }

    /* ───────────────── 계산대에 전부 올리기 ───────────────── */
    public void PlaceAllToCounter() // ★추가
    {
        // ✅ 초기 개수만큼만 반복 (혹시 중간에 뭔가 꼬여도 무한루프 방지)
        int count = carried.Count;
        for (int i = 0; i < count; i++)
            PlaceToCounter();

        // PlaceItem 이 null 반환 시(슬롯 부족) 대비
        if (counterSlot != null) heldItem = null;
    }

    /* ───────────────── 결제 완료 후 반납 ───────────────── */
    public void ReleaseSlot()
    {
        if (counterSlot != null)
            CounterManager.Instance.ReturnSlot(counterSlot);   // ★변경: 매니저에 돌려주기
        counterSlot = null;
    }
}