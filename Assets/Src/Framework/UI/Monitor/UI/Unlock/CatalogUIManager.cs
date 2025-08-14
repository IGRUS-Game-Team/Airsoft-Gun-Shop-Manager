// CatalogUIManager.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatalogUIManager : MonoBehaviour
{
    [Header("Refs")]
    public ItemDatabase database;          // SO
    public Transform content;              // ScrollView Content
    public ShopItemUnlockCELL cellPrefab;        // 프리팹(이미지/이름/Unlock 버튼 포함)
    public GameObject nameDialogPrefab;    // 이름 설정 팝업 프리팹
    public Transform popup;

    [Header("Filter Buttons")]
    public Button btnMainWeapon, btnProtective, btnConsumable, btnExhibition; //btnAll;

    private readonly List<ShopItemUnlockCELL> spawned = new();

    void Awake()
    {
        //btnAll.onClick.AddListener(() => Populate(null));
        btnMainWeapon.onClick.AddListener(() => Populate(ItemCategory.MainWeapon));
        btnProtective.onClick.AddListener(() => Populate(ItemCategory.ProtectiveGear));
        btnConsumable.onClick.AddListener(() => Populate(ItemCategory.Consumable));
        btnExhibition.onClick.AddListener(() => Populate(ItemCategory.Exhibition));
    }

    void OnEnable() => Populate(null); // 기본: 전체

public void Populate(ItemCategory? filter)
{
    Debug.Log("populate");

    // 1) Content 밑 자식 전부 제거 (안전하게 역순 삭제)
    for (int i = content.childCount - 1; i >= 0; i--)
    {
        Destroy(content.GetChild(i).gameObject);
    }
    spawned.Clear();

    // 2) 새로 채우기
    foreach (var item in database.items)
    {
        if (!item) continue;
        if (filter.HasValue && item.category != filter.Value) continue;

        var cell = Instantiate(cellPrefab, content);
        cell.Setup(item, this);
        spawned.Add(cell);
    }

    // 3) 레이아웃 강제 갱신 (빠른 클릭 시 UI 정리)
    Canvas.ForceUpdateCanvases();

    if (content is RectTransform rt)
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
}



    // 이름 팝업 열기 → 셀에서 호출
    public void OpenNameDialog(ItemData item)
    {
        var dialog = Instantiate(nameDialogPrefab, popup); // UI 상위에 띄움
        var ui = dialog.GetComponent<ItemNameDialog>();
        ui.Open(item, onApply: (newName) =>
        {
            ItemOverrideStore.Instance.SetCustomName(item.itemId, newName);
            // 셀들 갱신(표시 이름 바뀜)
            foreach (var c in spawned) c.RefreshText();
        });
    }
}
