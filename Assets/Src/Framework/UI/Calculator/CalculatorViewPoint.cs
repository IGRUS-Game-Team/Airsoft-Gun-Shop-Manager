using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 장지원 8.5 계산기 화면
/// 계산기를 클릭시 카메라가 스무스하게 고정된 화면으로 이동한다.
/// 줌업 줌아웃하는 과정의 속도를 시네머신 브레인을 가져와 조정
/// 
/// 아웃라인은 이때동안 비활성화 되어있는 것은 추후 구현할 예정
/// </summary>
/// 
/// 박정민 8/10 수정
/// 기존 코드가 IInteractable 인터페이스를 상속 받지 않아 상속 받는 형태로
/// 리펙토링 하였습니다.
/// 또한 계산기 열고 닫힐 때 인게임세팅창이 계속 뜨는 문제가 발생하여 GlobalInteractionflags 스태틱 클래스를
/// 추가하여 ESCinteractionBehaviour.cs 에 추가 조건 분기를 설정해 오류를 해결했습니다.

public class CalculatorViewPoint : MonoBehaviour, IInteractable
{
    [Header("Cameras")]
    [SerializeField] CinemachineVirtualCamera calculatorCamera; 
    [SerializeField] CinemachineVirtualCamera defaultCamera;    
    [SerializeField] float blendTime = 0.5f;

    [SerializeField] GraphicRaycaster FuckingGraphicRaycaster;

    private CinemachineBrain brain;
    private bool isInCalculatorView;

    // 커서/컨트롤 상태 저장
    private bool wasCursorVisible;
    private CursorLockMode prevLockMode;

    // 플레이어 컨트롤러 참조
    private MonoBehaviour playerController;
    private CharacterController characterController;

    void Awake()
    {
        brain = Camera.main ? Camera.main.GetComponent<CinemachineBrain>() : null;
        if (brain == null)
            Debug.LogWarning("[Calculator] CinemachineBrain을 찾지 못했습니다. 메인 카메라 확인 필요.");

        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<MonoBehaviour>(); // 프로젝트의 실제 컨트롤러 타입으로 교체
            characterController = player.GetComponent<CharacterController>();
        }
    }

    void OnEnable()
    {
        if (InteractionController.Instance != null)
            InteractionController.Instance.OnExitUI += ExitInspection;
    }

    void OnDisable()
    {
        if (InteractionController.Instance != null)
            InteractionController.Instance.OnExitUI -= ExitInspection;
    }

    public void Interact()
    {
        EnterInspection();
    }

    public void EnterInspection()
    {
        if (isInCalculatorView) return;

        Debug.Log("[Calculator] UI 모드 진입");
        FuckingGraphicRaycaster.enabled = false;

        // 커서 상태 저장
        wasCursorVisible = Cursor.visible;
        prevLockMode = Cursor.lockState;

        // ESC에서 세팅창 안뜨도록 플래그++
        GlobalInteractionFlagS.ModalDepth++;

        // 카메라 전환
        if (brain) brain.m_DefaultBlend.m_Time = blendTime;
        if (defaultCamera) defaultCamera.Priority = 0;
        if (calculatorCamera) calculatorCamera.Priority = 10;

        // 플레이어 조작 비활성화
        if (playerController) playerController.enabled = false;
        if (characterController) characterController.enabled = false;

        // 커서 상태
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        isInCalculatorView = true;
    }

    public void ExitInspection()
    {
        if (!isInCalculatorView) return;

        Debug.Log("[Calculator] UI 모드 종료");
        FuckingGraphicRaycaster.enabled = true;

        // 카메라 복귀
        if (brain) brain.m_DefaultBlend.m_Time = blendTime;
        if (defaultCamera) defaultCamera.Priority = 10;
        if (calculatorCamera) calculatorCamera.Priority = 0;

        // 플레이어 조작 재활성화
        if (playerController) playerController.enabled = true;
        if (characterController) characterController.enabled = true;

        // 커서 복구
        Cursor.lockState = prevLockMode;
        Cursor.visible = wasCursorVisible;

        // 모달 플래그 해제 (한 프레임 뒤에)
        StartCoroutine(ReleaseModalFlagNextFrame());

        isInCalculatorView = false;
    }

    private IEnumerator ReleaseModalFlagNextFrame()
    {
        yield return null;
        GlobalInteractionFlagS.ModalDepth--;
        Debug.Log($"[Calculator] ModalDepth={GlobalInteractionFlagS.ModalDepth}");
    }
    //     [SerializeField] CinemachineVirtualCamera calculatorCamera; // 계산기 화면 고정 카메라
    //     [SerializeField] CinemachineVirtualCamera defaultCamera; //player follow 카메라
    //     [SerializeField] InputActionAsset playerInteraction; // Inspector에서 PlayerInteraction 드래그, 인풋액션임 ecs
    //     [SerializeField] float blendTime = 0.5f; // 카메라 전환 시간


    //     private InputAction exitUIAction; // 인풋액션 가져오기
    //     private bool isInCalculatorView = false; // 현재 계산기 화면인지 확인하는 변수
    //     private CinemachineBrain brain;

    //     void Start()
    //     {
    //         // 시네머신 속도
    //         brain = Camera.main.GetComponent<CinemachineBrain>();

    //         // ExitUI 액션 직접 가져오기
    //         exitUIAction = playerInteraction.FindAction("ExitUI");
    //         exitUIAction.performed += OnExitUI; //performed로 키를 눌렀다 표시
    //         exitUIAction.Enable(); //지속적인 입력 감지를 위함 
    //     }

    //     //오브젝트 파괴 시 메모리 누수 방지
    //     void OnDestroy()
    //     {
    //         exitUIAction.performed -= OnExitUI; 
    //         exitUIAction?.Disable();
    //     }

    //     //코드로 인풋 직접 연결
    //     private void OnExitUI(InputAction.CallbackContext context)
    //     {
    //         if (isInCalculatorView)
    //         {
    //             Debug.Log("ESC로 돌아가기");
    //             SwitchToDefaultView();
    //         }
    //     }

    //     // 기본 메서드, 마우스를 누를 때
    //     void OnMouseDown()
    //     {

    //         if (RaycastDetector.Instance.HitObject == this.gameObject)
    //         {
    //             SwitchToCalculatorView();
    //         }
    //     }

    //     // 계산기 눌렀을 때 메서드
    //     public void SwitchToCalculatorView()
    //     {
    //         Debug.Log("계산기 카메라로 전환");

    //         Cursor.visible = true;
    //         Cursor.lockState = CursorLockMode.None;

    //         brain.m_DefaultBlend.m_Time = blendTime;
    //         isInCalculatorView = true;
    //         defaultCamera.Priority = 0;
    //         calculatorCamera.Priority = 10;
    //     }

    //     public void SwitchToDefaultView()
    //     {
    //         Debug.Log("기본 카메라로 전환");

    //         // 설정창이 열려있지 않을 때만 커서 숨기기
    //         if (!InGameSettingManager.Instance.GetIsSettingOpen())
    //         {
    //             Cursor.visible = false;
    //             Cursor.lockState = CursorLockMode.Locked;
    //         }

    //         brain.m_DefaultBlend.m_Time = blendTime;
    //         isInCalculatorView = false;
    //         defaultCamera.Priority = 10;
    //         calculatorCamera.Priority = 0;
    //     }


}