using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 모니터 UI 내에서 '나가기' 버튼 역할을 하는 컴포넌트입니다.
/// 버튼에 연결하면 UI 모드를 종료합니다.
/// </summary>
public class MonitorUIExitButtonController : MonoBehaviour
{

    [SerializeField] Button exitButton;

    public void OnExitButtonPressed()
    {
        MonitorUIModeManager.Instance.ExitUIMode();
    }
}
