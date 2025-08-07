using UnityEngine;

/// <summary>
/// ColliderAuto.cs 박정민
/// 박스 콜라이더 자동으로 씌워주는 스크립트
/// 콜라이더 쓰고자 하는 오브젝트에 컴포넌트로 넣고 ...누른 후 reset누르면 적용
/// </summary>
[ExecuteAlways] // 에디터에서도 작동
public class ColliderAuto : MonoBehaviour
{
    void Reset()
    {
        UpdateCollider();
    }

    [ContextMenu("Update Collider")]
    public void UpdateCollider()
    {
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();

        if (childRenderers.Length == 0)
        {
            Debug.LogWarning("자식 오브젝트에 Renderer가 없습니다.");
            return;
        }

        Bounds bounds = childRenderers[0].bounds;
        for (int i = 1; i < childRenderers.Length; i++)
        {
            bounds.Encapsulate(childRenderers[i].bounds);
        }

        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
        {
            box = gameObject.AddComponent<BoxCollider>();
        }

        box.center = transform.InverseTransformPoint(bounds.center);
        box.size = transform.InverseTransformVector(bounds.size);
    }
}
