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
        int hours = (timeUI.totalGameMinutes / 60) % 24; // 시간 계산 (TimeUI)
        bool isBetweenDeadline = hours >= 20; // 20:00 ~ 23:59 (마감시간)인지

        if (isEnterPressed && !isAdjustmentCanvasActive && isBetweenDeadline) // enter 인식 && 정산캔버스활성화X && 마감시간
        {
            OnEnterDayEnd(); // EnterDayEnd라는 액션 인식 함수 호출
            isEnterPressed = false;
        }
        else if (isEnterPressed && !isAdjustmentCanvasActive && !isBetweenDeadline)
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

    void OnEnterDayEnd()
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


        NextDayTime();
    }

    public void NextDayTime()
    {
        timeUI.totalGameMinutes = 480; // 다시 아침 8:00부터 시간 흐르기 시작
        timeUI.isTimePaused = false; // 시간 멈춘 거 풀기
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