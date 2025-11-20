using UnityEngine;

public class kkimotti : MonoBehaviour
{
    [Header("한 번에 변화시킬 평판 값(+/-)")]
    [SerializeField] private float step = 5f;

    [Header("오버레이 표시 토글 키")]
    [SerializeField] private KeyCode toggleOverlayKey = KeyCode.F1;

    [Header("증가/감소/리셋 키")]
    [SerializeField] private KeyCode increaseKey = KeyCode.Equals;    // '=' (플러스와 같이 쓰이는 키)
    [SerializeField] private KeyCode decreaseKey = KeyCode.Minus;     // '-'
    [SerializeField] private KeyCode resetKey    = KeyCode.BackQuote; // '`' (물결키)

    [Header("넘침 방지(선택)")]
    [SerializeField] private bool clampToRange = true;
    [SerializeField] private float minRep = -100f;
    [SerializeField] private float maxRep =  100f;

    [Header("오버레이 UI")]
    [SerializeField] private bool showOverlay = true;
    [SerializeField] private Vector2 overlayPos = new Vector2(12, 12);

    // 내부 캐시
    private float lastRepShown;

    void Update()
    {
        // 오버레이 토글
        if (Input.GetKeyDown(toggleOverlayKey))
            showOverlay = !showOverlay;

        // 평판 증가
        if (Input.GetKeyDown(increaseKey))
            Add(step);

        // 평판 감소
        if (Input.GetKeyDown(decreaseKey))
            Add(-step);

        // 평판 0으로 리셋
        if (Input.GetKeyDown(resetKey))
            SetRaw(0f);

        // 화면 표시용 캐시
        lastRepShown = GetCurrentRep();
    }

    void OnGUI()
    {
        if (!showOverlay) return;

        var style = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = 12
        };

        string text =
            $"[Reputation Debug]\n" +
            $"Rep: {lastRepShown:0.##}\n" +
            $"(+){KeyToString(increaseKey)}  (-){KeyToString(decreaseKey)}  (Reset){KeyToString(resetKey)}  (Toggle){KeyToString(toggleOverlayKey)}\n" +
            $"Step: {step}  Clamp: {(clampToRange ? $"{minRep}~{maxRep}" : "Off")}";

        Vector2 size = style.CalcSize(new GUIContent(text));
        Rect rect = new Rect(overlayPos.x, overlayPos.y, size.x + 8, size.y + 8);
        GUI.Box(rect, text, style);
    }

    // === Core ===
    private void Add(float delta)
    {
        var rep = ReputationState.Instance;
        if (rep == null) { Debug.LogWarning("[ReputationCheat] ReputationState.Instance 없음"); return; }

        if (clampToRange)
        {
            float cur = GetCurrentRep();
            float next = Mathf.Clamp(cur + delta, minRep, maxRep);
            rep.SetRaw(next);
        }
        else
        {
            rep.Add(delta);
        }
    }

    private void SetRaw(float value)
    {
        var rep = ReputationState.Instance;
        if (rep == null) { Debug.LogWarning("[ReputationCheat] ReputationState.Instance 없음"); return; }

        if (clampToRange)
            value = Mathf.Clamp(value, minRep, maxRep);

        rep.SetRaw(value);
    }

    private float GetCurrentRep()
    {
        // SettlementManager 등에서 쓰던 정적 접근자 사용
        return Mathf.Round(ReputationState.CurrentGlobal * 100f) / 100f;
    }

    private static string KeyToString(KeyCode key)
    {
        // 보기 좋은 키 문자열
        switch (key)
        {
            case KeyCode.Equals: return "=";
            case KeyCode.Minus:  return "-";
            case KeyCode.BackQuote: return "`";
            default: return key.ToString();
        }
    }

    // ===== 에디터에서 우클릭 > ContextMenu로도 조작 가능 =====
    [ContextMenu("Rep +Step")]
    private void Ctx_Increase() => Add(step);

    [ContextMenu("Rep -Step")]
    private void Ctx_Decrease() => Add(-step);

    [ContextMenu("Rep Reset 0")]
    private void Ctx_Reset()    => SetRaw(0f);
}