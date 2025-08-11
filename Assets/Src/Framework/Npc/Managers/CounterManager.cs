using System.Collections.Generic;
using UnityEngine;

public class CounterManager : MonoBehaviour
{
    public static CounterManager Instance { get; private set; }

    [Header("카운터 슬롯들 (Inspector 순서대로 사용)")]
    [SerializeField] Transform[] counterSlots;
    readonly Queue<Transform> pool = new();

    [Header("결제용 프리팹")]
    [SerializeField] GameObject cashPrefab;
    [SerializeField] GameObject cardPrefab;

    [Header("스캐너, 봉투 위치, 스캔 효과음")]
    [SerializeField] Transform scannerPoint;    // ★바코드 위치
    [SerializeField] Transform bagPoint;        // ★봉투 위치
    [SerializeField] AudioClip beepClip;       // ★삑 효과음

    readonly Dictionary<NpcController, Transform> npcToSlot = new();
    readonly Dictionary<NpcController, GameObject> npcToPay = new();
    readonly Dictionary<NpcController, int> npcBaggedCount = new();
    private readonly HashSet<NpcController> readyToPay = new();

    public bool IsReadyToPay(NpcController npc) => readyToPay.Contains(npc);
    public void MarkReadyToPay(NpcController npc) => readyToPay.Add(npc);
    public void ClearReadyToPay(NpcController npc) => readyToPay.Remove(npc);

    

    void Awake()
    {
         // 싱글턴 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (Transform counterSlot in counterSlots) pool.Enqueue(counterSlot);
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

        item.gameObject.GetComponent<CheckoutItemBehaviour>().Init(this, npc, scannerPoint, bagPoint, beepClip);

        return slot;
    }

    /* ─ 상품 한 개가 봉투에 완전히 들어갔을 때 호출 ─ */
    public void OnItemBagged(NpcController npc)                         // ★추가
    {
        if (!npcBaggedCount.ContainsKey(npc))
            npcBaggedCount[npc] = 0;
        npcBaggedCount[npc]++;

        readyToPay.Add(npc);
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

    public void ShowPaymentObject(NpcController npc, Transform handSocket, PaymentType method)
    {
        if (npcToPay.ContainsKey(npc)) return;  // 중복 생성 방지

        GameObject prefab = (method == PaymentType.Cash) ? cashPrefab : cardPrefab;

        GameObject payObj = Instantiate(prefab, handSocket.position, handSocket.rotation, handSocket);
        payObj.transform.localPosition = Vector3.zero;
        payObj.transform.localRotation = Quaternion.identity;

        npcToPay[npc] = payObj;
    }



    //추가 === 장지원
    #region 계산 결제 로직
    private NpcController currentNpcForPayment;
    private float npcPaymentAmount; // 손님 결제 금액


    //계산 시작
    public void StartCalculatorPayment(NpcController npc)
    {
        currentNpcForPayment = npc; //계산을 처리할 npc

        SubscribeCalculatorEvents(); //계산 이벤트 시작
    }

    
    //success 이벤트가 진행할 메서드
    private void HandlePaymentSuccess() //계산 성공
    {
        if (currentNpcForPayment != null)
        {
            //결제 완료 처리
            CompletePayment(currentNpcForPayment);
            GameState.Instance.AddMoney(npcPaymentAmount); //손님이 결제한 금액 매출액에 추가
        }

        //이벤트 구독 해제
        UnsubscribeCalculatorEvents();
    }
    private void HandlePaymentFailure()//계산 실패
    {
        //계산 실패시 UI는 CalculatorErrorUI에 구현완료
    }


    private void SubscribeCalculatorEvents()
    {
        CalculatorOk.SuccessCompare += HandlePaymentSuccess;
        CalculatorOk.FailedCompare += HandlePaymentFailure;
    }

    private void UnsubscribeCalculatorEvents()
    {
        CalculatorOk.SuccessCompare -= HandlePaymentSuccess;
        CalculatorOk.FailedCompare -= HandlePaymentFailure;
    }
    #endregion
}