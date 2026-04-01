using System.Collections.Generic;
using UnityEngine;

public class ClerkInteraction : MonoBehaviour, IInteractable, IHasInteractionPrompts
{
    public void Interact()
    {
        AutoClerkController.Instance?.ToggleWorking();
    }

    public void GetPrompts(PlayerInteractionContext ctx, List<InteractionPrompt> prompts)
    {
        if (AutoClerkController.Instance == null) return;
        bool working = AutoClerkController.Instance.IsWorking;
        string action = working ? "Rest" : "Work";
        prompts.Add(new InteractionPrompt(InputHint.LMB, action));
    }
}
