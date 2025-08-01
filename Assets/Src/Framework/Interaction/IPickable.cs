/// <summary>
/// 박정민 25/8/1
/// 플레이어가 '잡을 수 있는' 오브젝트를 나타내는 인터페이스입니다.
/// Interact()는 PickUp()으로 연결되며, Drop()을 통해 놓을 수 있습니다.
/// </summary>
/// <remarks>
/// 구현 예시: 박스, 장비, 진열 가능한 아이템 등
/// </remarks>
public interface IPickable : IInteractable
{
    /// <summary>
    /// 오브젝트를 잡습니다.
    /// </summary>
    void PickUp();

    /// <summary>
    /// 오브젝트를 놓습니다.
    /// </summary>
    void SetDown();

    void ThrowObject();
}