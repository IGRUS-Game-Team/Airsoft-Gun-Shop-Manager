using UnityEngine;

public class InputApplier : MonoBehaviour
{
    // 프로젝트 상황에 맞게 마우스 처리/감도 적용
    // (예: 카메라 컨트롤 스크립트에 전달)
    [Header("Optional camera target")]
    public Camera playerCamera;

    public void Apply(SettingsData d)
    {
        // 예시: 마우스 감도 전달(사용자 카메라 컨트롤러 스크립트가 있다면 그쪽으로 넘기세요)
        var controller = FindFirstObjectByType<MonoBehaviour>(); // placeholder
        // TODO: 실제 컨트롤러에 d.mouseSensitivity, d.invertY 적용

        // 패드 진동은 실제 게임패드 래퍼에서 사용
        // TODO: 진동 on/off 전달
    }
}
