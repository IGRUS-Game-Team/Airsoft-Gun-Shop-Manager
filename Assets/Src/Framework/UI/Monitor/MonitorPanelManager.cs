using Unity.VisualScripting;
using UnityEngine;

public class MonitorPanelManager : MonoBehaviour
{
    [SerializeField] ShopUIController shopUIController;
    [SerializeField] GameObject panelMain;
    [SerializeField] GameObject panelStockOrder;
    //[SerializeField] GameObject panelBank;
    [SerializeField] GameObject panelManagement;
    [SerializeField] GameObject penelUnlock;
    [SerializeField] GameObject panelHiring;

    void Start()
    {
        ReturnMainPanel();
    }
    public void ShowPanel(int index)
    {
        if (index == 2 && shopUIController != null) shopUIController.Populate();
        panelMain.SetActive(index == 1);
        panelStockOrder.SetActive(index == 2);
       // panelBank.SetActive(index == 3);
        panelManagement.SetActive(index == 4);
        penelUnlock.SetActive(index == 5);
        if (panelHiring != null) panelHiring.SetActive(index == 6);

        TutorialEvents.RaiseMonitorTabOpened(index);
    }

    public bool IsMainPanelActive => panelMain != null && panelMain.activeSelf;

    public void ReturnMainPanel()
    {
        panelMain.SetActive(true);
        panelStockOrder.SetActive(false);
      //  panelBank.SetActive(false);
        panelManagement.SetActive(false);
        penelUnlock.SetActive(false);
        if (panelHiring != null) panelHiring.SetActive(false);
    }
}