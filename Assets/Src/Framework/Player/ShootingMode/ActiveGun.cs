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

    FirstPersonController firstPersonController;
    ShootingGun currentGun;

    float timeSinceLastShot = 0f;
    float defaultFOV;
    float defaultRotationSpeed;

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
    }

    void OnDisable()
    {
        playerShooting.Disable();
    }

    void Update()
    {
        HandleZoom();
    }

    void Shooting()
    {
        timeSinceLastShot += Time.deltaTime;

        if (timeSinceLastShot >= shootingGunSO.FireRate)
        {
            currentGun.Shoot(shootingGunSO);
            timeSinceLastShot = 0f;
        }
    }

    void HandleZoom()
    {
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
        ShootingGun newGun = Instantiate(shootingGunSO.GunPrefab, transform).GetComponent<ShootingGun>();
        currentGun = newGun;
        this.shootingGunSO = shootingGunSO;
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
