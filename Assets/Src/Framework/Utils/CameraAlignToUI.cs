using UnityEngine;

/// <summary>
/// UI Canvas를 바라보는 방향으로 카메라를 자동 정렬합니다.
/// </summary>
public class CameraAlignToUI : MonoBehaviour
{
    [SerializeField] private Transform uiCanvas;  // UI 캔버스 Transform
    [SerializeField] private float distance = 2f; // 카메라와 UI 사이 거리

    void Start()
    {
        if (uiCanvas == null)
        {
            Debug.LogWarning("UI Canvas가 지정되지 않았습니다.");
            return;
        }

        // UI 캔버스 정면 방향 기준으로 카메라 위치 계산
        Vector3 direction = -uiCanvas.forward; // 캔버스 뒤쪽에서 바라보게
        transform.position = uiCanvas.position + direction * distance;

        // UI 정면을 바라보도록 회전
        transform.rotation = Quaternion.LookRotation(uiCanvas.position - transform.position);
    }
}
