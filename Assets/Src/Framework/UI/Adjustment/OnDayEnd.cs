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
    [SerializeField] AudioClip UIAppearSound;
    [SerializeField] AudioSource audioSource;
    [SerializeField] StarterAssets.StarterAssetsInputs playerInput; // 액션맵

    private bool isAdjustmentCanvasActive = false; // 시간이 지났을 때만 정산 UI가 활성화되어야 하므로 false가 기본값

    void Update()
    {
        if (playerInput.dayEnd && !isAdjustmentCanvasActive) // enter 인식 && 
        {
            OnEnterDayEnd(); // EnterDayEnd라는 액션 인식 함수 호출
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
        foreach (Transform text in TextGroup) // TextGroup 내에 있는 Text들 0.4초 간격으로 띄우기
        {
            yield return new WaitForSeconds(.4f);
            audioSource.PlayOneShot(UIAppearSound);
            text.gameObject.SetActive(true);
        }
    }
}