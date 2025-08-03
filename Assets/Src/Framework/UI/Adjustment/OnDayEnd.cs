using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OnDayEnd : MonoBehaviour
{
    [SerializeField] GameObject AdjustmentCanvas;
    [SerializeField] GameObject BackgroundImage;
    [SerializeField] Transform TextGroup;

    private bool isAdjustmentCanvasActive = false;

    void Update()
    {
        if (!isAdjustmentCanvasActive && Input.GetKeyDown(KeyCode.Return))
        {
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
