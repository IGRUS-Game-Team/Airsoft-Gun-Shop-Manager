using System.Collections.Generic;
using UnityEngine;

public class CounterManager : MonoBehaviour
{
    public static CounterManager Instance { get; private set; }

    [Header("카운터 슬롯들 (Inspector 순서대로 사용)")]
    [SerializeField] Transform[] counterSlots;
    readonly List<Transform> freeSlots = new();

    [Header("결제용 프리팹")]
    [SerializeField] GameObject cashPrefab;
    [SerializeField] GameObject cardPrefab;

    [Header("스캐너, 봉투 위치, 스캔 효과음")]
    [SerializeField] Transform scannerPoint;    // 바코드 위치
    [SerializeField] Transform bagPoint;        // 봉투 위치
    [SerializeField] AudioClip beepClip;       // 삑 효과음

    readonly Dictionary<NpcController, Transform> npcToSlot = new();
    readonly Dictionary<NpcController, GameObject> npcToPay = new();
    readonly Dictionary<NpcController, int> npcScannedCount = new();
    readonly Dictionary<NpcController, int> npcBaggedCount = new();
    private readonly HashSet<NpcController> readyToPay = new();
    readonly Dictionary<NpcController, int> npcCheckoutTargetCount = new(); // ★추가
    private CountorMonitorController countorMonitorController;


    public bool IsReadyToPay(NpcController npc) => readyToPay.Contains(npc);
    public void MarkReadyToPay(NpcController npc) => readyToPay.Add(npc);
    public void ClearReadyToPay(NpcController npc) => readyToPay.Remove(npc);
    public int FreeSlotCount => freeSlots.Count;



    void Awake()
    {
        // 싱글턴 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        freeSlots.AddRange(counterSlots);
        countorMonitorController = FindFirstObjectByType<CountorMonitorController>();
    }

    /* ─ NPC가 들고 온 상품 내려놓기 + 슬롯 배정 ─ */
    public Transform PlaceItem(NpcController npc, GameObject item, Vector3 worldScale)
    {
        var rnd = TakeRandomFreeSlot();

        Transform slot = rnd;
        item.transform.SetParent(slot, false);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.SetActive(true);

        Vector3 p = slot.lossyScale;
        item.transform.localScale = new Vector3(
            p.x != 0 ? worldScale.x / p.x : 1f,
            p.y != 0 ? worldScale.y / p.y : 1f,
            p.z != 0 ? worldScale.z / p.z : 1f
        );

        var beh = item.GetComponent<CheckoutItemBehaviour>();
        beh.Init(this, npc, scannerPoint, bagPoint, beepClip);

        return slot;
    }

    // ★추가: 이번 손님 총 개수 세팅 (계산대에 다 올리기 “직전”에 호출)
    public void BeginCheckout(NpcController npc, int totalItems)
    {
        npcCheckoutTargetCount[npc] = Mathf.Max(0, totalItems);
        npcBaggedCount[npc] = 0;
        npcScannedCount[npc] = 0;           // ★추가: 스캔 카운트 초기화
        readyToPay.Remove(npc);
    }

    /* ─ 상품 한 개가 봉투에 완전히 들어갔을 때 호출 ─ */
    public void OnItemBagged(NpcController npc)
    {
        if (!npcCheckoutTargetCount.TryGetValue(npc, out var target))
        {
            return; // ★ 조기 리턴 (ready 계산 안 함)
        }

        int cur = (npcBaggedCount.TryGetValue(npc, out var c) ? c : 0) + 1;
        npcBaggedCount[npc] = cur;
    }

    public void OnItemScanned(NpcController npc)
    {
        if (!npcCheckoutTargetCount.TryGetValue(npc, out var target)) return;
        if (!npcScannedCount.ContainsKey(npc)) npcScannedCount[npc] = 0;

        npcScannedCount[npc]++;

        bool ready = npcScannedCount[npc] >= target && target > 0;
        if (ready)
        {
            readyToPay.Add(npc);
        }

    }


    /* ─ 슬롯 반납 전용 ─ */
    public void ReturnSlot(Transform slot)
    {
        if (slot == null) return;
        if (!freeSlots.Contains(slot)) freeSlots.Add(slot);
    }

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

    public bool HasCheckoutStarted(NpcController npc)
    {
        return npcCheckoutTargetCount.ContainsKey(npc);
    }

    Transform TakeRandomFreeSlot()
    {
        if (freeSlots.Count == 0) return null;
        int idx = Random.Range(0, freeSlots.Count);
        Transform t = freeSlots[idx];
        freeSlots.RemoveAt(idx);
        return t;
    }

#if UNITY_EDITOR
    [SerializeField] bool debugDrawSlots = true;

    void OnDrawGizmos()
    {
        if (!debugDrawSlots || counterSlots == null) return;

        var free = new HashSet<Transform>(freeSlots);
        foreach (var s in counterSlots)
        {
            if (s == null) continue;
            Gizmos.color = free.Contains(s) ? new Color(0, 1, 0, 0.35f) : new Color(1, 0, 0, 0.35f); // free=초록, 점유=빨강
            Gizmos.DrawSphere(s.position + Vector3.up * 0.05f, 0.05f);
        }
    }
#endif


    //추가 === 장지원
    #region 계산 결제 로직
    [Header("Cash UI")]
    [SerializeField] private CashRegisterUI cashUI;
    [SerializeField] private CashRegisterEnterHandler cashRegisterEnterHandler;
    private NpcController currentNpcForPayment;
    private float npcPaymentAmount; // 손님 결제 금액
    private float npcSendMe { get; set; }

    // 내부 상태
    private bool cashSessionActive = false;


    //계산 시작
    public void StartCalculatorPayment(NpcController npc)
    {
        currentNpcForPayment = npc; //계산을 처리할 npc

        // ✅ 중복 구독 방지
        UnsubscribeCalculatorEvents();  // 수정 (준서)
        SubscribeCalculatorEvents(); //계산 이벤트 시작
    }


    //success 이벤트가 진행할 메서드
    private void HandleCardPaymentSuccess() //계산 성공
    {
        Debug.Log(currentNpcForPayment);

        if (currentNpcForPayment != null)
        {
            //결제 완료 처리
            CompletePayment(currentNpcForPayment);
            Debug.Log(npcPaymentAmount);
            GameState.Instance.AddMoney(npcPaymentAmount); //손님이 결제한 금액 매출액에 추가
            countorMonitorController.Clear();
        }

        //이벤트 구독 해제
        UnsubscribeCalculatorEvents();
    }
    private void HandleCardPaymentFailure()//계산 실패
    {
        //계산 실패시 UI는 CalculatorErrorUI에 구현완료
    }


    private void SubscribeCalculatorEvents()
    {
        CalculatorOk.SuccessCompare += HandleCardPaymentSuccess;
        CalculatorOk.FailedCompare += HandleCardPaymentFailure;
    }

    private void UnsubscribeCalculatorEvents()
    {
        CalculatorOk.SuccessCompare -= HandleCardPaymentSuccess;
        CalculatorOk.FailedCompare -= HandleCardPaymentFailure;
    }






    // 기존 카드 로직은 그대로 냅두고 새롭게 현금 로직만 여기 추가하면 됨.
    public void StartCashPayment(NpcController npc)
    {
        Debug.Log("현금계산 시작");
        currentNpcForPayment = npc;

        // 1) 이번 손님 총 결제금액 계산(모니터에서 가져오거나, 보유한 API 사용)
        //    상황에 맞게 치환하세요.
        npcPaymentAmount = (countorMonitorController != null)
            ? countorMonitorController.GetCurrentTotalAmount()   
            : 0f;
        
        float value = npcPaymentAmount + npcPaymentAmount / Random.Range(10, 15);
        value = Mathf.Round(value * 100f) / 100f; // 숫자 자체 반올림
        npcSendMe = value;
        Debug.Log( countorMonitorController.GetCurrentTotalAmount());
        // 2) 캐시 UI 초기화(손님이 낸 돈은 일단 0, 총액은 npcPaymentAmount)
        if (cashUI != null)
        {
            cashUI.SetValues(received: npcSendMe, total: npcPaymentAmount);
        }
        Debug.Log(countorMonitorController.GetCurrentTotalAmount());
        // 3) 이벤트 구독 + 세션 on
        UnsubscribeCashRegisterEvents(); // 혹시 남아있으면 정리
        SubscribeCashRegisterEvents();
        cashSessionActive = true;

        // (선택) 서랍 열기/프리팹 노출 등은 여기서 처리
        cashRegisterEnterHandler.OpenBasket();
        // registerMoneyRoot.SetActive(true);
    }

    private void SubscribeCashRegisterEvents()
    {
        CashRegisterUI.SuccessCompare += HandleCashPaymentSuccess;
        CashRegisterUI.FailedCompare += HandleCashPaymentFailure;
    }

    private void UnsubscribeCashRegisterEvents()
    {
        CashRegisterUI.SuccessCompare -= HandleCashPaymentSuccess;
        CashRegisterUI.FailedCompare  -= HandleCashPaymentFailure;
    }

    private void HandleCashPaymentSuccess() //계산 성공
    {
        // 매출 반영
        GameState.Instance.AddMoney(npcSendMe - cashUI.GetCurrentGiven());
        Debug.Log(npcSendMe + " "+ cashUI.GetCurrentGiven());
        // 결제 완료 처리
        if (currentNpcForPayment != null)
            CompletePayment(currentNpcForPayment);

        // UI 정리
        countorMonitorController.Clear();
        cashUI?.Clear();

        // 상태/이벤트 정리
        //cashRegisterEnterHandler.CloseBasket();
        npcSendMe = 0;
        cashSessionActive = false;
        UnsubscribeCashRegisterEvents();
    }

    private void HandleCashPaymentFailure()//계산 실패(= 아직 모자람)
    {
        // 굳이 세션 종료하지 않고 계속 입력 받도록 둔다.
        // 필요하면 여기서 "금액이 부족합니다" 같은 피드백 UI 호출
        // cashUI.ShowNotEnoughMessage();
    }
    #endregion
}