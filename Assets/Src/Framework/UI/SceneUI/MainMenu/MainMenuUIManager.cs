using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIManager : MonoBehaviour
{

    [SerializeField] public GameObject settingPopup;
    [SerializeField] public GameObject saveLoadPopup;

    public void OnClickNewGame()
    {
        Debug.Log("새 게임 시작");
        SceneManager.LoadScene("RealFinal");
    }

    public void OnClickLoadSave()
    {
        Debug.Log("저장된 세이브 목록 창 열기");
        if (saveLoadPopup != null)
            saveLoadPopup.SetActive(true);
    }

    public void OnClickSetting()
    {
        Debug.Log("설정창 열기");
        settingPopup.SetActive(true);
    }

    public void OnClickQuit()
    {
        Debug.Log("게임 종료");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif 
    }
}
