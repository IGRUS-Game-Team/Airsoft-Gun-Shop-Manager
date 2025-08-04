using UnityEngine;

/// <summary>
/// 8/1 박정민
/// 모니터UI와 상호작용 중 ESC를 누르면 상호작용 모드를 탈출할 수 있도록 하는 스크립트입니다.
/// 상호작용중이 아닐시에는 setting창이 열리도록 구현함(8/4 추가)
/// TODO : 계산할때도 탈출 구현
/// </summary>
/// 
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
            InGameSettingManager.Instance.SetSetting();
        }
    }
}