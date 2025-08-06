using System.Collections.Generic;
using UnityEngine;

/// 한 칸 슬롯 – 최대 2개의 상품을 안쪽→바깥쪽 순으로 보관
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class ShelfSlot : MonoBehaviour
{
    public ShelfGroup ParentGroup { get; internal set; }

    /* ---------- 설정 ---------- */
    public const int Capacity = 2;

    [Header("진열 포인트 (0 = 안쪽, 1 = 바깥쪽)")]
    [SerializeField] Transform[] points = new Transform[Capacity];

    /* ---------- 내부 ---------- */
    readonly List<GameObject> items = new();

    /* ---------- 프로퍼티 ---------- */
    public int  ItemCount => items.Count;      // ← SlotFillBehaviour가 사용
    public bool IsFull    => items.Count >= Capacity;
    public bool HasItem   => items.Count > 0;

    /* ---------- 외부에서 위치 얻기 ---------- */
    public Transform GetSnapPoint(int idx) => points[idx];

    /* ---------- 외부에서 아이템 추가 ---------- */
    public void RegisterNewItem(GameObject go) => items.Add(go);

    /* ---------- NPC 가 꺼낼 때 ---------- */
    public GameObject PopItem()
    {
        if (!HasItem) return null;
        int last = items.Count - 1;          // 바깥쪽
        GameObject go = items[last];
        items.RemoveAt(last);
        go.transform.SetParent(null);
        return go;
    }

    /* ---------- (기존) 예약 해제용 ---------- */
    public void Release() => ParentGroup?.Release();

    void Awake()
    {
        if (ParentGroup == null)
            ParentGroup = GetComponentInParent<ShelfGroup>(true);

        if (points.Length != Capacity)
            Debug.LogWarning($"{name} : Points 배열에 2개(안쪽·바깥쪽) 넣어 주세요");
    }
}