using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] CinemachineVirtualCamera calculatorCamera; // 계산기 고정
    [SerializeField] CinemachineVirtualCamera defaultCamera;    // 플레이어 추적
    [SerializeField] float blendTime = 0.5f;

    private CinemachineBrain brain;
    private bool isInCalculatorView;

    void Awake()
    {
        brain = Camera.main ? Camera.main.GetComponent<CinemachineBrain>() : null;
        if (brain == null)
            Debug.LogWarning("[Calculator] CinemachineBrain을 찾지 못했습니다. 메인 카메라를 확인하세요.");
    }

    void OnEnable()
    {
        // 프로젝트 표준 입력 파이프라인 구독
        if (InteractionController.Instance != null)
            InteractionController.Instance.OnExitUI += HandleExitUI;
    }

    void OnDisable()
    {
        if (InteractionController.Instance != null)
            InteractionController.Instance.OnExitUI -= HandleExitUI;
    }

    // 라우터가 호출하는 진입 지점
    public void Interact()
    {
        // 자기 자신이 레이캐스트 타깃인지 별도 확인이 필요 없다.
        // InputContextRouter가 이미 적절한 대상만 Interact()를 호출함.
        SwitchToCalculatorView();
    }

    private void HandleExitUI()
    {
        if (isInCalculatorView)
            SwitchToDefaultView();
    }

    private void SwitchToCalculatorView()
    {
        Debug.Log("[Calculator] 계산기 카메라로 전환");
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (brain) brain.m_DefaultBlend.m_Time = blendTime;
        isInCalculatorView = true;

        // ESC에서 세팅창이 뜨지 않도록 플래그 활성화
        GlobalInteractionFlagS.ModalDepth++;
        Debug.Log(GlobalInteractionFlagS.ModalDepth);

        if (defaultCamera) defaultCamera.Priority = 0;
        if (calculatorCamera) calculatorCamera.Priority = 10;
    }
    private void SwitchToDefaultView()
    {
        Debug.Log("[Calculator] 기본 카메라로 전환");

        if (!InGameSettingManager.Instance.GetIsSettingOpen())
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (brain) brain.m_DefaultBlend.m_Time = blendTime;
        isInCalculatorView = false;

        // 한 프레임 뒤에 모달 플래그 해제
        StartCoroutine(ReleaseModalFlagNextFrame());
        Debug.Log(GlobalInteractionFlagS.ModalDepth);

        if (defaultCamera) defaultCamera.Priority = 10;
        if (calculatorCamera) calculatorCamera.Priority = 0;
    }
    private IEnumerator ReleaseModalFlagNextFrame()
    {
        yield return null; // 다음 프레임까지 대기
        GlobalInteractionFlagS.ModalDepth--;
        Debug.Log(GlobalInteractionFlagS.ModalDepth);
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