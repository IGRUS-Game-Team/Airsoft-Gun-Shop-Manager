using UnityEngine;

/// <summary>
/// 이지연 사격 모드 on/off 관리하는 스크립트
/// </summary>

public class ShootingZoneManager : MonoBehaviour
{
    [SerializeField] ShootingMode[] shootingZones;
    [SerializeField] GameObject targetImage;

    bool isActive = false;

    public void ToggleZones(ActiveGun activeGun)
    {
        isActive = !isActive; // on이면 off로, off면 on으로
        targetImage.SetActive(isActive);

        foreach (var zone in shootingZones)
        {
            zone.isZoneActive = isActive; // 각 존 켜고 끄기
        }

        if (!isActive && activeGun != null)
        {
            activeGun.DropGun(); // 모드 OFF 시 무기 버리기
        }

        if (!isActive && activeGun != null) activeGun.DropGun();
        Debug.Log("사격 모드" + (isActive ? "활성화" : "비활성화"));
    }
}