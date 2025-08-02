using UnityEngine.EventSystems;

public static class UIUtility
{
    /// <summary>
    /// 마우스가 현재 UI 요소 위에 있는지 여부를 반환합니다.
    /// </summary>
    public static bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}