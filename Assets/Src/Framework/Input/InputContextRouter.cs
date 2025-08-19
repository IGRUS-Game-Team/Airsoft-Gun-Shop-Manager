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

        // [추가] 총 들고 있으면 상호작용 막기
        var activeGun = FindFirstObjectByType<ActiveGun>();
        if (activeGun != null && activeGun.enabled && activeGun.gameObject.activeInHierarchy)
        {
            // 총 모드면 InputContextRouter는 상호작용 안함
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

        // 2) 평소처럼 레이캐스트 대상 처리
        if (hit == null) return;
        var intr = hit.GetComponentInParent<IInteractable>();
        if (intr != null) intr.Interact();
    }
}
