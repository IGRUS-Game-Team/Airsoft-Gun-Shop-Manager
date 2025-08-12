using UnityEngine;
using UnityEngine.AI;

// NPC의 이동, 애니메이션, 상태 전환을 관리하는 클래스
public class NpcController : MonoBehaviour
{
public NavMeshAgent Agent { get; private set; }
public Animator Animator { get; private set; }
public StateMachine stateMachine { get; private set; }

public Transform targetShelfSlot { get; set; }
public ShelfGroup targetShelfGroup { get; set; }
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

private Transform doorPoint;
public Transform HandTransform => handSocket;

private void Awake()
{
    Agent = GetComponent<NavMeshAgent>();
    Animator = GetComponentInChildren<Animator>();
    stateMachine = new StateMachine();
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

public void AllowEntry(Transform shelfSlot, Transform exitPoint)
{
    targetShelfSlot  = shelfSlot;
    targetShelfGroup = shelfSlot.GetComponent<ShelfSlot>()?.ParentGroup;
    this.exitPoint   = exitPoint;
    stateMachine.SetState(new NpcState_ToShelf(this));
}

public void StartLeaving(Transform exitPoint)
{
    DoorProcessed = true;
    this.exitPoint = exitPoint;
    isLeaving = true;

    targetShelfGroup?.Release();
    targetShelfGroup = null;
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
    stateMachine.SetState(new NpcState_Leave(this));
}
}