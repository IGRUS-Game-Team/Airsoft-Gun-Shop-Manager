using UnityEngine;

/// <summary>
/// 디버그용: F9를 누르면 뉴스 이벤트를 순서대로 강제 발생시킨다.
/// 메인 게임 씬의 아무 오브젝트에 붙여서 사용.
/// </summary>
public class NewsDeskDebugTrigger : MonoBehaviour
{
    [Header("키 매핑 (Inspector에서 변경 가능)")]
    [SerializeField] private KeyCode triggerKey = KeyCode.F9;

    [Header("NewsDeskLoader 참조")]
    [SerializeField] private NewsDeskLoader loader;

    [Header("테스트할 이벤트 이름")]
    [SerializeField] private string[] eventNames =
    {
        "[ Relaxation of gun regulations ]",
        "[ Popularity of shooting game ]",
        "[ Popularity of gun-action movies ]",
        "[ Military festival opens ]",
        "[ Gun control following a shooting incident ]",
        "[ Economic recession ]",
        "[ Gun control protest ]"
    };

    private int currentIndex = 0;

    private void Update()
    {
        if (Input.GetKeyDown(triggerKey))
        {
            if (loader == null)
            {
                loader = FindObjectOfType<NewsDeskLoader>();
                if (loader == null)
                {
                    Debug.LogError("[NewsDeskDebug] NewsDeskLoader를 찾을 수 없습니다.");
                    return;
                }
            }

            string eventName = eventNames[currentIndex];
            Debug.Log($"[NewsDeskDebug] {triggerKey} pressed → 이벤트 발생: {eventName}");

            // TV 화면 갱신 이벤트 발생
            SocialEventManager.InvokeNewsScreenUpdate(eventName);
            loader.TriggerNews(eventName);

            currentIndex = (currentIndex + 1) % eventNames.Length;
        }
    }
}
