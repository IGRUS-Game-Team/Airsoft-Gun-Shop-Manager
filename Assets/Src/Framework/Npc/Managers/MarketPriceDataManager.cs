using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 장지원 8.14 시세 관리하는 곳
/// 
/// 시세 = 원가 *20% + 시장변동률
/// socialeventmanager에서 시장변동률을 가져와 시세 리스트에 적용한다.
/// </summary>
public class MarketPriceDataManager : MonoBehaviour
{

    public static MarketPriceDataManager Instance { get; private set; }//싱글톤

    [Header("ItemDatabase 상품 데이터베이스")]
    [SerializeField] private ItemDatabase itemDatabase;
    
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

    //딕셔너리의 시세 get
    public float GetMarketPrice(int id)
    {
        return currentMarketPrice[id];
    }
    

}
