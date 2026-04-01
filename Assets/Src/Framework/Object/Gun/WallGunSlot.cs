using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShelfSlot), typeof(Collider))]
public class WallGunSlot : MonoBehaviour, IInteractable, IHasInteractionPrompts
{
    [Header("총 걸 위치/각도 보정 (SnapPoint 기준)")]
    [SerializeField] private Vector3 localOffset = Vector3.zero;
    [SerializeField] private Vector3 localEuler  = Vector3.zero;

    private ShelfSlot shelfSlot;
    private PlayerObjectHoldController hold;
    private GameObject _hungGun;  // HangGunDirect로 걸린 총 (가격표 없음)

    private void Awake()
    {
        shelfSlot = GetComponent<ShelfSlot>();
        hold      = PlayerObjectHoldController.Instance;
    }

    /// <summary>현재 아이템 수 기준으로 다음 스냅 포인트 반환.</summary>
    private Transform GetNextSnapPoint()
    {
        int idx = shelfSlot.ItemCount;
        if (idx >= ShelfSlot.Capacity) return null;
        return shelfSlot.GetSnapPoint(idx) ?? transform;
    }

    /// <summary>슬롯이 비어있는지 확인.</summary>
    public bool IsEmpty => !shelfSlot.IsFull;

    /// <summary>스냅 포인트 기준 월드 위치/회전을 대상 Transform에 적용.</summary>
    public void PositionAtSnap(Transform t)
    {
        var snap = GetNextSnapPoint();
        if (snap == null) snap = transform;
        t.position = snap.TransformPoint(localOffset);
        t.rotation = snap.rotation * Quaternion.Euler(localEuler);
    }

    /// <summary>
    /// 플레이어 손을 거치지 않고 총기를 직접 슬롯에 건다.
    /// WallGunPlacementMode에서 호출.
    /// </summary>
    public bool HangGunDirect(GameObject gunObj)
    {
        if (gunObj == null || !IsEmpty) return false;

        var gun = gunObj.GetComponent<GunInteraction>();
        if (gun == null) return false;

        var snap = GetNextSnapPoint();
        if (snap == null) return false;

        gunObj.SetActive(true);

        // 스냅 포인트에 부착
        var t = gunObj.transform;
        t.SetParent(snap, false);
        t.localPosition = localOffset;
        t.localRotation = Quaternion.Euler(localEuler);
        t.localScale = Vector3.one;

        // 물리 끄기
        var blockHold = gunObj.GetComponent<BlockIsHolding>();
        if (blockHold != null) blockHold.DisablePhysics();

        var col = gunObj.GetComponentInChildren<Collider>();
        if (col != null) col.enabled = false;

        // 가격표 없이 슬롯에 등록
        shelfSlot.ReRegisterItem(gunObj);

        Debug.Log($"[WallGunSlot] HangGunDirect: {gunObj.name} → {name}");
        return true;
    }

    public void GetPrompts(PlayerInteractionContext ctx, List<InteractionPrompt> prompts)
    {
        // 총기 배치모드이거나 총을 들고 있을 때만 프롬프트 표시
        if (!WallGunPlacementMode.IsActive && !(ctx.holdingGun)) return;

        if (IsEmpty)
            prompts.Add(new InteractionPrompt(InputHint.LMB, "Hang"));
        else
            prompts.Add(new InteractionPrompt(InputHint.LMB, "Take"));
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
        if (held == null && !IsEmpty)
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

        if (shelfSlot.IsFull)
        {
            Debug.Log("[WallGunSlot] 슬롯이 가득 참");
            return;
        }

        var snap = GetNextSnapPoint();
        if (snap == null) return;

        // Player Hold 상태 해제
        held.isHeld = false;
        if (hold.heldObject == held)
            hold.heldObject = null;

        // 슬롯 스냅 포인트로 붙이기
        var t = held.transform;
        t.SetParent(snap, false);
        t.localPosition = localOffset;
        t.localRotation = Quaternion.Euler(localEuler);
        t.localScale = Vector3.one;

        // 떨어지지 않도록 물리 끄기 + 콜라이더 끄기
        held.DisablePhysics();
        var col = held.GetComponentInChildren<Collider>();
        if (col != null) col.enabled = false;

        // 가격표 없이 슬롯에 재등록
        shelfSlot.ReRegisterItem(held.gameObject);

        Debug.Log("[WallGunSlot] 총 벽에 다시 걸림");
    }

    private void TryTakeGun()
    {
        Debug.Log("[WallGunSlot] TryTakeGun");

        // HangGunDirect로 걸린 총 우선, 아니면 ShelfSlot에서 꺼냄
        GameObject go;
        if (_hungGun != null)
        {
            go = _hungGun;
            _hungGun = null;
        }
        else
        {
            go = shelfSlot.PopItem();
        }

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