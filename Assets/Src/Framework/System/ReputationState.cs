using TMPro;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class FloatEvent : UnityEvent<float> { }


public class ReputationState : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────
    public static ReputationState Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 전역 상태 유지
    }
    // ──────────────────────────────────────────────────────────

    [Header("평판 범위")]
    [SerializeField] private float minRep = -100f;
    [SerializeField] private float maxRep =  100f;
    [SerializeField] private float currentRep = 0f;   // -100~100
    [SerializeField] private TextMeshProUGUI currentRepTMP;
    [SerializeField] private bool useInteger = true;   // 정수 표시 여부
    [SerializeField] private int  decimals   = 0;      // 소수 자릿수 (useInteger=false일 때 사용)
    [SerializeField] private bool showPlusSign = false; // 양수에 + 붙일지

    // 새 이벤트
    public FloatEvent OnChanged01 = new FloatEvent();  // 0=-100, 0.5=0, 1=+100
    public FloatEvent OnChangedRaw = new FloatEvent(); // -100~100

    // 기존 UI 호환(이벤트/속성)
    [HideInInspector] public FloatEvent OnReputationChanged = new FloatEvent(); // = OnChanged01
    public float Current => currentRep;

    public float CurrentRaw => currentRep;
    public float Min => minRep;
    public float Max => maxRep;

    public void SetRaw(float raw)
    {
        // 방어: min < max 보장
        if (minRep >= maxRep)
        {
            Debug.LogWarning($"[ReputationState] minRep({minRep}) >= maxRep({maxRep})라 자동 보정합니다.");
            float tmpMin = minRep;
            minRep = Mathf.Min(minRep, maxRep - 0.0001f);
            maxRep = Mathf.Max(maxRep, tmpMin + 0.0001f);
        }

        float clamped = Mathf.Clamp(raw, minRep, maxRep);
        if (Mathf.Approximately(clamped, currentRep)) return;

        currentRep = clamped;
        UpdateRepText();          

        if (currentRepTMP)
            currentRepTMP.text = $"{currentRep:F2}/100";

        // 0~1 정규화 (중앙=0.5)
        float t01 = Mathf.InverseLerp(minRep, maxRep, currentRep);

        // 이벤트 브로드캐스트
        OnChangedRaw.Invoke(currentRep);
        OnChanged01.Invoke(t01);
        OnReputationChanged.Invoke(t01); // 호환 이벤트
    }

    public void Add(float delta) => SetRaw(currentRep + delta);
    public void Sub(float delta) => SetRaw(currentRep - delta);

    // 레거시 0~100 입력 지원(0->-100, 50->0, 100->100)
    public void SetFromLegacy0100(float legacy)
    {
        legacy = Mathf.Clamp(legacy, 0f, 100f);
        float newRaw = 2f * legacy - 100f;
        SetRaw(newRaw);
    }

    // 텍스트 갱신 유틸
    private void UpdateRepText()
    {
        if (!currentRepTMP) return;

        // 표시 값 포맷
        string textValue;
        if (useInteger)
        {
            int v = Mathf.RoundToInt(currentRep);
            textValue = showPlusSign && v > 0 ? $"+{v}" : v.ToString();
        }
        else
        {
            // F{decimals}로 소수 고정 자리
            string fmt = "F" + Mathf.Max(0, decimals);
            textValue = currentRep.ToString(fmt);
            if (showPlusSign && currentRep > 0f) textValue = "+" + textValue;
        }

        // 요청대로 항상 /100 고정
        currentRepTMP.text = $"{textValue}/100";
    }

    // 전역 접근 헬퍼(원하면 사용)
    public static float CurrentGlobal => Instance ? Instance.currentRep : 0f;
    public static void SetRawGlobal(float raw) { if (Instance) Instance.SetRaw(raw); }
    public static void AddGlobal(float d)     { if (Instance) Instance.Add(d);     }

    private void Start() => SetRaw(currentRep); // 초기 이벤트 발행

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 에디터에서 값 바꾸면 즉시 반영(Play 중/비중 상관없이)
        if (Application.isPlaying) SetRaw(currentRep);
        else
        {
            // 텍스트만 미리 갱신
            UpdateRepText();          
        }
    }
#endif
}
