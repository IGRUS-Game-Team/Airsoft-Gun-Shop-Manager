using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 첫 사회 이벤트 뉴스 종료 후 TV/신문 안내 오버레이.
/// Phase 1: 딤 + "A social event has occurred!" 안내
/// Phase 2: 딤 + "TV로 다시보기 / 신문으로 상세정보" 안내
/// </summary>
public class SocialEventTutorialManager : MonoBehaviour
{
    public static SocialEventTutorialManager Instance { get; private set; }

    // ════════════════════════════════════════════
    //  Inspector 필드
    // ════════════════════════════════════════════
    [Header("딤")]
    [SerializeField, Range(0f, 1f)] private float dimAlpha = 0.75f;

    [Header("폰트")]
    [SerializeField] private TMP_FontAsset font;

    [Header("Phase 1 — 안내 메시지")]
    [SerializeField] private string welcomeText = "A Social Event Has Occurred!";
    [SerializeField] private float welcomeFontSize = 42f;
    [SerializeField] private Color welcomeColor = Color.white;
    [SerializeField] private string welcomeSubText = "Social events can affect market prices.";
    [SerializeField] private float welcomeSubFontSize = 24f;
    [SerializeField] private Color welcomeSubColor = new Color(0.85f, 0.85f, 0.85f);
    [SerializeField] private string continueText = "Click to continue";
    [SerializeField] private float continueFontSize = 20f;
    [SerializeField] private Color continueColor = new Color(0.7f, 0.7f, 0.7f);

    [Header("Phase 2 — TV / 신문 안내")]
    [SerializeField] private string tvInfoText = "You can rewatch the news by clicking the TV.";
    [SerializeField] private float tvInfoFontSize = 28f;
    [SerializeField] private Color tvInfoColor = Color.white;
    [SerializeField] private string paperInfoText = "Read the newspaper for detailed market information.";
    [SerializeField] private float paperInfoFontSize = 28f;
    [SerializeField] private Color paperInfoColor = Color.white;
    [SerializeField] private string startText = "Click to start";
    [SerializeField] private float startFontSize = 20f;
    [SerializeField] private Color startColor = new Color(0.7f, 0.7f, 0.7f);

    // ════════════════════════════════════════════
    //  내부 상태
    // ════════════════════════════════════════════
    const string KeyDone = "SocialEventTutorialDone";
    const int DimSortOrder = 1500;

    /// <summary>오버레이가 진행 중인지 여부 (다른 튜토리얼이 대기할 때 사용)</summary>
    public bool IsRunning { get; private set; }

    private GameObject _canvasRoot;
    private Image _dimImage;
    private CanvasGroup _welcomeGroup;
    private CanvasGroup _infoGroup;

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
        TutorialEvents.OnSocialEventNewsDismissed += HandleNewsDismissed;
        Debug.Log("[SocialEventTutorialManager] OnEnable — 이벤트 구독 완료");
    }

    void OnDisable()
    {
        TutorialEvents.OnSocialEventNewsDismissed -= HandleNewsDismissed;
        if (_canvasRoot != null) Destroy(_canvasRoot);
    }

    // ════════════════════════════════════════════
    //  이벤트 핸들러
    // ════════════════════════════════════════════
    void HandleNewsDismissed()
    {
        int done = PlayerPrefs.GetInt(KeyDone, 0);
        Debug.Log($"[SocialEventTutorialManager] HandleNewsDismissed 호출됨 (KeyDone={done})");

        if (done == 1) return;

        IsRunning = true;
        Debug.Log("[SocialEventTutorialManager] ShowOverlay 시작");
        StartCoroutine(ShowOverlay());
    }

    // ════════════════════════════════════════════
    //  오버레이 시퀀스
    // ════════════════════════════════════════════
    IEnumerator ShowOverlay()
    {
        // 한 프레임 대기 (뉴스 씬 완전 언로드 후)
        yield return null;

        // 이동 + 시점 잠금 + BGM 뮤트
        GlobalInteractionFlagS.ModalDepth++;
        MusicPlayer.Mute();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        BuildUI();
        Debug.Log("[SocialEventTutorialManager] BuildUI 완료 — 오버레이 표시 시작");

        // ── Phase 1: 사회 이벤트 안내 ──
        _welcomeGroup.gameObject.SetActive(true);
        _infoGroup.gameObject.SetActive(false);

        yield return FadeDim(0f, dimAlpha, 0.5f);
        yield return FadeGroup(_welcomeGroup, 0f, 1f, 0.4f);

        yield return WaitForClick();

        // ── Phase 2: TV / 신문 안내 ──
        yield return FadeGroup(_welcomeGroup, 1f, 0f, 0.3f);
        _welcomeGroup.gameObject.SetActive(false);

        _infoGroup.gameObject.SetActive(true);
        yield return FadeGroup(_infoGroup, 0f, 1f, 0.4f);

        yield return WaitForClick();

        // ── 종료 ──
        yield return FadeGroup(_infoGroup, 1f, 0f, 0.3f);
        yield return FadeDim(dimAlpha, 0f, 0.4f);

        // 잠금 해제 + BGM 복원
        GlobalInteractionFlagS.ModalDepth--;
        MusicPlayer.Unmute();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        IsRunning = false;

        PlayerPrefs.SetInt(KeyDone, 1);
        PlayerPrefs.Save();

        Destroy(_canvasRoot);
        _canvasRoot = null;
    }

    // ════════════════════════════════════════════
    //  UI 빌드
    // ════════════════════════════════════════════
    void BuildUI()
    {
        // ── Canvas ──
        _canvasRoot = new GameObject("SocialEventTutorialOverlay");
        _canvasRoot.transform.SetParent(transform);
        var canvas = _canvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = DimSortOrder;
        var scaler = _canvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        _canvasRoot.AddComponent<GraphicRaycaster>();

        // ── 풀스크린 딤 ──
        var dimObj = new GameObject("Dim");
        dimObj.transform.SetParent(_canvasRoot.transform, false);
        _dimImage = dimObj.AddComponent<Image>();
        _dimImage.color = new Color(0f, 0f, 0f, 0f);
        _dimImage.raycastTarget = false;
        Stretch(dimObj.GetComponent<RectTransform>());

        // ── Phase 1: 사회 이벤트 안내 ──
        var welcomeObj = CreateStretchedGroup(_canvasRoot.transform, "WelcomeGroup");
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

        // ── Phase 2: TV / 신문 안내 ──
        var infoObj = CreateStretchedGroup(_canvasRoot.transform, "InfoGroup");
        _infoGroup = infoObj.GetComponent<CanvasGroup>();
        _infoGroup.alpha = 0f;

        var tvTMP = CreateTMP(infoObj.transform, "TvInfo",
            tvInfoText, tvInfoFontSize, tvInfoColor, FontStyles.Bold, font);
        SetAnchored(tvTMP.rectTransform, 0.05f, 0.95f, 0.55f, 0.68f);

        var paperTMP = CreateTMP(infoObj.transform, "PaperInfo",
            paperInfoText, paperInfoFontSize, paperInfoColor, FontStyles.Bold, font);
        SetAnchored(paperTMP.rectTransform, 0.05f, 0.95f, 0.42f, 0.55f);

        var startTMP = CreateTMP(infoObj.transform, "Start",
            startText, startFontSize, startColor, FontStyles.Italic, font);
        SetAnchored(startTMP.rectTransform, 0.2f, 0.8f, 0.32f, 0.42f);
    }

    // ════════════════════════════════════════════
    //  유틸
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

    IEnumerator FadeDim(float from, float to, float dur)
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

    IEnumerator FadeGroup(CanvasGroup cg, float from, float to, float dur)
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

    IEnumerator WaitForClick()
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
    [ContextMenu("DEBUG: Reset Social Event Tutorial")]
    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(KeyDone);
        PlayerPrefs.DeleteKey("SocialEvent_FirstDayDone");
    }
}
