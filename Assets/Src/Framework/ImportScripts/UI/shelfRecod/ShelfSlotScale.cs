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
    }
    
    private void AdjustScale()
    {
        transform.localScale = Vector3.one * scaleMultiplier;
    }
    
    // Inspector에서 실시간으로 테스트할 수 있는 메서드
    [ContextMenu("Apply Scale")]
    private void ApplyScale()
    {
        AdjustScale();
    }
}
