using UnityEngine;

[RequireComponent(typeof(BlockIsHolding))]
public class SmallBoxInteraction : MonoBehaviour, IPickable
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

    // 클릭(Interaction)으로 호출
    public void Interact()
    {
        Debug.Log("[SmallBox] Interact called on " + name);
        if (isOpened) return;

        // 이미 뭔가 들고 있으면 이 박스는 안 집기 (필요에 따라 변경 가능)
        if (holdController != null && holdController.heldObject != null)
            return;

        // 아직 안 들고 있으면 → 손으로 집기 (화면 중앙 고정)
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

    // 애니메이션 끝에서 호출: 박스 제거 + 총기 스폰
    public void SpawnGunFromBox()
    {
        // 0) 혹시 한 번 이상 실행되지 않게 방어
        if (gunPrefab == null) return;

        // 1) 상자가 있던 위치/회전/부모 기억해 두기
        Transform boxTr = transform;           // SmallBoxInteraction 달린 오브젝트 (작은 박스 루트)
        Transform parent = boxTr.parent;        // 보통 holdPoint 밑에 매달려 있을 것
        Vector3 pos = boxTr.position;
        Quaternion rot = boxTr.rotation;

        // 2) 들고 있던 상태 정리 (holdController랑 끊기)
        if (holdController != null && holdController.heldObject == holdData)
        {
            holdController.heldObject = null;
        }
        holdData.isHeld = false;

        // 3) 상자 오브젝트 제거
        Destroy(gameObject);

        // 4) 총 프리팹을 "상자가 있던 자리"에 생성
        GameObject gun = Instantiate(gunPrefab, pos, rot, parent);

        // // 만약 parent가 holdPoint 라면, local 기준으로 딱 붙이고 싶으면 이렇게:
        // gun.transform.localPosition = Vector3.zero;
        // gun.transform.localRotation = Quaternion.identity;

        // 필요하면 여기서 총의 리지드바디/콜라이더 끄기 (손에 고정용)
        var rb = gun.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.detectCollisions = false;
        }

        var col = gun.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // 나중에 "총 들고 있는 상태 관리"를 할 시스템이 있으면
        // 거기에 gun을 넘겨주는 코드를 여기에 추가하면 됨.
    }

    // ===== IPickable 구현 =====

    public void PickUp()
    {
        if (holdController == null) return;
        if (holdController.heldObject != null) return;

        // 1) 선반에 꽂혀 있던 상태라면, 선반과의 관계를 먼저 끊는다
        DetachFromShelfIfNeeded();

        // 2) 실제로 플레이어 손에 들기
        holdController.SetHeldObject(holdData);

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
    }

    public void ThrowObject()
    {
        // 개별 박스를 던지는 기능이 필요하면 나중에 여기에 구현
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