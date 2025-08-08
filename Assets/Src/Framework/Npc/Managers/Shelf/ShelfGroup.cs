// using System.Collections.Generic;
// using UnityEngine;

// // 세로 칸(Left Slots·Right Slots)을 대표하는 컴포넌트.
// [DisallowMultipleComponent]
// public class ShelfGroup : MonoBehaviour
// {
//     // 자식 슬롯들 자동 수집
//     [SerializeField] private List<ShelfSlot> slots = new();

//     public bool IsReserved { get; private set; }

//     private void Awake()
//     {
//         if (slots.Count == 0)
//             GetComponentsInChildren(slots);

//         // 각 슬롯이 나를 가리키게 해 둔다
//         foreach (var slot in slots)
//             slot.ParentGroup = this;
//     }

//     public bool TryReserve()
//     {
//         if (IsReserved) return false;
//         IsReserved = true;
//         return true;
//     }

//     public void Release() => IsReserved = false;

//     // 아직 예약 안 됐고, 상품이 있는 슬롯 하나 반환
//     public ShelfSlot GetFreeSlotWithItem()
//     {
//         if (IsReserved) return null;
//         foreach (var s in slots)
//             if (s.HasItem) return s;
//         return null;
//     }

//     public List<ShelfSlot> GetItemSlots()
//     {
//         List<ShelfSlot> list = new ();
//         foreach (var s in slots)
//             if (s.HasItem) list.Add(s);
//         return list;
//     }
// }

using System.Collections.Generic;
using UnityEngine;

/// 세로 칸 묶음
[DisallowMultipleComponent]
public class ShelfGroup : MonoBehaviour
{
    [SerializeField] private List<ShelfSlot> slots = new();
    public bool IsReserved { get; private set; }

    private void Awake()
    {
        if (slots.Count == 0)
            GetComponentsInChildren(slots);

        foreach (var s in slots)
            s.ParentGroup = this;
    }

    public bool TryReserve()
    {
        if (IsReserved) return false;
        IsReserved = true;
        return true;
    }

    public void Release() => IsReserved = false;

    public ShelfSlot GetFreeSlotWithItem()
    {
        if (IsReserved) return null;
        foreach (var s in slots)
            if (s.HasItem) return s;
        return null;
    }

    public List<ShelfSlot> GetItemSlots()
    {
        var list = new List<ShelfSlot>();
        foreach (var s in slots)
            if (s.HasItem) list.Add(s);
        return list;
    }
}