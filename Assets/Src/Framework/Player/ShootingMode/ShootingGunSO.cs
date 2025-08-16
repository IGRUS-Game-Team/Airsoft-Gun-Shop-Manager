using UnityEngine;

[CreateAssetMenu(fileName = "ShootingGunSO", menuName = "Scriptable Objects/ShootingGunSO")]
public class ShootingGunSO : ScriptableObject
{
    public GameObject GunPrefab;
    public float FireRate = .5f;
    public AnimationClip ShootAnimation;
    public bool CanZoom = false;
    public float ZoonAmount = 10f;
    public float ZoomRotationSpeed = .3f;
}
