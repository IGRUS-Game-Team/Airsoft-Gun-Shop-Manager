using TMPro;
using UnityEngine;
/// <summary>
/// 8.7 장지원 가격 지정란
/// 
/// 가격을 적으면 그것을 보내는 역할을 한다.
/// </summary>
public class PriceInputHandler : MonoBehaviour
{

    public TMP_InputField inputValue;
    private PriceSettingController priceSettingController;
    private PriceObserver priceObserver;
    private int itemId;
    private void Start()
    {
        priceObserver = FindFirstObjectByType<PriceObserver>();
        priceSettingController = GetComponentInParent<PriceSettingController>();
    }

    // 지정한 값을 float으로 관찰자에게 보내기
    // 가격표, 모니터의 가격관리 화면에 이용
    public float SendFloatPrice()
    {//string -> float 변경
        bool isSuccess = float.TryParse(inputValue.text, out float inputPrice);


        if (!isSuccess)
        {
            Debug.Log("가격 string -> float 변환 실패");
            return 0;
        }
        itemId = priceSettingController.SendItemId();
        priceObserver.UpdatePrice(itemId, inputPrice);//<<옵저버 item id를 어디서 가져오지 카드한테?
        return inputPrice;
    }


}
