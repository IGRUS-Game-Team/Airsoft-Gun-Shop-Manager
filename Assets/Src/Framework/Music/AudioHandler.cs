using UnityEngine;

/// <summary>
/// 08.06 이지연 박스 효과음 관련해 InteractionController 이벤트 구독
/// AudioManager.cs 내 함수 호출하여 클릭하면 PickUp사운드, 떨어트리면 Drop사운드, 던지면 ThrowSound 나도록 하였습니다.
/// </summary>

public class AudioHandler : MonoBehaviour
{
    private BlockIsHolding lastHeldObject;

    void Awake()
    {
        // InteractionController의 이벤트 구독
        if (InteractionController.Instance != null)
        {
            InteractionController.Instance.OnClick += PlayPickUpSound; // 구독자 추가
            InteractionController.Instance.OnDrop += PlayDropSound;
            InteractionController.Instance.OnThrowBox += PlayThrowSound;
        }
        else Debug.Log("이벤트가 존재하지 않습니다");
    }

    void PlayPickUpSound()
    {
        var currentHeld = PlayerObjectHoldController.Instance.heldObject;

        if (currentHeld != null && currentHeld != lastHeldObject) // 오브젝트를 들었을 때 && 오브젝트 들고 있지 않을 때 
        {
            AudioManager.Instance.PlayBoxPickUpSound();
            lastHeldObject = currentHeld;
        }
    }

    void PlayDropSound()
    {
        if (PlayerObjectHoldController.Instance.heldObject != null)
        {
            AudioManager.Instance.PlayBoxDropSound();
        }

        lastHeldObject = null; // 오브젝트를 떨어트렸으니 들고있던 오브젝트 null 설정
    }

    void PlayThrowSound()
    {
        if (PlayerObjectHoldController.Instance.heldObject != null)
        {
            AudioManager.Instance.PlayBoxThrowSound();
        }

        lastHeldObject = null; // 오브젝트를 던졌으니 들고있던 오브젝트 null 설정
    }

    void OnDestroy()
    {
        // 끝나면 구독 해제
        if (InteractionController.Instance != null)
        {
            InteractionController.Instance.OnClick -= PlayPickUpSound;
            InteractionController.Instance.OnDrop -= PlayDropSound;
            InteractionController.Instance.OnThrowBox -= PlayThrowSound;
        }
    }
}
