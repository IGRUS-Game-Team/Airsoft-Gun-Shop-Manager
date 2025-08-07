using System.Collections.Generic;
using UnityEngine;

public class CounterManager : MonoBehaviour
{
    public static CounterManager I { get; private set; }

    [Header("카운터 슬롯들 (Inspector 순서대로 사용)")]
    [SerializeField] Transform[] counterSlots;
    readonly Queue<Transform> pool = new();

    [Header("결제용 프리팹")]
    [SerializeField] GameObject cashPrefab;
    [SerializeField] GameObject cardPrefab;

    [Header("스캐너, 봉투 위치, 스캔 효과음")]
    [SerializeField] Transform scannerPoint;    // ★바코드 위치
    [SerializeField] Transform bagPoint;        // ★봉투 위치
    [SerializeField] AudioClip  beepClip;       // ★삑 효과음

    readonly Dictionary<NpcController, Transform> npcToSlot = new();
    readonly Dictionary<NpcController, GameObject> npcToPay = new();
    readonly Dictionary<NpcController, int> npcBaggedCount = new();
    readonly HashSet<NpcController> pendingPay = new();   // ★추가: 결제 대기 목록
    public static CounterManager Instance { get; private set; }

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        foreach (var s in counterSlots) pool.Enqueue(s);

        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    /* ─ NPC가 들고 온 상품 내려놓기 + 슬롯 배정 ─ */
    public Transform PlaceItem(NpcController npc, GameObject item, Vector3 worldSize)
    {
        if (npcToSlot.ContainsKey(npc) == false)
        {
            if (pool.Count == 0) { Debug.LogWarning("카운터 슬롯 부족"); return null; }
            npcToSlot[npc] = pool.Dequeue();
        }

        Transform slot = npcToSlot[npc];

        item.transform.SetParent(slot, false);
        item.transform.localPosition = Vector3.zero;
        item.SetActive(true);

        Vector3 s = slot.lossyScale;
        item.transform.localScale = new Vector3(
            worldSize.x / s.x,
            worldSize.y / s.y,
            worldSize.z / s.z
        );

        item.gameObject.AddComponent<CheckoutItemBehaviour>().Init(this, npc, scannerPoint, bagPoint, beepClip);

        return slot;
    }

    /* ─ 상품 한 개가 봉투에 완전히 들어갔을 때 호출 ─ */
    public void OnItemBagged(NpcController npc)                         // ★추가
    {
        if (!npcBaggedCount.ContainsKey(npc))
            npcBaggedCount[npc] = 0;
        npcBaggedCount[npc]++;

        // 필요하다면: 모든 상품이 담긴 후 카드/현금에 반짝 효과 주기 등
    }

    public void MarkPendingPayment(NpcController npc) => pendingPay.Add(npc);    // ★추가

    /* ─ 플레이어가 카드/현금을 클릭해 결제 대기 ─ */
    public void Pay(NpcController npc)                                  // ★추가
    {
        CompletePayment(npc);   // 슬롯 · 돈/카드 정리 및 줄 전진
    }

    /* ─ 현금/카드 생성 ─ */
    public void ShowPaymentObject(NpcController npc, Transform handSocket)
    {
        if (npcToPay.ContainsKey(npc)) return;           // 중복 방지

        GameObject prefab = Random.value < 0.5f ? cashPrefab : cardPrefab;
        GameObject payObj = Instantiate(prefab, handSocket);
        payObj.transform.localPosition = Vector3.zero;

        npcToPay[npc] = payObj;
    }

    /* ─ 슬롯 반납 전용 ─ */
    public void ReturnSlot(Transform slot) => pool.Enqueue(slot);

    /* ─ 결제 완료 처리 ─ */
    public void CompletePayment(NpcController npc)
    {
        // 1) 돈/카드 파괴
        if (npcToPay.TryGetValue(npc, out var payObj))
        {
            Destroy(payObj);
            npcToPay.Remove(npc);
        }

        // 2) 슬롯 반납
        if (npcToSlot.TryGetValue(npc, out var slot))
        {
            ReturnSlot(slot);
            npcToSlot.Remove(npc);
        }

        // 3) 줄 한 칸 전진
        QueueManager.Instance?.DequeueFront();

        // 4) NPC 내부 플래그·슬롯 정리
        npc.GetComponent<CarriedItemHandler>()?.ReleaseSlot();
        npc.OnPaymentCompleted();
    }
}