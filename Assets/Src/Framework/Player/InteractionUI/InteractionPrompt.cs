using System;
using System.Collections.Generic;

public enum InputHint
{
    LMB, RMB, E, R, G, ESC, QE
}

[Serializable]
public struct InteractionPrompt
{
    public InputHint input;
    public string description;

    public InteractionPrompt(InputHint input, string description)
    {
        this.input = input;
        this.description = description;
    }
}

// 오브젝트가 “내가 어떤 프롬프트를 띄울지” 직접 제공하고 싶을 때만 구현
public interface IHasInteractionPrompts
{
    void GetPrompts(PlayerInteractionContext ctx, List<InteractionPrompt> prompts);
}

// 플레이어 현재 상태(들고 있는지, 모니터 모드인지 등)
public class PlayerInteractionContext
{
    public bool inMonitorMode;
    public bool hasHeld;
    public bool holdingBox;
    public bool holdingGun;
}
