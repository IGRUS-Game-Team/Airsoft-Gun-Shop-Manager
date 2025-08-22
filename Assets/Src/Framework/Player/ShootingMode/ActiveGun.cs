using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using StarterAssets;
using Unity.VisualScripting;

/// <summary>
/// 이지연 
/// </summary>

public class ActiveGun : MonoBehaviour
{
    [SerializeField] ShootingGunSO shootingGunSO;
    [SerializeField] private PlayerShooting playerShooting;
    [SerializeField] CinemachineVirtualCamera playerFollowCamera;
    [SerializeField] GameObject zoomVignette;
    [SerializeField] ShootingZoneManager shootingZoneManager;

    FirstPersonController firstPersonController; // 사격 반동 조절
    ShootingGun currentGun;

    float timeSinceLastShot = 0f; // 단발 사격 시간 계산
    float defaultFOV;
    float defaultRotationSpeed;
    private bool isShooting; // 마우스를 누르고 있는 중인지 여부 확인
    private float nextFireTime; // 자동 사격 간격 확인

    private System.Action onClickHandler; // 단발사격용(InteractionController.OnClick)의 핸들
    private System.Action<InputAction.CallbackContext> onShootStartedHandler;  // 자동사격 시작
    private System.Action<InputAction.CallbackContext> onShootCanceledHandler; // 자동사격 종료

    void Awake()
    {
        firstPersonController = GetComponentInParent<FirstPersonController>();
        playerShooting = new PlayerShooting();
        defaultFOV = playerFollowCamera.m_Lens.FieldOfView;
        defaultRotationSpeed = firstPersonController.RotationSpeed;
    }

    void Start()
    {
        currentGun = GetComponentInChildren<ShootingGun>();
    }

    void Update()
    {
        HandleZoom();
        if (shootingGunSO == null || currentGun == null) return;

        if (isShooting && Time.time >= nextFireTime) // 자동 사격
        {
            currentGun.Shoot(shootingGunSO);
            nextFireTime = Time.time + shootingGunSO.FireRate;
        }
    }

    void OnEnable()
    {
        playerShooting.Enable(); // 사격 Input 활성화
        playerShooting.Player.Range.performed += OnRangePerformed; // Range 이벤트 구독
    }

    void OnDisable()
    {
        playerShooting.Player.Range.performed -= OnRangePerformed; // Range 이벤트 해제
        UnsubscribeShootInput(); // 사격 관련 이벤트 확실히 해제
        playerShooting.Disable(); // 사격 Input 비활성화
    }

    void OnDestroy()
    {
        playerShooting?.Disable();
        UnsubscribeShootInput();
    }

    void OnRangePerformed(InputAction.CallbackContext ctx)
    {
        shootingZoneManager.ToggleZones(this);
    }

    void SingleShot() // 단발 사격
    {
        if (currentGun == null || shootingGunSO == null) return;
        timeSinceLastShot += Time.deltaTime;

        if (timeSinceLastShot >= shootingGunSO.FireRate)
        {
            currentGun.Shoot(shootingGunSO);
            timeSinceLastShot = 0f;
        }
    }

    void StartShooting()
    {
        isShooting = true;
        nextFireTime = Time.time;
    }

    void StopShooting()
    {
        isShooting = false;
    }

    void HandleZoom() // 우클릭 줌
    {
        if (shootingGunSO == null) return;
        if (!shootingGunSO.CanZoom) return;

        if (playerShooting.Player.zoom.IsPressed())
        {
            playerFollowCamera.m_Lens.FieldOfView = shootingGunSO.ZoonAmount;
            zoomVignette.SetActive(true);
            firstPersonController.ChangeRotationSpeed(shootingGunSO.ZoomRotationSpeed);
        }
        else
        {
            playerFollowCamera.m_Lens.FieldOfView = defaultFOV;
            zoomVignette.SetActive(false);
            firstPersonController.ChangeRotationSpeed(defaultRotationSpeed);
        }
    }

    public void SwitchGun(ShootingGunSO shootingGunSO)
    {
        Debug.Log($"Player picked up {shootingGunSO.name}");

        UnsubscribeShootInput(); // 이전 사격 이벤트 전부 해제 

        if (currentGun != null) // 이전 총 제거
        {
            Destroy(currentGun.gameObject);
            currentGun = null;
        }

        this.shootingGunSO = shootingGunSO; // SO 교체

        if (shootingGunSO != null && shootingGunSO.GunPrefab != null) // 새로운 총 생성
        {
            currentGun = Instantiate(shootingGunSO.GunPrefab, transform).GetComponent<ShootingGun>();

            Debug.Log($"Switched to gun: {shootingGunSO.name}");

            SubscribeShootInput();
        }
        else Debug.LogWarning("SwitchGun failed");
    }

    public void DropGun()
    {
        UnsubscribeShootInput();

        if (currentGun != null)
        {
            Destroy(currentGun.gameObject);
            currentGun = null;
        }
        shootingGunSO = null;
    }

    void SubscribeShootInput()
    {
        if (shootingGunSO == null) return;

        if (shootingGunSO.IsAuto) // 자동사격
        {
            if (onShootStartedHandler == null)
                onShootStartedHandler = (ctx) => StartShooting();

            if (onShootCanceledHandler == null)
                onShootCanceledHandler = (ctx) => StopShooting();

            playerShooting.Player.Shoot.started += onShootStartedHandler;
            playerShooting.Player.Shoot.canceled += onShootCanceledHandler;

            if (InteractionController.Instance != null && onClickHandler != null)
                InteractionController.Instance.OnClick -= onClickHandler;
        }
        else // 단발사격
        {
            if (onClickHandler == null)
            {
                onClickHandler = () => SingleShot();
            }

            if (InteractionController.Instance != null)
            {
                InteractionController.Instance.OnClick += onClickHandler;
            }
            
            if (onShootStartedHandler != null)
            {
                playerShooting.Player.Shoot.started -= onShootStartedHandler;
                playerShooting.Player.Shoot.canceled -= onShootCanceledHandler;
            }
        }
    }

    void UnsubscribeShootInput()
    {
        if (onShootStartedHandler != null)
        {
            playerShooting.Player.Shoot.started -= onShootStartedHandler;
            playerShooting.Player.Shoot.canceled -= onShootCanceledHandler;
        }
        
        if (InteractionController.Instance != null && onClickHandler != null)
            InteractionController.Instance.OnClick -= onClickHandler;
    }
}