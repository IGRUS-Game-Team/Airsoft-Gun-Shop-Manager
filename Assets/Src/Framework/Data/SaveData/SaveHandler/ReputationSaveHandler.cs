using UnityEngine;

public class ReputationSaveHandler : MonoBehaviour, ISaveable
{
    private ReputationState rep;

    private void Awake()
    {
        // 싱글톤이지만, 안전하게 탐색
        rep = ReputationState.Instance ?? FindFirstObjectByType<ReputationState>();
        if (rep == null)
            Debug.LogWarning("[ReputationSaveHandler] ReputationState를 찾지 못했습니다.");
    }

    public object CaptureData()
    {
        if (rep == null) return null;

        return new ReputationSaveData
        {
            currentRep = rep.CurrentRaw
            // min/max까지 버전 유지하려면 여기에 추가 저장
            // minRep = rep.Min,
            // maxRep = rep.Max
        };
    }

    public void RestoreData(object data)
    {
        var loaded = data as ReputationSaveData;
        if (rep == null || loaded == null) return;

        // min/max를 저장·복원할 경우엔 먼저 범위를 세팅하고(필요시),
        // 그 다음 raw 값을 넣어야 합니다.
        // rep.SetRange(loaded.minRep, loaded.maxRep); // 범위 API가 있다면

        rep.SetRaw(loaded.currentRep); // 텍스트/이벤트까지 갱신됨
    }
}
