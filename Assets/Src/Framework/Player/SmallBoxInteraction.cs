using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BlockIsHolding))]
public class SmallBoxInteraction : MonoBehaviour, IPickable, IHasInteractionPrompts
{
    [Header("참조")]
    [SerializeField] private PlayerObjectHoldController holdController;
    [SerializeField] private Animator animator;

    [Header("내용물(총기)")]
    [SerializeField] private GameObject gunPrefab; // 개봉 후 화면 중앙에 보일 총기 프리팹
    
    // 작은 박스를 들 때 카메라 기준 오프셋(원하는 값으로 인스펙터에서 조절)
    [Header("손에 들렸을 때 위치/각도 보정")]
    [SerializeField] private Vector3 heldLocalOffset = new Vector3(0f, 0.7f, 0.3f);
    [SerializeField] private Vector3 heldLocalEuler = new Vector3(0f, 90f, 0f);

    private BlockIsHolding holdData;
    private bool isOpened = false;

    private static readonly int HashOpen = Animator.StringToHash("Open");


    private void Awake()
    {
        holdData = GetComponent<BlockIsHolding>();
        if (holdController == null) holdController = FindFirstObjectByType<PlayerObjectHoldController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (animator != null)
            animator.enabled = false;
    }

    /// Awake에서 못 찾았을 때를 대비한 지연 초기화 (Instance 싱글턴 우선)
    private PlayerObjectHoldController HoldCtrl
    {
        get
        {
            if (holdController == null)
                holdController = PlayerObjectHoldController.Instance;
            if (holdController == null)
                holdController = FindFirstObjectByType<PlayerObjectHoldController>();
            return holdController;
        }
    }

    // 클릭(Interaction)으로 호출
    public void Interact()
    {
        Debug.Log($"[SmallBox] Interact called on {name} | isOpened={isOpened} | HoldCtrl={HoldCtrl} | heldObject={HoldCtrl?.heldObject}");
        if (isOpened) { Debug.Log("[SmallBox] BLOCKED: isOpened"); return; }

        // 이미 뭔가 들고 있으면 이 박스는 안 집기 (필요에 따라 변경 가능)
        if (HoldCtrl != null && HoldCtrl.heldObject != null)
        { Debug.Log("[SmallBox] BLOCKED: already holding " + HoldCtrl.heldObject.name); return; }

        // 아직 안 들고 있으면 → 손으로 집기 (화면 중앙 고정)
        Debug.Log("[SmallBox] → calling PickUp()");
        PickUp();
    }

    // E키(OpenHeldBox 액션)에서 호출될 함수
    public void OpenBox()
    {
        if (isOpened) return;
        if (!holdData.isHeld) return;   // 손에 들고 있을 때만

        isOpened = true;

        // 여기서만 Animator 켜고, 바로 Open 재생
        animator.enabled = true;

        // (혹시 트리거 말고 클립 이름 직접 재생하고 싶으면)
        animator.Play("Open", 0, 0f);
    }

    // 애니메이션 끝에서 호출: 박스 제거 + 총기를 WallGunSlot 배치 모드로
    public void SpawnGunFromBox()
    {
        if (gunPrefab == null) return;

        Transform boxTr = transform;
        Vector3 pos = boxTr.position;
        Quaternion rot = boxTr.rotation;

        // 들고 있던 상태 정리
        if (holdController != null && holdController.heldObject == holdData)
            holdController.heldObject = null;
        holdData.isHeld = false;

        // 상자 제거
        Destroy(gameObject);

        // 총 생성 (월드 루트에)
        GameObject gun = Instantiate(gunPrefab, pos, rot);

        // WallGunSlot 배치 모드 진입
        if (WallGunPlacementMode.Instance != null)
            WallGunPlacementMode.Instance.EnterMode(gun);
    }

    // ===== IHasInteractionPrompts 구현 =====

    public void GetPrompts(PlayerInteractionContext ctx, List<InteractionPrompt> prompts)
    {
        if (holdData.isHeld)
        {
            if (!isOpened)
                prompts.Add(new InteractionPrompt(InputHint.E, "Open Box"));
        }
        else
        {
            prompts.Add(new InteractionPrompt(InputHint.LMB, "Pick Up"));
        }
    }

    // ===== IPickable 구현 =====

    public void PickUp()
    {
        if (HoldCtrl == null) { Debug.LogWarning("[SmallBox] PickUp FAILED: HoldCtrl is null"); return; }
        if (HoldCtrl.heldObject != null) { Debug.LogWarning("[SmallBox] PickUp FAILED: already holding " + HoldCtrl.heldObject.name); return; }

        Debug.Log($"[SmallBox] PickUp proceeding | holdData={holdData} | holdData.isHeld={holdData?.isHeld}");

        // 1) 선반에 꽂혀 있던 상태라면, 선반과의 관계를 먼저 끊는다
        DetachFromShelfIfNeeded();

        // 2) 실제로 플레이어 손에 들기
        holdController.SetHeldObject(holdData);
        Debug.Log($"[SmallBox] SetHeldObject done | isHeld={holdData?.isHeld}");

        // 3) 작은 박스는 위치/각도를 따로 보정해서 화면에 잘 보이게
        var t = holdData.transform;
        t.localPosition = heldLocalOffset;
        t.localRotation = Quaternion.Euler(heldLocalEuler);
    }

    public void SetDown()
    {
        if (!holdData.isHeld) return;

        var rb = GetComponent<Rigidbody>();
        var col = GetComponentInChildren<Collider>();

        transform.SetParent(holdData.originalParent, true);
        if (col) col.enabled = true;

        holdData.EnablePhysics();

        holdData.isHeld = false;
        if (holdController != null && holdController.heldObject == holdData)
            holdController.heldObject = null;

        Debug.Log("상자 내려 놓음");
    }

    public void ThrowObject()
    {
        if (!holdData.isHeld) return;

        var rb = GetComponent<Rigidbody>();
        var col = GetComponentInChildren<Collider>();

        transform.SetParent(holdData.originalParent, true);
        if (col) col.enabled = true;

        holdData.EnablePhysics();

        if (rb)
            rb.AddForce(Camera.main.transform.forward * 10f, ForceMode.Impulse);

        holdData.isHeld = false;
        if (holdController != null && holdController.heldObject == holdData)
            holdController.heldObject = null;
    }

    // 선반에서 집어 들 때, 선반과의 관계를 끊어준다
    private void DetachFromShelfIfNeeded()
    {
        // 이 박스가 선반(ShelfSlot)의 자식이라면
        var shelf = GetComponentInParent<ShelfSlot>();
        if (shelf != null)
        {
            // 1) NPC가 더 이상 이 물건을 선반에 있다고 보지 않도록 리스트에서 제거
            shelf.RemoveItem(gameObject);

            // 2) 계층에서도 선반 밑에서 떼어낸다
            transform.SetParent(null);

            // 3) 이제 이 박스의 "원래 부모"는 월드 루트(null)가 된다
            holdData.originalParent = transform.parent;  // = null
        }
    }
}