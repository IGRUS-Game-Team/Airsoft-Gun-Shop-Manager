using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameSettingUI : MonoBehaviour
{
    [SerializeField] GameObject settingUI;
    [SerializeField] private Canvas settingCanvas;
    [SerializeField] private int sortOrder = 999;

    [Header("Save Slot UI (ES3SlotManager가 붙은 오브젝트)")]
    [SerializeField] private GameObject saveSlotPanel;

    private ES3SlotManager _slotManager;
    private bool _callbackRegistered;

    public void OnEnable()
    {
        Time.timeScale = 0;
        MusicPlayer.Mute();

        if (settingCanvas != null)
        {
            settingCanvas.overrideSorting = true;
            settingCanvas.sortingOrder = sortOrder;
        }
    }

    public void OnDisable()
    {
        Time.timeScale = 1;
        MusicPlayer.Unmute();
    }

    public void OnClickMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu 1");
    }

    public void OnClickSetting()
    {
        settingUI.SetActive(true);
    }

    /// <summary>
    /// Save 버튼 클릭 → ES3 슬롯 목록 패널을 열어 슬롯 선택 후 저장
    /// </summary>
    public void OnClickSave()
    {
        if (saveSlotPanel == null)
        {
            Debug.LogWarning("[InGameSettingUI] saveSlotPanel 참조가 없습니다.");
            return;
        }

        // ES3SlotManager 콜백을 런타임 등록 (프리팹 간 연결 불가하므로)
        if (!_callbackRegistered)
        {
            _slotManager = saveSlotPanel.GetComponent<ES3SlotManager>();
            if (_slotManager == null)
                _slotManager = saveSlotPanel.GetComponentInChildren<ES3SlotManager>(true);

            if (_slotManager != null)
            {
                _slotManager.onAfterSelectSlot.AddListener(OnSlotSelectedForSave);
                _callbackRegistered = true;
            }
        }

        saveSlotPanel.SetActive(true);
    }

    private void OnSlotSelectedForSave()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();

        // 저장 후 슬롯 패널 닫기
        if (saveSlotPanel != null)
            saveSlotPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        // 콜백 해제
        if (_callbackRegistered && _slotManager != null)
            _slotManager.onAfterSelectSlot.RemoveListener(OnSlotSelectedForSave);
    }
}
