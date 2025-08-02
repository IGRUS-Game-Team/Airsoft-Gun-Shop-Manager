using UnityEngine;

public class MonitorPanelManager : MonoBehaviour
{
    [SerializeField] GameObject panelMain;
    [SerializeField] GameObject panelStockOrder;
    [SerializeField] GameObject panelBank;
    [SerializeField] GameObject panelManagement;

    public void ShowPanel(int index)
    {
        panelMain.SetActive(index == 1);
        panelStockOrder.SetActive(index == 2);
        panelBank.SetActive(index == 3);
        panelManagement.SetActive(index == 4);
    }

    public void ReturnMainPanel()
    {
        panelMain.SetActive(true);
        panelStockOrder.SetActive(false);
        panelBank.SetActive(false);
        panelManagement.SetActive(false);
    }
}