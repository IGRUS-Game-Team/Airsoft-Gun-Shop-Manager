using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyChangeUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform panelRoot;        // Vertical Layout Group 패널
    [SerializeField] private GameObject changeTextPrefab; // TextMeshProUGUI 프리팹
    [SerializeField] private float showDuration = 3f;     // 유지 시간
    [SerializeField] private float fadeDuration = 1f;     // 사라지는 시간

    // 비활성 상태에서 발생한 변동을 저장해두고, 활성화 시 표시
    private readonly List<float> pendingDeltas = new();

    private void Start()
    {
        if (GameState.Instance != null)
            GameState.Instance.OnMoneyDelta.AddListener(HandleMoneyChanged);
    }

    private void OnDestroy()
    {
        if (GameState.Instance != null)
            GameState.Instance.OnMoneyDelta.RemoveListener(HandleMoneyChanged);
    }

    private void OnEnable()
    {
        // 비활성 중 코루틴이 멈춰서 남아있는 잔여 오브젝트 정리
        CleanUpPanel();

        // 비활성 동안 쌓인 변동분 일괄 표시
        foreach (var delta in pendingDeltas)
            ShowDelta(delta);
        pendingDeltas.Clear();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        CleanUpPanel();
    }

    private void CleanUpPanel()
    {
        if (panelRoot == null) return;
        for (int i = panelRoot.childCount - 1; i >= 0; i--)
            Destroy(panelRoot.GetChild(i).gameObject);
    }

    private void HandleMoneyChanged(float delta, float current)
    {
        if (Mathf.Approximately(delta, 0f)) return;

        if (!gameObject.activeInHierarchy)
        {
            pendingDeltas.Add(delta);
            return;
        }

        ShowDelta(delta);
    }

    private void ShowDelta(float delta)
    {
        GameObject go = Instantiate(changeTextPrefab, panelRoot);
        var text = go.GetComponent<TextMeshProUGUI>();

        if (text != null)
        {
            text.text = $"{(delta >= 0 ? "+" : "")}{delta:0}$";
            text.color = (delta >= 0) ? Color.green : Color.red;
        }

        StartCoroutine(FadeOutRoutine(go, text));
    }

    private IEnumerator FadeOutRoutine(GameObject go, TextMeshProUGUI text)
    {
        // 유지 시간만큼 대기 (timeScale 무관)
        yield return new WaitForSecondsRealtime(showDuration);

        if (text != null)
        {
            Color startColor = text.color;
            float t = 0f;

            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
                text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
        }

        Destroy(go);
    }
}
