using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class TrashBinInteractionBehaviour : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        BlockIsHolding heldObject = PlayerObjectHoldController.Instance?.heldObject;
        BoxContainer boxObject = heldObject?.GetComponent<BoxContainer>();

        if (boxObject != null)
        {
            Destroy(heldObject.gameObject);
        }
        else
        {
            return;
        }

    }
}
