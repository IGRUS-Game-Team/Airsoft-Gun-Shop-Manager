using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StarterAssets;

/// <summary>
/// OnDayEnd.cs 08/03 이지연
/// OnDayEnd 오브젝트에 붙은 스크립트로, 액션맵 통해 enter 감지하면 Text가 0.4초마다 나타나는 기능입니다. 
/// </summary>


public class OnDayEnd : MonoBehaviour
{
    [SerializeField] GameObject AdjustmentCanvas;
    [SerializeField] GameObject BackgroundImage;
    [SerializeField] Transform TextGroup;
    [SerializeField] AudioClip AdjustmentAppearSound;
    [SerializeField] AudioClip UIAppearSound;
    [SerializeField] AudioSource audioSource;
    //[SerializeField] StarterAssets.StarterAssetsInputs playerInput; // 액션맵
    [SerializeField] TimeUI timeUI;

    private bool isAdjustmentCanvasActive = false; // 시간이 지났을 때만 정산 UI가 활성화되어야 하므로 false가 기본값
    private bool isEnterPressed = false;
    public static bool isDayEndUIActive = false;
    private bool hasAutoEndTriggered = false; // 자동 종료
    private void Start()
    {
        InteractionController.Instance.OnDayEnd += HandleExitKeyPressed;
        isDayEndUIActive = false;
    }

    private void OnDisable()
    {
        if (InteractionController.Instance != null)
            InteractionController.Instance.OnDayEnd -= HandleExitKeyPressed;
    }

    private void HandleExitKeyPressed()
    {
        isEnterPressed = true;
    }

    private void OpenSetting()
    {
        isDayEndUIActive = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseSetting() //정산 ui 꺼질때 이 메서드 넣어주세요
    {
        isDayEndUIActive = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; //커서 안보임
    }

    void Update()
    {
        var sm = SettlementManager.Instance;
        int open  = sm != null ? sm.OpenHour  : 8;
        int close = sm != null ? sm.CloseHour : 20;

        int hours = (timeUI.totalGameMinutes / 60) % 24;
        bool isAfterClose = hours >= close; // ← 하드코드 20 대신

       if (isEnterPressed && !isAdjustmentCanvasActive && isAfterClose)
        {
            OnEnterDayEnd();
            isEnterPressed = false;
        }   
    
        else if (isEnterPressed && !isAdjustmentCanvasActive && !isAfterClose)
        {
            isEnterPressed = false;
            Debug.Log("아직 마감 시간이 되지 않았습니다.");
        }

        else if (timeUI.totalGameMinutes == 1440 && !hasAutoEndTriggered && !timeUI.isTimePaused) // 00:00 가 됐을 때 정산UI 띄우기
        {
            hasAutoEndTriggered = true; // 자동 종료 (Updeate 내에서 OnEnterDayEnd가 한 번만 호출되도록)
            OnEnterDayEnd();
        }
    }

    public void OnEnterDayEnd()
    {
        OpenSetting();
        audioSource.PlayOneShot(AdjustmentAppearSound);

        AdjustmentCanvas.SetActive(true); 
        BackgroundImage.SetActive(true); 
        StartCoroutine(ShowDelayText());
        isAdjustmentCanvasActive = true;
        timeUI.isTimePaused = true; // 시간 멈추기
    }

    public void StartNextDay()
    {
        CloseSetting(); //추가함 -박정민-
        AdjustmentCanvas.SetActive(false);
        BackgroundImage.SetActive(false);
        isAdjustmentCanvasActive = false;
        hasAutoEndTriggered = false;

        SettlementManager.Instance?.ResetToday(); // 하루 집계 초기화 추가함 -이준서-

        NextDayTime();
    }

    public void NextDayTime()
    {
        var sm = SettlementManager.Instance;
        int open = sm != null ? sm.OpenHour : 8;
        timeUI.totalGameMinutes = open * 60;
        timeUI.isTimePaused = false;
        timeUI.ForceUpdate();
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