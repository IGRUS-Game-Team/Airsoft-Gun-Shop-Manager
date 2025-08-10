using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Header("NPC 퇴장 위치")]
    [SerializeField] Transform exitPoint;

    [Header("매장 최대 인원")]
    [SerializeField] int maxInStore = 10;

    const string TagNpc = "Npc";
    int insideCount;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Npc")) return;

        var npc = other.GetComponent<NpcController>();
        if (npc == null || npc.isLeaving) return;

        /* 이미 Door 처리가 끝난 NPC면 무시 */
        if (npc.DoorProcessed) return;

        /* 플래그를 먼저 세워 두어 중복 호출 차단 */
        npc.DoorProcessed = true;

        /* ── 인원 제한 ── */
        if (insideCount >= maxInStore)
        {   npc.StartLeaving(exitPoint);  return; }

        /* ── 선반 빈 칸 예약 ── */
        if (!ShelfManager.Instance.TryGetAvailableSlot(out ShelfSlot slot))
        {   npc.StartLeaving(exitPoint);   return; }

        /* ── 입장 확정 ── */
        insideCount++;
        npc.SetDoor(this);
        npc.inStore = true;
        npc.AllowEntry(slot.transform, exitPoint);
    }
    
    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(TagNpc)) return;
        var npc = other.GetComponent<NpcController>();
        if (npc != null && npc.inStore && npc.isLeaving)
        {
            insideCount = Mathf.Max(0, insideCount - 1);
            npc.inStore = false;
        }
    }
}