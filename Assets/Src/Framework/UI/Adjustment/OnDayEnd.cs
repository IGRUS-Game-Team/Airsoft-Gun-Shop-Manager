using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;

/// <summary>
/// OnDayEnd.cs 이지연
/// OnDayEnd 오브젝트에 붙은 스크립트로, 액션맵 통해 enter 감지하면 Text가 0.4초마다 나타나는 기능입니다. 


public class OnDayEnd : MonoBehaviour
{
    [SerializeField] GameObject AdjustmentCanvas;
    [SerializeField] GameObject BackgroundImage;
    [SerializeField] Transform TextGroup;
    [SerializeField] AudioClip AdjustmentAppearSound;
    [SerializeField] AudioSource audioSource;
    [SerializeField] StarterAssets.StarterAssetsInputs playerInput;

    private bool isAdjustmentCanvasActive = false;

    void Update()
    {
        if (playerInput.DayEnd && !isAdjustmentCanvasActive)
        {
            OnEnterDayEnd();
        }
    }

    void OnEnterDayEnd()
    {
        audioSource.PlayOneShot(AdjustmentAppearSound);

        AdjustmentCanvas.SetActive(true);
        BackgroundImage.SetActive(true);
        StartCoroutine(ShowDelayText());
        isAdjustmentCanvasActive = true;
    }

    IEnumerator ShowDelayText()
    {
        foreach (Transform text in TextGroup)
        {
            yield return new WaitForSeconds(.5f);
            text.gameObject.SetActive(true);
        }
    }
}