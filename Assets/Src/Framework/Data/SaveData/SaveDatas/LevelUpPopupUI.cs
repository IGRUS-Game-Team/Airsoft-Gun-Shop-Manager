using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpPopupUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] GameObject popupRoot;          
    [SerializeField] TextMeshProUGUI levelTMP;      
    [SerializeField] TextMeshProUGUI levelTOP;      
    [SerializeField] TextMeshProUGUI levelBody;     
    [SerializeField] CanvasGroup cg;                
    [SerializeField] Image panelImage;              
    [SerializeField] SpriteRenderer panelSprite;    

    [Header("Timing")]
    [SerializeField] float fadeIn  = 0.2f;
    [SerializeField] float showSec = 1.5f;
    [SerializeField] float fadeOut = 0.25f;

    Coroutine running;

    void Awake()
    {
        Debug.Log("[LevelUpPopupUI] Awake");
        // 팝업 비활성 + 알파 0으로 초기화
        if (popupRoot) popupRoot.SetActive(false);
        SetAlpha(0f);
    }

    void OnEnable()
    {
        Debug.Log("[LevelUpPopupUI] OnEnable 호출됨");
        TrySubscribe();
    }

    void Start()
    {
        Debug.Log("[LevelUpPopupUI] Start 호출됨");
        TrySubscribe();
    }

    void OnDisable()
    {
        Debug.Log("[LevelUpPopupUI] OnDisable 호출됨");
        if (RevenueXPTracker.Instance != null)
        {
            RevenueXPTracker.Instance.OnLevelChanged.RemoveListener(HandleLevelChanged);
            Debug.Log("[LevelUpPopupUI] 이벤트 구독 해제 완료");
        }
    }

    void TrySubscribe()
    {
        if (RevenueXPTracker.Instance != null)
        {
            RevenueXPTracker.Instance.OnLevelChanged.RemoveListener(HandleLevelChanged);
            RevenueXPTracker.Instance.OnLevelChanged.AddListener(HandleLevelChanged);
            Debug.Log("[LevelUpPopupUI] RevenueXPTracker 이벤트 구독 성공");
        }
        else
        {
            Debug.LogWarning("[LevelUpPopupUI] RevenueXPTracker.Instance 없음");
        }
    }

    void HandleLevelChanged(int newLevel)
    {
        Debug.Log($"[LevelUpPopupUI] HandleLevelChanged fired: {newLevel}");

        // newLevel == 0일 때도 로그 확인
        if (newLevel <= 0)
        {
            Debug.Log("[LevelUpPopupUI] newLevel <= 0 이므로 무시됨");
            return;
        }

        if (running != null)
        {
            Debug.Log("[LevelUpPopupUI] 이전 코루틴 중단");
            StopCoroutine(running);
        }

        Debug.Log($"[LevelUpPopupUI] ShowRoutine 시작 (레벨 {newLevel})");
        running = StartCoroutine(ShowRoutine(newLevel));
    }

    IEnumerator ShowRoutine(int level)
    {
        Debug.Log($"[LevelUpPopupUI] ShowRoutine 실행 중... (level={level})");

        if (levelTMP) levelTMP.text = $"LV. {level}";

        switch (level)
        {
            case 1:
                levelTOP.text = $"Now, Your Level is {level}";
                levelBody.text = "Your store just expanded by 50%!\nNow you can host up to 8 visitors at once!";
                break;
            case 2:
                levelTOP.text = $"Now, Your Level is {level}";
                levelBody.text = "An indoor shooting range is unlocked!\nCustomers can now enjoy it for just $10!";
                break;
            case 3:
                levelTOP.text = $"Now, Your Level is {level}";
                levelBody.text = "A rare collection zone has opened!\nCollectors rush in ×2, and you can host up to 15 visitors!";
                break;
            default:
                Debug.LogWarning($"[LevelUpPopupUI] 정의되지 않은 레벨 {level}, 팝업 생략");
                yield break;
        }

        Debug.Log("[LevelUpPopupUI] popupRoot 활성화");
        popupRoot.SetActive(true);

        yield return FadeTo(1f, fadeIn);           
        Debug.Log("[LevelUpPopupUI] FadeIn 완료");

        yield return new WaitForSeconds(showSec);  
        Debug.Log("[LevelUpPopupUI] showSec 대기 완료");

        yield return FadeTo(0f, fadeOut);          
        Debug.Log("[LevelUpPopupUI] FadeOut 완료");

        popupRoot.SetActive(false);
        Debug.Log("[LevelUpPopupUI] popupRoot 비활성화");
        running = null;
    }

    IEnumerator FadeTo(float targetAlpha, float dur)
    {
        Debug.Log($"[LevelUpPopupUI] FadeTo 시작: target={targetAlpha}, dur={dur}");

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

        Debug.Log($"[LevelUpPopupUI] FadeTo 완료: 최종 alpha={targetAlpha}");
    }

    float GetAlpha()
    {
        if (cg) return cg.alpha;
        if (panelImage) return panelImage.color.a;
        if (panelSprite) return panelSprite.color.a;
        return 1f;
    }

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
