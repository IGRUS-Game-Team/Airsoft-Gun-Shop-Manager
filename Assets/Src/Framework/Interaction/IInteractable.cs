/// <summary>
/// 박정민 25/8/1
/// 상호작용이 가능한 모든 오브젝트가 구현해야 하는 기본 인터페이스입니다.
/// 클릭 등 사용자 입력에 반응하여 특정 동작을 수행할 수 있도록 합니다.
/// 예: 박스 클릭 → 집기, 모니터 클릭 → UI 열기
/// </summary>
public interface IInteractable
{
    void Interact();
}
