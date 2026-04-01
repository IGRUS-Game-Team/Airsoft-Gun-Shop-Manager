using UnityEngine;

public class DoorStateSaveHandler : MonoBehaviour, ISaveable
{
    public object CaptureData()
    {
        var door = FindFirstObjectByType<DoorTrigger>();
        return new DoorStateSaveData
        {
            isOpen = door != null && door.IsOpen
        };
    }

    public void RestoreData(object data)
    {
        if (data == null) return;
        var loaded = data as DoorStateSaveData;
        if (loaded == null) return;

        var door = FindFirstObjectByType<DoorTrigger>();
        if (door != null)
        {
            door.SetOpen(loaded.isOpen);
        }

        // 문 푯말 비주얼도 갱신
        var sign = FindFirstObjectByType<DoorSignInteraction>();
        if (sign != null)
        {
            sign.RefreshVisual();
        }

        Debug.Log($"[Load] DoorState ← isOpen={loaded.isOpen}");
    }
}
