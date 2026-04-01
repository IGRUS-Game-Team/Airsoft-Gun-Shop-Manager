using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NewsPaperController : MonoBehaviour
{
    [Header("신문 text 요소")]
    [SerializeField] TextMeshProUGUI sentence;

    private string currentTitle;
    private string currentEventState;
    private string currentItemName;
    private string selectedSentence;

    private const string BOOM_HEX = "#1A9A1A";
    private const string RECESSION_HEX = "#C02626";

    // ── 불황 상세 템플릿 ──
    private readonly List<string> recessionTemplates = new()
    {
        "<b>{event_name}</b>\n\n" +
        "Due to recent developments,\n" +
        "demand for <b>{item_name}</b> has dropped significantly.\n\n" +
        "Selling prices may fall by <b><color=" + RECESSION_HEX + ">10-30%</color></b>.\n\n" +
        "Consider adjusting your pricing strategy\n" +
        "to minimize losses.",

        "<b>{event_name}</b>\n\n" +
        "The market is experiencing a downturn\n" +
        "affecting <b>{item_name}</b>.\n\n" +
        "Consumer demand is weakening\n" +
        "and prices are expected to <b><color=" + RECESSION_HEX + ">decline</color></b>.\n\n" +
        "Cautious inventory management is recommended.",

        "<b>{event_name}</b>\n\n" +
        "A notable slowdown in sales\n" +
        "for <b>{item_name}</b> has been reported.\n\n" +
        "Retailers may need to <b><color=" + RECESSION_HEX + ">lower prices</color></b>\n" +
        "to move inventory.\n\n" +
        "Market outlook: {event_status}"
    };

    // ── 호황 상세 템플릿 ──
    private readonly List<string> boomTemplates = new()
    {
        "<b>{event_name}</b>\n\n" +
        "Demand for <b>{item_name}</b>\n" +
        "has surged following recent events.\n\n" +
        "Prices are expected to rise by <b><color=" + BOOM_HEX + ">10-30%</color></b>.\n\n" +
        "Now is an excellent time to capitalize\n" +
        "on increased consumer interest.",

        "<b>{event_name}</b>\n\n" +
        "Consumer interest in <b>{item_name}</b>\n" +
        "is at an all-time high.\n\n" +
        "Market prices are trending <b><color=" + BOOM_HEX + ">upward</color></b>\n" +
        "with strong sales ahead.\n\n" +
        "Stock up to meet the growing demand.",

        "<b>{event_name}</b>\n\n" +
        "The market is heating up\n" +
        "for <b>{item_name}</b>.\n\n" +
        "Sales are <b><color=" + BOOM_HEX + ">surging</color></b>\n" +
        "as customers rush to buy.\n\n" +
        "Market outlook: {event_status}"
    };

    // ── 평범한 하루 ──
    private readonly List<string> dayTemplates = new()
    {
        "All is quiet today,\nwith no major news to report.\n\nSometimes, a perfectly ordinary day\nis the greatest gift of all.",
        "The market is stable,\nand the world is peaceful.\n\nA perfect day to calmly plan\nfor the next opportunity.",
        "Have a good feeling about today?\n\nA small discovery\ncould lead to great fortune.",
        "Lady Luck seems to be\nsmiling on you today.\n\nWhatever you do\nis bound to have a great outcome."
    };

    private const string DEFAULT_MESSAGE =
        "No news yet today.\n\nCheck back after 8:00 AM for the latest market report.";

    // ── 데이터 저장 (8시마다 SocialEventManager → NewsPaperObject → 여기) ──
    public void SaveData(string eventName, string eventStatus, string itemName)
    {
        currentTitle = eventName;
        currentEventState = eventStatus;
        currentItemName = itemName;
    }

    // ── 8시마다 호출: 이벤트 종류에 맞는 랜덤 문장 선택 ──
    public void SelectRandomSentence()
    {
        if (string.IsNullOrEmpty(currentTitle))
        {
            selectedSentence = DEFAULT_MESSAGE;
            return;
        }

        if (currentTitle == "Day")
        {
            selectedSentence = dayTemplates[Random.Range(0, dayTemplates.Count)];
            return;
        }

        // Decrease → 불황, 그 외 → 호황
        if (currentEventState != null && currentEventState.Contains("Decrease"))
            selectedSentence = recessionTemplates[Random.Range(0, recessionTemplates.Count)];
        else
            selectedSentence = boomTemplates[Random.Range(0, boomTemplates.Count)];
    }

    // ── 신문 클릭할 때마다 호출 ──
    public void UpdateDisplay()
    {
        // 8시 전이거나 데이터가 아직 없으면 기본 메시지
        if (selectedSentence == null)
        {
            sentence.text = DEFAULT_MESSAGE;
            return;
        }

        if (currentTitle == "Day")
        {
            sentence.text = selectedSentence;
            return;
        }

        bool isRecession = currentEventState != null && currentEventState.Contains("Decrease");
        string hex = isRecession ? RECESSION_HEX : BOOM_HEX;

        string coloredItem = $"<color={hex}>{currentItemName ?? ""}</color>";
        string coloredStatus = $"<color={hex}>{currentEventState ?? ""}</color>";

        sentence.text = selectedSentence
            .Replace("{event_name}", currentTitle ?? "")
            .Replace("{event_status}", coloredStatus)
            .Replace("{item_name}", coloredItem);
    }

    // 오케이 클릭 시 close ui 발동 : 인스펙터에서 연결
    public void CloseNews()
    {
        ClickObjectUIManager.Instance.CloseUI(this.gameObject);
    }
}
