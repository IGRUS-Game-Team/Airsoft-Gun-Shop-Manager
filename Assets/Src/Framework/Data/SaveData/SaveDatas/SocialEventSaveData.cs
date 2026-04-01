using System.Collections.Generic;

[System.Serializable]
public class SocialEventSaveData
{
    public int strategyIndex;           // 0=Normal, 1=Recession, 2=Boom
    public List<int> affectedItemIds = new();
    public float marketModifier;
    public string eventName;
    public string statusText;
    public bool firstDayDone;
}
