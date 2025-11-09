using UnityEngine;

public class PlayerHeldBoxOpenController : MonoBehaviour
{
    private void Start()
    {
        // E키 액션 구독
        InteractionController.Instance.OnOpenHeldBox += HandleOpenHeldBox;
    }

    private void OnDestroy()
    {
        if (InteractionController.Instance != null)
        {
            InteractionController.Instance.OnOpenHeldBox -= HandleOpenHeldBox;
        }
    }

    private void HandleOpenHeldBox()
    {
        // 1) 지금 뭐 들고 있는지 확인
        var holdCtrl = PlayerObjectHoldController.Instance;
        if (holdCtrl == null) return;

        var held = holdCtrl.heldObject;
        if (held == null) return;

        // 2) 그 오브젝트에 SmallBoxInteraction이 붙어 있는지 확인
        var small = held.GetComponent<SmallBoxInteraction>();
        if (small == null) return;

        // 3) 개봉
        small.OpenBox();
    }
}