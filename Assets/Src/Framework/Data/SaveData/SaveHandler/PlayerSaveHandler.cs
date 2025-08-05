using UnityEngine;
using StarterAssets;

public class PlayerSaveHandler : MonoBehaviour, ISaveable
{
    private FirstPersonController playerController;

    private void Awake()
    {
        playerController = FindFirstObjectByType<FirstPersonController>();
    }

    public object CaptureData()
    {
        return new PlayerSaveData
        {
            position = playerController.transform.position,
            rotation = playerController.transform.rotation
        };
    }

    public void RestoreData(object data)
    {
        PlayerSaveData playerData = data as PlayerSaveData;
        if (playerData == null) return;

        CharacterController cc = playerController.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        playerController.transform.position = playerData.position;
        playerController.transform.rotation = playerData.rotation;

        if (cc != null) cc.enabled = true;
    }
}
