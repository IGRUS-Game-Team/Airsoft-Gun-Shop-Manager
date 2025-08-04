using UnityEngine;

// 출구 콜라이더에 닿은 NPC를 씬에서 제거한다.
public class ExitDespawner : MonoBehaviour
{
    private const string TagNpc = "Npc";     // NPC 태그. 다른 스크립트와 동일하게 유지

    // 트리거 안에 무언가 들어오면 한 번 호출된다.
    private void OnTriggerEnter(Collider other)
    {
        // NPC만 처리
        if (!other.CompareTag(TagNpc)) return;

        // NPC 오브젝트 파괴 → 풀링 시스템이 있다면 여기서 반환 처리로 변경
        Destroy(other.gameObject);
    }
}