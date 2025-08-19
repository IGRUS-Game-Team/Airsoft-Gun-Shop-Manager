using UnityEngine;

public abstract class NpcPurchaseStrategy : ScriptableObject
{
    // 최종 상한가 계산 (시세와 NPC의 N%)
    public abstract float GetPriceCap(float marketPrice, NpcProfile npc);

    // 살지 여부 (상한가와 최종 제시가 비교)
    public virtual bool WillBuy(float offerPrice, float marketPrice, NpcProfile npc)
    {
        float cap = GetPriceCap(marketPrice, npc);
        return offerPrice <= cap;
    }
}