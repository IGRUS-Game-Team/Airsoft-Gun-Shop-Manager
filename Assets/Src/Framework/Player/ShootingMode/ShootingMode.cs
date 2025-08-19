using UnityEngine;

public class ShootingMode : MonoBehaviour
{
    [SerializeField] ShootingGunSO shootingGunSO;
    public bool isZoneActive = false;
 
    const string PLAYER_STRING = "Player";

    void OnTriggerEnter(Collider other)
    {
        if (!isZoneActive) return;

        if (other.CompareTag(PLAYER_STRING))
        {
            ActiveGun activeGun = other.GetComponentInChildren<ActiveGun>();

            if (activeGun != null)
            {
                Debug.Log($"[ShootingMode] {shootingGunSO.name} 총 획득!");
                activeGun.SwitchGun(shootingGunSO);
                // this.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("ActiveGun is null!");
            }
        }      
    }
}
