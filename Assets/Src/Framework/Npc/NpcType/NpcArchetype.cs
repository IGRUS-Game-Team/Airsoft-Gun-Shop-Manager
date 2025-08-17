using UnityEngine;

[CreateAssetMenu(menuName = "Game/NPC Archetype", fileName = "NPC_Archetype")]
public class NpcArchetype : ScriptableObject
{
    [Header("프리팹의 타입 표시용")]
    public NpcType type = NpcType.Normal;

    [Header("구매 상한의 N% 범위 (시세 + N%)")]
    [Tooltip("예) 일반: 0~10, 매니아: 10~30, 수집가: 30~70")]
    public Vector2 NPercentRange = new Vector2(0, 10);

    public float SampleNPercent()
    {
        return Random.Range(NPercentRange.x, NPercentRange.y);
    }
}