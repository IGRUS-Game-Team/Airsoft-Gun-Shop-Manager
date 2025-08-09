using UnityEngine;

/// <summary>
/// 클릭 시 레이캐스트로 오브젝트를 찾고, IInteractable이 붙어 있으면 Interact()를 실행합니다.
/// </summary>
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

        if (UIUtility.IsPointerOverUI()) return;

        var hit = (RaycastDetector.Instance != null) ? RaycastDetector.Instance.HitObject : null;

        // 1) 들고 있는 오브젝트를 조준 중이거나, 아무 것도 안 맞았을 때 → held 우선
        if (hold != null && hold.heldObject != null)
        {
            bool aimedAtHeld = hit != null && (hit.transform.root == hold.heldObject.transform.root);
            if (hit == null || aimedAtHeld)
            {
                var intrHeld = hold.heldObject.GetComponentInParent<IInteractable>();
                if (intrHeld != null) { intrHeld.Interact(); return; }
            }
        }

        // 2) 평소처럼 레이캐스트 대상 처리
        if (hit == null) return;
        var intr = hit.GetComponentInParent<IInteractable>();
        if (intr != null) intr.Interact();
    }
}
