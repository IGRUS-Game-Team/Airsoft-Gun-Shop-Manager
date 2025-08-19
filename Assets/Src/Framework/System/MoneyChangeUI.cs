using System.Collections;
using TMPro;
using UnityEngine;

public class MoneyChangeUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform panelRoot;        // Vertical Layout Group 패널
    [SerializeField] private GameObject changeTextPrefab; // TextMeshProUGUI 프리팹
    [SerializeField] private float showDuration = 3f;     // 유지 시간
    [SerializeField] private float fadeDuration = 1f;     // 사라지는 시간

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

    private void HandleMoneyChanged(float delta, float current)
    {
        if (Mathf.Approximately(delta, 0f)) return;

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
        // 유지 시간만큼 대기
        yield return new WaitForSeconds(showDuration);

        if (text != null)
        {
            Color startColor = text.color;
            float t = 0f;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
                text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
        }

        Destroy(go);
    }
}
