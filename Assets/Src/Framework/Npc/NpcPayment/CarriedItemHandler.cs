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
        if (heldItem == null) return;

        // 슬롯이 없으면 빌림
        if (counterSlot == null)
            counterSlot = CounterManager.Instance.PlaceItem(npc, heldItem, worldSize);

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