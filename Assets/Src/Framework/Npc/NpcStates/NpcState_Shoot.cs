using UnityEngine;

public class NpcState_Shoot : IState
{
    private readonly NpcController npc;

    // 전체 체류 시간(애니+중간 텀 포함)
    private float totalRemaining;

    // 루프 사이 텀 제어
    private bool  isPausing;
    private float pauseRemaining;
    private float savedAnimatorSpeed = 1f;
    private int   lastLoopIndex; // normalizedTime의 정수부
    private float prevLoopFrac = -1f;

    private Transform weaponTf;
    private RuntimeAnimatorController originalController;

    public NpcState_Shoot(NpcController npc) { this.npc = npc; }

    public void Enter()
    {
        var lane = npc.TargetLane;
        var anim = npc.Animator;
        if (lane == null || anim == null) { npc.StartLeaving(npc.exitPoint); return; }

        // 무기 장착
        TrySpawnAndAttachWeapon();

        // (선택) 오버라이드
        originalController = anim.runtimeAnimatorController;
        if (lane.overrideController) anim.runtimeAnimatorController = lane.overrideController;

        // 사격 애니메이션 시작 (해당 State는 Loop On 권장)
        if (!string.IsNullOrEmpty(lane.shootTriggerName))
        {
            anim.ResetTrigger(lane.shootTriggerName);
            anim.SetTrigger(lane.shootTriggerName);
        }
        else if (!string.IsNullOrEmpty(lane.shootStateName))
        {
            anim.CrossFade(Animator.StringToHash(lane.shootStateName), 0.05f, 0, 0f);
        }

        // 전체 체류 시간(초) — 0이면 기본값
        totalRemaining = (lane.shootDuration > 0f ? lane.shootDuration : 3.0f);

        // 루프 감지 초기화
        lastLoopIndex  = 0;
        isPausing      = false;
        pauseRemaining = 0f;

        weaponTf = npc.heldItem ? npc.heldItem.transform : null;
        ApplyOffset(); // 손 오프셋 즉시 반영
    }

    public void Tick()
    {
        ApplyOffset();

        var lane = npc.TargetLane;
        var anim = npc.Animator;
        if (!anim) return;

        // ★ 사격장 1회 이용 수입 반영 (Inspector 금액 사용)
        SettlementManager.Instance?.RegisterShootingRangeUse();

        const int layer = 0; // ← Shoot가 레이어1(상체)면 1로 바꿔!
        var info = anim.IsInTransition(layer)
            ? anim.GetNextAnimatorStateInfo(layer)
            : anim.GetCurrentAnimatorStateInfo(layer);

        // 전체 체류 시간
        totalRemaining -= Time.deltaTime;
        if (totalRemaining <= 0f)
        {
            CleanupHeldWeapon();
            RestoreAnimator();
            npc.ReleaseShootingLane();
            npc.StartLeaving(npc.exitPoint);
            return;
        }

        // 루프 경계 감지: 분수부가 커졌다가 작아지면(0.98→0.02) 경계 통과
        float frac = info.normalizedTime - Mathf.Floor(info.normalizedTime);

        // 첫 프레임 초기화
        if (prevLoopFrac < -0.5f)
        {
            prevLoopFrac = frac;
            return;
        }

        // 전이로 값이 튀는 노이즈 여유
        bool crossed = !isPausing && lane.loopPauseSeconds > 0f && (frac + 0.01f) < prevLoopFrac;

        if (crossed)
        {
            // 잠깐 멈춤
            savedAnimatorSpeed = anim.speed;
            anim.speed = 0f;
            isPausing = true;
            pauseRemaining = lane.loopPauseSeconds;
        }

        if (isPausing)
        {
            pauseRemaining -= Time.deltaTime;
            if (pauseRemaining <= 0f)
            {
                // 재개
                anim.speed = savedAnimatorSpeed <= 0f ? 1f : savedAnimatorSpeed;
                isPausing = false;
                // 같은 경계에서 재트리거 방지
                prevLoopFrac = frac;
            }
            return;
        }

        // 다음 프레임 비교 대비
        prevLoopFrac = frac;
    }

    public void Exit()
    {
        CleanupHeldWeapon();
        RestoreAnimator();
    }

    // ─ 내부 헬퍼 ───────────────────────────────────────
    private void TrySpawnAndAttachWeapon()
    {
        var lane = npc.TargetLane; var socket = npc.HandTransform;
        if (!lane || !socket) return;

        CleanupHeldWeapon();
        if (!lane.weaponPrefab) return;

        var go = Object.Instantiate(lane.weaponPrefab);
        PrepareAsProp(go);
        go.transform.SetParent(socket, false);

        npc.heldItem = go;
        npc.hasItemInHand = true;
        weaponTf = go.transform;
    }

    private void ApplyOffset()
    {
        if (weaponTf == null || npc.TargetLane == null) return;
        var lane = npc.TargetLane;
        weaponTf.localPosition = lane.handLocalPosition;
        weaponTf.localRotation = BuildRotation(lane.handLocalEuler, lane.rotationOrder);
        weaponTf.localScale    = lane.handLocalScale;
    }

    private void CleanupHeldWeapon()
    {
        if (npc.heldItem) Object.Destroy(npc.heldItem);
        npc.heldItem = null;
        npc.hasItemInHand = false;
        weaponTf = null;
    }

    private void RestoreAnimator()
    {
        if (npc.Animator)
        {
            npc.Animator.speed = 1f;
            if (originalController) npc.Animator.runtimeAnimatorController = originalController;
        }
        originalController = null;
    }

    private static void PrepareAsProp(GameObject go)
    {
        if (!go) return;
        foreach (var rb in go.GetComponentsInChildren<Rigidbody>(true))
        { rb.isKinematic = true; rb.detectCollisions = false; }
        foreach (var c in go.GetComponentsInChildren<Collider>(true))
        { c.enabled = false; }
    }

    // 회전 순서 유틸 (이전 답과 동일)
    private static Quaternion BuildRotation(Vector3 euler, RotationOrder order)
    {
        var rx = Quaternion.AngleAxis(euler.x, Vector3.right);
        var ry = Quaternion.AngleAxis(euler.y, Vector3.up);
        var rz = Quaternion.AngleAxis(euler.z, Vector3.forward);
        switch (order)
        {
            case RotationOrder.XYZ: return rz * ry * rx;
            case RotationOrder.XZY: return ry * rz * rx;
            case RotationOrder.YXZ: return rz * rx * ry; // 추천
            case RotationOrder.YZX: return rx * rz * ry;
            case RotationOrder.ZXY: return ry * rx * rz;
            case RotationOrder.ZYX: return rx * ry * rz;
            default: return rz * rx * ry;
        }
    }
}