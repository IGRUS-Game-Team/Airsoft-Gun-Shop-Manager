using UnityEngine;

/// <summary>
/// UI 모드 진입 및 해제 담당 매니저 (카메라, 입력 제한 등 포함)
/// </summary>
public class MonitorUIModeManager : MonoBehaviour
{
    public static MonitorUIModeManager Instance { get; private set; }

    private Camera previousCam;
    private GameObject player;
    private CharacterController characterController;   // ★ 필드로 뺌
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

        if (player == null)
        {
            Debug.LogError("[MonitorUIModeManager] 'Player' 태그 오브젝트를 찾지 못했습니다.");
            return;
        }

        // 플레이어 컨트롤 스크립트 비활성 (기존 로직 유지)
        if (player.TryGetComponent(out MonoBehaviour controller))
            controller.enabled = false;

        // 카메라 전환
        if (previousCam != null)
            previousCam.gameObject.SetActive(false);

        if (monitorCam != null)
            monitorCam.gameObject.SetActive(true);

        // ★ 여기서 한 번만 가져와서 필드에 저장
        characterController = player.GetComponentInChildren<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        else
        {
            Debug.LogWarning($"[MonitorUIModeManager] {player.name} 아래에서 CharacterController를 찾지 못했습니다. (EnterUIMode)");
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        inUIMode = true;
    }

    public void ExitUIMode()
    {
        if (!inUIMode) return;

        // 원래 시점 복구
        if (previousCam != null)
            previousCam.gameObject.SetActive(true);

        GameObject monitorCamObj = GameObject.FindWithTag("MonitorCam");
        if (monitorCamObj)
            monitorCamObj.SetActive(false);

        if (player != null && player.TryGetComponent(out MonoBehaviour controller))
            controller.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ★ 필드로 저장해둔 characterController 다시 켜기
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        else if (player != null)
        {
            // 혹시라도 씬에서 바뀌었을 경우 대비
            characterController = player.GetComponentInChildren<CharacterController>();
            if (characterController != null)
                characterController.enabled = true;
            else
                Debug.LogWarning($"[MonitorUIModeManager] {player.name} 아래에서 CharacterController를 찾지 못했습니다. (ExitUIMode)");
        }

        inUIMode = false;
    }
}
