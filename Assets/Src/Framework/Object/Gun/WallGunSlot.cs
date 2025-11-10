using UnityEngine;

[RequireComponent(typeof(ShelfSlot))]
public class WallGunSlot : MonoBehaviour, IInteractable
{
    [Header("총 걸 위치/각도 보정(슬롯 기준 로컬)")]
    [SerializeField] private Vector3 localOffset = Vector3.zero;
    [SerializeField] private Vector3 localEuler  = Vector3.zero;

    private ShelfSlot shelfSlot;
    private PlayerObjectHoldController hold;
    private Transform snapPoint;   // 총이 실제로 붙을 위치 (ShelfSlot의 point[0])

    private void Awake()
    {
        shelfSlot = GetComponent<ShelfSlot>();
        hold      = PlayerObjectHoldController.Instance;

        // ShelfSlot의 안쪽 포인트 하나만 쓰자 (0번째)
        snapPoint = shelfSlot.GetSnapPoint(0);
        if (snapPoint == null)
            snapPoint = transform; // 없으면 자기 자신
    }

    public void Interact()
    {
        if (hold == null) return;

        var held = hold.heldObject;

        // 1) 플레이어가 총을 들고 있고, 슬롯이 비어 있으면 → 벽에 건다
        if (held != null && !shelfSlot.IsFull)
        {
            TryHangGun(held);
            return;
        }

        // 2) 플레이어가 빈손이고, 슬롯에 총이 있으면 → 다시 집기
        if (held == null && shelfSlot.HasItem)
        {
            TryTakeGun();
        }
    }

    private void TryHangGun(BlockIsHolding held)
    {
        // 들고 있는 게 총인지 확인 (GunInteraction 붙어 있는지)
        var gun = held.GetComponent<GunInteraction>();
        if (gun == null) return;   // 총이 아니면 무시

        // 1) 플레이어 손에서 떼기
        held.isHeld = false;
        if (hold.heldObject == held)
            hold.heldObject = null;

        // 2) 부모를 벽 슬롯의 snapPoint로 변경
        var t = held.transform;
        t.SetParent(snapPoint, false);
        t.localPosition = localOffset;
        t.localRotation = Quaternion.Euler(localEuler);

        // 3) 물리 끄고(떨어지지 않게), 콜라이더는 필요하면 꺼도 됨
        held.DisablePhysics();
        var col = held.GetComponentInChildren<Collider>();
        if (col != null) col.enabled = false;

        // 4) ShelfSlot에 등록해서 가격표 이벤트 발생시키기
        shelfSlot.RegisterNewItem(held.gameObject);

        Debug.Log("[WallGunSlot] 총 벽에 걸림 + RegisterNewItem 호출");
    }

    private void TryTakeGun()
    {
        // ShelfSlot에서 바깥쪽(마지막) 아이템 꺼내기
        GameObject go = shelfSlot.PopItem();
        if (go == null) return;

        var gunHold = go.GetComponent<BlockIsHolding>();
        var gun     = go.GetComponent<GunInteraction>();
        if (gunHold == null || gun == null) return;

        // 콜라이더 다시 켜주기
        var col = go.GetComponentInChildren<Collider>();
        if (col != null) col.enabled = true;

        // 그냥 총의 PickUp()을 호출하면
        //  - PlayerObjectHoldController.SetHeldObject()
        //  - 물리 끄기
        //  - 화면 위치/각도 보정
        // 까지 기존 로직 그대로 사용 가능
        gun.PickUp();

        Debug.Log("[WallGunSlot] 벽에서 총 꺼내서 다시 집음");
    }
}