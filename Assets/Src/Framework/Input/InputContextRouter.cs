using UnityEngine;

/// <summary>
/// 클릭 시 레이캐스트로 오브젝트를 찾고, IInteractable이 붙어 있으면 Interact()를 실행합니다.
/// </summary>
// public class InputContextRouter : MonoBehaviour
// {
//     private void Start()
//     {
//         InteractionController.Instance.OnClick += HandleInteract;
//     }

//     private void HandleInteract()
//     {
//         if (UIUtility.IsPointerOverUI()) return;

//         GameObject hit = RaycastDetector.Instance.HitObject;
//         if (hit == null) return;

//         // 핵심 수정: 부모까지 뒤지기
//         var interactable = hit.GetComponentInParent<IInteractable>();
//         if (interactable != null)
//         {
//             interactable.Interact();
//         }
//     }
//}
public class InputContextRouter : MonoBehaviour
{
    private bool clickRequested = false;

    private void Start()
    {
        InteractionController.Instance.OnClick += () => clickRequested = true;
    }

    private void Update()
    {
        if (!clickRequested) return;
        clickRequested = false;

        if (UIUtility.IsPointerOverUI()) return;

        GameObject hit = RaycastDetector.Instance.HitObject;
        if (hit == null) return;

        var interactable = hit.GetComponentInParent<IInteractable>();
        if (interactable != null)
        {
            interactable.Interact();
        }
    }
}

