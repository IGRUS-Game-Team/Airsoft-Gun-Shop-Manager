using System.Collections;
using UnityEngine;

/// 하루 마감(정산) UI 컨트롤러
/// - Enter/KeypadEnter 로 열기(마감시간 이후)
/// - InteractionController.OnDayEnd 이벤트로도 열기
/// - 자정(00:00) 자동 마감 유지
/// - 다음날로 넘어가면 모든 가드/코루틴/텍스트 확실히 리셋
public class OnDayEnd : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] GameObject AdjustmentCanvas;
    [SerializeField] GameObject BackgroundImage;
    [SerializeField] Transform  TextGroup;
    [SerializeField] AudioClip  AdjustmentAppearSound;
    [SerializeField] AudioClip  UIAppearSound;
    [SerializeField] AudioSource audioSource;
    [SerializeField] TimeUI     timeUI;

    [Header("Input Fallback")]
    [Tooltip("InteractionController 이벤트가 없어도 Enter 키로 열 수 있게 함")]
    [SerializeField] bool useEnterFallback = true;

    // 내부 상태
    bool isAdjustmentCanvasActive = false;
    bool isEnterRequested         = false;
    bool subscribed               = false;
    bool hasShownToday            = false;   // 오늘 하루에 정산창을 한 번이라도 띄웠는지
    Coroutine showTextCo          = null;    // 텍스트 순차 표시 코루틴 핸들

    public static bool isDayEndUIActive = false;

    void OnEnable()
    {
        TrySubscribe();
        if (timeUI != null)
            timeUI.OnDayChanged.AddListener(ResetFlagsForNewDay); // 다음날 되면 플래그 리셋
    }

    void Start()
    {
        TrySubscribe();
        isDayEndUIActive = false;

        if (!timeUI) Debug.LogWarning("[OnDayEnd] TimeUI 미지정");
        if (!AdjustmentCanvas || !BackgroundImage || !TextGroup || !audioSource)
            Debug.LogWarning("[OnDayEnd] UI/오디오 레퍼런스 중 누락 있음");
    }

    void OnDisable()
    {
        Unsubscribe();
        if (timeUI != null)
            timeUI.OnDayChanged.RemoveListener(ResetFlagsForNewDay);
    }

    void TrySubscribe()
    {
        if (subscribed) return;
        if (InteractionController.Instance != null)
        {
            InteractionController.Instance.OnDayEnd += OnExternalDayEndRequest;
            subscribed = true;
        }
    }

    void Unsubscribe()
    {
        if (subscribed && InteractionController.Instance != null)
            InteractionController.Instance.OnDayEnd -= OnExternalDayEndRequest;
        subscribed = false;
    }

    void Update()
    {
        if (!subscribed) TrySubscribe();

        // 다른 UI(설정 등) 열려 있으면 리턴 → Enter가 거기서 소비될 수 있음
        if (InGameSettingManager.Instance != null &&
            InGameSettingManager.Instance.GetIsSettingOpen())
            return;

        if (useEnterFallback &&
            (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            isEnterRequested = true;

        int minutes = timeUI ? timeUI.totalGameMinutes : 480;
        int hours   = (minutes / 60) % 24;

        var sm    = SettlementManager.Instance;
        int close = sm ? sm.CloseHour : 20;
        bool afterClose = hours >= close;

        // 수동 마감
        if (isEnterRequested && !hasShownToday)
        {
            if (afterClose) OpenDayEndUI();
            else Debug.Log($"[OnDayEnd] 아직 마감 전 (현재 {hours:D2}시, 마감 {close}시)");
            isEnterRequested = false;
        }

        // 자정 자동 마감 (00:00)
        if (timeUI && !hasShownToday && !timeUI.isTimePaused)
        {
            int h = (timeUI.totalGameMinutes / 60) % 24;
            int m = timeUI.totalGameMinutes % 60;
            if (h == 0 && m == 0) OpenDayEndUI();
        }
    }

    void OnExternalDayEndRequest() => isEnterRequested = true;

    void OpenDayEndUI()
    {
        if (hasShownToday) return;
        hasShownToday = true;

        // 텍스트 초기화(모두 끔)
        if (TextGroup)
            foreach (Transform t in TextGroup) t.gameObject.SetActive(false);

        isDayEndUIActive         = true;
        isAdjustmentCanvasActive = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        if (AdjustmentAppearSound) audioSource.PlayOneShot(AdjustmentAppearSound);
        if (AdjustmentCanvas)  AdjustmentCanvas.SetActive(true);
        if (BackgroundImage)   BackgroundImage.SetActive(true);

        showTextCo = StartCoroutine(ShowDelayText());

        if (timeUI) timeUI.isTimePaused = true;

        int close = SettlementManager.Instance ? SettlementManager.Instance.CloseHour : 20;
        Debug.Log($"[OnDayEnd] 정산 UI 오픈 (마감 {close}시)");
    }

    public void StartNextDay()
    {
        // UI 끄기
        if (AdjustmentCanvas)  AdjustmentCanvas.SetActive(false);
        if (BackgroundImage)   BackgroundImage.SetActive(false);

        // 하루 집계 리셋
        SettlementManager.Instance?.ResetToday();

        // 시간 재개 + 오픈 시간으로 점프
        int open = SettlementManager.Instance ? SettlementManager.Instance.OpenHour : 8;
        if (timeUI)
        {
            timeUI.isTimePaused     = false;
            timeUI.totalGameMinutes = open * 60;
            timeUI.ForceUpdate();
        }

        // 커서 원복
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        // 내부 플래그/코루틴 정리
        ResetFlagsForNewDay();

        Debug.Log("[OnDayEnd] 다음날 시작 - 상태 리셋 완료");
    }

    IEnumerator ShowDelayText()
    {
        foreach (Transform t in TextGroup)
        {
            yield return new WaitForSeconds(0.4f);
            if (UIAppearSound) audioSource.PlayOneShot(UIAppearSound);
            t.gameObject.SetActive(true);
        }
        showTextCo = null;
    }

    /// 다음날 시작 시 공통 리셋(버튼/자정/외부 이벤트 모두 이 함수 호출)
    void ResetFlagsForNewDay()
    {
        isDayEndUIActive         = false;
        isAdjustmentCanvasActive = false;
        hasShownToday            = false;
        isEnterRequested         = false;

        if (showTextCo != null) { StopCoroutine(showTextCo); showTextCo = null; }

        if (TextGroup)
            foreach (Transform t in TextGroup) t.gameObject.SetActive(false);
    }
}
