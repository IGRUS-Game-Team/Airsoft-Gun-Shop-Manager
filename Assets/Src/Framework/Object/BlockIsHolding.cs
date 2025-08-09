using UnityEngine;

/// <summary>
/// 박스가 '잡힌 상태'인지와 원래 부모 정보를 저장하는 컴포넌트입니다.
/// 잡기(PickUp) / 놓기(Drop) 동작 시 위치 및 물리 설정 복구에 사용됩니다.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BlockIsHolding : MonoBehaviour
{
    /// <summary>
    /// 현재 이 오브젝트가 잡혀 있는지 여부
    /// </summary>
    public bool isHeld = false;

    /// <summary>
    /// 원래 이 오브젝트가 소속되어 있던 부모 Transform (놓을 때 복원)
    /// </summary>
    public Transform originalParent;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalParent = transform.parent;
    }

    /// <summary>
    /// 잡기 상태일 때 Rigidbody를 비활성화합니다.
    /// </summary>
    public void DisablePhysics()
    {
        if (rb == null) return;

        rb.isKinematic = true;
        rb.detectCollisions = false;
        rb.useGravity = false;
    }

    /// <summary>
    /// 놓기 상태일 때 Rigidbody를 활성화합니다.
    /// </summary>
    public void EnablePhysics()
    {
        if (rb == null) return;

        rb.isKinematic = false;
        rb.detectCollisions = true;
        rb.useGravity = true;
    }
}
