// ShopItemCell.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUnlockCELL : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public TextMeshProUGUI txtName;
    public TextMeshProUGUI txtPrice;
    public Button btnUnlock;

    private ItemData data;
    private CatalogUIManager owner;
    private bool priceRevealed;
    private bool unlocked;

    // 외부에서 셋업
public void Setup(ItemData item, CatalogUIManager owner)
{
    this.data = item; 
    this.owner = owner;
    
    if (icon != null && item.icon != null)
        icon.sprite = item.icon;
    
    txtPrice.text = data.baseCost.ToString();

    unlocked = UnlockedItemsStore.IsUnlocked(item.itemId);
    priceRevealed = true; // 원래 unlocked 였는데 → true로 바꿔서 항상 가격 표시

    btnUnlock.onClick.RemoveAllListeners();
    btnUnlock.onClick.AddListener(OnClickUnlock);

    RefreshText();
    RefreshButtonVisual();
}


    public void RefreshText()
    {
        // 커스텀 이름이 있으면 그걸로; 없으면 SO의 itemName 사용
        txtName.text = ItemOverrideStore.Instance.GetDisplayName(data);
        txtPrice.text = data.baseCost.ToString();
    }

    private void RefreshButtonVisual()
    {
        if (unlocked)
        {
            btnUnlock.interactable = false;
            btnUnlock.GetComponentInChildren<TextMeshProUGUI>().text = "Unlocked";
            txtPrice.text = "";
        }
        else if (priceRevealed)
        {
            btnUnlock.GetComponentInChildren<TextMeshProUGUI>().text = "Buy";
            btnUnlock.GetComponentInChildren<TextMeshProUGUI>().text = $"price {Mathf.RoundToInt(data.unlockCost)}";
        }
        else
        {
            btnUnlock.GetComponentInChildren<TextMeshProUGUI>().text = "Unlock";
            txtPrice.text = "";
        }
    }

    private void OnClickUnlock()
    {
        if (unlocked) return;

        if (!priceRevealed)
        {
            priceRevealed = true;       // 1클릭: 가격 공개
            RefreshButtonVisual();
            return;
        }

        // 2클릭: 구매 시도 → 돈 확인 후 차감
        var gs = FindFirstObjectByType<GameState>();
        int price = Mathf.RoundToInt(data.baseCost);
        if (gs == null) { Debug.LogWarning("GameState 없음"); return; }

        if (gs.Money >= price) // Money/SetMoney는 기존 세이브 파이프라인과 연결됨 
        {
            gs.SetMoney(gs.Money - price);

            unlocked = true;
            UnlockedItemsStore.MarkUnlocked(data.itemId);

            RefreshButtonVisual();

            // 이름 설정 팝업 오픈
            owner.OpenNameDialog(data);
        }
        else
        {
            // 돈 부족 피드백
            // TODO: 사운드/트윈
            Debug.Log("돈 부족");
        }
    }
}
