using UnityEngine;
using UnityEngine.UI;

public class NextDay : MonoBehaviour
{
    [SerializeField] DateUI dateUI;
    [SerializeField] TimeUI timeUI;
    [SerializeField] OnDayEnd onDayEnd;
    [SerializeField] Button nextDayButton;

    private void Awake()
    {
        nextDayButton.onClick.AddListener(ChangeNextDay);
    }

    void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ChangeNextDay();
            }
        }

    public void ChangeNextDay()
    {
        dateUI.UpdateDate();
        onDayEnd.StartNextDay();
    }
}
