using UnityEngine;

/// <summary>
/// RaycastDetector.cs 박정민
/// Player에 붙는 스크립트입니다.
/// Player가 바라보는 방향으로 ray를 쏴서 맞는 오브젝트를 HitObject 변수에 저장하는 역할을 합니다.
/// </summary>

public class RaycastDetector : MonoBehaviour
{
    public static RaycastDetector Instance { get; private set; }
    // 다른클래스에서 바로 상태를 알 수 있게 static으로 지정하였습니다. 추후 더 나은 방법이 있나 고민
    public GameObject HitObject { get; private set; }

    [SerializeField] float range = 5f; //ray의 길이를 정할 수 있음
    [SerializeField] LayerMask layer; // 지정한 레이어만 hit 됨. ex) box 레이어 <- 작명 바꿔야할듯
    private void Awake() => Instance = this;

    void Update()
    {
        HitObject = null;

        if (Camera.main == null) return; //카메라 없으면 update 탈출하도록 탈출조건 설정했습니다.

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, range, layer))
        {
            HitObject = hit.collider.gameObject; // ray 맞은 오브젝트를 HitObject 변수에 저장합니다.
            // 저장된 오브젝트는 RaycastDetector Instance.HitObject로 전 클래스에서 확인 가능합니다.
            Debug.DrawRay(ray.origin, ray.direction * range, Color.blue); //광선이 어떻게 나가는지 보여주는 디버그코드
        }
    }
}

