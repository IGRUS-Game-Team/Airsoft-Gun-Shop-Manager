using UnityEngine;

public class InGameSettingManager : MonoBehaviour
{
    [SerializeField] GameObject IngameSettingUI;
    public static InGameSettingManager Instance;

    private bool IsSettingOpen = false; //세팅이 활성화 상태일때 , 디폴트 비활성화
    
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
        else CloseSetting(); //커서x
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
        Cursor.visible = false; //커서 안보임
    }

}
