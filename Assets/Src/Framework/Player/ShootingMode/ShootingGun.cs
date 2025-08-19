using UnityEngine;
using Cinemachine;

/// <summary>
/// 
/// </summary>

public class ShootingGun : MonoBehaviour
{
    [SerializeField] StarterAssets.StarterAssetsInputs playerInput;
    [SerializeField] ParticleSystem ShootingParticle;
    [SerializeField] ParticleSystem HitVFXParticle;
    [SerializeField] Animator animator;

    CinemachineImpulseSource impulseSource;

    public int GunIndex;

    void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Shoot(ShootingGunSO shootingGunSO)
    {
        ShootingParticle.Play();
        HitVFXParticle.Play();
        animator.Play(shootingGunSO.ShootAnimation.name, 0, 0f);
        impulseSource.GenerateImpulse();
        AudioManager.Instance.PlayGunSound(GunIndex);

        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity))
        {
            Debug.Log(hit.collider.name);
        }
    }
}
