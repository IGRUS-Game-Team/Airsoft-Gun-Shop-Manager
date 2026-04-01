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

        // IPickable 구현이 있으면 그쪽에 위임 (상태 정리를 해당 컴포넌트가 담당)
        var pickable = held.GetComponent<IPickable>();
        if (pickable != null)
        {
            pickable.ThrowObject();
            TutorialEvents.RaiseThrew();
            return;
        }

        // IPickable이 없는 경우 직접 처리 (폴백)
        held.transform.SetParent(null, true);

        var col = held.GetComponentInChildren<Collider>();
        if (col != null) col.enabled = true;

        held.EnablePhysics();

        var rb = held.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(Camera.main.transform.forward * throwForce, ForceMode.Impulse);
        }

        held.isHeld = false;
        PlayerObjectHoldController.Instance.heldObject = null;
        TutorialEvents.RaiseThrew();

        Debug.Log("[Throw] 던짐, 부모=null");
    }
}
