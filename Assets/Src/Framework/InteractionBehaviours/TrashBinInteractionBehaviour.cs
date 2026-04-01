using System.Collections.Generic;
using UnityEngine;

public class TrashBinInteractionBehaviour : MonoBehaviour, IInteractable, IHasInteractionPrompts
{
    public void Interact()
    {
        BlockIsHolding heldObject = PlayerObjectHoldController.Instance?.heldObject;
        BoxContainer boxObject = heldObject?.GetComponent<BoxContainer>();

        if (boxObject != null)
        {
            PlayerObjectHoldController.Instance.heldObject = null;
            TutorialEvents.RaiseBoxTrashed();
            Destroy(heldObject.gameObject);
        }
    }

    public void GetPrompts(PlayerInteractionContext ctx, List<InteractionPrompt> prompts)
    {
        if (ctx.hasHeld)
        {
            prompts.Add(new InteractionPrompt(InputHint.LMB, "Discard"));
        }
    }
}
