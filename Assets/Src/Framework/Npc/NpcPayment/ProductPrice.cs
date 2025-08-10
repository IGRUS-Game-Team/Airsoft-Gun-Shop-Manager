using UnityEngine;

/// ▶ 각 상품 프리팹에 붙여 두면 baseCost 를 손쉽게 읽어 갈 수 있다.
public class ProductPrice : MonoBehaviour
{
    [Tooltip("가격 정보가 들어 있는 SO")]
    public ItemData data;          // ← 이미 있는 ScriptableObject

    public float Price => data != null ? data.baseCost : 0f;
}