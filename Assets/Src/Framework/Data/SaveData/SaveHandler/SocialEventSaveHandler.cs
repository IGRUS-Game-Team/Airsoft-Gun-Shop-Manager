using UnityEngine;

public class SocialEventSaveHandler : MonoBehaviour, ISaveable
{
    private SocialEventManager socialEvent;

    private void Awake()
    {
        socialEvent = SocialEventManager.Instance ?? FindFirstObjectByType<SocialEventManager>();
    }

    private void EnsureRefs()
    {
        if (socialEvent == null)
            socialEvent = SocialEventManager.Instance ?? FindFirstObjectByType<SocialEventManager>(FindObjectsInactive.Include);
    }

    public object CaptureData()
    {
        EnsureRefs();
        if (socialEvent == null) return null;

        return new SocialEventSaveData
        {
            strategyIndex   = socialEvent.GetStrategyIndex(),
            affectedItemIds = socialEvent.GetAffectedItemIds(),
            marketModifier  = socialEvent.GetMarketModifier(),
            eventName       = socialEvent.GetEventName(),
            statusText      = socialEvent.GetStatusText(),
            firstDayDone    = socialEvent.GetFirstDayDone()
        };
    }

    public void RestoreData(object data)
    {
        EnsureRefs();
        var loaded = data as SocialEventSaveData;
        if (loaded == null || socialEvent == null) return;

        socialEvent.RestoreState(loaded);
    }
}
