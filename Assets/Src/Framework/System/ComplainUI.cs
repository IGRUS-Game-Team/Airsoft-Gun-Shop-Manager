using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComplainUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform panelRoot;       // 배경 패널 (고정 크기)
    [SerializeField] private GameObject changeTextPrefab;
    [SerializeField] private float showDuration = 15f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private int maxMessages = 6;
    [SerializeField] private float spacing = 10f;       // 텍스트 간 간격

    [Header("Refs")]
    [SerializeField] private TimeUI timeUI;

    // FIFO 큐: 가장 오래된 메시지를 추적
    private readonly LinkedList<GameObject> messageQueue = new();

    private void Start()
    {
        // panelRoot에 VerticalLayoutGroup 자동 구성 (텍스트 겹침 방지)
        if (panelRoot != null)
        {
            var vlg = panelRoot.GetComponent<VerticalLayoutGroup>();
            if (vlg == null)
                vlg = panelRoot.gameObject.AddComponent<VerticalLayoutGroup>();

            vlg.childAlignment         = TextAnchor.UpperLeft;
            vlg.childControlHeight     = true;   // TMP가 자체 높이를 결정
            vlg.childControlWidth      = true;
            vlg.childForceExpandHeight = false;   // 빈 공간으로 늘리지 않음
            vlg.childForceExpandWidth  = true;
            vlg.spacing                = spacing;
            vlg.padding                = new RectOffset(8, 8, 8, 8);

            // ContentSizeFitter가 있으면 제거 → 패널 크기를 고정 유지
            var csf = panelRoot.GetComponent<ContentSizeFitter>();
            if (csf != null)
                Destroy(csf);
        }

        if (timeUI != null)
            timeUI.OnComplainWithTime += HandleComplainChanged;
    }

    private void OnDestroy()
    {
        if (timeUI != null)
            timeUI.OnComplainWithTime -= HandleComplainChanged;
    }

    private void HandleComplainChanged(string timeString, ComplainReason reason)
    {
        // 최대 메시지 수 초과 시 가장 오래된(FIFO) 것부터 즉시 제거
        while (messageQueue.Count >= maxMessages)
        {
            var oldest = messageQueue.First.Value;
            messageQueue.RemoveFirst();
            if (oldest != null) Destroy(oldest);
        }

        GameObject go = Instantiate(changeTextPrefab, panelRoot);
        messageQueue.AddLast(go);

        var text = go.GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            text.text              = FormatComplaint(timeString, reason);
            text.fontSize          = 20f;
            text.enableWordWrapping = true;
            text.overflowMode      = TextOverflowModes.Ellipsis;
        }

        // 각 텍스트에 LayoutElement를 붙여서 최소 높이 보장
        var le = go.GetComponent<LayoutElement>();
        if (le == null) le = go.AddComponent<LayoutElement>();
        le.minHeight       = 28f;
        le.preferredHeight = 28f;

        StartCoroutine(FadeAndRemove(go));
    }

    private string FormatComplaint(string time, ComplainReason reason)
    {
        switch (reason)
        {
            case ComplainReason.Expensive:
                return $"<color=#FFA040>[Price]</color>  {time}  Customer complained about the high price.";

            case ComplainReason.PaymentDelay:
                return $"<color=#FF6B6B>[Service]</color>  {time}  Customer is tired of waiting at the counter.";

            case ComplainReason.Protest:
                return $"<color=#FF4444>[Protest]</color>  {time}  Customer left due to the protest outside.";

            default:
                return $"{time}  A customer is complaining.";
        }
    }

    /// showDuration 후 페이드 아웃 → 파괴. FIFO 큐에서도 제거.
    private IEnumerator FadeAndRemove(GameObject go)
    {
        yield return new WaitForSeconds(showDuration);

        var text = go != null ? go.GetComponent<TextMeshProUGUI>() : null;
        if (text != null)
        {
            Color c = text.color;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                text.color = new Color(c.r, c.g, c.b, Mathf.Lerp(1f, 0f, t / fadeDuration));
                yield return null;
            }
        }

        messageQueue.Remove(go);
        if (go != null) Destroy(go);
    }
}
