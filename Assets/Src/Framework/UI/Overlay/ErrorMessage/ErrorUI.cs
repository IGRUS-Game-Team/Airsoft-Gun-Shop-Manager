using System.Collections;
using UnityEngine;

public class ErrorUI : MonoBehaviour
{
    // 인스펙터 창에서 실제 UI 그룹을 이 변수에 연결합니다.
    public GameObject errorGroup; 

    void Start()
    {
        // 스크립트가 시작될 때, UI 그룹을 보이지 않게 비활성화합니다.
        if (errorGroup != null)
        {
            errorGroup.SetActive(false);
        }

        // 이벤트 구독
        CalculatorOk.FailedCompare += ShowErrorMessage;
        Debug.Log("ErrorUI 이벤트 구독 완료");
    }

    void OnDestroy()
    {
        // 게임 오브젝트가 파괴될 때 안전하게 이벤트 구독을 해제합니다.
        CalculatorOk.FailedCompare -= ShowErrorMessage;
    }

    void ShowErrorMessage()
    {
        Debug.Log("FailedCompare 이벤트 수신! 에러 메시지 코루틴을 시작합니다.");
        StartCoroutine(UIActivate());
    }

    IEnumerator UIActivate()
    {
        Debug.Log("Error Group 활성화");
        if (errorGroup != null)
        {
            errorGroup.SetActive(true);
        }
        
        yield return new WaitForSeconds(1.5f);
        
        Debug.Log("Error Group 비활성화");
        if (errorGroup != null)
        {
            errorGroup.SetActive(false);
        }
    }
}