using Cinemachine;
using UnityEngine;

public class Raycast : MonoBehaviour
{
    public static Raycast Instance { get; private set; } //
    public GameObject HitObject { get; private set; } //감지된 오브젝트
    public static bool IsOverlayUIMode { get; set; } = false; //오버레이 ui 모드

    [Header("Raycast 설정")]
    [SerializeField] private float range = 5f; //Ray 사거라
    [SerializeField] private LayerMask layer;// 감지할 레이어

    [Header("카메라 참조")]    
    [SerializeField] private CinemachineVirtualCamera playerCamera;

    private void Awake() => Instance = this;

    void Update()
    {
        //마우스 클릭
        if (Input.GetMouseButtonDown(0))
        {
            CheckRaycastHit();
        }
    }

    private void CheckRaycastHit()
    {
        Ray ray = GetRayByMode();
        
        if (Physics.Raycast(ray, out RaycastHit hit, range, layer))
        {
            HitObject = hit.collider.gameObject;
        }
        else
        {
            HitObject = null;
        }
    }

//UI 창 상태에 따른 Ray(커서, 정중앙)
    private Ray GetRayByMode()
    {
        Vector3 screenPoint = IsOverlayUIMode
        //변수 = 조건식
            ? Input.mousePosition // 참일 때 값
            : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0); //거짓일때 값

        return Camera.main.ScreenPointToRay(screenPoint);
    }

//UI 창 상태일 때 커서 및 화면 설정
    public void SetOverlayUIMode(bool isUIMode)
    {
        ;
        IsOverlayUIMode = isUIMode;

        Cursor.lockState = isUIMode ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isUIMode;
        playerCamera.enabled = !isUIMode;

        Debug.Log($"UI 모드: {(isUIMode ? "활성화" : "비활성화")}");
    }
}