using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ReputationUI : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private ReputationState reputationState;
    [SerializeField] private Slider slider;       // 선형 게이지 (min=0, max=1 권장)
    [SerializeField] private Image  fillImage;    // 색을 바꿀 Image(슬라이더 Fill 등)

    [Header("애니메이션")]
    [SerializeField] private float fillDuration = 0.25f; // 값 변경 시 보간 시간

    [Header("범위(-100~100, 0이 중앙)")]
    [SerializeField] private float minValue = -100f;
    [SerializeField] private float maxValue =  100f;

    [Header("색상 팔레트 설정")]
    [Tooltip("원시평판(-100~100) 기준 몇 단위마다 색을 바꿀지")]
    [SerializeField] private int colorStepSize = 10;      // 10단위 색 변경
    [Tooltip("팔레트를 수동 지정(비우면 그라디언트에서 자동 생성)")]
    [SerializeField] private Color[] stepColors;          // 길이: steps+1 (예: 21)
    [Tooltip("팔레트 자동 생성용 그라디언트(빨강→노랑→초록)")]
    [SerializeField] private Gradient colorGradient;

    [Header("함께 색을 바꿀 아이콘들")]
    [Tooltip("UI 이미지/텍스트(TMP 포함)는 Graphic으로 수집")]
    [SerializeField] private Graphic[] tintedGraphics;     // Image, RawImage, Text, TextMeshProUGUI 등
    [Tooltip("월드 스페이스 아이콘(스프라이트)은 선택")]
    [SerializeField] private SpriteRenderer[] tintedSprites;

    [Tooltip("색 변경 시 각 대상의 기존 알파값 유지")]
    [SerializeField] private bool preserveAlpha = true;

    private Coroutine animCo;

void Awake()
{
    if (colorGradient == null || colorGradient.colorKeys == null || colorGradient.colorKeys.Length == 0)
        colorGradient = BuildDefaultGradient();

    if (!reputationState)                       // ← 추가
        reputationState = ReputationState.Instance; // ← 추가

    BuildPaletteIfNeeded();

    if (reputationState != null)
        reputationState.OnReputationChanged.AddListener(SetNormalized);
    else
        Debug.LogWarning("[ReputationUI] ReputationState 인스턴스를 찾을 수 없습니다.");
}

    private void SetNormalized(float normalized01) // 0~1 (0=-100, 0.5=0, 1=+100)
    {
        normalized01 = Mathf.Clamp01(normalized01);

        if (animCo != null) StopCoroutine(animCo);
        animCo = StartCoroutine(AnimateFill(normalized01));

        ApplyQuantizedColor(normalized01); // 10단위 팔레트 색 적용(게이지+아이콘 동시)
    }

    private IEnumerator AnimateFill(float target01)
    {
        float start = slider ? slider.value : (fillImage ? fillImage.fillAmount : 0f);
        float t = 0f;

        while (t < fillDuration)
        {
            t += Time.deltaTime;
            float v = Mathf.Lerp(start, target01, t / fillDuration);

            if (slider)    slider.value       = v;
            if (fillImage) fillImage.fillAmount = v;

            yield return null;
        }

        if (slider)    slider.value       = target01;
        if (fillImage) fillImage.fillAmount = target01;
        animCo = null;
    }

    void Start()
    {
        if (slider) { slider.minValue = 0f; slider.maxValue = 1f; }

        float init01 = 0f;
        if (reputationState)
        {
            float raw = reputationState.Current; // -100~100 가정(호환용 Current)
            init01 = Mathf.InverseLerp(minValue, maxValue, raw);
        }

        if (slider)    slider.value       = init01;
        if (fillImage) fillImage.fillAmount = init01;

        ApplyQuantizedColor(init01); // 시작 시 색 반영(게이지+아이콘)
    }

    /// <summary>정규화 값(0~1)을 10단위 팔레트로 양자화하여 색상 적용</summary>
    private void ApplyQuantizedColor(float t01)
    {
        EnsureValidRange();
        BuildPaletteIfNeeded();

        // 정규화(0~1) → 원시값(-100~100) → 10단위 인덱스
        float raw = Mathf.Lerp(minValue, maxValue, t01);                 // -100..100
        int steps = Mathf.RoundToInt((maxValue - minValue) / colorStepSize); // 200/10=20
        steps = Mathf.Max(1, steps); // 안전장치

        int idx = Mathf.RoundToInt((raw - minValue) / colorStepSize);   // 0..20
        idx = Mathf.Clamp(idx, 0, steps);

        Color c = (stepColors != null && stepColors.Length == steps + 1)
            ? stepColors[idx]
            : colorGradient.Evaluate((float)idx / steps);

        // 게이지 + 추가 아이콘들 모두 색 적용
        ApplyColorToAllTargets(c);
    }

    /// <summary>게이지 Fill, Graphic[], SpriteRenderer[]에 일괄 색 적용</summary>
    private void ApplyColorToAllTargets(Color c)
    {
        if (fillImage)
            fillImage.color = PreserveAlpha(fillImage.color, c);

        if (tintedGraphics != null)
        {
            for (int i = 0; i < tintedGraphics.Length; i++)
            {
                var g = tintedGraphics[i];
                if (!g) continue;
                g.color = PreserveAlpha(g.color, c);
            }
        }

        if (tintedSprites != null)
        {
            for (int i = 0; i < tintedSprites.Length; i++)
            {
                var s = tintedSprites[i];
                if (!s) continue;
                s.color = PreserveAlpha(s.color, c);
            }
        }
    }

    private Color PreserveAlpha(Color original, Color target)
    {
        if (!preserveAlpha) return target;
        target.a = original.a;
        return target;
    }

    private void BuildPaletteIfNeeded()
    {
        EnsureValidRange();

        int steps = Mathf.RoundToInt((maxValue - minValue) / colorStepSize);
        steps = Mathf.Max(1, steps); // 최소 1

        // 팔레트가 없거나 길이가 맞지 않으면 그라디언트로 자동 생성
        if (stepColors == null || stepColors.Length != steps + 1)
        {
            stepColors = new Color[steps + 1];
            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps; // 0..1
                stepColors[i] = colorGradient.Evaluate(t);
            }
        }
    }

    private void EnsureValidRange()
    {
        if (minValue >= maxValue)
        {
            Debug.LogWarning($"[ReputationUI] minValue({minValue}) >= maxValue({maxValue})라서 자동 수정합니다.");
            if (Mathf.Approximately(minValue, maxValue)) maxValue = minValue + 0.0001f;
            if (colorStepSize <= 0) colorStepSize = 10;
        }
        if (colorStepSize <= 0) colorStepSize = 10;
    }

    private static Gradient BuildDefaultGradient()
    {
        var g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.red,    0f),  // -100
                new GradientColorKey(Color.yellow, 0.5f),//   0
                new GradientColorKey(Color.green,  1f),  // +100
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );
        return g;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (colorGradient == null || colorGradient.colorKeys == null || colorGradient.colorKeys.Length == 0)
            colorGradient = BuildDefaultGradient();
        BuildPaletteIfNeeded();
    }
#endif
}
