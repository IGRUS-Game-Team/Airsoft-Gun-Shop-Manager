using System.Collections;
using UnityEngine;

[RequireComponent(typeof(NpcController))]
public class PickAnimEvents : MonoBehaviour
{
    [SerializeField] Transform handSocket;   // 비워 두면 NPC 소켓을 자동 사용

    NpcController npc;
    GameObject    grabbedItem;

    void Awake()
    {
        npc        = GetComponent<NpcController>();
        if (handSocket == null) handSocket = npc.HandTransform;
    }

    // ── 1) 손이 상품에 닿는 프레임 ───────────────────────────────
    // 애니메이션 이벤트 이름: OnGrabReached
    public void OnGrabReached()
    {
        Transform slotTf = npc.targetShelfSlot;
        if (slotTf == null) return;

        var slot = slotTf.GetComponent<ShelfSlot>();
        if (slot == null || slot.HasItem == false) return;

        grabbedItem = slot.PopItem();                 // 슬롯에서 분리
        if (grabbedItem == null) return;

        // 손에 붙여 한두 프레임만 보이도록
        grabbedItem.transform.SetParent(handSocket, false);
        grabbedItem.transform.localPosition = Vector3.zero;

        npc.heldItem      = grabbedItem;              // NPC도 기억
        npc.hasItemInHand = true;

        // **안전장치**: 1프레임 뒤에 무조건 숨김
        StartCoroutine(HideNextFrame());
    }

    // ── 2) 팔이 완전히 돌아온 프레임 (선택 이벤트) ──────────────
    // 애니메이션 이벤트 이름: OnGrabFinished (있으면 호출)
    public void OnGrabFinished()
    {
        HideItem();                                   // 중복 호출돼도 안전
    }

    // ── 3) 1프레임 딜레이 뒤 숨기기 ────────────────────────────
    IEnumerator HideNextFrame()
    {
        yield return null;                            // 다음 Render Frame
        HideItem();
    }

    void HideItem()
    {
        if (grabbedItem == null) return;
        grabbedItem.SetActive(false);                 // 화면에서 제거
        grabbedItem = null;
    }
}