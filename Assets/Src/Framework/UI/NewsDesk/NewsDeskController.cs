using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// NewsDeskScene에 배치.
/// 뉴스 TV 화면 + 자막 연출 + 아무 키 입력으로 닫기.
/// </summary>
public class NewsDeskController : MonoBehaviour
{
    [System.Serializable]
    public class NewsEntry
    {
        public string eventName;
        public Texture tvTexture;
        [TextArea] public string[] subtitleLines;
    }

    [Header("이벤트별 뉴스 데이터 (Inspector에서 세팅)")]
    [SerializeField] private NewsEntry[] entries;

    [Header("TV 화면")]
    [SerializeField] private NewsStage3D_UINoSlideController tvScreen;

    [Header("자막 UI")]
    [SerializeField] private TextMeshProUGUI subtitleText;

    [Header("아무 키를 눌러 계속")]
    [SerializeField] private GameObject pressAnyKeyHint;

    [Header("자막 속도 (글자당 초)")]
    [Tooltip("한 글자를 읽는 데 걸리는 시간. 0.08 ≈ 분당 750자")]
    [SerializeField] private float secondsPerCharacter = 0.08f;

    [Header("자막 최소 표시 시간(초)")]
    [SerializeField] private float minDisplayTime = 1.5f;

    [Header("뉴스 UI를 다른 UI 위에 표시할 Canvas")]
    [Tooltip("NewsDeskScene의 Canvas에 설정. Sort Order를 높게 잡으면 다른 UI 위에 표시됨")]
    [SerializeField] private Canvas newsCanvas;

    [Header("Canvas Sort Order (높을수록 앞)")]
    [SerializeField] private int canvasSortOrder = 100;

    public event Action OnDismissed;

    private bool waitingForKey;

    private void Awake()
    {
        if (newsCanvas != null)
        {
            newsCanvas.overrideSorting = true;
            newsCanvas.sortingOrder = canvasSortOrder;
        }
    }

    public void Show(string eventName)
    {
        NewsEntry entry = FindEntry(eventName);
        if (entry == null)
        {
            Debug.LogWarning($"[NewsDeskController] '{eventName}' 에 해당하는 뉴스 엔트리가 없습니다.");
            OnDismissed?.Invoke();
            return;
        }

        if (tvScreen != null && entry.tvTexture != null)
            tvScreen.SetIssueTexture(entry.tvTexture);

        if (pressAnyKeyHint != null)
            pressAnyKeyHint.SetActive(false);

        StartCoroutine(SubtitleRoutine(entry.subtitleLines));
    }

    private static string StripBrackets(string s)
    {
        if (s == null) return null;
        return s.Trim().TrimStart('[').TrimEnd(']').Trim();
    }

    private NewsEntry FindEntry(string eventName)
    {
        if (entries == null) return null;
        string stripped = StripBrackets(eventName);
        foreach (var e in entries)
        {
            if (string.Equals(StripBrackets(e.eventName), stripped, StringComparison.OrdinalIgnoreCase))
                return e;
        }
        return null;
    }

    private IEnumerator SubtitleRoutine(string[] lines)
    {
        if (subtitleText != null)
        {
            subtitleText.text = "";
            subtitleText.enableWordWrapping = false;
        }

        if (lines != null)
        {
            foreach (var line in lines)
            {
                if (subtitleText != null)
                    subtitleText.text = line;

                float duration = Mathf.Max(line.Length * secondsPerCharacter, minDisplayTime);
                yield return new WaitForSecondsRealtime(duration);
            }
        }

        // 자막 숨기고 pressAnyKey 표시
        if (subtitleText != null)
            subtitleText.text = "";

        if (pressAnyKeyHint != null)
            pressAnyKeyHint.SetActive(true);

        waitingForKey = true;
    }

    private void Update()
    {
        if (!waitingForKey) return;

        // ESC는 InGameSettingUI용이므로 뉴스 닫기에서 제외
        if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape))
        {
            waitingForKey = false;
            OnDismissed?.Invoke();
        }
    }
}
