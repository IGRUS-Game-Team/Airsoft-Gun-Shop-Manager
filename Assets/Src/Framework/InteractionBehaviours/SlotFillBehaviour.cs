using UnityEngine;

/// 플레이어가 슬롯을 클릭하면 상품을 진열 (안쪽→바깥쪽, 최대 2개)
[RequireComponent(typeof(ShelfSlot))]
public class SlotFillBehaviour : MonoBehaviour, IInteractable
{
    [Tooltip("기본 진열 프리팹 (없으면 무시)")]
    [SerializeField] GameObject defaultProduct;

    ShelfSlot slot;

    void Awake() => slot = GetComponent<ShelfSlot>();

    public void Interact()
    {
        if (slot.IsFull) return;

        GameObject prefab = defaultProduct;
        if (prefab == null)
        {
            Debug.LogWarning($"{name} : defaultProduct 가 비어 있어 진열 불가");
            return;
        }

        int idx = slot.ItemCount;                   // 0 또는 1
        Transform snap = slot.GetSnapPoint(idx);

        GameObject go = Instantiate(prefab, snap.position, snap.rotation, slot.transform);
        go.transform.localScale = new Vector3(1.4f,1.4f,1.4f); //상품 프리팹 크기

        slot.RegisterNewItem(go);                   // 리스트에 추가
    }
}