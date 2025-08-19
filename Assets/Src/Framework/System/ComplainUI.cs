using System.Collections;
using TMPro;
using UnityEngine;

public class ComplainUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform panelRoot;
    [SerializeField] private GameObject changeTextPrefab;
    [SerializeField] private float showDuration = 15f;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Refs")]
    [SerializeField] private TimeUI timeUI; // 인스펙터에서 연결

    private void Start()
    {
        if (timeUI != null)
            timeUI.OnComplainWithTime.AddListener(HandleComplainChanged);
    }

    private void OnDestroy()
    {
        if (timeUI != null)
            timeUI.OnComplainWithTime.RemoveListener(HandleComplainChanged);
    }

    private void HandleComplainChanged(string timeString)
    {
        GameObject go = Instantiate(changeTextPrefab, panelRoot);
        var text = go.GetComponent<TextMeshProUGUI>();

        if (text != null)
        {
            text.text = $"{timeString} : A customer is complaining.";
        }

        StartCoroutine(FadeOutRoutine(go, text));
    }

    private IEnumerator FadeOutRoutine(GameObject go, TextMeshProUGUI text)
    {
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