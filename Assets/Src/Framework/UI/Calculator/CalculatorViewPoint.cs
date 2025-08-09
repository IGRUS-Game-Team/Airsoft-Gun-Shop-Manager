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

public class CalculatorViewPoint : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera calculatorCamera; // 계산기 화면 고정 카메라
    [SerializeField] CinemachineVirtualCamera defaultCamera; //player follow 카메라
    [SerializeField] InputActionAsset playerInteraction; // Inspector에서 PlayerInteraction 드래그, 인풋액션임 ecs
    [SerializeField] float blendTime = 0.5f; // 카메라 전환 시간


    private InputAction exitUIAction; // 인풋액션 가져오기
    private bool isInCalculatorView = false; // 현재 계산기 화면인지 확인하는 변수
    private CinemachineBrain brain;
    
    void Start()
    {
        // 시네머신 속도
        brain = Camera.main.GetComponent<CinemachineBrain>();

        // ExitUI 액션 직접 가져오기
        exitUIAction = playerInteraction.FindAction("ExitUI");
        exitUIAction.performed += OnExitUI; //performed로 키를 눌렀다 표시
        exitUIAction.Enable(); //지속적인 입력 감지를 위함 
    }
    
    //오브젝트 파괴 시 메모리 누수 방지
    void OnDestroy()
    {
        exitUIAction.performed -= OnExitUI; 
        exitUIAction?.Disable();
    }
    
    //코드로 인풋 직접 연결
    private void OnExitUI(InputAction.CallbackContext context)
    {
        if (isInCalculatorView)
        {
            Debug.Log("ESC로 돌아가기");
            SwitchToDefaultView();
        }
    }
    
    // 기본 메서드, 마우스를 누를 때
    void OnMouseDown()
    {

        if (RaycastDetector.Instance.HitObject == this.gameObject)
        {
            SwitchToCalculatorView();
        }
    }

    // 계산기 눌렀을 때 메서드
    public void SwitchToCalculatorView()
    {
        Debug.Log("계산기 카메라로 전환");
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        brain.m_DefaultBlend.m_Time = blendTime;
        isInCalculatorView = true;
        defaultCamera.Priority = 0;
        calculatorCamera.Priority = 10;
    }

    public void SwitchToDefaultView()
    {
        Debug.Log("기본 카메라로 전환");
        
        // 설정창이 열려있지 않을 때만 커서 숨기기
        if (!InGameSettingManager.Instance.GetIsSettingOpen())
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        brain.m_DefaultBlend.m_Time = blendTime;
        isInCalculatorView = false;
        defaultCamera.Priority = 10;
        calculatorCamera.Priority = 0;
    }
}