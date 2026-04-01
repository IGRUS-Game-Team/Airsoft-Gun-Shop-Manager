using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 최초 시작 시 환영 + 튜토리얼 안내 오버레이.
/// Phase 1: 전체 딤(튜토리얼 포함 전부 가림) + 환영 메시지
/// Phase 2: 전체 딤 유지 + 튜토리얼 패널만 딤 위에 표시 + 안내 텍스트
/// </summary>
public class TutorialWelcomeOverlay : MonoBehaviour
{
    [Header("튜토리얼 패널 (딤 위로 올릴 대상)")]
    [SerializeField] private RectTransform tutorialPanel;

    [Header("딤")]
    [SerializeField, Range(0f, 1f)] private float dimAlpha = 0.75f;

    [Header("폰트")]
    [SerializeField] private TMP_FontAsset font;

    [Header("Phase 1 — 환영 메시지")]
    [SerializeField] private string welcomeText = "Welcome to Airsoft Gun Shop Manager!";
    [SerializeField] private float welcomeFontSize = 42f;
    [SerializeField] private Color welcomeColor = Color.white;
    [SerializeField] private string continueText = "Click to continue";
    [SerializeField] private float continueFontSize = 22f;
    [SerializeField] private Color continueColor = new Color(0.7f, 0.7f, 0.7f);

    [Header("Phase 2 — 튜토리얼 안내")]
    [SerializeField] private float arrowFontSize = 52f;
    [SerializeField] private Color arrowColor = new Color(1f, 0.15f, 0.15f);
    [SerializeField] private string followText = "Follow the tutorial above!";
    [SerializeField] private float followFontSize = 30f;
    [SerializeField] private Color followColor = Color.white;
    [SerializeField] private string startText = "Click to start";
    [SerializeField] private float startFontSize = 20f;
    [SerializeField] private Color startColor = new Color(0.7f, 0.7f, 0.7f);

    const string KeyShown = "TutorialWelcomeShown";
    const int DimSortOrder = 1500;
    const int AboveDimSortOrder = 1501;

    // 런타임 UI
    private GameObject _canvasRoot;
    private Image _dimImage;
    private CanvasGroup _welcomeGroup;
    private CanvasGroup _followGroup;

    // Phase 2에서 tutorialPanel에 붙이는 임시 Canvas
    private Canvas _tutorialOverrideCanvas;

    void Start()
    {
        if (PlayerPrefs.GetInt(KeyShown, 0) == 1)
        {
            Destroy(this);
            return;
        }

        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        // 이동 + 시점 잠금 + BGM 뮤트
        GlobalInteractionFlagS.ModalDepth++;
        MusicPlayer.Mute();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        BuildUI();

        // ── Phase 1: 환영 메시지 ──
        _welcomeGroup.gameObject.SetActive(true);
        _followGroup.gameObject.SetActive(false);

        yield return FadeDim(0f, dimAlpha, 0.5f);
        yield return Fade(_welcomeGroup, 0f, 1f, 0.4f);

        yield return WaitForClick();

        // ── Phase 2: 튜토리얼 하이라이트 ──
        yield return Fade(_welcomeGroup, 1f, 0f, 0.3f);
        _welcomeGroup.gameObject.SetActive(false);

        // 튜토리얼 패널을 딤 위로 올리기
        RaiseTutorialPanel();

        _followGroup.gameObject.SetActive(true);
        yield return Fade(_followGroup, 0f, 1f, 0.4f);

        yield return WaitForClick();

        // ── 종료 ──
        yield return Fade(_followGroup, 1f, 0f, 0.3f);
        yield return FadeDim(dimAlpha, 0f, 0.4f);

        // 튜토리얼 패널 원래대로
        RestoreTutorialPanel();

        // 잠금 해제 + BGM 복원
        GlobalInteractionFlagS.ModalDepth--;
        MusicPlayer.Unmute();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PlayerPrefs.SetInt(KeyShown, 1);
        PlayerPrefs.Save();
        Destroy(_canvasRoot);
        Destroy(this);
    }

    // ════════════════════════════════════════════
    //  UI 빌드
    // ════════════════════════════════════════════
    void BuildUI()
    {
        // ── Canvas ──
        _canvasRoot = new GameObject("TutorialWelcomeOverlay");
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

        // ── Phase 1: 환영 텍스트 ──
        var welcomeObj = CreateStretchedGroup(_canvasRoot.transform, "WelcomeGroup");
        _welcomeGroup = welcomeObj.GetComponent<CanvasGroup>();
        _welcomeGroup.alpha = 0f;

        var titleTMP = CreateTMP(welcomeObj.transform, "Title",
            welcomeText, welcomeFontSize, welcomeColor, FontStyles.Bold, font);
        SetAnchored(titleTMP.rectTransform, 0f, 1f, 0.45f, 0.6f);

        var subTMP = CreateTMP(welcomeObj.transform, "Sub",
            continueText, continueFontSize, continueColor, FontStyles.Italic, font);
        SetAnchored(subTMP.rectTransform, 0f, 1f, 0.35f, 0.45f);

        // ── Phase 2: 안내 텍스트 + 화살표 ──
        var followObj = CreateStretchedGroup(_canvasRoot.transform, "FollowGroup");
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
        // 동적으로 추가한 경우 제거
        if (tutorialPanel.gameObject.GetComponent<GraphicRaycaster>() == null)
            Destroy(_tutorialOverrideCanvas);
        _tutorialOverrideCanvas = null;
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

    // ════════════════════════════════════════════
    //  페이드 / 입력 대기
    // ════════════════════════════════════════════
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

    IEnumerator Fade(CanvasGroup cg, float from, float to, float dur)
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
    [ContextMenu("DEBUG: Reset Welcome Overlay")]
    public void ResetOverlay()
    {
        PlayerPrefs.DeleteKey(KeyShown);
    }
}
