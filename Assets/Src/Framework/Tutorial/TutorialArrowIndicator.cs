using UnityEngine;

/// <summary>
/// 화면 위 2D 화살표로 목표 Transform 방향을 가리킨다.
/// 목표가 화면 안이고 가까우면 숨기고, 화면 밖이면 가장자리에 회전해서 표시한다.
/// </summary>
public class TutorialArrowIndicator : MonoBehaviour
{
    [SerializeField] RectTransform arrowUI;   // 화살표 이미지의 RectTransform
    [SerializeField] float edgeOffset = 80f;  // 화면 가장자리 여백(px)
    [SerializeField] float hideDistance = 1f;  // 이 거리 이내면 화살표 숨김

    Transform target;
    Camera cam;

    public void SetTarget(Transform t)
    {
        target = t;
        if (arrowUI != null) arrowUI.gameObject.SetActive(t != null);
    }

    public void ClearTarget()
    {
        target = null;
        if (arrowUI != null) arrowUI.gameObject.SetActive(false);
    }

    void Awake()
    {
        if (arrowUI != null) arrowUI.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (arrowUI == null || target == null)
        {
            if (arrowUI != null) arrowUI.gameObject.SetActive(false);
            return;
        }

        // 메인 카메라 캐시
        if (cam == null) cam = Camera.main;
        if (cam == null) { arrowUI.gameObject.SetActive(false); return; }

        Vector3 targetPos = target.position;
        float dist = Vector3.Distance(cam.transform.position, targetPos);

        // 가까우면 숨김
        if (dist < hideDistance)
        {
            arrowUI.gameObject.SetActive(false);
            return;
        }

        // 가격표 UI가 열려 있으면 화살표 숨김 (UI 가림 방지)
        if (PriceCardController.IsAnyPriceUIOpen)
        {
            arrowUI.gameObject.SetActive(false);
            return;
        }

        Vector3 vp = cam.WorldToViewportPoint(targetPos);
        bool behind = vp.z < 0f;
        bool onScreen = !behind && vp.x > 0f && vp.x < 1f && vp.y > 0f && vp.y < 1f;

        arrowUI.gameObject.SetActive(true);

        // 부모 Canvas의 RectTransform 사이즈
        RectTransform canvasRect = arrowUI.parent as RectTransform;
        if (canvasRect == null) canvasRect = arrowUI;
        Vector2 canvasSize = canvasRect.rect.size;

        if (onScreen)
        {
            // 화면 안 → 타겟 위에 표시
            Vector2 screenPos = new Vector2(vp.x * canvasSize.x, vp.y * canvasSize.y);
            // 약간 위에
            screenPos.y += 40f;
            arrowUI.anchoredPosition = screenPos - canvasSize * 0.5f;
            arrowUI.localRotation = Quaternion.Euler(0, 0, -90f); // 아래를 가리킴
        }
        else
        {
            // 화면 밖 or 뒤 → 가장자리에 클램프
            Vector2 dir;
            if (behind)
            {
                // 뒤에 있으면 반전
                dir = new Vector2(0.5f - vp.x, 0.5f - vp.y);
            }
            else
            {
                dir = new Vector2(vp.x - 0.5f, vp.y - 0.5f);
            }
            dir.Normalize();

            float halfW = canvasSize.x * 0.5f - edgeOffset;
            float halfH = canvasSize.y * 0.5f - edgeOffset;

            // 방향 벡터를 화면 가장자리까지 스케일
            float scale = Mathf.Min(
                dir.x != 0 ? Mathf.Abs(halfW / dir.x) : float.MaxValue,
                dir.y != 0 ? Mathf.Abs(halfH / dir.y) : float.MaxValue
            );

            Vector2 edgePos = dir * scale;
            arrowUI.anchoredPosition = edgePos;

            // 화살표 회전 (오른쪽을 가리키는 스프라이트 기준)
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            arrowUI.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
