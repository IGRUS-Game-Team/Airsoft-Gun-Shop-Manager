using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NPC의 이동, 애니메이션, 상태 전환을 관리하는 메인 컨트롤러.
///
/// [상태머신 패턴]
/// StateMachine이 현재 상태의 Enter/Update/Exit를 호출.
/// 각 상태는 NpcStates/ 폴더의 NpcState_*.cs 파일에 정의됨.
///
/// [NPC 생애주기]
/// NpcSpawnManager가 생성 → Start()에서 NpcState_ToDoor로 초기화
/// → DoorTrigger가 AllowEntry/AllowRange 호출 → 쇼핑 or 사격
/// → 결제 완료 또는 퇴장 시 NpcState_Leave → 디스폰
///
/// [주요 상태 전이]
/// ToDoor → (DoorTrigger) → ToShelf → PickItem → ToQueue → QueueWait → ToCounter → OfferPayment → Leave
///                        → ToRange → Shoot → Leave
///                        → Wander → Leave
///
/// [새 상태 추가]
/// 1. NpcStates/에 새 클래스 생성 (Enter/Update/Exit 구현)
/// 2. 적절한 시점에 stateMachine.SetState(new NpcState_New(this)) 호출
/// </summary>
public class NpcController : MonoBehaviour
{
    public NavMeshAgent Agent { get; private set; }
    public Animator Animator { get; private set; }
    public StateMachine stateMachine { get; private set; }

    // ─── 기존 선반 관련 ───
    public Transform targetShelfSlot { get; set; }
    public ShelfGroup targetShelfGroup { get; set; }

    // ─── 사격장 관련(추가) ───
    public ShootingLane TargetLane { get; private set; }

    public Transform exitPoint { get; set; }
    public DoorTrigger door { get; private set; }

    // 줄 자리 “배정 기록”만 보관 (이동은 상태가 함)
    public Transform QueueTarget { get; private set; }

    public bool DoorProcessed { get; set; } = false;

    public GameObject heldItem { get; set; } = null;
    public bool hasItemInHand { get; set; } = false;
    public bool inStore { get; set; } = false;
    public bool isLeaving { get; set; } = false;

    public bool PaymentDone { get; private set; }

    [SerializeField] private Transform handSocket;

    [Header("IK 시스템")]
    [SerializeField] private NpcIKSystem ikSystem;
    public NpcIKSystem IKSystem => ikSystem;

    private Transform doorPoint;
    public Transform HandTransform => handSocket;
    public System.Action<float> onScheduleNextShot;
    public void Anim_ScheduleNextShot(float delay) => onScheduleNextShot?.Invoke(delay);

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponentInChildren<Animator>();
        stateMachine = new StateMachine();
        if (ikSystem == null)
        {
            ikSystem = GetComponentInChildren<NpcIKSystem>();
        }
    }

    private void Start()
    {
        stateMachine.SetState(new NpcState_ToDoor(this, doorPoint));
    }

    private void Update()
    {
        stateMachine.Tick();
    }

    public void SetDoor(DoorTrigger door) => this.door = door;
    public void SetDoorPoint(Transform doorPoint) => this.doorPoint = doorPoint;

    public void AcceptQueueAssignment(Transform node)
    {
        QueueTarget = node;
    }

    // ─── 선반 입장 ───
    public void AllowEntry(Transform shelfSlot, Transform exitPoint)
    {
        targetShelfSlot  = shelfSlot;
        targetShelfGroup = shelfSlot.GetComponent<ShelfSlot>()?.ParentGroup;
        this.exitPoint   = exitPoint;
        stateMachine.SetState(new NpcState_ToShelf(this));
    }

    // ─── 사격장 입장(추가) ───
    public void AllowRange(ShootingLane lane, Transform exitPoint)
    {
        if (lane == null) return;
        if (!lane.TryReserve(this)) return; // 점유 실패 시 무시

        TargetLane = lane;
        this.exitPoint = exitPoint;
        stateMachine.SetState(new NpcState_ToRange(this));
    }

    public void ReleaseShootingLane()
    {
        if (TargetLane != null)
        {
            TargetLane.Release(this);
            TargetLane = null;
        }
    }

    public void StartLeaving(Transform exitPoint)
    {
        DoorProcessed = true;
        this.exitPoint = exitPoint;
        isLeaving = true;

        // 선반/사격장 점유 해제
        targetShelfGroup?.Release();
        targetShelfGroup = null;

        ReleaseShootingLane();

        stateMachine.SetState(new NpcState_Leave(this));
    }

    // TODO : 결제 시스템 스크립트로 메서드 옮기기
    public void OnPaymentCompleted()
    {
        PaymentDone = true;
        if (targetShelfGroup != null)
        {
            targetShelfGroup.Release();
            targetShelfGroup = null;
        }
        // ★ 정산 집계는 CounterManager가 직접 호출하므로 여기서는 제거 (중복 방지)

        stateMachine.SetState(new NpcState_Leave(this));
    }
}