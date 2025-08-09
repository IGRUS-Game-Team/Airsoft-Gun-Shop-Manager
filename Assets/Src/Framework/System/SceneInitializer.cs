using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneInitializer : MonoBehaviour
{
    [SerializeField] GameObject loadingUI;
    //[SerializeField] Slider progressBar; // optional

    private void Start()
    {
        StartCoroutine(InitializeScene());
    }

    IEnumerator InitializeScene()
    {
        loadingUI.SetActive(true);
        yield return null; // 한 프레임 쉬고 로딩 UI 먼저 뜨게 함

        yield return StartCoroutine(InitializeManagers());

        yield return new WaitForSeconds(0.3f); // 연출용 딜레이
        loadingUI.SetActive(false);
    }

    IEnumerator InitializeManagers()
    {
        // FindObjectOfType 또는 수동 참조
        //SaveManager.Instance.Init(); 
        //MonitorShopCartManager cart = FindFirstObjectByType<MonitorShopCartManager>();
        // PlayerController player = FindObjectOfType<PlayerController>();
        // RoomManager room = FindObjectOfType<RoomManager>();

        // 불러오기 등 작업 처리
        // cart.LoadCartData(ES3.Load(...));
        // player.transform.position = ES3.Load<Vector3>("playerPosition");
        // room.LoadUnlockedRooms(ES3.Load(...));

        // 진행률 연출 (선택사항)
        // for (int i = 0; i <= 100; i += 10)
        // {
        //     progressBar.value = i / 100f;
        //     yield return new WaitForSeconds(0.02f);
        // }

        yield return null;
    }
}