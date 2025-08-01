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
    private BlockIsHolding holdData;

    private void Awake()
    {
        holdData = GetComponent<BlockIsHolding>();
    }

    public void Interact()
    {
        if (holdData.isHeld)
        {
            // 아무 동작 안 함, 별도 키(G/R)로 제어
        }
        else
        {
            PickUp();
        }
    }

    public void PickUp()
    {
        PlayerObjectHoldController.Instance.SetHeldObject(holdData);
    }

    public void SetDown()
    {
        if (!holdData.isHeld) return;

        Vector3 placePos = FindObjectOfType<PlacePreview>().PreviewPosition + Vector3.up * 0.5f;
        holdData.transform.position = placePos;
        holdData.transform.rotation = Quaternion.identity;

        holdData.transform.SetParent(holdData.originalParent);

        var col = holdData.GetComponentInChildren<Collider>();
        if (col != null) col.enabled = true;

        holdData.EnablePhysics();
        holdData.isHeld = false;
        PlayerObjectHoldController.Instance.heldObject = null;
    }

    public void ThrowObject()
    {
        if (!holdData.isHeld) return;

        holdData.transform.SetParent(holdData.originalParent);

        var col = holdData.GetComponentInChildren<Collider>();
        if (col != null) col.enabled = true;

        holdData.EnablePhysics();

        Rigidbody rb = holdData.GetComponent<Rigidbody>();
        rb.AddForce(Camera.main.transform.forward * 10f, ForceMode.Impulse);

        holdData.isHeld = false;
        PlayerObjectHoldController.Instance.heldObject = null;
    }
}

