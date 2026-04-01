using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DoorSignInteraction : MonoBehaviour, IInteractable, IHasInteractionPrompts
{
    [Header("참조")]
    [SerializeField] private DoorTrigger doorTrigger;   // 문 트리거
    [SerializeField] private TMP_Text    signText;      // 푯말 글자 (TextMeshPro 또는 TextMeshProUGUI 둘 다 가능)

    [Header("색상")]
    [SerializeField] private Color openColor   = Color.green;
    [SerializeField] private Color closedColor = Color.red;

    private void Awake()
    {
        if (doorTrigger == null)
        {
            doorTrigger = FindFirstObjectByType<DoorTrigger>();
            if (doorTrigger == null)
            {
                Debug.LogWarning("[DoorSign] DoorTrigger 를 찾지 못했습니다.");
            }
        }

        RefreshVisual();
    }

    public void Interact()
    {
        if (doorTrigger == null)
        {
            Debug.LogWarning("[DoorSign] DoorTrigger 없음, 토글 불가");
            return;
        }

        doorTrigger.ToggleOpen();
        RefreshVisual();

        if (doorTrigger.IsOpen)
            TutorialEvents.RaiseShopOpened();
    }

    public void RefreshVisual()
    {
        if (doorTrigger == null || signText == null) return;

        if (doorTrigger.IsOpen)
        {
            signText.text  = "OPEN";
            signText.color = openColor;
        }
        else
        {
            signText.text  = "CLOSED";
            signText.color = closedColor;
        }
    }

    public void GetPrompts(PlayerInteractionContext ctx, List<InteractionPrompt> prompts)
    {
        if (doorTrigger == null) return;
        string action = doorTrigger.IsOpen ? "Close Shop" : "Open Shop";
        prompts.Add(new InteractionPrompt(InputHint.LMB, action));
    }
}