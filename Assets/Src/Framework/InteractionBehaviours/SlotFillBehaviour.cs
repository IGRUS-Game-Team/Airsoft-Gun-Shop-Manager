using System.Collections.Generic;
using UnityEngine;

/// 플레이어가 슬롯을 클릭하면 상품을 진열 (안쪽→바깥쪽, 최대 2개)
[RequireComponent(typeof(ShelfSlot))]
public class SlotFillBehaviour : MonoBehaviour, IInteractable, IHasInteractionPrompts
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
        if (hold == null || hold.heldObject == null) return;

        // 상품 박스(SmallBoxInteraction)를 들고 있으면 → 재진열
        var smallBox = hold.heldObject.GetComponent<SmallBoxInteraction>();
        if (smallBox != null)
        {
            PlaceSmallBoxOnShelf(smallBox);
            return;
        }

        // 배달 박스(BoxContainer)를 들고 있으면 → 신규 진열
        BoxContainer box = hold.heldObject.GetComponent<BoxContainer>();
        if (box == null || box.Remaining <= 0 || box.Item == null)
        {
            Debug.LogWarning($"[{name}] 박스가 없거나 수량이 0이어서 진열 불가");
            return;
        }

        if (!box.IsOpen)
        {
            Debug.LogWarning($"[{name}] 박스가 닫혀 있어서 진열 불가");
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

        SetLayerRecursively(go, LayerMask.NameToLayer("Box")); // RaycastDetector가 쓰는 레이어 이름으로

        // displayPrefab에 포함된 기존 상호작용 컴포넌트 제거
        // SmallBoxInteraction은 보존 (인스펙터에서 설정된 animator, gunPrefab 참조 유지)
        // 그 외 IPickable(GunInteraction 등)만 제거
        foreach (var existing in go.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (existing is IPickable && !(existing is SmallBoxInteraction))
                DestroyImmediate(existing);
        }

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

        // 상품 전체를 감싸는 BoxCollider 추가 (기존 콜라이더 빈틈으로 레이캐스트가 선반에 맞는 문제 방지)
        AddBoundsCollider(go);

        // 하이라이트용 BlockOutLiner 추가
        if (go.GetComponent<BlockOutLiner>() == null)
            go.AddComponent<BlockOutLiner>();

        slot.RegisterNewItem(go);

        // 수량 차감
        box.TakeOne();

        TutorialEvents.RaiseItemPlacedOnShelf();
    }

    /// <summary>
    /// 플레이어가 들고 있는 상품 박스(SmallBoxInteraction)를 선반에 다시 올려놓는다.
    /// </summary>
    private void PlaceSmallBoxOnShelf(SmallBoxInteraction smallBox)
    {
        var heldData = smallBox.GetComponent<BlockIsHolding>();
        if (heldData == null) return;

        int idx = slot.ItemCount;
        Transform snap = slot.GetSnapPoint(idx);

        // 플레이어 손에서 해제
        heldData.isHeld = false;
        if (hold.heldObject == heldData)
            hold.heldObject = null;

        // 선반 스냅 포인트에 배치
        var t = smallBox.transform;
        t.SetParent(snap, false);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = new Vector3(1.4f, 1.4f, 1.4f);

        // 물리 끄기 (선반 위에서 안 떨어지게)
        var rb = smallBox.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.detectCollisions = true;
        }

        // originalParent 갱신
        heldData.originalParent = snap;

        // 슬롯에 등록 (가격표 생성 포함)
        slot.RegisterNewItem(smallBox.gameObject);

        Debug.Log($"[SlotFill] 상품 박스 재진열: {smallBox.name} → {name}");
    }

    static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    /// <summary>
    /// 렌더러 전체 영역을 감싸는 BoxCollider를 루트에 추가.
    /// display prefab의 MeshCollider 빈틈으로 레이캐스트가 뒤의 선반에 맞는 문제를 방지한다.
    /// </summary>
    static void AddBoundsCollider(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds worldBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            worldBounds.Encapsulate(renderers[i].bounds);

        var box = go.AddComponent<BoxCollider>();
        box.center = go.transform.InverseTransformPoint(worldBounds.center);
        Vector3 ls = go.transform.lossyScale;
        box.size = new Vector3(
            worldBounds.size.x / Mathf.Abs(ls.x),
            worldBounds.size.y / Mathf.Abs(ls.y),
            worldBounds.size.z / Mathf.Abs(ls.z)
        );
    }

    public void GetPrompts(PlayerInteractionContext ctx, List<InteractionPrompt> prompts)
    {
        if (slot.IsFull) return;
        if (hold == null || hold.heldObject == null) return;

        // 상품 박스를 들고 있으면 → 재진열 가능
        if (hold.heldObject.GetComponent<SmallBoxInteraction>() != null)
        {
            prompts.Add(new InteractionPrompt(InputHint.LMB, "Display"));
            return;
        }

        // 배달 박스를 들고 있고 열려 있으면 → 진열 가능
        if (ctx.holdingBox)
        {
            var box = hold.heldObject.GetComponent<BoxContainer>();
            if (box != null && box.IsOpen)
            {
                prompts.Add(new InteractionPrompt(InputHint.LMB, "Display"));
            }
        }
    }

}
