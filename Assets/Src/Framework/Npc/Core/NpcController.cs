using UnityEngine;
using UnityEngine.AI;

// NPC의 이동, 애니메이션, 상태 전환을 관리하는 클래스
public class NpcController : MonoBehaviour
{
    /* --- 컴포넌트 캐싱 --- */
    public NavMeshAgent Agent { get; private set; }   // 경로 탐색·이동을 담당
    public Animator Animator { get; private set; }    // 애니메이션 제어를 담당

    /* --- 상태 머신 --- */
    public StateMachine stateMachine { get; private set; }  // 상태 전환 로직

    /* --- 외부 설정 값 --- */
    public Transform targetShelfSlot { get; set; }   // 접근할 선반 슬롯
    public ShelfGroup targetShelfGroup { get; private set; } // 접근할 세로 칸
    public Transform exitPoint { get; set; }   // 퇴장 지점
    public DoorTrigger door          { get; private set; }   // 입구 트리거
    public Transform QueueTarget     { get; private set; }   // 줄서기 목표 노드
    public Transform LookTarget      { get; private set; }   // 바라볼 대상
    public bool DoorProcessed { get; set; } = false;

    /* --- 내부 플래그 --- */
    public GameObject  heldItem      { get; set; } = null;
    public bool hasItemInHand { get; set; } = false;  // 손에 물건 소지 여부
    public bool inStore { get; set; } = false;          // 매장 내부 여부
    public bool isLeaving     { get; set; } = false;          // 퇴장 중 여부

    /* ---------- 결제 완료 플래그 ---------- */
    public bool PaymentDone { get; private set; }

    /* --- 인스펙터 주입 --- */
    [SerializeField] private GameObject cashPrefab;    // 현금 프리팹
    [SerializeField] private GameObject cardPrefab;    // 카드 프리팹
    [SerializeField] private Transform handSocket;     // 물건 생성 위치(손 소켓)

    /* --- 내부 관리용 --- */
    private GameObject spawnedObject;   // 손에 쥔 오브젝트 레퍼런스
    private Transform  doorPoint;       // 이동 목적지(출입문)
    
    public GameObject CashPrefab    => cashPrefab;    // 현금 프리팹
    public GameObject CardPrefab    => cardPrefab;    // 카드 프리팹
    public Transform HandTransform  => handSocket;    // 손 소켓 Transform
    public void OnPaymentCompleted() => PaymentDone = true;

    // Awake: 컴포넌트와 상태 머신 초기화
    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponentInChildren<Animator>();
        stateMachine = new StateMachine();
    }

    // Start: 최초 상태로 출입문 이동 상태 설정
    private void Start()
    {
        stateMachine.SetState(
            new NpcState_ToDoor(this, doorPoint));
    }

    // Update: 매 프레임 상태의 Tick 실행
    private void Update()
    {
        stateMachine.Tick();
    }

    // 입구 트리거를 저장
    public void SetDoor(DoorTrigger door)
    {
        this.door = door;
    }

    // 출입문 위치를 저장
    public void SetDoorPoint(Transform doorPoint)
    {
        this.doorPoint = doorPoint;
    }

    // 줄서기 목표 노드를 설정하고 이동 재개
    public void SetQueueTarget(Transform node)
    {
        QueueTarget         = node;
        Agent.isStopped     = false;
        Agent.SetDestination(node.position);
    }

    // 바라볼 대상을 설정
    public void SetLookTarget(Transform target)
    {
        LookTarget = target;
    }

    // 매장 입장이 허가되면 호출
    public void AllowEntry(Transform shelfSlot, Transform exitPoint)
    {
        // ① 선반 슬롯 Transform 저장
        targetShelfSlot = shelfSlot;

        // ② 세로 칸(ShelfGroup) 캐싱 – 뒤에서 Release() 하려면 필요
        targetShelfGroup = shelfSlot.GetComponent<ShelfSlot>()?.ParentGroup;

        // ③ 퇴장 지점 저장 후 상태 전환
        this.exitPoint = exitPoint;
        stateMachine.SetState(new NpcState_ToShelf(this));
    }

    // 퇴장 시작을 외부에서 호출
    public void StartLeaving(Transform exitPoint, string reason = "")
    {
        DoorProcessed = true;

        this.exitPoint = exitPoint;
        isLeaving      = true;

        // 세로 칸 예약 해제
        targetShelfGroup?.Release();
        targetShelfGroup = null;
        stateMachine.SetState(new NpcState_Leave(this));
    }

    // 손에 현금 또는 카드를 생성하고 소지 상태로 전환
    public void ShowMoneyOrCard()
    {
        // 손에 잡은 상품이 아직 비활성 상태로 있으면 완전히 제거
        if (heldItem != null)
        {
            Destroy(heldItem);
            heldItem = null;
        }

        if (spawnedObject != null)
        {
            return;  // 이미 소지 중이면 중복 생성 방지
        }

        // 랜덤 선택: 현금 또는 카드
        GameObject prefab = Random.value < 0.5f ? cashPrefab : cardPrefab;

        // 손 소켓에 인스턴스화
        spawnedObject = Instantiate(prefab, handSocket);
        spawnedObject.transform.localPosition = Vector3.zero;
        
        hasItemInHand = true;
    }

    // 손에 든 오브젝트 제거 및 소지 상태 해제
    public void HideMoneyOrCard()
    {
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
            spawnedObject   = null;
            hasItemInHand   = false;
        }
    }

    // LookTarget을 향해 부드럽게 회전
    public void FaceLookTarget(float speedDegPerSec)
    {
        if (LookTarget == null)
        {
            return;  // 대상이 없으면 무시
        }

        // 수평 방향 벡터 계산
        Vector3 direction = LookTarget.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;  // 유효하지 않은 벡터면 무시
        }

        // 현재 회전에서 목표 회전으로 보간
        Quaternion target = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            target,
            speedDegPerSec * Time.deltaTime);
    }
}