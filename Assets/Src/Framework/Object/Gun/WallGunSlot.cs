using UnityEngine;

[RequireComponent(typeof(ShelfSlot), typeof(Collider))]
public class WallGunSlot : MonoBehaviour, IInteractable
{
    [Header("총 걸 위치/각도 보정 (SnapPoint 기준)")]
    [SerializeField] private Vector3 localOffset = Vector3.zero;
    [SerializeField] private Vector3 localEuler  = Vector3.zero;

    private ShelfSlot shelfSlot;
    private PlayerObjectHoldController hold;
    private Transform snapPoint;

    private void Awake()
    {
        shelfSlot = GetComponent<ShelfSlot>();
        hold      = PlayerObjectHoldController.Instance;

        // ShelfSlot 안쪽 포인트(0번)를 기본 스냅 포인트로 사용
        snapPoint = shelfSlot.GetSnapPoint(0);
        if (snapPoint == null) snapPoint = transform;
    }

    public void Interact()
    {
        Debug.Log($"[WallGunSlot] Interact on {name}");

        if (hold == null)
        {
            hold = PlayerObjectHoldController.Instance;
            if (hold == null)
            {
                Debug.LogWarning("[WallGunSlot] holdController 없음");
                return;
            }
        }

        var held = hold.heldObject;
        Debug.Log($"[WallGunSlot] 현재 held = {(held ? held.name : "없음")}, slot HasItem={shelfSlot.HasItem}");

        // 1) 뭔가 들고 있으면 → 걸려고 시도
        if (held != null /* && !shelfSlot.IsFull */)
        {
            TryHangGun(held);
            return;
        }

        // 2) 빈손이고, 슬롯에 아이템 있으면 → 다시 집기
        if (held == null && shelfSlot.HasItem)
        {
            TryTakeGun();
        }
    }

    private void TryHangGun(BlockIsHolding held)
    {
        Debug.Log($"[WallGunSlot] TryHangGun: {held.name}");

        // 들고 있는 게 총인지 확인
        var gun = held.GetComponent<GunInteraction>();
        if (gun == null)
        {
            Debug.Log("[WallGunSlot] GunInteraction 없음 → 총이 아님, 무시");
            return;
        }

        // Player Hold 상태 해제
        held.isHeld = false;
        if (hold.heldObject == held)
            hold.heldObject = null;

        // 슬롯 스냅 포인트로 붙이기
        var t = held.transform;
        t.SetParent(snapPoint, false);
        t.localPosition = localOffset;
        t.localRotation = Quaternion.Euler(localEuler);

        // 떨어지지 않도록 물리 끄기 + 콜라이더 끄기
        held.DisablePhysics();
        var col = held.GetComponentInChildren<Collider>();
        if (col != null) col.enabled = false;

        // 가격표 시스템에 등록
        shelfSlot.RegisterNewItem(held.gameObject);

        Debug.Log("[WallGunSlot] 총 벽에 걸림 + RegisterNewItem 호출");
    }

    private void TryTakeGun()
    {
        Debug.Log("[WallGunSlot] TryTakeGun");

        GameObject go = shelfSlot.PopItem();
        if (go == null)
        {
            Debug.LogWarning("[WallGunSlot] PopItem 결과 없음");
            return;
        }

        var gun = go.GetComponent<GunInteraction>();
        var col = go.GetComponentInChildren<Collider>();
        if (col != null) col.enabled = true;

        if (gun != null)
        {
            gun.PickUp();
            Debug.Log("[WallGunSlot] 벽에서 총 꺼내서 다시 집음");
        }
        else
        {
            Debug.LogWarning("[WallGunSlot] GunInteraction 없음");
        }
    }
}