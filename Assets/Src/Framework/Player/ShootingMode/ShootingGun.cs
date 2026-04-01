using UnityEngine;
using Cinemachine;

public class ShootingGun : MonoBehaviour
{
    [SerializeField] StarterAssets.StarterAssetsInputs playerInput;
    [SerializeField] Transform muzzlePoint;           // BB탄 발사 위치 (비어 있으면 ShootingParticle 위치 사용)
    [SerializeField] ParticleSystem ShootingParticle;  // 기존 총구 화염 (레거시, 발사 안 함)
    [SerializeField] ParticleSystem HitVFXParticle;
    [SerializeField] Animator animator;

    [Header("BB Pellet")]
    [SerializeField] float pelletSpeed = 100f;
    [SerializeField] float pelletScale = 0.06f;   // BB탄 지름 (Unity 단위)
    [SerializeField] float trailWidth = 0.02f;
    [SerializeField] float trailTime = 0.15f;
    [SerializeField] float maxDistance = 200f;

    CinemachineImpulseSource impulseSource;
    private static Material bbMaterial;

    public int GunIndex;

    void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Shoot(ShootingGunSO shootingGunSO)
    {
        animator.Play(shootingGunSO.ShootAnimation.name, 0, 0f);
        impulseSource.GenerateImpulse();
        AudioManager.Instance.PlayGunSound(GunIndex);

        // 발사 위치 결정
        Vector3 muzzlePos = muzzlePoint != null
            ? muzzlePoint.position
            : (ShootingParticle != null ? ShootingParticle.transform.position : transform.position);

        Camera cam = Camera.main;
        bool didHit = Physics.Raycast(
            cam.transform.position, cam.transform.forward,
            out RaycastHit hit, maxDistance);

        Vector3 targetPos = didHit
            ? hit.point
            : cam.transform.position + cam.transform.forward * maxDistance;

        SpawnBBPellet(muzzlePos, targetPos, didHit);
    }

    void SpawnBBPellet(Vector3 from, Vector3 to, bool didHit)
    {
        GameObject pellet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pellet.name = "BBPellet";
        pellet.transform.position = from;
        pellet.transform.localScale = Vector3.one * pelletScale;

        // 충돌체 제거
        Destroy(pellet.GetComponent<Collider>());

        // 흰색 Unlit 머티리얼
        if (bbMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            bbMaterial = new Material(shader);
            bbMaterial.color = Color.white;
        }
        pellet.GetComponent<Renderer>().sharedMaterial = bbMaterial;

        // 궤적 (TrailRenderer)
        TrailRenderer trail = pellet.AddComponent<TrailRenderer>();
        trail.time = trailTime;
        trail.startWidth = trailWidth;
        trail.endWidth = 0f;
        trail.material = new Material(bbMaterial);
        trail.startColor = Color.white;
        trail.endColor = new Color(1f, 1f, 1f, 0f);
        trail.minVertexDistance = 0.1f;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;

        // 이동 스크립트
        BBPellet bb = pellet.AddComponent<BBPellet>();
        bb.Init(to, pelletSpeed, HitVFXParticle, didHit);
    }
}
