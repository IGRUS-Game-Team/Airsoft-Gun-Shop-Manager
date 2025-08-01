using UnityEngine;

/// <summary>
/// UI 모드 진입 및 해제 담당 매니저 (카메라, 입력 제한 등 포함)
/// </summary>
public class MonitorUIModeManager : MonoBehaviour
{
    public static MonitorUIModeManager Instance { get; private set; }

    private Camera previousCam;
    private GameObject player;
    private bool inUIMode = false;

    public bool getInUIMode()
    {
        return inUIMode;
    }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void EnterUIMode(Camera monitorCam)
    {
        if (inUIMode) return;

        // 플레이어 시점 저장
        previousCam = Camera.main;
        player = GameObject.FindWithTag("Player");
        if (player.TryGetComponent(out MonoBehaviour controller))
            controller.enabled = false;

        // 카메라 전환
        previousCam.gameObject.SetActive(false);
        monitorCam.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        inUIMode = true;
    }

    public void ExitUIMode()
    {
        if (!inUIMode) return;

        // 원래 시점 복구
        previousCam.gameObject.SetActive(true);
        GameObject monitorCamObj = GameObject.FindWithTag("MonitorCam");
        if (monitorCamObj) monitorCamObj.SetActive(false);

        if (player.TryGetComponent(out MonoBehaviour controller))
            controller.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        inUIMode = false;
    }
}

