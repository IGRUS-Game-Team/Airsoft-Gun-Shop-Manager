using RootMotion.FinalIK;
using UnityEngine;

public class NpcIKSystem : MonoBehaviour
{
    [SerializeField] private FullBodyBipedIK fbbik;

    public void ApplyWeaponTargets(WeaponIKTargets targets)
    {
        if (fbbik == null) return;

        if (targets.leftHandTarget != null)
        {
            fbbik.solver.leftHandEffector.target = targets.leftHandTarget;
            fbbik.solver.leftHandEffector.positionWeight = 1f;   // 위치만 적용
            fbbik.solver.leftHandEffector.rotationWeight = 0f;   // 회전은 무시
            fbbik.solver.leftHandEffector.maintainRelativePositionWeight = 0f; // 상대 위치 유지 안 함
        }

        // 오른손은 아예 안 씀
        fbbik.solver.rightHandEffector.positionWeight = 0f;
        fbbik.solver.rightHandEffector.rotationWeight = 0f;
    }
    
    public void DisableIK()
    {
        if (fbbik == null) return;

        fbbik.solver.leftHandEffector.positionWeight = 0f;
        fbbik.solver.leftHandEffector.rotationWeight = 0f;
        fbbik.solver.rightHandEffector.positionWeight = 0f;
        fbbik.solver.rightHandEffector.rotationWeight = 0f;
    }
}