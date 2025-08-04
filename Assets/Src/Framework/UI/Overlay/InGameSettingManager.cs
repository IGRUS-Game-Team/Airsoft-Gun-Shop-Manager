using UnityEngine;

public class InGameSettingManager : MonoBehaviour
{
    [SerializeField] GameObject IngameSettingUI;
    public static InGameSettingManager Instance;

    private bool IsSettingOpen = false;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool GetIsSettingOpen()
    {
        return IsSettingOpen;
    }
    public void SetSetting()
    {
        if (!IsSettingOpen) OpenSetting();
        else CloseSetting();
    }
    private void OpenSetting()
    {
        IsSettingOpen = true;
        IngameSettingUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseSetting()
    {
        IsSettingOpen = false;
        IngameSettingUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

}
