using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClerkHiringPanel : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] ClerkDatabase clerkDatabase;

    [Header("UI")]
    [SerializeField] Transform contentRoot;
    [SerializeField] ClerkCardCell cellPrefab;
    [SerializeField] TextMeshProUGUI centerMessage;

    private readonly List<ClerkCardCell> spawnedCells = new();
    private Coroutine messageCoroutine;

    void OnEnable()
    {
        Populate();
    }

    void Populate()
    {
        // 기존 자식 제거
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);
        spawnedCells.Clear();

        if (clerkDatabase == null) return;

        foreach (var clerk in clerkDatabase.clerks)
        {
            if (clerk == null) continue;

            var cell = Instantiate(cellPrefab, contentRoot);
            cell.Setup(clerk, this);
            spawnedCells.Add(cell);
        }

        // 레이아웃 강제 갱신
        Canvas.ForceUpdateCanvases();
        if (contentRoot is RectTransform rt)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        HideCenterMessage();
    }

    public void TryHireClerk(ClerkData clerkData)
    {
        if (AutoClerkController.Instance == null) return;
        if (AutoClerkController.Instance.IsHired) return;

        if (!GameState.Instance.SpendMoney(clerkData.hiringCost))
        {
            ShowCenterMessage("Not enough money!");
            return;
        }

        SettlementManager.Instance?.RegisterPurchaseCost(clerkData.hiringCost);
        AutoClerkController.Instance.HireClerk(clerkData);

        ShowCenterMessage("Hiring successful, this employee will start work from tomorrow.");
        RefreshAllCards();
    }

    public void FireClerk()
    {
        if (AutoClerkController.Instance == null) return;

        AutoClerkController.Instance.FireClerk();
        RefreshAllCards();
    }

    void RefreshAllCards()
    {
        foreach (var cell in spawnedCells)
            cell.RefreshButtonState();
    }

    void ShowCenterMessage(string msg)
    {
        if (centerMessage == null) return;

        centerMessage.text = msg;
        centerMessage.gameObject.SetActive(true);

        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);
        messageCoroutine = StartCoroutine(HideMessageAfterDelay(3f));
    }

    void HideCenterMessage()
    {
        if (centerMessage != null)
            centerMessage.gameObject.SetActive(false);
    }

    IEnumerator HideMessageAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        HideCenterMessage();
        messageCoroutine = null;
    }
}
