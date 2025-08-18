using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// 장지원 8.7 3D오브젝트 클릭시, UI 화면 진입
/// </summary>
public class ClickObjectUIManager : MonoBehaviour
{
    public static ClickObjectUIManager Instance { get; private set; }
    private bool isUIOpen = false;


    [Header("플레이어 입력 시스템 참조")]
    [SerializeField] private CinemachineBrain cinemachineBrain;

    [Header("기본 커서 설정")]
    [SerializeField] private bool defaultCursorVisible = false;
    [SerializeField] private CursorLockMode defaultLockMode = CursorLockMode.Locked;


    private void Awake()
    { // ① 첫 번째 인스턴스인지 확인
        if (Instance == null) 
        {
            Instance = this;   // ② 자기 자신을 static 변수에 저장
            DontDestroyOnLoad(gameObject); // ③ 씬 전환시에도 파괴되지 않게 설정
            InitializeCursor(); // ④ 초기 커서 설정
        }
        else  // ⑤ 이미 인스턴스가 존재하는 경우
        {
            Destroy(gameObject); // ⑥ 중복된 자신을 파괴
        }
    }

    private void InitializeCursor()
    {
        SetCursorState(defaultCursorVisible, defaultLockMode);
    }

    public void OpenUI(GameObject uiPanel) 
    {
        if (uiPanel == null)
        {
            Debug.Log("Ui창이 할당되지 않았습니다.");
            return;
        }

        isUIOpen = true;
        uiPanel.SetActive(isUIOpen);
        SetCursorState(isUIOpen, CursorLockMode.None);
        FreezeCamera(isUIOpen); 

    }

    public void CloseUI(GameObject uiPanel)
    {
        if (uiPanel == null)
        {
            Debug.LogWarning("UIManager: 닫으려는 UI 패널이 null입니다.");
            return;
        }

        uiPanel.SetActive(false);

        // 화면 고정 해제: 카메라 컨트롤러 활성화
        FreezeCamera(false);

        // 기본 커서 상태로 복원
        SetCursorState(defaultCursorVisible, defaultLockMode);

        isUIOpen = false;
        Debug.Log($"UI 닫힘: {uiPanel.name} - 화면 고정 해제됨");
    }



//커서 상태
    private void SetCursorState(bool visible, CursorLockMode lockMode)
    {
        Cursor.visible = visible;
        Cursor.lockState = lockMode;
    }

private void FreezeCamera(bool freeze)
{
 // CinemachineBrain이 할당되었는지 확인합니다.
    // 이 컴포넌트는 보통 Main Camera에 붙어있습니다.
    if (cinemachineBrain != null)
    {
        // Brain 자체를 끄고 켜서 모든 시네머신 카메라의 동작을 제어합니다.
        cinemachineBrain.enabled = !freeze;
        Debug.Log($"시네머신 브레인 {(freeze ? "고정" : "활성화")}");
    }
    else
    {
        Debug.LogWarning("FreezeCamera: cinemachineBrain이 할당되지 않았습니다.");
    }
}
    
    
    public bool IsUIOpen => isUIOpen;
}
