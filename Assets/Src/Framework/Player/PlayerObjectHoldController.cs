using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using System;
/// <summary>
/// PlayerObejctHoldController.cs 박정민
/// input맵에 있는 Hold 키를 누르면 blockOutLiner가 켜지고 selected = true인 오브젝트를 
/// Player /.../holdPoint 계층 구조 아래로 이동시키고 rigidbody를 isKinetic 켜서 고정시키는 역할을 하는 클래스
/// </summary>

public class PlayerObjectHoldController : MonoBehaviour
{
    public static PlayerObjectHoldController Instance { get; private set; }

    public BlockIsHolding heldObject;
    [SerializeField] Transform holdPoint;

    private void Awake()
    {
        Instance = this;
    }

    public void SetHeldObject(BlockIsHolding target)
    {
        if (heldObject != null) return;

        heldObject = target;
        heldObject.isHeld = true;
        heldObject.originalParent = target.transform.parent;

        var rb = heldObject.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.detectCollisions = false;

        heldObject.GetComponent<Collider>().enabled = false;

        heldObject.transform.SetParent(holdPoint);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;
    }

    public void ReleaseHeldObject()
    {
        if (heldObject == null) return;

        heldObject.transform.SetParent(heldObject.originalParent);
        var rb = heldObject.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.detectCollisions = true;

        heldObject.GetComponent<Collider>().enabled = true;
        heldObject.isHeld = false;
        heldObject = null;
    }
}

// public class PlayerObjectHoldController : MonoBehaviour
// {
//     [SerializeField] float placeRange = 3f;
//     [SerializeField] Transform holdPoint; // 플레이어 카메라 아래있는 holdPoint
//     [SerializeField] float pickupRange;
//     [SerializeField] LayerMask pickupLayer;
//     [SerializeField] Camera PlayerCam;

//     SelectionManager selectionManager;
//     PlayerObjectDropController playerObjectDropController;
//     PlayerObjectSetController playerObjectSetController;


//     public BlockIsHolding heldObject;
//     private bool isHolding => heldObject != null;
//     void Awake()
//     {
//         selectionManager = GetComponent<SelectionManager>();
//         playerObjectDropController = GetComponent<PlayerObjectDropController>();
//         playerObjectSetController = GetComponent<PlayerObjectSetController>();
//         pickupRange = selectionManager.pickupRange;
//         pickupLayer = selectionManager.pickupLayer; //selectionManager의 pickupRange와 pickupLayer를 가져옴
//     }
//     void Start()
//     {
//         InterActionController.Instance.OnClick += TryPickup; //이벤트 구독자 추가.
//     }

//     private void TryPickup()
//     {
//         if (heldObject != null) return;

//         Ray ray = PlayerCam.ScreenPointToRay(Mouse.current.position.ReadValue());
//         if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupLayer))
//         {
//             BlockIsHolding target = hit.collider.GetComponentInParent<BlockIsHolding>();
//             if (target == null || target.isHeld) return;
//             //테스트코드
//             string tag = hit.collider.transform.root.tag; //맞은 물체의 태그를 가져옴
//             if (tag != "Box") return;

//             SetHeldObject(target);
//             heldObject.originalParent = target.transform.parent; // 원래 부모 기억

//             var rb = heldObject.GetComponent<Rigidbody>();
//             TurnOffPhysics(rb);

//             heldObject.GetComponent<Collider>().enabled = false;

//             GameObject rootObj = heldObject.transform.gameObject;
//             rootObj.transform.SetParent(holdPoint); //오브젝트 계층 전체를 holdPoint 아래로 넣기
//             rootObj.transform.localPosition = Vector3.zero;
//             rootObj.transform.localRotation = Quaternion.identity;
//             // 하위 계층에 있는 모델을 옮기는 구조가 아니라 전체 계층을 전부 이동시키기 위해 로직 변경
//         }
//     }

//     private static void TurnOffPhysics(Rigidbody rb)
//     {
//         rb.isKinematic = true;
//         rb.detectCollisions = false;
//         rb.useGravity = false;
//     }
//     public void ReleaseHeldObject()
//     {
//         if (heldObject != null)
//         {
//             heldObject.isHeld = false;
//             heldObject = null;
//         }
//     }
//     public void SetHeldObject(BlockIsHolding target)
//     {
//         heldObject = target;
//         heldObject.isHeld = true;
//         playerObjectDropController.heldObject = target;
//         playerObjectSetController.heldObject = target;

//     }
// }
