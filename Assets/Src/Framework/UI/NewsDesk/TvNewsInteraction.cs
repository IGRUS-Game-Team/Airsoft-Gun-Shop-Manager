using UnityEngine;

/// <summary>
/// TV 오브젝트에 붙이는 스크립트.
/// 클릭하면 오늘의 뉴스를 다시 볼 수 있다.
/// 이벤트가 없는 날("Day")이면 아무 일도 일어나지 않는다.
/// </summary>
public class TvNewsInteraction : MonoBehaviour, IInteractable
{
    private NewsDeskLoader loader;

    private void Start()
    {
        loader = FindObjectOfType<NewsDeskLoader>();
    }

    public void Interact()
    {
        if (loader == null)
        {
            loader = FindObjectOfType<NewsDeskLoader>();
            if (loader == null) return;
        }

        if (string.IsNullOrEmpty(loader.LastEventName))
        {
            Debug.Log("[TvNews] 오늘은 뉴스가 없습니다.");
            return;
        }

        loader.TriggerNews(loader.LastEventName);
    }
}
