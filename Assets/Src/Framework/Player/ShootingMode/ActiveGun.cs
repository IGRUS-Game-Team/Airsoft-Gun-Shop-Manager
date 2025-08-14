using UnityEngine;
using StarterAssets;

public class ActiveGun : MonoBehaviour
{
    [SerializeField] StarterAssets.StarterAssetsInputs playerInput;
    [SerializeField] ShootingGunSO shootingGunSO;
    [SerializeField] Animator animator;
   
    ShootingGun currentGun;
    public AudioSource audiosource;
    
    const string SHOOT_STRING = "Shoot";

    float timeSinceLastShot = 0f;

    void Awake()
    {
        if (InteractionController.Instance != null)
        {
            InteractionController.Instance.OnClick += Shooting;
        }
    }

    void Start()
    {
        currentGun = GetComponentInChildren<ShootingGun>();
    }

    void Update()
    {
        timeSinceLastShot += Time.deltaTime;
        if (playerInput.range) Shooting();
    }

    void Shooting()
    {
        if (timeSinceLastShot >= shootingGunSO.FireRate)
        {
            currentGun.Shoot(shootingGunSO);
            animator.Play(SHOOT_STRING, 0, 0f);
            timeSinceLastShot = 0f;
        }

    }
}
