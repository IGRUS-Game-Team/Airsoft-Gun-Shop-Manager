using UnityEngine;

public class ShootingZoneManager : MonoBehaviour
{
    [SerializeField] GameObject shootingZones;
    bool isActive = false;

    public void ToggleZones(ActiveGun activeGun)
    {
        isActive = !isActive;
        shootingZones.SetActive(isActive);

        if (!isActive && activeGun != null) activeGun.DropGun();

        Debug.Log("사격 모드" + (isActive ? "활성화" : "비활성화"));
    }
}
