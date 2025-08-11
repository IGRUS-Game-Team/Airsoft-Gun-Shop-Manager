using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// 장지원 8.7 가격표 컨트롤러
/// 가격표 클릭시 UI 호출
/// </summary>
public class PriceCardController : MonoBehaviour, IPriceChangeable
{
    [Header("가격 세팅창")]
    [SerializeField] GameObject setting;

    [Header("텍스트 요소들")]
    [SerializeField] TextMeshProUGUI price;
    [SerializeField] TextMeshProUGUI productname;
    [SerializeField] TextMeshProUGUI left;
    private float currentPrice;
    private float currentName;
    private float currentLeft;

    void OnEnable()//이름 업데이트
    {
        ShelfSlot.OnProductPlaced +=UpdateName;
    }

    void OnDisable()
    {
        ShelfSlot.OnProductPlaced -=UpdateName;
    }


    private void OnMouseDown()//클릭하면 Ui 호출 -> 이거 인터페이스로 바꾸라고?
    {
        if (RaycastDetector.Instance.HitObject == this.gameObject)
        //레이케스트에 가격표 == 이 스크립트가 들어있는 오브젝트
        {
            setting.SetActive(true);
            ClickObjectUIManager.Instance.OpenUI(setting);
        }
    }

    // //사람이 input변경하면 가격이 변한다
    // // setting창 Okay 버튼이랑 연결
    // public void UpdatePrice()
    // {
    //     price.text = PriceInputHandler.Instance.SendStirngPrice();
    // }
    private void UpdateName(ItemData itemData) {
        productname.text = itemData.name;
    }


    //옵저버에서 값 받음
    public void OnPriceChanged(int itemId, float newPrice, float oldPrice)
    {
        price.text = "$  "+newPrice.ToString();
    }

    
}
