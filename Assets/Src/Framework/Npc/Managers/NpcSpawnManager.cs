using System.Collections.Generic;
using UnityEngine;

// 일정 간격으로 NPC를 소환해 매장으로 보내는 매니저
public class NpcSpawnManager : MonoBehaviour
{
    /* ---------- 인스펙터에서 설정할 필드 ---------- */

    [SerializeField] private Transform npcParent;     // 씬 계층에서 소환된 NPC를 담을 부모
    [SerializeField] private Transform doorPoint;     // NPC가 향할 출입문 Transform
    [SerializeField] private Transform[] spawnPoints; // NPC 스폰 위치 후보들
    [SerializeField] private GameObject[] npcPrefabs; // NPC 프리팹 모음
    [SerializeField] private float minDelay = 1f;     // 스폰 간 최소 지연 시간
    [SerializeField] private float maxDelay = 3f;     // 스폰 간 최대 지연 시간

    /* ---------- 내부 관리용 필드 ---------- */

    private readonly List<NpcController> npcPool = new List<NpcController>(); // 현재 활성 NPC 목록
    private float nextSpawnTime;   // 다음 스폰 시각

    /* ---------- 매 프레임 호출 ---------- */

    private void Update()
    {
        // 1) 파괴된 NPC를 풀에서 제거해 메모리 누수 방지
        npcPool.RemoveAll((NpcController npc) => npc == null);

        // 2) 아직 스폰 시각이 안 됐으면 종료
        if (Time.time < nextSpawnTime) return;

        // 3) NPC 한 명 소환
        SpawnNpc();

        // 4) 다음 스폰 시각 계산
        nextSpawnTime = Time.time + Random.Range(minDelay, maxDelay);
    }

    /* ---------- NPC 소환 로직 ---------- */

    private void SpawnNpc()
    {
        // 0) 프리팹이나 스폰포인트가 없으면 조용히 종료
        if (npcPrefabs.Length == 0 || spawnPoints.Length == 0) return;

        // 1) 프리팹과 스폰 위치를 무작위로 고른다
        int prefabIndex = Random.Range(0, npcPrefabs.Length);      // 사용할 프리팹 번호
        int pointIndex  = Random.Range(0, spawnPoints.Length);      // 사용할 스폰지점 번호

        // 2) NPC 프리팹을 인스턴스화한다
        GameObject npcObject = Instantiate(
            npcPrefabs[prefabIndex],                   // 생성할 프리팹
            spawnPoints[pointIndex].position,          // 생성 위치
            Quaternion.identity,                       // 회전값(없음)
            npcParent);                                // 부모 설정

        // 3) 필수 컴포넌트 얻기
        NpcController npcController = npcObject.GetComponent<NpcController>();

        // 4) 만약 프리팹에 NpcController가 없다면 경고만 출력하고 종료
        if (npcController == null)
        {
            return;
        }

        // 5) 풀에 등록해 나중에 관리
        npcPool.Add(npcController);

        // 6) NPC에게 출입문 위치를 알려준다
        npcController.SetDoorPoint(doorPoint);
    }
}