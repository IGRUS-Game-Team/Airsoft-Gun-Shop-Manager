using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NewsPaperController : MonoBehaviour
{
    [Header("신문 text 요소")]
    [SerializeField] TextMeshProUGUI sentence;


    private string currentTitle;
    private string currentEventState;
    private string currentItemName;
    private string selectedSentence;

    private List<string> randomSentence = new List<string>(){ //문장을 고정해서 나가기로
        "Breaking News: {event_name} is making headlines, and experts are predicting a significant {event_status} for {item_name}.",
        "Following recent reports of {event_name}, analysts forecast a notable {event_status} in the market for {item_name}.",
        "The recent confirmation of {event_name} is expected to directly influence sales of {item_name}, leading to a clear {event_status}."
    };
    private List<string> randomSentenceDay = new List<string>()// 평범한 하루의 멘트
    {
        "All is quiet today, with no major news to report. Sometimes, a perfectly ordinary day is the greatest gift of all.",
        "The market is stable, and the world is peaceful. A perfect day to calmly plan for the next opportunity.",
        "Have a good feeling about today? A small discovery could lead to great fortune.",
        "Lady Luck seems to be smiling on you today. Whatever you do is bound to have a great outcome."
    };



    //오브젝트에서 이 메서드를 통하여 ui에 데이터를 세이브해준다
    public void SaveData(string EeventName, string EeventStatus, string EitemName)
    {
        currentTitle = EeventName;
        currentEventState = EeventStatus;
        currentItemName = EitemName;
    }

    //랜덤 정하기 -> 이걸 특정 즉 8시마다 진행해야함 -> time스크립트에 이걸 이어주면 될 것 같다 이 스크립트르 참조해서
    public void SelectRandomSentence()
    {
        selectedSentence = randomSentence[Random.Range(0, randomSentence.Count)]; //랜덤 문자열 선택
        Debug.Log($"{selectedSentence}");
        //이게 updatedisplay보다 먼저 나와야함
    } 


    //디스플레이 업데이트->오브젝트 클릭할 때마다 호출되는 메서드
    public void UpdateDisplay()
    {
        if (currentTitle == "Day") //평상시일때
        {
            sentence.text = randomSentenceDay[Random.Range(0, randomSentenceDay.Count)];
            return;
        }
        sentence.text = selectedSentence //문자열 변환
            .Replace("{event_name}", currentTitle)
            .Replace("{event_status}", currentEventState)
            .Replace("{item_name}", currentItemName);

    }

    //오케이 클릭시 close ui 발동 : 인스펙터에서 연결
    public void CloseNews()
    {
        ClickObjectUIManager.Instance.CloseUI(this.gameObject);
    }


}

