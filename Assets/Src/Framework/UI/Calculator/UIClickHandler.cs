using UnityEngine;
using UnityEngine.EventSystems;

public class UIClickHandler : MonoBehaviour
{
     void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // UI 클릭 체크를 먼저 실행
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("UI 클릭됨 - 3D raycast 무시");
                return; // UI가 클릭되면 3D raycast 실행하지 않음
            }
        }
    }
}
