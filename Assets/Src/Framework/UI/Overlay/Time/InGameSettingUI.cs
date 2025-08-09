using UnityEngine;
using UnityEngine.SceneManagement;
public class InGameSettingUI : MonoBehaviour
{
    public void OnClickMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}