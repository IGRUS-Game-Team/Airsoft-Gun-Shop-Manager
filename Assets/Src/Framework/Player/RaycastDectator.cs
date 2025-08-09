using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// RaycastDetector.cs 박정민
/// Player에 붙는 스크립트입니다.
/// Player가 바라보는 방향으로 ray를 쏴서 맞는 오브젝트를 HitObject 변수에 저장하는 역할을 합니다.
/// 
/// </summary>

public class RaycastDetector : MonoBehaviour
{
    public static RaycastDetector Instance { get; private set; }
    // 다른클래스에서 바로 상태를 알 수 있게 static으로 지정하였습니다. 추후 더 나은 방법이 있나 고민
    public GameObject HitObject { get; private set; }

    [SerializeField] float range = 5f; //ray의 길이를 정할 수 있음
    [SerializeField] LayerMask layer; // 지정한 레이어만 hit 됨. ex) box 레이어 <- 작명 바꿔야할듯

    private Camera mainCamera; // 카메라 


    private void Awake()
    {
        Instance = this;
        mainCamera = Camera.main; // 시작할 때 한 번만 찾아오기 : 장지원
        
    }

    void Update()
    {
        HitObject = null;

        // 카메라가 없으면 아무것도 하지 않음 (맨 위로 올려서 먼저 체크하는 것이 효율적)
        if (mainCamera == null) return;

        // UI위에 마우스가 존재한다면
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        //UI 위가 아닐 때만 3D 물리 레이캐스트를 실행합니다.
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, range, layer))
        {
            HitObject = hit.collider.gameObject;
            Debug.DrawRay(ray.origin, ray.direction * range, Color.blue);
        }
    }
}

