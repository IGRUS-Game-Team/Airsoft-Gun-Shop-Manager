using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 장지원 8.14 시세 관리하는 곳
/// 
/// 시세 = 원가 *120% + 시장변동률
/// socialeventmanager에서 시장변동률을 가져와 시세 리스트에 적용한다.
/// </summary>
public class MarketPriceDataManager : MonoBehaviour
{

    public static MarketPriceDataManager Instance { get; private set; }//싱글톤

    [Header("ItemDatabase 상품 데이터베이스")]
    [SerializeField] private ItemDatabase itemDatabase;
    //시세 딕셔너리는 필요하다
    //모든 so의 원가를 가져와서 딕셔너리화 시켜야할까
    //아니면 database에서 빼오고 정제된 값을 시세 딕셔너리에 넣어야할까<<일단 이거 채택
    private Dictionary<int, float> currentMarketPrice = new Dictionary<int, float>(); //시세 저장 딕셔너리 : id, 시세

    void OnEnable()
    {
        SocialEventManager.OnMarketPriceChanged += ChangeMarketPrice;
    }
    void OnDisable()
    {
        SocialEventManager.OnMarketPriceChanged -= ChangeMarketPrice;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        InitializeMarketPrices();
    }



    //중앙제어에게 받은 아이템과 시장변동률을 바꾼다
    private void ChangeMarketPrice(int id, float marketPriceRate)
    {
        ItemData selectItem = itemDatabase.GetById(id);// itemDatabase에서 itemId 가 id인 데이터 찾기

        float marketPrice = (float)(selectItem.baseCost * (1.2 + marketPriceRate));//시세 생성

        //이걸 딕셔너리에 적용
        if (currentMarketPrice.ContainsKey(selectItem.itemId))//이미 존재하는 id라면
        {
            currentMarketPrice[selectItem.itemId] = marketPrice;
        }
        else
        {
            currentMarketPrice.Add(selectItem.itemId, marketPrice);//없다면 추가
        }

        //근데 드는 의문하도 idid이래놔서 이 아이디가 과연 다 같은 아이디인가 조심해야할듯?
    }

    //초기 시세 설정 원가 *1.2
    private void InitializeMarketPrices()
    {
        //딕셔너리에서 모든 itemdata를 순회하여 딕셔너리에 itemdata.id랑 , 정제된 시세 저장
        foreach (var item in itemDatabase.items)
        {
            if (item != null && item.itemId != 0)
            {
                float baseMarketPrice = item.baseCost * 1.2f;
                currentMarketPrice[item.itemId] = baseMarketPrice;
            }
        }
    }

    //딕셔너리의 시장변동 get
    public float GetMarketPrice(int id)
    {
        return currentMarketPrice[id];
    }

    // 추가 : id키 없을 때 에러나는 거 막기 - 준서
    public bool TryGetMarketPrice(int id, out float price)
    {
        if (currentMarketPrice.TryGetValue(id, out price))
            return true;

        var item = itemDatabase.GetById(id);
        if (item != null)
        {
            price = item.baseCost * 1.2f;      // 기본 시세 규칙
            currentMarketPrice[id] = price;    // 캐시
            return true;
        }

        price = 0f;
        return false;
    }

}
