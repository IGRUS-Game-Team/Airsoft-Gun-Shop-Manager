using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// OnDayEnd.cs 이지연
/// OnDayEnd 오브젝트에 붙은 스크립트로, 
/// 오브젝트(ex box, monitor 등)의 선택됨 유무를 판단하는 클래스입니다.
/// </summary>

public class OnDayEnd : MonoBehaviour
{
    [SerializeField] GameObject AdjustmentCanvas;
    [SerializeField] GameObject BackgroundImage;
    [SerializeField] Transform TextGroup;
    [SerializeField] AudioClip AdjustmentAppearSound;
    [SerializeField] AudioSource audioSource;

    private bool isAdjustmentCanvasActive = false;

    void Update()
    {
        if (!isAdjustmentCanvasActive && Input.GetKeyDown(KeyCode.Return))
        {
            audioSource.PlayOneShot(AdjustmentAppearSound);

            AdjustmentCanvas.SetActive(true);
            BackgroundImage.SetActive(true);
            StartCoroutine(ShowDelayText());
            isAdjustmentCanvasActive = true;
        }
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
