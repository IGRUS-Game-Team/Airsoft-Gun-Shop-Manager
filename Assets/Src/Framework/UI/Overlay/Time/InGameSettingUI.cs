using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public class InGameSettingUI : MonoBehaviour
{
    [SerializeField] GameObject settingUI;

    public void OnEnable()
    {
        Time.timeScale = 0;
    }
    public void OnDisable()
    {
        Time.timeScale = 1;
    }
    public void OnClickMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void OnClickSetting()
    {
        settingUI.SetActive(true);
    }
}