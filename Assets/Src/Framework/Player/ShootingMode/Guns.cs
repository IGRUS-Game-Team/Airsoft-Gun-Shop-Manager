using UnityEngine;

/// <summary>
/// Player, Gun과 연결되는 스크립트
/// 각 구역별 함수가 호출되면 각각의 총기가 활성화됨
/// </summary>

public class Guns : MonoBehaviour
{
    [SerializeField] StarterAssets.StarterAssetsInputs playerInput;
    [SerializeField] ParticleSystem ShootingParticle;

    void Awake()
    {
        if (InteractionController.Instance != null)
        {
            InteractionController.Instance.OnClick += Shooting;
        } 
    }

    void Start()
    {

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
    }
}
