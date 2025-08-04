using UnityEngine;

// 선반의 “가로 칸(슬롯)”을 나타내는 컴포넌트.
// 예약 여부는 자신을 품고 있는 ShelfGroup이 전담한다.
[DisallowMultipleComponent]
public class ShelfSlot : MonoBehaviour
{
    // ① 같은 어셈블리(프로젝트) 안에서는 값을 넣을 수 있게 internal set
    public ShelfGroup ParentGroup { get; internal set; }

    // 상품이 하나라도 달려 있으면 true
    public bool HasItem => transform.childCount > 0;

    // --------― 기존 인터페이스 호환용 --------
    public bool IsReserved => ParentGroup != null && ParentGroup.IsReserved;

    public bool TryReserve()  => ParentGroup != null && ParentGroup.TryReserve();

    public void Release()     => ParentGroup?.Release();
    // ---------------------------------------

    private void Awake()
    {
        ParentGroup = GetComponentInParent<ShelfGroup>(true);
        if (ParentGroup == null)
            Debug.LogWarning($"{name} : 부모 ShelfGroup 없음!");
    }
}