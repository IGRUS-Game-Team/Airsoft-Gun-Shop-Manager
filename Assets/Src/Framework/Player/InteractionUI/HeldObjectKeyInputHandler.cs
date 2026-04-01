using UnityEngine;

public class HeldObjectKeyInputHandler : MonoBehaviour
{
    private PlayerObjectHoldController hold;

    private void Awake()
    {
        hold = FindFirstObjectByType<PlayerObjectHoldController>();
    }

    private void Update()
    {
        if (hold == null || hold.heldObject == null) return;

        if (MonitorUIModeManager.Instance != null && MonitorUIModeManager.Instance.getInUIMode()) return;
        if (ClickObjectUIManager.Instance != null && ClickObjectUIManager.Instance.IsUIOpen) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            var pickable = hold.heldObject.GetComponentInParent<IPickable>();
            if (pickable != null)
            {
                pickable.ThrowObject();
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            var pickable = hold.heldObject.GetComponentInParent<IPickable>();
            if (pickable != null)
            {
                pickable.SetDown();
            }
        }
    }
}
