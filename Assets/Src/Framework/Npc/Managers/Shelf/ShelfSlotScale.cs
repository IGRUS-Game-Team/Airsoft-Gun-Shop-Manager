using UnityEngine;
/// <summary>
/// 진열장 크기 강제 조절
/// </summary>
public class ShelfSlotScale : MonoBehaviour
{
    [Header("크기 조절")]
    [SerializeField] private float scaleMultiplier = 1.2f;

    void Start()
    {
        AdjustScale();
        ResetColliderScale();
    }
    
    private void AdjustScale()
    {
        transform.localScale = Vector3.one * scaleMultiplier;
    }

    private void ResetColliderScale()
    {
        // 모든 콜라이더 찾기 (자신 + 자식들)
        Collider[] colliders = GetComponentsInChildren<Collider>();
        
        foreach (Collider col in colliders)
        {
            // 콜라이더의 크기를 역으로 계산해서 원래대로 만들기
            float reverseScale = 1f / scaleMultiplier;
            
            if (col is BoxCollider boxCol)
            {
                boxCol.size *= reverseScale;
            }
            else if (col is SphereCollider sphereCol)
            {
                sphereCol.radius *= reverseScale;
            }
            else if (col is CapsuleCollider capsuleCol)
            {
                capsuleCol.radius *= reverseScale;
                capsuleCol.height *= reverseScale;
            }
        }
    }
    
    // Inspector에서 실시간으로 테스트할 수 있는 메서드
    [ContextMenu("Apply Scale")]
    private void ApplyScale()
    {
        AdjustScale();
    }
}
