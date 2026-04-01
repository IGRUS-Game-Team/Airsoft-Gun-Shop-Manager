using System.Collections.Generic;
using UnityEngine;

public class MonitorInteractionBehaviour : MonoBehaviour, IInspectable, IHasInteractionPrompts
{
    [SerializeField] Camera monitorUICam;
    [SerializeField] RenderTextureUIClicker uiClicker;

    public void Interact()
    {
        EnterInspection();
    }

    public void EnterInspection()
    {
        if (monitorUICam == null)
        {
            Debug.LogError("monitorUICam이 연결되지 않았습니다. 에디터에서 연결해주세요!");
            return;
        }
        MonitorUIModeManager.Instance.EnterUIMode(monitorUICam);
        
    }

    public void ExitInspection()
    {
        MonitorUIModeManager.Instance.ExitUIMode();
    }

    public void GetPrompts(PlayerInteractionContext ctx, List<InteractionPrompt> prompts)
    {
        if (ctx.inMonitorMode) return; // 이미 UI 모드면 표시 안함

        prompts.Add(new InteractionPrompt(InputHint.LMB, "Use"));
    }
}
