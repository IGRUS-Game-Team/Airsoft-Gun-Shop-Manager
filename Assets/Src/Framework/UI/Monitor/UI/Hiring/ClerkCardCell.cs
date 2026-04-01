using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClerkCardCell : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public TextMeshProUGUI txtName;
    public TextMeshProUGUI txtCost;
    public Button btnAction;

    private ClerkData data;
    private ClerkHiringPanel owner;
    private float lastClickTime = -1f;

    public ClerkData Data => data;

    public void Setup(ClerkData clerkData, ClerkHiringPanel panel)
    {
        data = clerkData;
        owner = panel;

        if (icon != null && clerkData.icon != null)
            icon.sprite = clerkData.icon;

        if (txtName != null)
            txtName.text = clerkData.clerkName;

        if (txtCost != null)
            txtCost.text = $"${clerkData.hiringCost:0}";

        btnAction.onClick.RemoveAllListeners();
        btnAction.onClick.AddListener(OnClickAction);

        RefreshButtonState();
    }

    public void RefreshButtonState()
    {
        var ctrl = AutoClerkController.Instance;
        if (ctrl == null) return;

        var btnText = btnAction.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText == null) return;

        if (ctrl.IsHired)
        {
            if (ctrl.HiredClerkId == data.clerkId)
            {
                btnAction.interactable = true;
                btnText.text = "Fire";
            }
            else
            {
                btnAction.interactable = false;
                btnText.text = "Hire";
            }
        }
        else
        {
            btnAction.interactable = true;
            btnText.text = "Hire";
        }
    }

    private void OnClickAction()
    {
        // RenderTextureUIClicker(마우스다운)와 EventSystem(마우스업)이 중복 클릭 발생 방지
        if (Time.unscaledTime - lastClickTime < 0.3f) return;
        lastClickTime = Time.unscaledTime;

        var ctrl = AutoClerkController.Instance;
        if (ctrl == null) return;

        if (ctrl.IsHired && ctrl.HiredClerkId == data.clerkId)
        {
            owner.FireClerk();
        }
        else if (!ctrl.IsHired)
        {
            owner.TryHireClerk(data);
        }
    }
}
