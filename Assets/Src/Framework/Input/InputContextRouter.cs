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

    // private void Update()
    // {
    //     if (!clickRequested) return;
    //     clickRequested = false;

    //     if (UIUtility.IsPointerOverUI()) return;

    //     GameObject hit = RaycastDetector.Instance.HitObject;
    //     if (hit == null) return;

    //     var interactable = hit.GetComponentInParent<IInteractable>();
    //     if (interactable != null)
    //     {
    //         interactable.Interact();
    //     }

    // }
    private void Update()
{
    if (!clickRequested) return;
    clickRequested = false;

    if (UIUtility.IsPointerOverUI()) return;

    GameObject hit = RaycastDetector.Instance.HitObject;
    if (hit == null) return;

    // 🔽 이 부분 추가
    Debug.Log("🎯 Raycast hit: " + hit.name);

    var interactable = hit.GetComponentInParent<IInteractable>();
    if (interactable != null)
    {
        Debug.Log("🧠 CheckoutItemBehaviour.Interact() 호출됨 - this: " + gameObject.name);
        Debug.Log("✅ Interactable object type: " + interactable.GetType());
        Debug.Log("✅ Interactable object name: " + ((MonoBehaviour)interactable).gameObject.name);
        interactable.Interact();
    }
    else
    {
        Debug.LogWarning("❌ Interactable not found on: " + hit.name);
    }
}

}

