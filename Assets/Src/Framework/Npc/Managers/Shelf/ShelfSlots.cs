using System;
using System.Collections.Generic;
using UnityEngine;

/// 한 칸 슬롯 – 최대 2개의 상품을 안쪽→바깥쪽 순으로 보관
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class ShelfSlot : MonoBehaviour
{
    public ShelfGroup ParentGroup { get; internal set; }
    public static event Action<ItemData, Vector3, Transform> OnProductPlacedToFactory; //so 가격표를 전달
    //Transform은 부모가 된 카드슬롯의 위치
    [Header("shelf slot 오브젝트")]
    [SerializeField] private Transform priceCardParent; 
    
    /* ---------- 설정 ---------- */
    public const int Capacity = 2;

    [Header("진열 포인트 (0 = 안쪽, 1 = 바깥쪽)")]
    [SerializeField] Transform[] points = new Transform[Capacity];

    [Header("NPC 서기 포인트 (선택)")]
    [Tooltip("NPC가 정확히 설 바닥 점. 없으면 슬롯 피벗/보정점 사용")]
    [SerializeField] private Transform standPoint;
    public Transform StandPoint => standPoint;

    /* ---------- 내부 ---------- */
    readonly List<GameObject> items = new();

    /* ---------- 프로퍼티 ---------- */
    public int  ItemCount => items.Count;      // ← SlotFillBehaviour가 사용
    public bool IsFull    => items.Count >= Capacity;
    public bool HasItem   => items.Count > 0;

    /* ---------- 외부에서 위치 얻기 ---------- */
    public Transform GetSnapPoint(int idx) => points[idx];

    /* ---------- 외부에서 아이템 추가 ---------- */ //수정 so값 넘기기
    public void RegisterNewItem(GameObject go)
    {
         items.Add(go);
        var itemDataManager = go.GetComponent<ItemDataManager>();
        ItemData itemDatas = itemDataManager.GetItemData();

        Vector3 priceCardPosition = transform.position + new Vector3(-0.14f,0.3f,-0.14f);

        // 3. 이벤트 호출 수정: 부모 Transform(priceCardParent)을 함께 전달
        if (priceCardParent != null)
        {
            OnProductPlacedToFactory?.Invoke(itemDatas, priceCardPosition, priceCardParent);
        }
        
    } 

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

#if UNITY_EDITOR
    [ContextMenu("Create StandPoint child")]
    private void CreateStandPoint()
    {
        if (standPoint != null) return;
        var go = new GameObject("StandPoint");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;        // 일단 슬롯 피벗에 배치
        go.transform.localRotation = Quaternion.identity; // forward는 나중에 선반을 향하도록 돌려줘
        standPoint = go.transform;
        UnityEditor.Selection.activeTransform = standPoint;
    }
#endif

    void OnDrawGizmos()
    {
        if (standPoint == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(standPoint.position, 0.08f);
        // forward 확인(선반을 바라보게 맞춰 두는 걸 권장)
        Gizmos.DrawRay(standPoint.position, standPoint.forward * 0.35f);
    }
}