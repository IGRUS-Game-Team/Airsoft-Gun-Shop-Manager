using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 메인 게임 씬에 배치.
/// SocialEventManager.OnNewsScreenUpdate 구독하여
/// "Day"가 아닌 이벤트일 때 NewsDeskScene을 Additive로 로드한다.
/// </summary>
public class NewsDeskLoader : MonoBehaviour
{
    [Header("시간 UI (isTimePaused 제어)")]
    [SerializeField] private TimeUI timeUI;

    [Header("NewsDeskScene 이름 (Build Settings에 등록 필요)")]
    [SerializeField] private string newsDeskSceneName = "NewsDeskScene";

    [Header("뉴스 중에도 꺼지지 않을 Canvas (InGameSettingUI 등)")]
    [SerializeField] private Canvas[] excludeCanvases;

    private Camera mainCamera;
    private bool isNewsActive;
    private List<Canvas> disabledCanvases = new List<Canvas>();

    /// <summary>
    /// true인 동안 HandleNewsEvent가 뉴스 씬을 로드하지 않는다.
    /// 세이브 복원 시 TV 화면 이미지만 갱신하고 뉴스 씬은 띄우지 않기 위해 사용.
    /// </summary>
    public static bool SuppressAutoLoad { get; set; }

    /// <summary>
    /// 오늘 발생한 마지막 이벤트명. TV 클릭으로 다시 볼 때 사용.
    /// </summary>
    public string LastEventName { get; private set; }

    private void OnEnable()
    {
        SocialEventManager.OnNewsScreenUpdate += HandleNewsEvent;
    }

    private void OnDisable()
    {
        SocialEventManager.OnNewsScreenUpdate -= HandleNewsEvent;
    }

    private void HandleNewsEvent(string eventName)
    {
        if (eventName == "Day")
        {
            LastEventName = null;
            return;
        }
        LastEventName = eventName;

        // 세이브 복원 중에는 뉴스 씬을 띄우지 않는다 (TV 화면 이미지만 갱신)
        if (SuppressAutoLoad) return;

        TriggerNews(eventName);
    }

    /// <summary>
    /// 외부(디버그 등)에서 직접 뉴스를 띄울 때 사용.
    /// </summary>
    public void TriggerNews(string eventName)
    {
        if (isNewsActive) return;

        LastEventName = eventName;
        isNewsActive = true;
        StartCoroutine(LoadNewsScene(eventName));
    }

    private IEnumerator LoadNewsScene(string eventName)
    {
        // 시간 정지 + BGM 뮤트
        if (timeUI != null)
            timeUI.isTimePaused = true;
        MusicPlayer.Mute();

        // 메인 카메라 비활성화
        mainCamera = Camera.main;
        if (mainCamera != null)
            mainCamera.gameObject.SetActive(false);

        // 메인 씬의 모든 Canvas 비활성화
        DisableMainSceneCanvases();

        // 씬 Additive 로드
        AsyncOperation op = SceneManager.LoadSceneAsync(newsDeskSceneName, LoadSceneMode.Additive);
        yield return op;

        // NewsDeskController 찾아서 Show 호출
        NewsDeskController controller = FindNewsDeskController();
        if (controller != null)
        {
            controller.OnDismissed += OnNewsDismissed;
            controller.Show(eventName);
        }
        else
        {
            Debug.LogError("[NewsDeskLoader] NewsDeskScene에서 NewsDeskController를 찾을 수 없습니다.");
            OnNewsDismissed();
        }
    }

    private void DisableMainSceneCanvases()
    {
        disabledCanvases.Clear();
        Scene mainScene = gameObject.scene;
        var excludeSet = new HashSet<Canvas>(excludeCanvases ?? System.Array.Empty<Canvas>());

        foreach (var root in mainScene.GetRootGameObjects())
        {
            foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
            {
                if (canvas.enabled && !excludeSet.Contains(canvas))
                {
                    canvas.enabled = false;
                    disabledCanvases.Add(canvas);
                }
            }
        }
    }

    private void RestoreMainSceneCanvases()
    {
        foreach (var canvas in disabledCanvases)
        {
            if (canvas != null)
                canvas.enabled = true;
        }
        disabledCanvases.Clear();
    }

    private NewsDeskController FindNewsDeskController()
    {
        Scene newsScene = SceneManager.GetSceneByName(newsDeskSceneName);
        if (!newsScene.IsValid()) return null;

        foreach (var root in newsScene.GetRootGameObjects())
        {
            var ctrl = root.GetComponentInChildren<NewsDeskController>(true);
            if (ctrl != null) return ctrl;
        }
        return null;
    }

    private void OnNewsDismissed()
    {
        StartCoroutine(UnloadNewsScene());
    }

    private IEnumerator UnloadNewsScene()
    {
        // 메인 카메라 재활성화
        if (mainCamera != null)
            mainCamera.gameObject.SetActive(true);

        // 씬 언로드
        AsyncOperation op = SceneManager.UnloadSceneAsync(newsDeskSceneName);
        if (op != null)
            yield return op;

        // 메인 씬 Canvas 복구
        RestoreMainSceneCanvases();

        // 시간 재개 + BGM 복원
        if (timeUI != null)
            timeUI.isTimePaused = false;
        MusicPlayer.Unmute();

        isNewsActive = false;

        // 사회 이벤트 뉴스 종료 알림 (튜토리얼용)
        Debug.Log("[NewsDeskLoader] 뉴스 씬 언로드 완료 → RaiseSocialEventNewsDismissed 호출");
        TutorialEvents.RaiseSocialEventNewsDismissed();
    }
}
