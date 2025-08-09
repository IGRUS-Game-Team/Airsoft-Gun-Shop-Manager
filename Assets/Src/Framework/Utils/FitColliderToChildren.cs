using UnityEngine;

/// <summary>
/// 이 스크립트는 부모 오브젝트(Box1)의 BoxCollider를
/// 자식들의 Renderer bounds에 맞게 자동으로 설정해줍니다.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class FitColliderToChildren : MonoBehaviour
{
    private void Start()
    {
        ResizeColliderToFitChildren();
    }

    public void ResizeColliderToFitChildren()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            Debug.LogWarning("하위 렌더러가 없습니다.");
            return;
        }

        Bounds combinedBounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            combinedBounds.Encapsulate(renderers[i].bounds);
        }

        BoxCollider boxCol = GetComponent<BoxCollider>();
        Vector3 localCenter = transform.InverseTransformPoint(combinedBounds.center);
        Vector3 localSize = transform.InverseTransformVector(combinedBounds.size);

        boxCol.center = localCenter;
        boxCol.size = localSize;
    }
}
