using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(NpcController))]
public class NpcProfile : MonoBehaviour
{
    [Header("참조 (프리팹에서 연결)")]
    [SerializeField] private NpcArchetype archetype;
    [SerializeField] private NpcPurchaseStrategy strategy;

    [Header("이 NPC의 고유 N% (Awake에서 1회 샘플링)")]
    [SerializeField] private float sampledNPercent;

    public NpcArchetype Archetype => archetype;
    public NpcPurchaseStrategy Strategy => strategy;
    public float SampledNPercent => sampledNPercent;
    public NpcType Type => archetype != null ? archetype.type : NpcType.Normal;

    void Awake()
    {
        if (archetype == null || strategy == null)
        {
            Debug.LogWarning($"[NpcProfile] {name}: Archetype/Strategy 미연결");
            return;
        }
        sampledNPercent = archetype.SampleNPercent();
    }

    // 아이템 ID만 알고 있을 때 (시세는 매니저에서 가져옴)
    public bool WillBuyByItemId(int itemId, float offerPrice)
    {
        if (strategy == null || archetype == null) return false;

        float market;
        if (!MarketPriceDataManager.Instance.TryGetMarketPrice(itemId, out market))
            return false; // 시세 없으면 보수적으로 불가

        return strategy.WillBuy(offerPrice, market, this);
    }

    // 시세를 이미 갖고 있을 때
    public bool WillBuyWithMarket(float offerPrice, float marketPrice)
    {
        if (strategy == null || archetype == null) return offerPrice <= marketPrice;
        return strategy.WillBuy(offerPrice, marketPrice, this);
    }
}