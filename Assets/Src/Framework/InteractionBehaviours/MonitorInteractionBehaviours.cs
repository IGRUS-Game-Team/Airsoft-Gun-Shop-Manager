using UnityEngine;

public class MonitorInteractionBehaviour : MonoBehaviour, IInspectable
{
    [SerializeField] Camera monitorUICam;
    [SerializeField] RenderTextureUIClicker uiClicker;

    public void Interact()
    {
        EnterInspection();
    }

    public void EnterInspection()
    {
        if (monitorUICam == null)
        {
            Debug.LogError("monitorUICam이 연결되지 않았습니다. 에디터에서 연결해주세요!");
            return;
        }
        MonitorUIModeManager.Instance.EnterUIMode(monitorUICam);
        uiClicker.SetEnterTime(); 
        
    }

    public void ExitInspection()
    {
        MonitorUIModeManager.Instance.ExitUIMode();
    }
}
