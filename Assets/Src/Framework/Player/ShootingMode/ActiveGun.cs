using UnityEngine;
using UnityEngine.InputSystem;
using System.IO.Compression;
using Cinemachine;
using StarterAssets;

public class ActiveGun : MonoBehaviour
{
    [SerializeField] ShootingGunSO shootingGunSO;
    [SerializeField] private PlayerShooting playerShooting;
    [SerializeField] CinemachineVirtualCamera playerFollowCamera;
    [SerializeField] GameObject zoomVignette;
    [SerializeField] ShootingZoneManager shootingZoneManager;

    FirstPersonController firstPersonController;
    ShootingGun currentGun;

    float timeSinceLastShot = 0f;
    float defaultFOV;
    float defaultRotationSpeed;
    private bool isShooting; // 마우스를 누르고 있는 중인지 여부 확인
    private float nextFireTime; // 

    void Awake()
    {
        firstPersonController = GetComponentInParent<FirstPersonController>();
        playerShooting = new PlayerShooting();
        defaultFOV = playerFollowCamera.m_Lens.FieldOfView;
        defaultRotationSpeed = firstPersonController.RotationSpeed;

        if (InteractionController.Instance != null)
        {
            InteractionController.Instance.OnClick += Shooting;
        }
    }

    void Start()
    {
        currentGun = GetComponentInChildren<ShootingGun>();
    }

    void OnEnable()
    {
        playerShooting.Enable();
        playerShooting.Player.Range.performed += ctx =>
        {
            shootingZoneManager.ToggleZones(this);
        };
    }

    void OnDisable()
    {
        playerShooting.Disable();
    }

    void Update()
    {
        HandleZoom();
        if (shootingGunSO == null || currentGun == null) return;
        if (isShooting && Time.time >= nextFireTime)
        {
            currentGun.Shoot(shootingGunSO);
            nextFireTime = Time.time + shootingGunSO.FireRate;
        }
    }

    void Shooting()
    {
        if (currentGun == null || shootingGunSO == null) return;

        timeSinceLastShot += Time.deltaTime;

        if (timeSinceLastShot >= shootingGunSO.FireRate)
        {
            currentGun.Shoot(shootingGunSO);
            timeSinceLastShot = 0f;
        }
    }

    void HandleZoom()
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

        if (currentGun) Destroy(currentGun.gameObject);

        currentGun = Instantiate(shootingGunSO.GunPrefab, transform).GetComponent<ShootingGun>();
        this.shootingGunSO = shootingGunSO;

        InteractionController.Instance.OnClick -= Shooting;
        var shootAction = playerShooting.Player.Shoot;
        shootAction.started -= ctx => StartShooting();
        shootAction.canceled -= ctx => StopShooting();

        if (shootingGunSO.IsAuto)
        {
            shootAction.started += ctx => StartShooting();
            shootAction.canceled += ctx => StopShooting();
        }
        else
        {
            InteractionController.Instance.OnClick += Shooting;
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

    public void DropGun()
    {
        if (currentGun != null)
        {
            Destroy(currentGun.gameObject);
            currentGun = null;
            shootingGunSO = null;
        }
    }

    void OnDestroy()
    {
        playerShooting?.Disable();
        if (InteractionController.Instance != null)
        {
            InteractionController.Instance.OnClick -= Shooting;
        }
    }
}