using UnityEngine;

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

    public void Shoot(ShootingGunSO shootingGunSO)
    {
        ShootingParticle.Play();
        HitVFXParticle.Play();
        animator.Play(shootingGunSO.ShootAnimation.name, 0, 0f);

        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity))
        {
            Debug.Log(hit.collider.name);
        }
    }
}
