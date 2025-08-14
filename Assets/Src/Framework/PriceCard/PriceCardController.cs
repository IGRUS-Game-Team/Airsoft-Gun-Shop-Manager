using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// 장지원 8.7 가격표 컨트롤러
/// 가격표 클릭시 UI 호출
/// </summary>
public class PriceCardController : MonoBehaviour, IInteractable, IPriceChangeable
{
    [Header("가격 세팅창")]
    [SerializeField] GameObject setting;



    [Header("텍스트 요소들")]
    [SerializeField] TextMeshProUGUI price;
    [SerializeField] TextMeshProUGUI productname;
    [SerializeField] TextMeshProUGUI left;
    private ItemData currentItemData;
    private PriceObserver priceObserver;
    private PriceSettingController priceSettingController;
    private ShelfSlot shelfSlot;

    void Start()
    {
        shelfSlot = GetComponentInParent<ShelfSlot>();
        priceObserver = FindFirstObjectByType<PriceObserver>();
        priceSettingController = GetComponentInChildren<PriceSettingController>(true);
        
    }
    public void Interact()//클릭하면 Ui 호출 -> OnMOuseDown 말고
    {
        if (RaycastDetector.Instance.HitObject == this.gameObject)
        //레이케스트에 가격표 == 이 스크립트가 들어있는 오브젝트
        {
            if (!shelfSlot.HasItem)
            {//슬롯 아이템 개수 = 0
                Destroy(this.gameObject);
                return;
            }

            priceSettingController.ReceiveId(currentItemData.itemId); //id를 먼저 주기

            setting.SetActive(true); //왜 갑자기 안나던 오류가 여기서 나오는거지?

            ClickObjectUIManager.Instance.OpenUI(setting);
            priceSettingController.GetScriptableObject(currentItemData);
        }

        
    }

    //사람이 input변경하면 가격이 변한다
    //Okay버튼과 이어주기
    public void UpdatePrice()
    {
        Debug.Log("현재 옵저버 price " + priceObserver.GetPrice(currentItemData.itemId).ToString());
        string priceString = priceObserver.GetPrice(currentItemData.itemId).ToString();
        //옵저버 값 가져오기
        price.text = "$  " + (priceString == "" ? "0.00" : priceString);
    }

    public void UpdatePrice(ItemData itemData)//팩토리랑 연결
    {
        price.text = "$  " + itemData.baseCost.ToString();
        currentItemData = itemData;
        Debug.Log("updatePrice : "+currentItemData.itemId);
    }
    public void UpdateName(ItemData itemData)
    {
        productname.text = itemData.name;
    }


    //옵저버에서 값 받음
    public void OnPriceChanged(int itemId, float newPrice, float oldPrice)
    {
        price.text = "$  " + newPrice.ToString();
    }



    
}
