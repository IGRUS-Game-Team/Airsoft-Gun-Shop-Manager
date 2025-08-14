using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 장지원 8.13
/// 
/// 전략을 선택하고
/// 선택한 전략의 정보를 다른 스크립트에 전해
/// 변경사항을 적용하도록 하는 중앙제어 스크립트
/// 
/// >>> itemdatabase에 id를 순서대로 놓아야할듯? 이유는 졸려서 기억이안나..
/// </summary>
public class SocialEventManager : MonoBehaviour
{
    public static SocialEventManager Instance { get; private set; }//싱글톤

    //상품 목록 가져오기
    [Header("ItemDatabase 상품 데이터베이스")]
    [SerializeField] private ItemDatabase itemDatabase;


    //private MarketPriceDataManager marketPriceDataManager;


    //전략들 저장
    private ISocialEventStrategy currentStrategy; //인스턴스 참조
    private int itemId;  //상품 id -> 시장 변동률 적용할 상품
    private string itemName; //상품 이름
    private ItemData selectedItemData; //아이템 so


    // 옵저버 패턴을 위한 이벤트들
    public static event Action<int, float> OnMarketPriceChanged;  // id, 시장변동률
    public static event Action<string, string, string> OnEventUIUpdate; // 이벤트이름, 상태, 아이템이름

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //marketPriceDataManager = FindFirstObjectByType<MarketPriceDataManager>();
        }
        else
        {
            Destroy(gameObject);
        }
    }


    //전략 선택적으로 바꾸기
    public void SetSocialEventStrategy(ISocialEventStrategy Isocialevent)
    {
        this.currentStrategy = Isocialevent; //가져온 전략을 본 ScialEventManager에 저장하는 것
    }

    //전략 실행 및 데이터 전달
    public void ExecuteStrategy()
    {
        if (currentStrategy == null)
        {
            Debug.Log("전략 설정 안됨");
            return;
        }

        currentStrategy.GetEventStrategyData(); // 전략에서 랜덤 데이터 생성

        SelectRandomGun(); //사회이벤트 매니저에 랜덤 상품 저장

        //저장한것을 넘겨줘야함
        //근데 옵저버의 순서를 고려해서 내부로직인 marketpricemanager에게 줄 : 시장변동률, id
        //ui에게 그 다음으로 줄 id , 이벤트 이름, 이벤트 상태, 상품 이름 정도? 2갈래로 나뉠듯

        DeliverMarketPriceData();  // 먼저 시장 가격 업데이트
        DeliverEventUIData();      // 그 다음 UI 업데이트

    }

    // 시장 변동률 전달
    private void DeliverMarketPriceData()
    {
        if (selectedItemData != null)
        {
            
            OnMarketPriceChanged?.Invoke(selectedItemData.itemId, currentStrategy.MarketModifier);
            // // 직접 호출 방식
            // if (marketPriceDataManager != null)
            // {
            //     marketPriceDataManager.UpdateItemPrice(selectedItemData.itemId, currentStrategy.MarketModifier);
            // }

            // // Observer 패턴 방식 (다른 스크립트들이 구독 가능)

        }
    }

    //이벤트 이름, 상태, 무기 이름 전달
    private void DeliverEventUIData()
    {

        if (selectedItemData != null && currentStrategy != null)
        {
            string eventName = currentStrategy.EventName ?? "알 수 없는 이벤트";
            string eventStatus = currentStrategy.StatusText ?? "사회 상태?";
            string itemName = selectedItemData.itemName ?? "알 수 없는 상품";

            // 옵저버로 UI에 전달
            OnEventUIUpdate?.Invoke(eventName, eventStatus, itemName);
        }
    }


    //랜덤한 아이템 가져오기, 이름 아이디 저장
    private void SelectRandomGun() {
        selectedItemData = itemDatabase.GetRandomItemData(); //아이템 데이터에 저장
        itemName = selectedItemData.itemName;
        itemId = selectedItemData.itemId;
    }
    
    // 외부에서 현재 선택된 아이템 정보를 가져올 수 있는 메서드들
    public ItemData GetSelectedItem() => selectedItemData;
    public int GetSelectedItemId() => selectedItemData?.itemId ?? 0;
    public string GetSelectedItemName() => selectedItemData?.itemName ?? "없음";

}
