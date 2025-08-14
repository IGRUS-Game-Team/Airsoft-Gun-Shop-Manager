using UnityEngine;

[CreateAssetMenu(fileName = "ShootingGunSO", menuName = "Scriptable Objects/ShootingGunSO")]
public class ShootingGunSO : ScriptableObject
{
    public float FireRate = .5f;
    public AudioClip GunSound;
    public AnimationClip ShootAnimation;
}
