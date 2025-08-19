using UnityEngine;
/// <summary>
/// 장지원 8.16
/// 신문 오브젝트 클릭시 UI 화면 셋업
/// </summary>
public class NewsPaperObject : MonoBehaviour, IInteractable
{
    [Header("NewspaperUI")]
    [SerializeField] private GameObject newsPaperUIObject;
    private NewsPaperController newsPaperController;


    //newpapercontroller에 전달할 값
    private string currentTitleOB;
    private string currentEventStateOB;
    private string currentItemNameOB;
    void Start()
    {
        newsPaperController = GetComponentInChildren<NewsPaperController>(true);//자식에서 신문ui에 붙은 컴포넌트를 찾는다
    }
    
    void Awake()
    {
        SocialEventManager.OnEventUIUpdate += SaveCurrentEvent;
    }
    void OnDisable()
    {
        SocialEventManager.OnEventUIUpdate -= SaveCurrentEvent;
    }


    //받은 값 저장 -> 8시마다
    public void SaveCurrentEvent(string EeventName, string EeventStatus, string EitemName) 
    {
        newsPaperController.SaveData(EeventName, EeventStatus, EitemName);//Eevent로 수정
        newsPaperController.SelectRandomSentence(); //랜덤 문장 선택 -> 느려야하는 SelectRandomSentence
       

        currentTitleOB = EeventName;
        currentEventStateOB = EeventStatus;
        currentItemNameOB = EitemName;
        Debug.Log($"{currentTitleOB}/{currentEventStateOB}/{currentItemNameOB}");
    }
    
    //이미 이걸 클릭하기 이전에 받은 값 저장이 이루어져있어야 한다.
    public void Interact() //클릭할 때마다 호출
    {

        // //클릭 시 ui 활성화
        ClickObjectUIManager.Instance.OpenUI(newsPaperUIObject);

        newsPaperController.UpdateDisplay();
    }
}
