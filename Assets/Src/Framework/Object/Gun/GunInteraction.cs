using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BlockIsHolding))]
public class GunInteraction : MonoBehaviour, IPickable, IHasInteractionPrompts
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

    public void GetPrompts(PlayerInteractionContext ctx, List<InteractionPrompt> prompts)
    {
        if (!holdData.isHeld)
        {
            prompts.Add(new InteractionPrompt(InputHint.LMB, "Pick Up"));
        }
        // 들고 있을 때: R/G(Throw/Drop)는 라우터에서 자동 추가
        // Fire/Aim은 사격장 전용 시스템(ActiveGun)에서 처리
    }

    public void SetDown()
    {
        if (!holdData.isHeld) return;

        transform.SetParent(holdData.originalParent, true);
        if (col != null) col.enabled = true;
        if (rb != null) { rb.isKinematic = false; rb.detectCollisions = true; rb.useGravity = true; }

        holdData.isHeld = false;
        holdController.heldObject = null;
    }

    public void ThrowObject()
    {
        if (!holdData.isHeld) return;

        transform.SetParent(holdData.originalParent, true);
        if (col != null) col.enabled = true;
        if (rb != null)
        {
            rb.isKinematic = false; rb.detectCollisions = true; rb.useGravity = true;
            rb.AddForce(Camera.main.transform.forward * 10f, ForceMode.Impulse);
        }

        holdData.isHeld = false;
        holdController.heldObject = null;
    }
}