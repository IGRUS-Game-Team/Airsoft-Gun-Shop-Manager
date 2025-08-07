using System;
using UnityEngine;

/// <summary>
/// 08.06 이지연 박스 효과음 관련해 InteractionController 이벤트 구독
/// </summary>

public class AudioHandler : MonoBehaviour
{
    void Awake()
    {
        // InteractionController의 이벤트 구독
        if (InteractionController.Instance != null)
        {
            InteractionController.Instance.OnClick += PlayPickUpSound;
            InteractionController.Instance.OnDrop += PlayDropSound;
            InteractionController.Instance.OnThrowBox += PlayThrowSound;
        }
        else Debug.Log("이벤트가 존재하지 않습니다");
    }

    void PlayPickUpSound()
    {
        if (PlayerObjectHoldController.Instance.heldObject != null)
        {
           AudioManager.Instance.PlayBoxPickUpSound();
        }
    }

    void PlayDropSound()
    {
        if (PlayerObjectHoldController.Instance.heldObject != null)
        {
          AudioManager.Instance.PlayBoxDropSound();
        }
    }

    void PlayThrowSound()
    {
        if (PlayerObjectHoldController.Instance.heldObject != null)
        {
            AudioManager.Instance.PlayBoxThrowSound();
        }
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
