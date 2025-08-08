// using System.Collections.Generic;
// using UnityEngine;

// /// 선반 세로칸(ShelfGroup) 전체를 관리하는 중앙 매니저.
// /// ─ 빈 세로칸과 슬롯을 찾아 NPC에게 넘겨 주고
// /// ─ 재고가 하나라도 있는지 빠르게 알려 준다.
// public class ShelfManager : MonoBehaviour
// {
//     public static ShelfManager Instance { get; private set; }

//     [SerializeField] private List<ShelfGroup> groups = new(); // 씬 안의 모든 세로 칸

//     private void Awake()
//     {
//         // 싱글턴
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//         Instance = this;

//         // 그룹 목록 자동 수집 (인스펙터에 수동으로 넣어도 OK)
//         groups.AddRange(
//             Object.FindObjectsByType<ShelfGroup>(
//                 FindObjectsInactive.Include,
//                 FindObjectsSortMode.None));
//     }

//     /// 아직 예약되지 않았고, 상품이 있는 슬롯 하나를 찾아서 예약까지 처리.
//     /// 성공하면 true와 함께 슬롯 반환, 실패하면 false.
//     public bool TryGetAvailableSlot(out ShelfSlot slot)
//     {
//         /* 1) 예약 안 된 그룹 + 아이템 있는 그룹만 모아 둔다 */
//         List<ShelfGroup> candidates = new ();
//         foreach (var g in groups)
//             if (!g.IsReserved && g.GetFreeSlotWithItem() != null)
//                 candidates.Add(g);

//         /* 2) 후보가 없으면 실패 */
//         if (candidates.Count == 0)
//         {
//             slot = null;
//             return false;
//         }

//         /* 3) 무작위 그룹을 뽑아 TryReserve */
//         while (candidates.Count > 0)
//         {
//             int idx = Random.Range(0, candidates.Count);
//             var g   = candidates[idx];
//             candidates.RemoveAt(idx);          // 중복 방지

//             if (!g.TryReserve()) continue;     // 경합으로 실패 시 다른 후보

//             /* 4) 그룹 안에서 ‘아이템 있는 슬롯’ 목록 중 랜덤 */
//             var itemSlots = g.GetItemSlots();
//             slot = itemSlots[Random.Range(0, itemSlots.Count)];
//             return true;
//         }

//         slot = null;
//         return false;
//     }

//     /// “지금 선반에 팔 물건이 하나라도 달려 있나?”
//     public bool HasAnyDisplayItem()
//     {
//         foreach (var g in groups)
//             if (!g.IsReserved && g.GetFreeSlotWithItem() != null)
//                 return true;
//         return false;
//     }
// }

using System.Collections.Generic;
using UnityEngine;

/// 선반 전체 매니저
public class ShelfManager : MonoBehaviour
{
    public static ShelfManager Instance { get; private set; }

    [SerializeField] private List<ShelfGroup> groups = new();

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (groups.Count == 0)
        {
            groups.AddRange(
                Object.FindObjectsByType<ShelfGroup>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None));
        }
    }

    /// 예약 안 됐고 아이템 있는 슬롯 하나 반환(+그룹 예약)
    public bool TryGetAvailableSlot(out ShelfSlot slot)
    {
        var candidates = new List<ShelfGroup>();
        foreach (var g in groups)
            if (!g.IsReserved && g.GetFreeSlotWithItem() != null)
                candidates.Add(g);

        if (candidates.Count == 0)
        {
            slot = null;
            return false;
        }

        while (candidates.Count > 0)
        {
            int i = Random.Range(0, candidates.Count);
            var g = candidates[i];
            candidates.RemoveAt(i);

            if (!g.TryReserve()) continue;

            var itemSlots = g.GetItemSlots();
            slot = itemSlots[Random.Range(0, itemSlots.Count)];
            return true;
        }

        slot = null;
        return false;
    }

    public bool HasAnyDisplayItem()
    {
        foreach (var g in groups)
            if (!g.IsReserved && g.GetFreeSlotWithItem() != null)
                return true;
        return false;
    }
}