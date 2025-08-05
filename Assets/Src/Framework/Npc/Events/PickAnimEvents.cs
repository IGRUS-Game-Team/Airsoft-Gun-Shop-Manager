using System.Collections;
using UnityEngine;

[RequireComponent(typeof(NpcController))]
[RequireComponent(typeof(CarriedItemHandler))]   // 운반 담당 컴포넌트
public class PickAnimEvents : MonoBehaviour
{
    NpcController       npc;
    CarriedItemHandler  carrier;      // 손‧숨김‧카운터 배치 담당

    void Awake()
    {
        npc     = GetComponent<NpcController>();
        carrier = GetComponent<CarriedItemHandler>();
    }

    // ── 1) 손이 상품에 닿는 프레임 ───────────────────────────────
    // (애니메이션 클립 이벤트: OnGrabReached)
    public void OnGrabReached()
    {
        Transform slotTf = npc.targetShelfSlot;
        if (slotTf == null) return;

        var slot = slotTf.GetComponent<ShelfSlot>();
        if (slot == null || slot.HasItem == false) return;

        GameObject item = slot.PopItem();           // 슬롯에서 분리
        if (item == null) return;

        carrier.Attach(item);                       // ← 손에 부착·잠깐 노출
        npc.heldItem      = item;                   // NPC도 기억
        npc.hasItemInHand = true;

        // **안전장치**: 1프레임 뒤에 무조건 숨기기
        StartCoroutine(HideNextFrame());
    }

    // ── 2) 팔이 완전히 돌아온 프레임 (선택 이벤트) ──────────────
    // (애니메이션 클립 이벤트: OnGrabFinished)
    public void OnGrabFinished()
    {
        carrier.Hide();                             // 중복 호출돼도 안전
    }

    // ── 3) 1프레임 딜레이 뒤 숨기기 ────────────────────────────
    IEnumerator HideNextFrame()
    {
        yield return null;                          // 다음 Render Frame
        carrier.Hide();
    }
}