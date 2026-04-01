using UnityEngine;

/// <summary>
/// 디버그용: F10을 누르면 신문 이벤트를 순서대로 강제 발생시키고 신문 UI를 연다.
/// 메인 게임 씬의 아무 오브젝트에 붙여서 사용.
/// </summary>
public class NewsPaperDebugTrigger : MonoBehaviour
{
    [Header("키 매핑 (Inspector에서 변경 가능)")]
    [SerializeField] private KeyCode triggerKey = KeyCode.F10;

    [Header("NewsPaperObject 참조")]
    [SerializeField] private NewsPaperObject paperObject;

    [System.Serializable]
    public struct DebugEvent
    {
        public string eventName;
        public string eventStatus;
        public string itemName;
    }

    [Header("테스트할 이벤트 목록")]
    [SerializeField] private DebugEvent[] debugEvents =
    {
        new() { eventName = "[ Gun control following a shooting incident ]", eventStatus = "Decrease in demand", itemName = "AK-47" },
        new() { eventName = "[ Economic recession ]",                        eventStatus = "Decrease in demand", itemName = "All Guns" },
        new() { eventName = "[ Gun control protest ]",                       eventStatus = "Decrease in demand", itemName = "M4A1, SCAR-H" },
        new() { eventName = "[ Relaxation of gun regulations ]",             eventStatus = "Increase in demand", itemName = "AK-47, M4A1" },
        new() { eventName = "[ Popularity of shooting game ]",               eventStatus = "Increase in demand", itemName = "All Guns" },
        new() { eventName = "[ Popularity of gun-action movies ]",           eventStatus = "Increase in demand", itemName = "All Guns" },
        new() { eventName = "[ Military festival opens ]",                   eventStatus = "Increase in demand", itemName = "All Guns" },
        new() { eventName = "Day",                                           eventStatus = "",                   itemName = "" },
    };

    private int currentIndex = 0;

    private void Update()
    {
        if (Input.GetKeyDown(triggerKey))
        {
            if (paperObject == null)
            {
                paperObject = FindObjectOfType<NewsPaperObject>();
                if (paperObject == null)
                {
                    Debug.LogError("[NewsPaperDebug] NewsPaperObject를 찾을 수 없습니다.");
                    return;
                }
            }

            var e = debugEvents[currentIndex];
            Debug.Log($"[NewsPaperDebug] {triggerKey} pressed → 신문 이벤트: {e.eventName} / {e.eventStatus} / {e.itemName}");

            // 데이터 주입 + 랜덤 문장 선택
            paperObject.SaveCurrentEvent(e.eventName, e.eventStatus, e.itemName);

            // 신문 UI 열기
            paperObject.Interact();

            currentIndex = (currentIndex + 1) % debugEvents.Length;
        }
    }
}
