using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// 가격표 클릭시 UI 호출
/// </summary>
public class PriceCardController : MonoBehaviour
{
    [SerializeField] GameObject setting;
    
    private void OnMouseDown()
    {
        if (RaycastDetector.Instance.HitObject == this.gameObject)
        //레이케스트에 가격표 == 이 스크립트가 들어있는 오브젝트
        {
            Debug.Log("dd");
            setting.SetActive(true);
            ClickObjectUIManager.Instance.OpenUI(setting);
        }
    }
}
