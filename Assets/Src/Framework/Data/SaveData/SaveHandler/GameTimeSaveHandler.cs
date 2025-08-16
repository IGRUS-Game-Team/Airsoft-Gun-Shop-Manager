using UnityEngine;

public class GameTimeSaveHandler : MonoBehaviour, ISaveable
{
    TimeUI timeUI;

    void Awake()
    {
        timeUI = FindFirstObjectByType<TimeUI>();
        if (timeUI == null) Debug.Log("ㅇ");
    }
    

    public object CaptureData()
    {
        return new GameTimeSaveData
        {
            totalGameMinutes = timeUI.totalGameMinutes,
            accumulatedRealSeconds = timeUI.GetTotalPlayTimeInRealSeconds()
        };
    }

    public void RestoreData(object data)
    {
        GameTimeSaveData loaded = data as GameTimeSaveData;
        if (loaded == null) return;

        timeUI.totalGameMinutes = loaded.totalGameMinutes;
        timeUI.ForceUpdate(); // 시간 UI 갱신용 함수 (아래에 정의함)
        Debug.Log("와 ! 시간 로드 성공!");
    }
}