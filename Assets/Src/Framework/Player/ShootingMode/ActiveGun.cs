using UnityEngine;

public class ActiveGun : MonoBehaviour
{
    [SerializeField] StarterAssets.StarterAssetsInputs playerInput;
    [SerializeField] ShootingGunSO shootingGunSO;
    [SerializeField] ParticleSystem ShootingParticle;
    [SerializeField] Animator animator;
    
    const string SHOOT_STRING = "Shoot";

    void Awake()
    {
        if (InteractionController.Instance != null)
        {
            InteractionController.Instance.OnClick += Shooting;
        } 
    }

    void Update()
    {
        if (playerInput.range)
        {
            Shooting();
        }
    }

    void Shooting()
    {
        ShootingParticle.Play();
        animator.Play(SHOOT_STRING, 0, 0f);
    }
}
