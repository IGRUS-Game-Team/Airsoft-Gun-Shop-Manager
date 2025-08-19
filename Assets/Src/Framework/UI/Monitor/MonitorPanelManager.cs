using Unity.VisualScripting;
using UnityEngine;

public class MonitorPanelManager : MonoBehaviour
{
    ShopUIController shopUIController;
    [SerializeField] GameObject panelMain;
    [SerializeField] GameObject panelStockOrder;
    //[SerializeField] GameObject panelBank;
    [SerializeField] GameObject panelManagement;
    [SerializeField] GameObject penelUnlock;

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
    }

    public void ReturnMainPanel()
    {
        panelMain.SetActive(true);
        panelStockOrder.SetActive(false);
      //  panelBank.SetActive(false);
        panelManagement.SetActive(false);
        penelUnlock.SetActive(false);
        
    }
}