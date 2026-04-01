using UnityEngine;
using System.Collections;

/// <summary>
/// 플레이어 클릭 입력을 컨텍스트에 따라 적절한 핸들러로 라우팅.
///
/// [우선순위] (위에서부터 검사, 먼저 매칭되면 아래는 무시)
/// 1. PlacementManager.IsPlacing → 가구 배치 시스템이 처리
/// 2. WallGunPlacementMode.IsActive → 벽걸이 총 배치 시스템이 처리
/// 3. PriceCardController.IsAnyPriceUIOpen → 가격 수정 UI 열림 → 무시
/// 4. UIUtility.IsPointerOverUI() → UI 위 클릭 → 무시
/// 5. hold.heldObject != null → 들고 있는 오브젝트의 IInteractable
/// 6. SmallBoxInteraction → 선반 위 상품 직접 클릭 (맨손일 때)
/// 7. ShelfSlot + 오브젝트 보유 → SlotFillBehaviour (진열)
/// 8. IInteractable → 일반 상호작용
///
/// [주의]
/// - InteractionController.OnClick 이벤트를 구독하여 작동
/// - InteractionController가 늦게 초기화될 수 있어 코루틴으로 재시도
/// </summary>
public class InputContextRouter : MonoBehaviour
{
    private bool clickRequested;
    private PlayerObjectHoldController hold;

    private void Awake()
    {
        hold = FindFirstObjectByType<PlayerObjectHoldController>();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        var ic = InteractionController.Instance;
        if (ic != null) ic.OnClick -= OnClick;
        StopAllCoroutines();
    }

    private void TrySubscribe()
    {
        var ic = InteractionController.Instance;
        if (ic != null)
        {
            ic.OnClick += OnClick;
            Debug.Log("[Router] Subscribed to InteractionController");
        }
        else
        {
            StartCoroutine(SubscribeWhenReady());
        }
    }

    private IEnumerator SubscribeWhenReady()
    {
        while (InteractionController.Instance == null) yield return null;
        InteractionController.Instance.OnClick += OnClick;
        Debug.Log("[Router] Subscribed (late) to InteractionController");
    }

    private void OnClick() => clickRequested = true;

    private void Update()
    {
        if (!clickRequested) return;
        clickRequested = false;

        // 배치 모드 중에는 월드 상호작용 전부 무시 (좌클릭은 PlacementManager가 처리)
        if (PlacementManager.Instance != null && PlacementManager.Instance.IsPlacing)
            return;

        // WallGunSlot 배치 모드 중에는 월드 상호작용 무시
        if (WallGunPlacementMode.IsActive)
            return;

        // 가격 수정 UI가 떠 있으면, 월드 상호작용 전부 무시
        if (PriceCardController.IsAnyPriceUIOpen)
        {
            // 필요하면 로그
            // Debug.Log("[Router] Click ignored: Price UI open");
            return;
        }

        if (UIUtility.IsPointerOverUI()) return;

        if (RaycastDetector.Instance == null)
        { 
            Debug.LogWarning("[Router] RaycastDetector missing");
        }       
        var hit = (RaycastDetector.Instance != null) ? RaycastDetector.Instance.HitObject : null;

        // 1) 들고 있는 오브젝트 우선
        if (hold != null && hold.heldObject != null)
        {
            bool aimedAtHeld = hit != null && (hit.transform.root == hold.heldObject.transform.root);
            if (hit == null || aimedAtHeld)
            {
                var intrHeld = hold.heldObject.GetComponentInParent<IInteractable>();
                if (intrHeld != null) { intrHeld.Interact(); return; }
            }
        }

        // 1.5) 맨손일 때: 상품 또는 선반을 맞췄으면 SmallBox 우선 처리
        //      단, 가격표를 직접 맞춘 경우엔 건너뛴다
        if (hit != null && (hold == null || hold.heldObject == null))
        {
            if (hit.GetComponentInParent<PriceCardController>() == null)
            {
                // (a) RaycastAll 덕분에 hit 자체가 상품일 수 있음 → 직접 찾기
                var small = hit.GetComponentInParent<SmallBoxInteraction>();

                // (b) 선반 콜라이더를 맞췄을 때 → 자식 상품 찾기 (폴백)
                if (small == null)
                {
                    var shelf = hit.GetComponentInParent<ShelfSlot>();
                    if (shelf != null)
                        small = shelf.GetComponentInChildren<SmallBoxInteraction>();
                }

                if (small != null)
                {
                    small.Interact();
                    return;
                }
            }
        }

        // 1.7) 무언가를 들고 있을 때 선반 영역 클릭 → 선반(SlotFill)으로 우회
        //      (RaycastAll이 상품을 우선 반환하므로, 선반까지 도달 못하는 문제 방지)
        if (hit != null && hold != null && hold.heldObject != null)
        {
            var shelf = hit.GetComponentInParent<ShelfSlot>();
            if (shelf != null)
            {
                var slotIntr = shelf.GetComponent<IInteractable>();
                if (slotIntr != null)
                {
                    slotIntr.Interact();
                    return;
                }
            }
        }

        // 2) 평소처럼 레이캐스트 대상 처리
        if (hit == null) return;
        var intr = hit.GetComponentInParent<IInteractable>();
        if (intr != null) intr.Interact();
    }
}
