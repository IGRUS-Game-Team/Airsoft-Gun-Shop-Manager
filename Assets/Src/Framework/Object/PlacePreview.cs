using UnityEngine;

/// <summary>
/// Set 가능한 위치에 초록/빨강 프리뷰를 띄워주는 유틸 스크립트입니다.
/// PlayerObjectSetController에서 호출하여 상태를 관리합니다.
/// </summary>
public class PlacePreview : MonoBehaviour
{
    [SerializeField] private PlayerObjectHoldController holdController;
    [SerializeField] private GameObject previewGreen;
    [SerializeField] private GameObject previewRed;
    [SerializeField] private float placeRange = 3f;
    [SerializeField] private Transform holdPoint;

    public bool CanPlace { get; private set; } = false;
    public Vector3 PreviewPosition { get; private set; }

    public void UpdatePreview()
    {
        Ray ray = new Ray(holdPoint.position, holdPoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, placeRange))
        {
            bool valid = hit.collider.CompareTag("SettableSurface");

            PreviewPosition = hit.point;
            CanPlace = valid;

            previewGreen.SetActive(valid);
            previewRed.SetActive(!valid);

            MatchSizeToHeldObject(); // 👈 먼저 사이즈 맞춤

            // 중심점이 바닥에 닿지 않도록 y 방향으로 size 절반만큼 올려줌
            var renderer = holdController.heldObject?.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                Vector3 size = renderer.bounds.size;
                Vector3 liftedPos = PreviewPosition + new Vector3(0, size.y / 2f, 0);
                previewGreen.transform.position = liftedPos;
                previewRed.transform.position = liftedPos;
            }
        }
        else
        {
            previewGreen.SetActive(false);
            previewRed.SetActive(false);
            CanPlace = false;
        }
    }

    public void Hide()
    {
        previewGreen.SetActive(false);
        previewRed.SetActive(false);
        CanPlace = false;
    }
    
    public void MatchSizeToHeldObject()
    {
        var held = holdController.heldObject;
        if (held == null) return;

        var renderer = held.GetComponentInChildren<Renderer>();
        if (renderer == null) return;

        Vector3 size = renderer.bounds.size;

        previewGreen.transform.localScale = size;
        previewRed.transform.localScale = size;
    }
}
