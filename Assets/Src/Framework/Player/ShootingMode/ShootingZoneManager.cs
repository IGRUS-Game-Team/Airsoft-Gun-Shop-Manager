using UnityEngine;

public class ShootingZoneManager : MonoBehaviour
{
    public static ShootingZoneManager Instance { get; private set; }

    [SerializeField] ShootingMode[] shootingZones;

    private ActiveGun currentActiveGun;

    public bool IsInShootingMode { get; private set; }
    public bool CurrentGunCanZoom => currentActiveGun != null && currentActiveGun.CanZoom;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        foreach (var zone in shootingZones)
            zone.isZoneActive = true;
    }

    public void EnterShootingMode(ActiveGun gun)
    {
        IsInShootingMode = true;
        currentActiveGun = gun;
    }

    public void ExitShootingMode()
    {
        IsInShootingMode = false;
        if (currentActiveGun != null) currentActiveGun.DropGun();
        currentActiveGun = null;
    }
}