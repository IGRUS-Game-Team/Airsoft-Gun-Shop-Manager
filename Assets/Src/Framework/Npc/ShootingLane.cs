using UnityEngine;

public enum RotationOrder { XYZ, XZY, YXZ, YZX, ZXY, ZYX }

public class ShootingLane : MonoBehaviour
{
    [Header("NPC가 서는 바닥 점")]
    public Transform standPoint;

    [Header("정면 기준(없으면 이 오브젝트의 forward)")]
    public Transform lookTarget;

    [Header("이 레인에서 NPC가 들 총기 프리팹(없으면 손 빈 채로)")]
    public GameObject weaponPrefab;

    [Header("HandSocket 기준 부착 오프셋 (현장감 맞춰 조절)")]
    public Vector3 handLocalPosition;
    public Vector3 handLocalEuler;
    public Vector3 handLocalScale = Vector3.one;
    public RotationOrder rotationOrder = RotationOrder.YXZ; // yaw→pitch→roll 추천

    [Header("레인별 사격 애니메이션 설정")]
    [Tooltip("이 레인에서 재생할 애니메이터 스테이트 이름 (Trigger가 비어있을 때 사용)")]
    public string shootStateName = "";

    [Tooltip("이 값이 비어있지 않으면 Trigger를 SetTrigger하여 전이")]
    public string shootTriggerName = "";

    [Tooltip("애니메이션을 얼마나 재생할지(초). 0 이하이면 클립 길이에 의존")]
    public float shootDuration = 2.5f;

    [Tooltip("선택: 이 레인에서만 쓰는 AnimatorOverrideController")]
    public AnimatorOverrideController overrideController;

    [Header("루프 사이 텀(Looped State용)")]
    [SerializeField, Min(0f)] public float loopPauseSeconds = 0.3f;

    private NpcController occupant;

    public bool IsOccupied => occupant != null;
    public Vector3 StandPosition => standPoint ? standPoint.position : transform.position;
    public Vector3 Forward =>
        lookTarget ? (lookTarget.position - StandPosition).normalized : transform.forward;

    public bool TryReserve(NpcController npc)
    {
        if (IsOccupied) return false;
        occupant = npc;
        return true;
    }

    public void Release(NpcController npc = null)
    {
        if (npc != null && npc != occupant) return;
        occupant = null;
    }
}