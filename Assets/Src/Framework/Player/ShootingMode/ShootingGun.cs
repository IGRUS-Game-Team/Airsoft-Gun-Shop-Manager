using UnityEngine;
using Cinemachine;

/// <summary>
/// Player, Gun과 연결되는 스크립트
/// 각 구역별 함수가 호출되면 각각의 총기가 활성화됨
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
