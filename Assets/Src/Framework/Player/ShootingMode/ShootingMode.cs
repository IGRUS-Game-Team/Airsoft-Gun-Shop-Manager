using UnityEngine;

public class ShootingMode : MonoBehaviour
{
    [SerializeField] ShootingGunSO shootingGunSO;

    const string PLAYER_STRING = "Player";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(PLAYER_STRING))
        {
            ActiveGun activeGun = other.GetComponentInChildren<ActiveGun>();
            activeGun.SwitchGun(shootingGunSO);
            Destroy(this.gameObject);
        }      
    }

}
