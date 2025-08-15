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

    [Header("전략 확률 설정 (%)")]
    [SerializeField] private int[] strategyChances = { 50, 30, 20 }; // 배열로 관리

    //전략들 저장
    private ISocialEventStrategy currentStrategy; //인스턴스 참조
    private int itemId;  //상품 id -> 시장 변동률 적용할 상품
    private string itemName; //상품 이름
    private ItemData selectedItemData; //아이템 so


    // 옵저버 패턴을 위한 이벤트들
    public static event Action<int, float> OnMarketPriceChanged;  // id, 시장변동률
    public static event Action<string, string, string> OnEventUIUpdate; // 이벤트이름, 상태, 아이템이름
//------------------------------------------------------------------------------------------
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    
    //전략 실행 및 데이터 전달
    public void ExecuteStrategy()
    {
        SelectRandomStrategy();//전략 랜덤 선택
        if (currentStrategy == null)
        {
            Debug.Log("전략 설정 안됨");
            return;
        }
        Debug.Log(" ExecuteStrategy 시작");
        

        currentStrategy.GetEventStrategyData(); // 전략에서 랜덤 데이터 생성

        SelectRandomGun(); //사회이벤트 매니저에 랜덤 상품 저w장
        DeliverMarketPriceData();  // 먼저 시장 가격 업데이트
        DeliverEventUIData();      // 그 다음 UI 업데이트

    }

    // 시장 변동률 전달
    private void DeliverMarketPriceData()
    {
        if (selectedItemData != null)
            OnMarketPriceChanged?.Invoke(selectedItemData.itemId, currentStrategy.MarketModifier);
    }

    //이벤트 이름, 상태, 무기 이름 전달
    private void DeliverEventUIData()
    {
        if (selectedItemData != null && currentStrategy != null)
        {
            string eventName = currentStrategy.EventName ?? "알 수 없는 이벤트";
            string eventStatus = currentStrategy.StatusText ?? "사회 상태?";
            string itemName = ItemNameResolver.Get(selectedItemData)?? "알 수 없는 상품";

            // 옵저버로 UI에 전달
            OnEventUIUpdate?.Invoke(eventName, eventStatus, itemName);
        }
    }


    //랜덤한 아이템 가져오기, 이름 아이디 저장
    private void SelectRandomGun()
    {
        selectedItemData = itemDatabase.GetRandomItemData(); //아이템 데이터에 저장
        itemName = ItemNameResolver.Get(selectedItemData);
        itemId = selectedItemData.itemId;
        Debug.Log($"{selectedItemData} : {itemName} : {itemId}");
    }

    //전략 선택적으로 바꾸기
    public void SetSocialEventStrategy(ISocialEventStrategy Isocialevent)
    {
        this.currentStrategy = Isocialevent; //가져온 전략을 본 ScialEventManager에 저장하는 것
    }


    //""랜덤"" 전략 선택
    private void SelectRandomStrategy()
    {
        // 누적 확률 자동 계산
        int[] cumulativeChances = new int[strategyChances.Length];
        int total = 0;

        for (int i = 0; i < strategyChances.Length; i++)
        {
            total += strategyChances[i];
            cumulativeChances[i] = total;
        }

        int randomValue = UnityEngine.Random.Range(1, total + 1); // 1 ~ 총합

        // 선택된 전략 찾기
        for (int i = 0; i < cumulativeChances.Length; i++)
        {
            if (randomValue <= cumulativeChances[i])
            {
                currentStrategy = CreateStrategy(i); //선택한 전략 저장
                Debug.Log($"{currentStrategy.StatusText} 선택됨 (확률: {strategyChances[i]}%)");
                return;
            }
        }
    }


    //전략들 고르기
    private ISocialEventStrategy CreateStrategy(int index)
    {
        switch (index)
        {
            case 0: return new NormalEventStrategy();
            case 1: return new RecessionEventStrategy();
            case 2: return new BoomEventStrategy();
            default: return new NormalEventStrategy();
        }
    }

    
    // 외부에서 현재 선택된 아이템 정보를 가져올 수 있는 메서드들
    public ItemData GetSelectedItem() => selectedItemData;
    public int GetSelectedItemId() => selectedItemData?.itemId ?? 0;
    public string GetSelectedItemName() => ItemNameResolver.Get(selectedItemData) ?? "없음";

}
