using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class ProtestDirector : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] Transform rallyCenter;   // 빨간 동그라미 가운데 빈 오브젝트
    [SerializeField, Min(0f)] float rallyRadius = 1.8f; // 그 안에서 랜덤 배치
    [SerializeField] Transform exitPoint;     // 떠날 곳(맵 바깥)

    [Header("스폰 설정")]
    [SerializeField] GameObject protestorPrefab; // NavMeshAgent+Animator 포함 프리팹(아래 2번)
    [SerializeField, Min(1)] int spawnCount = 6;
    [SerializeField, Min(0f)] float spawnInterval = 0.2f;

    [Header("랠리 머무르는 시간(초) — 0이면 무한")]
    [SerializeField, Min(0f)] float protestDuration = 25f;

    readonly List<Protestor> spawned = new();

    void OnEnable()
    {
        // SocialEventManager가 이런 이벤트를 내보내게 해두면 자동 구독
        SocialEventManager.OnProtestToggled += HandleToggle;
    }
    void OnDisable()
    {
        SocialEventManager.OnProtestToggled -= HandleToggle;
    }

    // 외부에서 호출용(디자이너 버튼/스크립트)
    [ContextMenu("Start Protest")]
    public void StartProtest() => StartCoroutine(SpawnAll());

    [ContextMenu("Stop Protest")]
    public void StopProtest()
    {
        foreach (var p in spawned) if (p) p.Dismiss();
        spawned.Clear();
    }

    // (선택) 소셜 이벤트 훅
    void HandleToggle(bool on)
    {
        if (on) StartProtest();
        else    StopProtest();
    }

    IEnumerator SpawnAll()
    {
        // 중복 시작 방지: 기존 것 있으면 먼저 멈춤
        StopProtest();

        for (int i = 0; i < spawnCount; i++)
        {
            var pos = SampleOnNavmesh(GetSpawnAround(rallyCenter.position, 4f, 7f));
            var go  = Instantiate(protestorPrefab, pos, Quaternion.identity);
            var p   = go.GetComponent<Protestor>();
            if (p != null)
            {
                var rally = SampleOnNavmesh(GetRandomInCircle(rallyCenter.position, rallyRadius));
                p.Init(rally, exitPoint, protestDuration);
                spawned.Add(p);
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // ───── 유틸 ─────
    Vector3 GetRandomInCircle(Vector3 center, float radius)
    {
        var r = UnityEngine.Random.insideUnitCircle * radius;
        return new Vector3(center.x + r.x, center.y, center.z + r.y);
    }
    Vector3 GetSpawnAround(Vector3 center, float min, float max)
    {
        var dir = UnityEngine.Random.insideUnitCircle.normalized;
        float d = UnityEngine.Random.Range(min, max);
        return new Vector3(center.x + dir.x * d, center.y, center.z + dir.y * d);
    }
    Vector3 SampleOnNavmesh(Vector3 pos, float maxDist = 3f)
    {
        if (NavMesh.SamplePosition(pos, out var hit, maxDist, NavMesh.AllAreas))
            return hit.position;
        return pos; // 실패 시 원점 사용(네비 메시 꼭 그려두자)
    }
    
}