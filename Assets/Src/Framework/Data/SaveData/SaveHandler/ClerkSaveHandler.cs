using UnityEngine;

public class ClerkSaveHandler : MonoBehaviour, ISaveable
{
    public object CaptureData()
    {
        var ctrl = AutoClerkController.Instance;
        return new ClerkSaveData
        {
            isHired = ctrl != null && ctrl.IsHired,
            hiredClerkId = ctrl != null ? ctrl.HiredClerkId : -1
        };
    }

    public void RestoreData(object data)
    {
        if (data == null) return;
        var loaded = data as ClerkSaveData;
        if (loaded == null) return;

        if (loaded.isHired && AutoClerkController.Instance != null)
        {
            if (loaded.hiredClerkId > 0)
                AutoClerkController.Instance.RestoreHiredClerk(loaded.hiredClerkId);
            else
                AutoClerkController.Instance.RestoreHiredClerk(1); // fallback to id=1
        }

        Debug.Log($"[Load] Clerk <- hired={loaded.isHired}, clerkId={loaded.hiredClerkId}");
    }
}
