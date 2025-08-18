using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpPopupUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] GameObject popupRoot;          // 전체 패널 오브젝트
    [SerializeField] TextMeshProUGUI levelTMP;      // "LV. 2" 텍스트
    [SerializeField] CanvasGroup cg;                // UI 패널일 때
    [SerializeField] Image panelImage;              // UI Image일 때
    [SerializeField] SpriteRenderer panelSprite;    // 2D Sprite일 때

    [Header("Timing")]
    [SerializeField] float fadeIn  = 0.2f;
    [SerializeField] float showSec = 1.5f;
    [SerializeField] float fadeOut = 0.25f;

    Coroutine running;

    void OnEnable()
    {
        if (RevenueXPTracker.Instance != null)
            RevenueXPTracker.Instance.OnLevelChanged.AddListener(HandleLevelChanged);
    }

    void OnDisable()
    {
        if (RevenueXPTracker.Instance != null)
            RevenueXPTracker.Instance.OnLevelChanged.RemoveListener(HandleLevelChanged);
    }

    void HandleLevelChanged(int newLevel)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(ShowRoutine(newLevel));
    }

    IEnumerator ShowRoutine(int level)
    {
        if (levelTMP) levelTMP.text = $"LV. {level}";
        
        popupRoot.SetActive(true);

        yield return FadeTo(1f, fadeIn);           // 인
        yield return new WaitForSeconds(showSec);  // 대기
        yield return FadeTo(0f, fadeOut);          // 아웃

        popupRoot.SetActive(false);
        running = null;
    }

    IEnumerator FadeTo(float targetAlpha, float dur)
    {
        float t = 0f;
        float start = GetAlpha();

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(start, targetAlpha, t / dur);
            SetAlpha(a);
            yield return null;
        }
        SetAlpha(targetAlpha);
    }

    // 알파값 가져오기
    float GetAlpha()
    {
        if (cg) return cg.alpha;
        if (panelImage) return panelImage.color.a;
        if (panelSprite) return panelSprite.color.a;
        return 1f;
    }

    // 알파값 적용
    void SetAlpha(float a)
    {
        if (cg)
        {
            cg.alpha = a;
        }
        else if (panelImage)
        {
            var c = panelImage.color;
            c.a = a;
            panelImage.color = c;
        }
        else if (panelSprite)
        {
            var c = panelSprite.color;
            c.a = a;
            panelSprite.color = c;
        }
    }
}
