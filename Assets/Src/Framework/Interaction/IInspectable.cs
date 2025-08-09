/// <summary>
/// 박정민 25/8/1
/// 플레이어가 '상세보기' 또는 'UI 진입'할 수 있는 오브젝트를 나타내는 인터페이스입니다.
/// Interact()는 EnterInspection()으로 연결되며, ExitInspection()으로 UI를 종료할 수 있습니다.
/// </summary>
/// <remarks>
/// 구현 예시: 모니터, 키오스크, 전자 장치 등
/// </remarks>
public interface IInspectable : IInteractable
{
    /// <summary>
    /// UI 또는 인터페이스 모드에 진입합니다.
    /// </summary>
    void EnterInspection();

    /// <summary>
    /// UI 또는 인터페이스 모드에서 나옵니다.
    /// </summary>
    void ExitInspection();
}