using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class ProtestDirector : MonoBehaviour
{
    [Header("스폰 존 (이 원 안에서만 스폰)")]
    [SerializeField] Transform spawnCenter;
    [SerializeField, Min(0f)] float spawnRadius = 4f;

    [Header("랠리(매장 앞) 라인: A → B 사이로 퍼짐")]
    [SerializeField] Transform rallyLineA;
    [SerializeField] Transform rallyLineB;
    [SerializeField, Min(0.4f)] float slotSpacing = 1.0f;
    [SerializeField, Range(0f, 0.5f)] float slotJitter = 0.2f;

    [Header("정면 바라볼 기준(간판/매장 정면)")]
    [SerializeField] Transform faceTarget;

    [Header("퇴장 포인트(맵 밖)")]
    [SerializeField] Transform exitPoint;

    // ▼ 바운서(용역) 관련
    [Header("바운서 소환 설정")]
    [SerializeField] GameObject bouncerPrefab;
    [SerializeField] Transform bouncerSpawn;               // 없으면 spawnCenter 사용
    [SerializeField, Min(0f)] float bouncerCallCooldown = 15f;
    [SerializeField, Min(0f)] float bouncerEscortSpeed = 2.6f;
    [SerializeField, Min(0f)] float bouncerOrderRadius = 8f; // 이 반경 안의 시위대에게 퇴장명령

    float _nextCallReadyTime;
    bool _protestOn;

    // ▼▼ 여기만 바뀐 핵심: 프리팹 여러 개 + 가중치
    [Serializable]
    public class ProtestorVariant
    {
        public GameObject prefab;
        [Min(0f)] public float weight = 1f; // 확률 가중치
    }

    [Header("스폰 프리팹들")]
    [SerializeField] List<ProtestorVariant> variants = new();
    [SerializeField] bool alternateVariants = false;
    int altIndex = 0;

    [Header("스폰 설정")]
    [SerializeField, Min(1)] int spawnCount = 6;
    [SerializeField, Min(0f)] float spawnInterval = 0.2f;

    [Header("랠리 머무르는 시간(초) — 0이면 무한")]
    [SerializeField, Min(0f)] float protestDuration = 25f;

    readonly List<Protestor> spawned = new();
    Coroutine spawnCoro;

    public bool IsProtestOn => _protestOn;
    public float CooldownRemaining => Mathf.Max(0f, _nextCallReadyTime - Time.time);

    void OnEnable()
    {
        SocialEventManager.OnProtestToggled += HandleToggle;
    }
    void OnDisable()
    {
        SocialEventManager.OnProtestToggled -= HandleToggle;
        StopAll();
    }

    void HandleToggle(bool on)
    {
        if (on) StartProtest(spawnInterval <= 0f);
        else    StopProtest();
    }

    [ContextMenu("Start Protest (Instant)")] public void StartProtestInstant() => StartProtest(true);
    [ContextMenu("Start Protest (Gradual)")] public void StartProtestGradual() => StartProtest(false);

    public void StartProtest(bool instant = false)
    {
        StopProtest();

        var slots = BuildRallySlots(spawnCount);

        if (instant || spawnInterval <= 0f)
        {
            for (int i = 0; i < spawnCount; i++)
                SpawnOne(slots[i]);
        }
        else
        {
            spawnCoro = StartCoroutine(SpawnGradual(slots));
        }

        _protestOn = true;
    }

    [ContextMenu("Stop Protest")]
    public void StopProtest()
    {
        StopAll();
        foreach (var p in spawned) if (p) p.Dismiss();
        spawned.Clear();
        _protestOn = false;
    }

    void StopAll()
    {
        if (spawnCoro != null) { StopCoroutine(spawnCoro); spawnCoro = null; }
    }

    IEnumerator SpawnGradual(List<Vector3> slots)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            SpawnOne(slots[i]);
            if (spawnInterval > 0f) yield return new WaitForSeconds(spawnInterval);
        }
        spawnCoro = null;
    }

    void SpawnOne(Vector3 rallyPos)
    {
        var prefab = PickPrefab();
        if (!prefab) { Debug.LogWarning("[ProtestDirector] 프리팹이 설정되어 있지 않음"); return; }

        var spawnPos = SampleOnNavmesh(GetRandomInCircle(spawnCenter.position, spawnRadius), 3f);
        var go = Instantiate(prefab, spawnPos, Quaternion.identity);

        var p  = go.GetComponent<Protestor>();
        if (!p) { Debug.LogWarning("[ProtestDirector] 프리팹에 Protestor 컴포넌트가 필요함", prefab); Destroy(go); return; }

        p.Init(
            rallyPos: rallyPos,
            exitT:    exitPoint,
            dwellSec: protestDuration,
            faceT:    faceTarget
        );
        spawned.Add(p);
    }

    // ───────── 바운서(용역) 호출 API ─────────
    public void CallBouncer()
    {
        if (!_protestOn)
        {
            Debug.Log("[ProtestDirector] 현재 시위가 없음");
            return;
        }
        if (Time.time < _nextCallReadyTime)
        {
            Debug.Log("[ProtestDirector] 쿨다운 진행중");
            return;
        }
        if (!bouncerPrefab)
        {
            Debug.LogWarning("[ProtestDirector] bouncerPrefab 미설정");
            return;
        }

        _nextCallReadyTime = Time.time + bouncerCallCooldown;

        var spawnPos = (bouncerSpawn ? bouncerSpawn.position : spawnCenter.position);
        var pos = SampleOnNavmesh(spawnPos, 2f);
        var go  = Instantiate(bouncerPrefab, pos, Quaternion.identity);

        var b = go.GetComponent<BouncerEscort>();
        if (!b) { Debug.LogWarning("[ProtestDirector] 바운서 프리팹에 BouncerEscort 컴포넌트 필요"); Destroy(go); return; }

        b.Setup(
    director: this,
    rallyCenter: GetRallyCenter(),
    exitT: exitPoint,
    orderRadius: bouncerOrderRadius,
    escortSpeed: bouncerEscortSpeed,
    faceLookTarget: faceTarget,   // ★ 시위대가 바라보는 기준
    standAhead: 2.0f,             // 군중 앞 몇 m 지점에 설지 (원하면 조절)
    lateralOffset: 0f             // 살짝 옆으로 서고 싶으면 ±값
);

    }

    public void OrderDismissAll()
    {
        foreach (var p in spawned)
            if (p) p.Dismiss();
    }

    public IReadOnlyList<Protestor> GetSpawned() => spawned;

    public Vector3 GetRallyCenter()
    {
        if (!rallyLineA || !rallyLineB) return spawnCenter ? spawnCenter.position : transform.position;
        var a = rallyLineA.position; var b = rallyLineB.position;
        return new Vector3((a.x + b.x) * 0.5f, a.y, (a.z + b.z) * 0.5f);
    }

    // ── 프리팹 선택 로직 ──
    GameObject PickPrefab()
    {
        if (variants == null || variants.Count == 0) return null;

        if (alternateVariants)
        {
            var v = variants[altIndex % variants.Count];
            altIndex++;
            return v.prefab;
        }

        float total = 0f;
        foreach (var v in variants) total += Mathf.Max(0.0001f, v.weight);
        float r = UnityEngine.Random.Range(0f, total);
        foreach (var v in variants)
        {
            r -= Mathf.Max(0.0001f, v.weight);
            if (r <= 0f) return v.prefab;
        }
        return variants[variants.Count - 1].prefab;
    }

    // ── 슬롯 생성: A→B 선분 균등 배치 + 지터 ──
    List<Vector3> BuildRallySlots(int count)
    {
        var list = new List<Vector3>(count);
        if (!rallyLineA || !rallyLineB) return list;

        var A = rallyLineA.position;
        var B = rallyLineB.position;
        var dir = (B - A); dir.y = 0f;
        var length = dir.magnitude;
        var fwd = (length > 0.001f) ? dir / length : Vector3.right;

        var needLen = Mathf.Max((count - 1), 0) * slotSpacing;
        var start   = A + fwd * Mathf.Max(0f, (length - needLen) * 0.5f);

        for (int i = 0; i < count; i++)
        {
            var basePos = start + fwd * (i * slotSpacing);
            var right   = new Vector3(fwd.z, 0f, -fwd.x);
            var jitter  = right * UnityEngine.Random.Range(-slotJitter, slotJitter);
            var nav     = SampleOnNavmesh(basePos + jitter, 1.5f);
            list.Add(nav);
        }
        return list;
    }

    // ── 유틸 ──
    Vector3 GetRandomInCircle(Vector3 c, float r)
    {
        var v = UnityEngine.Random.insideUnitCircle * r;
        return new Vector3(c.x + v.x, c.y, c.z + v.y);
    }
    Vector3 SampleOnNavmesh(Vector3 pos, float maxDist)
    {
        return NavMesh.SamplePosition(pos, out var hit, maxDist, NavMesh.AllAreas) ? hit.position : pos;
    }
}