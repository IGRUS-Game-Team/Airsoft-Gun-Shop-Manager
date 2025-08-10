using UnityEngine;

[System.Serializable]
public class CounterSlotData : MonoBehaviour
{
    public GameObject itemObject; // 실제 내려놓은 게임 오브젝트
    public ItemData itemData;     // 이 안에 들어 있는 아이템 정보 (ScriptableObject)
    public int amount;            // 수량
}