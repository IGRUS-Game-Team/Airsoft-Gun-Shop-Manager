using UnityEngine;

[RequireComponent(typeof(BlockIsHolding))]
public class GunInteraction : MonoBehaviour, IPickable
{
    [SerializeField] private PlayerObjectHoldController holdController;
    [SerializeField] private Vector3 heldLocalOffset = new Vector3(0f, 0.7f, 0.3f);
    [SerializeField] private Vector3 heldLocalEuler  = new Vector3(0f, 0f, 0f);

    private BlockIsHolding holdData;
    private Rigidbody rb;
    private Collider col;

    private void Awake()
    {
        holdData = GetComponent<BlockIsHolding>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (holdController == null)
            holdController = FindFirstObjectByType<PlayerObjectHoldController>();
    }

    public void Interact()
    {
        if (holdController != null && holdController.heldObject != null)
        return;
        // 상호작용(클릭)하면 그냥 집기
        PickUp();
    }

    // === IPickable ===
    public void PickUp()
    {
        if (holdController == null) return;
        if (holdController.heldObject != null) return;

        holdController.SetHeldObject(holdData);

        // 손에 들릴 때 물리 끄기
        holdData.DisablePhysics();

        if (col != null)
            col.enabled = false;

        var t = holdData.transform;
        t.localPosition = heldLocalOffset;
        t.localRotation = Quaternion.Euler(heldLocalEuler);
    }

    public void SetDown()
    {

    }

    public void ThrowObject()
    {
        // 필요하면 나중에 구현
    }
}