using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 총기 사건 시위 발생 시 바운서 호출 안내 튜토리얼.
/// 시위 시작 → (뉴스/다른 튜토리얼 대기) → 인트로 오버레이 → 단계별 안내.
/// </summary>
public class BouncerTutorialManager : MonoBehaviour
{
    public static BouncerTutorialManager Instance { get; private set; }

    // ════════════════════════════════════════════
    //  Step 정의
    // ════════════════════════════════════════════
    enum Step
    {
        Inactive = -1,
        EnterMonitor,
        ReturnToMainMenu,
        CallBouncer,
        Done
    }

    // ════════════════════════════════════════════
    //  Inspector 필드
    // ════════════════════════════════════════════
    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI guideText;

    [Header("Arrow")]
    [SerializeField] private TutorialArrowIndicator arrowIndicator;

    [Header("Targets")]
    [SerializeField] private Transform monitorTransform;

    [Header("바운서 버튼 UI 화살표")]
    [SerializeField] private RectTransform bouncerButtonRect;

    [Header("Options")]
    [SerializeField] private float advanceDelay = 0.6f;

    // ════════════════════════════════════════════
    //  인트로 오버레이 Inspector 필드
    // ════════════════════════════════════════════
    [Header("인트로 — 튜토리얼 패널 (딤 위로 올릴 대상)")]
    [SerializeField] private RectTransform tutorialPanel;

    [Header("인트로 — 딤")]
    [SerializeField, Range(0f, 1f)] private float dimAlpha = 0.75f;

    [Header("인트로 — 폰트")]
    [SerializeField] private TMP_FontAsset font;

    [Header("인트로 Phase 1 — 안내 메시지")]
    [SerializeField] private string welcomeText = "Protesters Are Gathering!";
    [SerializeField] private float welcomeFontSize = 42f;
    [SerializeField] private Color welcomeColor = Color.white;
    [SerializeField] private string welcomeSubText = "A shooting incident has caused public outrage.";
    [SerializeField] private float welcomeSubFontSize = 24f;
    [SerializeField] private Color welcomeSubColor = new Color(0.85f, 0.85f, 0.85f);
    [SerializeField] private string continueText = "Click to continue";
    [SerializeField] private float continueFontSize = 20f;
    [SerializeField] private Color continueColor = new Color(0.7f, 0.7f, 0.7f);

    [Header("인트로 Phase 2 — 튜토리얼 안내")]
    [SerializeField] private float arrowFontSize = 52f;
    [SerializeField] private Color arrowColor = new Color(1f, 0.15f, 0.15f);
    [SerializeField] private string followText = "Follow the tutorial above!";
    [SerializeField] private float followFontSize = 30f;
    [SerializeField] private Color followColor = Color.white;
    [SerializeField] private string startText = "Click to start";
    [SerializeField] private float startFontSize = 20f;
    [SerializeField] private Color startColor = new Color(0.7f, 0.7f, 0.7f);

    // ════════════════════════════════════════════
    //  내부 상태
    // ════════════════════════════════════════════
    private Step current = Step.Inactive;
    private bool isAdvancing;
    private bool pendingActivation;
    const string KeyDone = "BouncerTutorialDone";
    const int DimSortOrder = 1500;
    const int AboveDimSortOrder = 1501;

    // 인트로 런타임 UI
    private GameObject _introCanvasRoot;
    private Image _dimImage;
    private CanvasGroup _welcomeGroup;
    private CanvasGroup _followGroup;
    private Canvas _tutorialOverrideCanvas;

    // 시위 발생 플래그 (뉴스 끝난 뒤 활성화 위해)
    private bool _protestTriggered;

    // 바운서 버튼 UI 화살표
    private GameObject _buttonArrowObj;

    // ════════════════════════════════════════════
    //  싱글톤
    // ════════════════════════════════════════════
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    // ════════════════════════════════════════════
    //  생명주기
    // ════════════════════════════════════════════
    void OnEnable()
    {
        SocialEventManager.OnProtestToggled += HandleProtestToggled;
        TutorialEvents.OnSocialEventNewsDismissed += HandleNewsDismissed;
    }

    void OnDisable()
    {
        SocialEventManager.OnProtestToggled -= HandleProtestToggled;
        TutorialEvents.OnSocialEventNewsDismissed -= HandleNewsDismissed;
        UnsubscribeTutorialEvents();
        DestroyButtonArrow();
        if (_introCanvasRoot != null) Destroy(_introCanvasRoot);
        RestoreTutorialPanel();
    }

    void Start()
    {
        // root 비활성화는 TutorialManager가 관리하므로 여기서 하지 않음
    }

    void Update()
    {
        if (!pendingActivation) return;

        // 뉴스/다른 튜토리얼이 끝날 때까지 대기
        if (GlobalInteractionFlagS.IsInModal) return;

        // 뉴스 다시보기 튜토리얼(SocialEventTutorialManager)이 끝날 때까지 대기
        if (SocialEventTutorialManager.Instance != null
            && SocialEventTutorialManager.Instance.IsRunning) return;

        pendingActivation = false;
        Activate();
    }

    // ════════════════════════════════════════════
    //  시위 이벤트 수신
    // ════════════════════════════════════════════
    void HandleProtestToggled(bool on)
    {
        if (!on) return;
        if (current != Step.Inactive) return;
        if (PlayerPrefs.GetInt(KeyDone, 0) == 1) return;

        // 뉴스가 끝난 뒤에 활성화하기 위해 플래그만 세움
        _protestTriggered = true;
    }

    void HandleNewsDismissed()
    {
        if (!_protestTriggered) return;
        _protestTriggered = false;
        pendingActivation = true;
    }

    // ════════════════════════════════════════════
    //  튜토리얼 시작 (인트로 오버레이 포함)
    // ════════════════════════════════════════════
    void Activate()
    {
        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        // 이동 + 시점 잠금 + BGM 뮤트
        GlobalInteractionFlagS.ModalDepth++;
        MusicPlayer.Mute();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        BuildIntroUI();

        // ── Phase 1: 안내 메시지 ──
        _welcomeGroup.gameObject.SetActive(true);
        _followGroup.gameObject.SetActive(false);

        yield return FadeIntroDim(0f, dimAlpha, 0.5f);
        yield return FadeIntroGroup(_welcomeGroup, 0f, 1f, 0.4f);

        yield return WaitForIntroClick();

        // ── Phase 2: 튜토리얼 하이라이트 ──
        yield return FadeIntroGroup(_welcomeGroup, 1f, 0f, 0.3f);
        _welcomeGroup.gameObject.SetActive(false);

        if (root) root.SetActive(true);
        RefreshGuideText(Step.EnterMonitor);
        RaiseTutorialPanel();

        _followGroup.gameObject.SetActive(true);
        yield return FadeIntroGroup(_followGroup, 0f, 1f, 0.4f);

        yield return WaitForIntroClick();

        // ── 인트로 종료 ──
        yield return FadeIntroGroup(_followGroup, 1f, 0f, 0.3f);
        yield return FadeIntroDim(dimAlpha, 0f, 0.4f);

        RestoreTutorialPanel();

        GlobalInteractionFlagS.ModalDepth--;
        MusicPlayer.Unmute();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Destroy(_introCanvasRoot);
        _introCanvasRoot = null;

        // 튜토리얼 단계 시작
        current = Step.EnterMonitor;
        SubscribeTutorialEvents();
        RefreshUI();
        UpdateArrow();
    }

    // ════════════════════════════════════════════
    //  인트로 UI 빌드
    // ════════════════════════════════════════════
    void BuildIntroUI()
    {
        _introCanvasRoot = new GameObject("BouncerTutorialIntroOverlay");
        _introCanvasRoot.transform.SetParent(transform);
        var canvas = _introCanvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = DimSortOrder;
        var scaler = _introCanvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        _introCanvasRoot.AddComponent<GraphicRaycaster>();

        var dimObj = new GameObject("Dim");
        dimObj.transform.SetParent(_introCanvasRoot.transform, false);
        _dimImage = dimObj.AddComponent<Image>();
        _dimImage.color = new Color(0f, 0f, 0f, 0f);
        _dimImage.raycastTarget = false;
        Stretch(dimObj.GetComponent<RectTransform>());

        // Phase 1
        var welcomeObj = CreateStretchedGroup(_introCanvasRoot.transform, "WelcomeGroup");
        _welcomeGroup = welcomeObj.GetComponent<CanvasGroup>();
        _welcomeGroup.alpha = 0f;

        var titleTMP = CreateTMP(welcomeObj.transform, "Title",
            welcomeText, welcomeFontSize, welcomeColor, FontStyles.Bold, font);
        SetAnchored(titleTMP.rectTransform, 0f, 1f, 0.52f, 0.65f);

        var subTMP = CreateTMP(welcomeObj.transform, "SubText",
            welcomeSubText, welcomeSubFontSize, welcomeSubColor, FontStyles.Normal, font);
        SetAnchored(subTMP.rectTransform, 0f, 1f, 0.43f, 0.52f);

        var contTMP = CreateTMP(welcomeObj.transform, "Continue",
            continueText, continueFontSize, continueColor, FontStyles.Italic, font);
        SetAnchored(contTMP.rectTransform, 0f, 1f, 0.33f, 0.42f);

        // Phase 2
        var followObj = CreateStretchedGroup(_introCanvasRoot.transform, "FollowGroup");
        _followGroup = followObj.GetComponent<CanvasGroup>();
        _followGroup.alpha = 0f;

        var arrowTMP = CreateTMP(followObj.transform, "Arrow",
            "\u25b2", arrowFontSize, arrowColor, FontStyles.Bold, font);
        SetAnchored(arrowTMP.rectTransform, 0.45f, 0.55f, 0.72f, 0.82f);

        var followTMP = CreateTMP(followObj.transform, "FollowText",
            followText, followFontSize, followColor, FontStyles.Bold, font);
        SetAnchored(followTMP.rectTransform, 0.1f, 0.9f, 0.6f, 0.72f);

        var startTMP = CreateTMP(followObj.transform, "FollowSub",
            startText, startFontSize, startColor, FontStyles.Italic, font);
        SetAnchored(startTMP.rectTransform, 0.2f, 0.8f, 0.54f, 0.62f);
    }

    // ════════════════════════════════════════════
    //  튜토리얼 패널 sorting 제어
    // ════════════════════════════════════════════
    void RaiseTutorialPanel()
    {
        if (tutorialPanel == null) return;
        _tutorialOverrideCanvas = tutorialPanel.gameObject.GetComponent<Canvas>();
        if (_tutorialOverrideCanvas == null)
            _tutorialOverrideCanvas = tutorialPanel.gameObject.AddComponent<Canvas>();
        _tutorialOverrideCanvas.overrideSorting = true;
        _tutorialOverrideCanvas.sortingOrder = AboveDimSortOrder;
    }

    void RestoreTutorialPanel()
    {
        if (_tutorialOverrideCanvas == null) return;
        _tutorialOverrideCanvas.overrideSorting = false;
        if (tutorialPanel != null
            && tutorialPanel.gameObject.GetComponent<GraphicRaycaster>() == null)
            Destroy(_tutorialOverrideCanvas);
        _tutorialOverrideCanvas = null;
    }

    // ════════════════════════════════════════════
    //  바운서 버튼 UI 화살표
    // ════════════════════════════════════════════
    void ShowButtonArrow()
    {
        if (bouncerButtonRect == null) return;
        if (_buttonArrowObj != null) return;

        _buttonArrowObj = new GameObject("BouncerButtonArrow");
        _buttonArrowObj.transform.SetParent(bouncerButtonRect, false);

        var tmp = _buttonArrowObj.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = "\u25bc"; // ▼
        tmp.fontSize = 36f;
        tmp.color = new Color(1f, 0.15f, 0.15f);
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        var rt = _buttonArrowObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(60f, 50f);

        // 버튼의 상단 중앙에 배치
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 5f);

        // 바운스 애니메이션
        StartCoroutine(BounceArrow(rt));
    }

    IEnumerator BounceArrow(RectTransform rt)
    {
        var basePos = rt.anchoredPosition;
        while (rt != null)
        {
            float offset = Mathf.Sin(Time.unscaledTime * 4f) * 8f;
            rt.anchoredPosition = basePos + new Vector2(0f, offset);
            yield return null;
        }
    }

    void DestroyButtonArrow()
    {
        if (_buttonArrowObj != null)
        {
            Destroy(_buttonArrowObj);
            _buttonArrowObj = null;
        }
    }

    // ════════════════════════════════════════════
    //  튜토리얼 이벤트 구독/해제
    // ════════════════════════════════════════════
    void SubscribeTutorialEvents()
    {
        TutorialEvents.OnEnteredMonitor += HandleEnteredMonitor;
        TutorialEvents.OnMonitorTabOpened += HandleMonitorTabOpened;
        TutorialEvents.OnBouncerCalled += HandleBouncerCalled;
    }

    void UnsubscribeTutorialEvents()
    {
        TutorialEvents.OnEnteredMonitor -= HandleEnteredMonitor;
        TutorialEvents.OnMonitorTabOpened -= HandleMonitorTabOpened;
        TutorialEvents.OnBouncerCalled -= HandleBouncerCalled;
    }

    // ════════════════════════════════════════════
    //  이벤트 핸들러
    // ════════════════════════════════════════════
    void HandleEnteredMonitor()
    {
        if (current == Step.EnterMonitor)
        {
            // 모니터 진입 → 메인 화면이면 ReturnToMainMenu 건너뛰기
            var pm = FindFirstObjectByType<MonitorPanelManager>();
            if (pm != null && pm.IsMainPanelActive)
                SkipTo(Step.CallBouncer);
            else
                TryAdvance();
        }
    }

    void HandleMonitorTabOpened(int index)
    {
        // 메인 메뉴(index 1) 선택 시 다음 단계로
        if (current == Step.ReturnToMainMenu && index == 1)
            TryAdvance();
    }

    void HandleBouncerCalled()
    {
        if (current == Step.CallBouncer)
        {
            DestroyButtonArrow();
            TryAdvance();
        }
    }

    // ════════════════════════════════════════════
    //  단계 진행
    // ════════════════════════════════════════════
    void TryAdvance()
    {
        if (isAdvancing) return;
        StartCoroutine(AdvanceAfterDelay());
    }

    void SkipTo(Step target)
    {
        if (isAdvancing) return;
        StartCoroutine(SkipToAfterDelay(target));
    }

    IEnumerator SkipToAfterDelay(Step target)
    {
        isAdvancing = true;
        yield return new WaitForSeconds(advanceDelay);

        current = target;
        RefreshUI();
        UpdateArrow();
        UpdateButtonArrow();

        isAdvancing = false;
    }

    IEnumerator AdvanceAfterDelay()
    {
        isAdvancing = true;
        yield return new WaitForSeconds(advanceDelay);

        current = (Step)((int)current + 1);
        RefreshUI();
        UpdateArrow();
        UpdateButtonArrow();

        isAdvancing = false;

        if (current == Step.Done)
        {
            PlayerPrefs.SetInt(KeyDone, 1);
            PlayerPrefs.Save();
            UnsubscribeTutorialEvents();
            DestroyButtonArrow();
            StartCoroutine(HideAfterDelay(3f));
        }
    }

    IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (root != null) root.SetActive(false);
    }

    // ════════════════════════════════════════════
    //  UI 갱신
    // ════════════════════════════════════════════
    void RefreshUI()
    {
        if (!guideText) return;
        guideText.text = GetGuideText();
    }

    void RefreshGuideText(Step step)
    {
        if (!guideText) return;
        guideText.text = GetGuideTextFor(step);
    }

    string GetGuideText() => GetGuideTextFor(current);

    string GetGuideTextFor(Step step)
    {
        switch (step)
        {
            case Step.EnterMonitor:      return "Go to the monitor";
            case Step.ReturnToMainMenu:  return "Return to the main menu";
            case Step.CallBouncer:       return "Press the Call Bouncer button";
            case Step.Done:              return "Bouncer is on the way!";
        }
        return "";
    }

    // ════════════════════════════════════════════
    //  화살표
    // ════════════════════════════════════════════
    void UpdateArrow()
    {
        if (arrowIndicator == null) return;

        switch (current)
        {
            case Step.EnterMonitor:
                arrowIndicator.SetTarget(monitorTransform);
                break;
            default:
                arrowIndicator.ClearTarget();
                break;
        }
    }

    void UpdateButtonArrow()
    {
        if (current == Step.CallBouncer)
            ShowButtonArrow();
        else
            DestroyButtonArrow();
    }

    // ════════════════════════════════════════════
    //  인트로 유틸
    // ════════════════════════════════════════════
    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static GameObject CreateStretchedGroup(Transform parent, string name)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        obj.AddComponent<CanvasGroup>();
        return obj;
    }

    static void SetAnchored(RectTransform rt, float xMin, float xMax, float yMin, float yMax)
    {
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static TextMeshProUGUI CreateTMP(Transform parent, string name,
        string text, float size, Color color, FontStyles style,
        TMP_FontAsset fontAsset = null)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) tmp.font = fontAsset;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = true;
        tmp.raycastTarget = false;
        return tmp;
    }

    IEnumerator FadeIntroDim(float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / dur));
            _dimImage.color = new Color(0f, 0f, 0f, a);
            yield return null;
        }
        _dimImage.color = new Color(0f, 0f, 0f, to);
    }

    IEnumerator FadeIntroGroup(CanvasGroup cg, float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / dur));
            yield return null;
        }
        cg.alpha = to;
    }

    IEnumerator WaitForIntroClick()
    {
        yield return null;
        while (!Input.GetMouseButtonDown(0)
               && !Input.GetKeyDown(KeyCode.Space)
               && !Input.GetKeyDown(KeyCode.Return))
        {
            yield return null;
        }
    }

    // ════════════════════════════════════════════
    //  디버그
    // ════════════════════════════════════════════
    [ContextMenu("DEBUG: Reset Bouncer Tutorial")]
    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(KeyDone);
        current = Step.Inactive;
        pendingActivation = false;
        _protestTriggered = false;
        DestroyButtonArrow();
        if (root) root.SetActive(false);
    }
}
