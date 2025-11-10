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

        // 1) 던질 때는 무조건 HoldPoint에서 분리해서 월드 기준으로 만든다
        held.transform.SetParent(null, true);   // ← 여기서 부모 완전 끊김

        // 2) 콜라이더 켜기
        var col = held.GetComponentInChildren<Collider>();
        if (col != null) col.enabled = true;

        // 3) 물리 켜기
        held.EnablePhysics();

        // 4) 힘을 앞으로 가해 던지기
        var rb = held.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(Camera.main.transform.forward * throwForce, ForceMode.Impulse);
        }

        // 5) 상태 정리
        held.isHeld = false;
        PlayerObjectHoldController.Instance.heldObject = null;

        Debug.Log("[Throw] 던짐, 부모=null");
    }
}
