using UnityEngine;

public class PlayerObjectDropObjectController : MonoBehaviour
{
    [SerializeField] PlacePreview preview;
    PlayerObjectHoldController playerObjectHoldController;

    void Awake()
    {
        playerObjectHoldController = GetComponent<PlayerObjectHoldController>();
    }

    void Start()
    {
        InteractionController.Instance.OnDrop += SetObject;
    }

    void Update()
    {
        if (playerObjectHoldController.heldObject != null)
            preview.UpdatePreview();
        else
            preview.Hide();
    }

    void SetObject()
    {
        var held = playerObjectHoldController.heldObject;
        if (held == null) return;

        if (!preview.CanPlace)
        {
            Debug.Log("놓을 수 없음");
            return;
        }

        var pickable = held.GetComponent<IPickable>();
        pickable?.SetDown();
    }
}
