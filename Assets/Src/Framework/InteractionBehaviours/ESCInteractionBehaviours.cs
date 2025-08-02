using UnityEngine;

public class ESCInteractionBehaviours : MonoBehaviour
{
    private void Start()
    {
        InteractionController.Instance.OnExitUI += HandleExitKeyPressed;
    }

    private void OnDisable()
    {
        if (InteractionController.Instance != null)
            InteractionController.Instance.OnExitUI -= HandleExitKeyPressed;
    }

    private void HandleExitKeyPressed()
    {
        if (MonitorUIModeManager.Instance.getInUIMode())
        {
            Debug.Log("UI 모드 → 나가기");
            MonitorUIModeManager.Instance.ExitUIMode();
        }
        else
        {
            Debug.Log("일반 모드 → 설정창 열기 (미구현)");
            // TODO: SettingsManager.Instance.OpenSettings(); 같은 식으로 나중에 연결
        }
    }
}