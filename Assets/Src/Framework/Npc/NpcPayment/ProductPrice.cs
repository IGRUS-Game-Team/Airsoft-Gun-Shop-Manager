using UnityEngine;

/// ▶ 각 상품 프리팹에 붙여 두면 baseCost 를 손쉽게 읽어 갈 수 있다.
[System.Serializable]
public class ProductPrice : MonoBehaviour
{
    public CounterSlotData data;
    public float Price => data != null ? data.itemData.baseCost : 0f;
    public int Amount => data != null ? data.amount : 0;
} //TODO : 데이터 고치기
//FIXME : 변화된 값 받아오기로 고치기