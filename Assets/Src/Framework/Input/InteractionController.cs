using System;
using UnityEngine;

/// <summary>
/// 인풋 액션을 구독해 외부에서 사용할 수 있도록 이벤트 형태로 분배하는 컨트롤러입니다.
/// </summary>
public class InteractionController : MonoBehaviour
{
    public static InteractionController Instance { get; private set; }

    private PlayerInteraction inputActions;

    public event Action OnClick;
    public event Action OnDrop;
    public event Action OnThrowBox;
    public event Action OnInteract;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        inputActions = new PlayerInteraction();
        inputActions.Player.Enable();

        inputActions.Player.Interaction.performed += ctx => OnClick?.Invoke();
        inputActions.Player.ThrowBox.performed += ctx =>OnThrowBox?.Invoke();
        inputActions.Player.DropBox.performed += ctx => OnDrop?.Invoke();
        //inputActions.Player.Interact.performed += ctx => OnInteract?.Invoke();
    }

    private void OnDestroy()
    {
        inputActions.Player.Interaction.performed -= ctx => OnClick?.Invoke();
        inputActions.Player.ThrowBox.performed -= ctx => OnDrop?.Invoke();
        inputActions.Player.DropBox.performed -= ctx => OnThrowBox?.Invoke();
        //inputActions.Player.Interact.performed -= ctx => OnInteract?.Invoke();
    }
}
