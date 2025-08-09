using UnityEngine;

/// <summary>
/// R 키로 현재 들고 있는 오브젝트를 던지는 기능을 수행합니다.
/// </summary>
public class PlayerObjectThrowBoxController : MonoBehaviour
{
    [SerializeField] float throwForce = 10f;

    private void Start()
    {
        InteractionController.Instance.OnThrowBox += Drop;
    }

    private void Drop()
    {
        var held = PlayerObjectHoldController.Instance.heldObject;
        if (held == null) return;

        // 부모 복원
        held.transform.SetParent(held.originalParent);

        // 콜라이더 켜기
        var col = held.GetComponentInChildren<Collider>();
        if (col != null) col.enabled = true;

        // 물리 켜기
        held.EnablePhysics();

        // 힘을 앞으로 가해 던지기
        Rigidbody rb = held.GetComponent<Rigidbody>();
        rb.AddForce(Camera.main.transform.forward * throwForce, ForceMode.Impulse);

        // 상태 해제
        held.isHeld = false;
        PlayerObjectHoldController.Instance.heldObject = null;
    }
}
