using System.Collections.Generic;
using UnityEngine;

public class ShootingRangeManager : MonoBehaviour
{
    public static ShootingRangeManager Instance { get; private set; }

    [Header("수동 등록: 레인들을 여기에 드래그 앤 드롭")]
    [SerializeField] private List<ShootingLane> lanes = new();

    // 임시 버퍼: 사용 가능한 레인들
    private readonly List<ShootingLane> _available = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        RemoveNulls();
    }

    [ContextMenu("Remove null lanes")]
    private void RemoveNulls()
    {
        lanes.RemoveAll(l => l == null);
    }

    public bool TryGetAvailableLane(out ShootingLane lane)
    {
        RemoveNulls();
        _available.Clear();

        foreach (var l in lanes)
        {
            if (!l) continue;
            if (!l.isActiveAndEnabled) continue;                         // ★ 비활성 레인 제외
            if (l.IsOccupied) continue;
            if (l.standPoint != null && !l.standPoint.gameObject.activeInHierarchy) continue; // ★ 스탠드포인트 꺼져도 제외
            _available.Add(l);
        }

        if (_available.Count == 0) { lane = null; return false; }

        int pick = Random.Range(0, _available.Count);
        lane = _available[pick];
        return true;
    }

    // (유지) 필요 시 코드로 등록/해제할 때 사용. 자동 등록은 없음.
    public void RegisterLane(ShootingLane lane)
    {
        if (!lane || lanes.Contains(lane)) return;
        lanes.Add(lane);
    }

    public void UnregisterLane(ShootingLane lane)
    {
        if (!lane) return;
        lanes.Remove(lane);
    }

    public IReadOnlyList<ShootingLane> Lanes => lanes;
}