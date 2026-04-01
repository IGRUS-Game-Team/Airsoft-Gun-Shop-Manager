using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionRouter : MonoBehaviour
{
    public event Action<List<InteractionPrompt>> OnPromptsChanged;

    private readonly List<InteractionPrompt> _prompts = new();
    private PlayerObjectHoldController hold;

    private void Awake()
    {
        hold = FindFirstObjectByType<PlayerObjectHoldController>();
    }

    private void Update()
    {
        BuildAndPublish();
    }

    private void BuildAndPublish()
    {
        _prompts.Clear();

        // 1) 컨텍스트 만들기
        var ctx = new PlayerInteractionContext();
        ctx.inMonitorMode = MonitorUIModeManager.Instance != null && MonitorUIModeManager.Instance.getInUIMode();

        ctx.hasHeld = (hold != null && hold.heldObject != null);
        ctx.holdingBox = ctx.hasHeld && hold.heldObject.GetComponentInParent<BoxInteractionBehaviour>() != null;
        ctx.holdingGun = ctx.hasHeld && hold.heldObject.GetComponentInParent<GunInteraction>() != null; // 있으면

        // 2-a) 배치 모드 — 전용 프롬프트만 표시
        if (PlacementManager.Instance != null && PlacementManager.Instance.IsPlacing)
        {
            _prompts.Add(new InteractionPrompt(InputHint.LMB, "Place"));
            _prompts.Add(new InteractionPrompt(InputHint.QE,  "Rotate"));
            _prompts.Add(new InteractionPrompt(InputHint.RMB, "Cancel"));
            Publish();
            return;
        }

        // 2-b) 사격 모드
        if (ShootingZoneManager.Instance != null && ShootingZoneManager.Instance.IsInShootingMode)
        {
            _prompts.Add(new InteractionPrompt(InputHint.LMB, "Shoot"));
            if (ShootingZoneManager.Instance.CurrentGunCanZoom)
                _prompts.Add(new InteractionPrompt(InputHint.RMB, "Scope"));
            _prompts.Add(new InteractionPrompt(InputHint.ESC, "Exit"));
            Publish();
            return;
        }

        // 2-c) UI 모드면(모니터/계산기 등) 프롬프트 제한하고 싶으면 여기서 분기
        if (ctx.inMonitorMode)
        {
            _prompts.Add(new InteractionPrompt(InputHint.ESC, "Exit"));
            Publish();
            return;
        }

        // 3) 들고 있는 상태 프롬프트
        if (ctx.hasHeld)
        {
            // 들고 있는 오브젝트 자체의 프롬프트 (예: 박스 열기/닫기)
            var heldProvider = hold.heldObject.GetComponentInParent<IHasInteractionPrompts>();
            if (heldProvider != null)
            {
                heldProvider.GetPrompts(ctx, _prompts);
            }

            _prompts.Add(new InteractionPrompt(InputHint.R, "Throw"));
            _prompts.Add(new InteractionPrompt(InputHint.G, "Drop"));
        }

        // 3.5) 현금 결제 중 → 레이캐스트 없이 항상 프롬프트 표시
        if (CashRegisterEnterHandler.Instance != null)
        {
            CashRegisterEnterHandler.Instance.GetPrompts(ctx, _prompts);
        }

        // 4) 레이캐스트 대상
        GameObject hit = (RaycastDetector.Instance != null) ? RaycastDetector.Instance.HitObject : null;
        if (hit != null)
        {
            bool handled = false;

            // (0) 박스를 들고 선반 영역을 바라볼 때 → 선반 프롬프트 우선 (Display)
            if (ctx.holdingBox)
            {
                var shelf = hit.GetComponentInParent<ShelfSlot>();
                if (shelf != null)
                {
                    var shelfProvider = shelf.GetComponent<IHasInteractionPrompts>();
                    if (shelfProvider != null)
                    {
                        shelfProvider.GetPrompts(ctx, _prompts);
                        handled = true;
                    }
                }
            }

            if (!handled)
            {
                // (a) 오브젝트가 프롬프트 직접 제공하는 경우
                var provider = hit.GetComponentInParent<IHasInteractionPrompts>();
                if (provider != null)
                {
                    provider.GetPrompts(ctx, _prompts);
                }
                else
                {
                    // (b) 기본값: Interact만이라도
                    var it = hit.GetComponentInParent<IInteractable>();
                    if (it != null)
                    {
                        _prompts.Add(new InteractionPrompt(InputHint.LMB, "Interact"));
                    }
                }
            }
        }

        Publish();
    }

    private void Publish()
    {
        OnPromptsChanged?.Invoke(_prompts);
    }
}
