using UnityEngine;
using System.Collections;

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

        // 1.5) 맨손일 때: "선반(ShelfSlot)" 을 맞췄을 때만, 그 위 SmallBox를 우선 처리
        if (hit != null && (hold == null || hold.heldObject == null))
        {
            // ★ hit 에 ShelfSlot 이 붙어 있을 때만 SmallBoxInteraction을 찾는다
            var shelf = hit.GetComponentInParent<ShelfSlot>();
            if (shelf != null)
            {
                var small = shelf.GetComponentInChildren<SmallBoxInteraction>();
                if (small != null)
                {
                    small.Interact();
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
