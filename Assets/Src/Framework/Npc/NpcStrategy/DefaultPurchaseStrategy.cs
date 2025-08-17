using UnityEngine;

[CreateAssetMenu(menuName = "Game/NPC Purchase Strategy/Default", fileName = "NPC_Strategy_Default")]
public class DefaultPurchaseStrategy : NpcPurchaseStrategy
{
    public override float GetPriceCap(float marketPrice, NpcProfile npc)
    {
        // 시세 + N%
        return marketPrice * (1f + npc.SampledNPercent * 0.01f);
    }
}