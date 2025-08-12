// 2025-08-13 이준서
// 목적: complain 애니메이션 클립에 Animation Event(예: 첫 프레임)에 연결해서
//       SettlementManager에 "불평 발생"을 통지. CounterManager 수정 불필요.

using UnityEngine;

public class NpcComplainAlerterEvents : MonoBehaviour
{
    NpcController npc;

    void Awake() { npc = GetComponentInParent<NpcController>(); }

    // 애니메이션 이벤트에서 호출 이름: OnComplainAnim
    public void OnComplainAnim()
    {
        SettlementManager.Instance?.MarkNpcComplained(npc);
    }
}