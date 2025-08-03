using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ShopUIController.cs 박정민
/// 아이템의 DB 가져와서 DB의 있는 아이템 수 만큼 아이템 카드 복사해서 ScrollView에 넣는 클래스
/// </summary>
public class ShopUIController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] ItemDatabase database;
    [SerializeField] Transform contentRoot;          // ScrollView/Viewport/Content
    [SerializeField] ItemCardView itemCardPrefab;    // 프리팹
    [SerializeField] MonitorShopCartManager cartManager;

    //public void OnClickFilter_All() => Populate(ItemCategory.All);
    public void OnClickFilter_MainWeapon() => Populate(ItemCategory.MainWeapon);
    public void OnClickFilter_ProtectiveGear() => Populate(ItemCategory.ProtectiveGear);
    public void OnClickFilter_Consumable() => Populate(ItemCategory.Consumable);
    public void OnClickFilter_Exhibition() => Populate(ItemCategory.Exhibition);
    List<ItemCardView> spawnedCards = new();

    void Start()
    {
        Populate(ItemCategory.MainWeapon);
    }

    public void Populate(ItemCategory itemCategory)
    {
        Clear();

        foreach (var item in database.items)
        {
            if (item.category != itemCategory) continue;
            var card = Instantiate(itemCardPrefab, contentRoot);
            card.Setup(item);
           
            //card.onAddToCart.AddListener(cartManager.AddItem); //
            card.onAddToCart.AddListener((data, amount) =>
            {
                cartManager.AddItem(data, amount);
            });
            spawnedCards.Add(card);
        }
    }

    void Clear()
    {
        foreach (var c in spawnedCards)
            Destroy(c.gameObject);
        spawnedCards.Clear();
    }
}