using UnityEngine;

/// <summary>
/// 박정민 25/8/1
/// 박스에 붙는 스크립트로, 박스를 플레이어가 집거나 놓을 수 있도록 합니다.
/// 박스를 클릭 시 집고, R 키로 던지고, G 키로 바닥에 놓는 기능을 수행합니다.
/// IPickable 인터페이스를 구현하여 상호작용 시 집기/놓기 동작을 수행합니다.
/// </summary>

[RequireComponent(typeof(BlockIsHolding))]
public class BoxInteractionBehaviour : MonoBehaviour, IPickable
{
    [SerializeField] PlayerObjectHoldController holdController; // 인스펙터에 드래그
    [SerializeField] BoxContainer box;                          // 같은 프리팹에 붙은 컴포넌트
    private BlockIsHolding holdData;

    void Awake()
    {
        holdData = GetComponent<BlockIsHolding>();
        if (box == null) box = GetComponent<BoxContainer>();
        if (holdController == null) holdController = FindFirstObjectByType<PlayerObjectHoldController>();
    }

    // 라우터가 호출
    public void Interact()
    {
        if (holdData.isHeld)
        {
            Debug.Log("박스 언박싱");
            // 들고 있는 상태에서 다시 클릭 → 뚜껑 토글
            box?.ToggleLid();
        }
        else
        {
            PickUp();
        }
    }

    public void PickUp()
    {
        if (holdController == null) return;
        holdController.SetHeldObject(holdData); // 기존 컨트롤러 API 그대로 사용
    }

    public void SetDown()
    {
        if (!holdData.isHeld) return;
        var rb = GetComponent<Rigidbody>();
        var col = GetComponentInChildren<Collider>();

        transform.SetParent(holdData.originalParent, true);
        if (col) col.enabled = true;

        // 물리 ON
        if (rb) { rb.isKinematic = false; rb.detectCollisions = true; rb.useGravity = true; }

        holdData.isHeld = false;
        holdController.heldObject = null;
    }

    public void ThrowObject()
    {
        if (!holdData.isHeld) return;

        var rb = GetComponent<Rigidbody>();
        var col = GetComponentInChildren<Collider>();

        transform.SetParent(holdData.originalParent, true);
        if (col) col.enabled = true;

        if (rb)
        {
            rb.isKinematic = false; rb.detectCollisions = true; rb.useGravity = true;
            rb.AddForce(Camera.main.transform.forward * 10f, ForceMode.Impulse);
        }

        holdData.isHeld = false;
        holdController.heldObject = null;
    }
}
