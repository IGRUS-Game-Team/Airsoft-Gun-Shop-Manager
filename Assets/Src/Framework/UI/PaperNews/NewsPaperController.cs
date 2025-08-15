using TMPro;
using UnityEngine;

public class NewsPaperController : MonoBehaviour
{
    [Header("신문 text 요소")]
    [SerializeField] TextMeshProUGUI title; //신문 제목
    [SerializeField] TextMeshProUGUI eventState; //이벤트 효과
    [SerializeField] TextMeshProUGUI itemName; // 대상 아이템

    private string currentTitle;
    private string currentEventState;
    private string currentItemName;

    void Start()
    {
        //처음에 시작화면에 나옴   
    }

    void OnEnable()
    {
        SocialEventManager.OnEventUIUpdate += SaveCurrentEvent;
    }
    void OnDisable()
    {
        SocialEventManager.OnEventUIUpdate -= SaveCurrentEvent;
    }

    //받은 값 저장하고 내보내기

    private void SaveCurrentEvent(string EeventName, string EeventStatus, string EitemName)
    {
        currentTitle = EeventName;
        currentEventState = EeventStatus;
        currentItemName = EitemName;

        title.text = currentTitle;
        eventState.text = currentEventState;
        itemName.text = currentItemName;
    }

}
