using System;
using UnityEngine;

/// <summary>
/// 인풋 액션을 구독해 외부에서 사용할 수 있도록 이벤트 형태로 분배하는 컨트롤러입니다.
/// </summary>
/// 
/// 8/10 박정민
/// 구독 해제 구문에 잘못 적힌 부분 발견
/// 코드 일부 수정함.
public class InteractionController : MonoBehaviour
{
    public static InteractionController Instance { get; private set; }

    private PlayerInteraction inputActions;

    public event Action OnClick;
    public event Action OnDrop;
    public event Action OnThrowBox;
    public event Action OnExitUI;
    public event Action OnDayEnd;

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
        inputActions.Player.ThrowBox.performed += ctx => OnThrowBox?.Invoke();
        inputActions.Player.DropBox.performed += ctx => OnDrop?.Invoke();
        inputActions.Player.ExitUI.performed += ctx => OnExitUI?.Invoke();
        inputActions.Player.DayEnd.performed += ctx => OnDayEnd?.Invoke();
    }

    private void OnDestroy()
    {
        inputActions.Player.Interaction.performed -= ctx => OnClick?.Invoke();
        inputActions.Player.ThrowBox.performed -= ctx => OnThrowBox?.Invoke();
        inputActions.Player.DropBox.performed -= ctx => OnDrop?.Invoke();
        inputActions.Player.ExitUI.performed -= ctx => OnExitUI?.Invoke();
        inputActions.Player.DayEnd.performed -= ctx => OnDayEnd?.Invoke();
    }
}
