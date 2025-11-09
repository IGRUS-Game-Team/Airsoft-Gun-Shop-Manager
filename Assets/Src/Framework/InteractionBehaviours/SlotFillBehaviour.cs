using UnityEngine;

/// 플레이어가 슬롯을 클릭하면 상품을 진열 (안쪽→바깥쪽, 최대 2개)
[RequireComponent(typeof(ShelfSlot))]
public class SlotFillBehaviour : MonoBehaviour, IInteractable
{
    [Tooltip("기본 진열 프리팹 (없으면 무시)")]
    [SerializeField] GameObject defaultProduct;

    ShelfSlot slot;
    PlayerObjectHoldController hold; // 들고 있는 박스 접근용

    void Awake()
    {
        slot = GetComponent<ShelfSlot>();
        hold = FindFirstObjectByType<PlayerObjectHoldController>();
    }

    public void Interact()
    {
        if (slot.IsFull) return;

        // heldObject 체크
        if (hold == null || hold.heldObject == null) return;

        BoxContainer box = hold.heldObject.GetComponent<BoxContainer>();
        if (box == null || box.Remaining <= 0 || box.Item == null)
        {
            Debug.LogWarning($"[{name}] 박스가 없거나 수량이 0이어서 진열 불가");
            return;
        }

        GameObject prefab = box.Item.displayPrefab;
        if (prefab == null)
        {
            Debug.LogWarning($"[{name}] {box.Item.itemName} 의 displayPrefab이 없습니다.");
            return;
        }

        // 스냅 포인트
        int idx = slot.ItemCount;
        Transform snap = slot.GetSnapPoint(idx);

        // 생성
        GameObject go = Instantiate(prefab, snap.position, snap.rotation, slot.transform);
        go.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);

        go.layer = LayerMask.NameToLayer("Box"); // RaycastDetector가 쓰는 레이어 이름으로

        // CounterSlotData 연결
        var csd = go.GetComponent<CounterSlotData>();
        if (csd == null) csd = go.AddComponent<CounterSlotData>();
        csd.itemObject = go;
        csd.itemData = box.Item;
        csd.amount = 1;

        // BlockIsHolding 동적 추가
        var holdData = go.GetComponent<BlockIsHolding>();
        if (holdData == null)
            holdData = go.AddComponent<BlockIsHolding>();
        // BlockIsHolding.Awake() 안에서 originalParent를 현재 부모(slot.transform)로 잡아준다.

        // 선반 위에서는 "안 떨어지게" + "레이캐스트는 살아 있게" 만들기
        var rb = go.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;     // 움직이지 않게
            rb.useGravity = false;     // 중력 끔
            rb.detectCollisions = true; // 레이캐스트를 위해 TRUE 유지
        }

        // SmallBoxInteraction 동적 추가
        var small = go.GetComponent<SmallBoxInteraction>();
        if (small == null)
            small = go.AddComponent<SmallBoxInteraction>();

        slot.RegisterNewItem(go);

        // 수량 차감
        box.TakeOne();
    }

}
