using TMPro;
using UnityEngine;
/// <summary>
/// 8.7 장지원 가격 지정란
/// 
/// 가격을 적으면 그것을 보내는 역할을 한다.
/// </summary>
public class PriceInputHandler : MonoBehaviour
{
    public static PriceInputHandler Instance { get; private set; }
    public TMP_InputField inputValue;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // 지정한 값을 float으로 보내기
    // 가격표, 모니터의 가격관리 화면에 이용
    public float SendFloatPrice()
    {//string -> float 변경
        bool isSuccess = float.TryParse(inputValue.text, out float inputPrice);

        if (!isSuccess)
        {
            Debug.Log("가격 string -> float 변환 실패");
            return 0;
        }

        return inputPrice;
    }

    public string SendStirngPrice()
    {
        return inputValue.text;
    }

}
