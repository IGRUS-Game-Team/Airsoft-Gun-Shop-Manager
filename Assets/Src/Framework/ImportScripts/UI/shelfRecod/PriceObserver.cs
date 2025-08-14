using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// PriceObserver.UpdatePrice() 호출하면 구독한 모든 관찰자들에게 알림이간다.
/// 값 동기화 가능(설정창, 가격표, 모니터 3개다 동일한 값 적용)
/// </summary>
public class PriceObserver : MonoBehaviour
{
    public static PriceObserver Instance{ get; private set; }
    // 전역 가격 관리자
    private Dictionary<int, ItemData> currentItemData = new();
    private Dictionary<int, float> currentPrices = new();//각 상품의 현재 가격을 저장하는 딕셔너리<상품id,현재 가격>
    private Dictionary<int, List<IPriceChangeable>> observers = new();//상품 구독중인 관찰자 목록<상품id,>

    [SerializeField] private ItemDatabase itemDatabase;

     void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        InitializePrices();// 초반 가격 딕셔너리에 원가 등록
    }

    private void InitializePrices() //초반 원가등록 메서드
    {
        
        if (itemDatabase != null)
        {
            foreach (var itemData in itemDatabase.items)//데이터베이스의 모든 itemdata를 순회
            {
                if (!currentPrices.ContainsKey(itemData.itemId)) // 판매가 딕셔너리에 so에 해당하는 상품 id 잇는지 확인 없다면
                {
                    currentPrices[itemData.itemId] = itemData.baseCost; //판매가 딕셔너리에 원가 등록
                    Debug.Log($"옵저버 상품 등록: {itemData.itemName} (ID: {itemData.itemId}) 초기 가격: {itemData.baseCost}"); //여기서부터 뭔가 이상함
                }

                currentItemData[itemData.itemId] = itemData; 
            }
        }
    }


    
    //관찰자 등록
    public void Subscribe(int itemId, IPriceChangeable observer)
    {
        if (!observers.ContainsKey(itemId))//해당 상품id의 관찰자 목록이 없다면 생성
            observers[itemId] = new List<IPriceChangeable>();
        observers[itemId].Add(observer);
    }

    //등록 해제(할필요..는 없을것같아 구현안함)


    //특정 상품 가격(정가)조회
    public float GetPrice(int itemId)
    {
        bool exists = currentPrices.TryGetValue(itemId, out float price);
        Debug.Log($"가격 조회 - 아이템 ID: {itemId}, 존재 여부: {exists}, 가격: {price}");
        return exists ? price : 0f; 

        //return currentPrices.TryGetValue(itemId, out float price) ? price : 0f;
    } 


    //가격 업데이트
    //가격 변경시 모든 구독자에게 자동 알림이 전송
    public void UpdatePrice(int itemId, float newPrice)//가격 변경할 상품, 새로운 가격
    {
        float oldPrice = GetPrice(itemId);
        currentPrices[itemId] = newPrice;//상품 가격 변경
        Debug.Log($"{oldPrice} / {newPrice}");

        // 모든 구독자에게 알림
        if (observers.ContainsKey(itemId))
            foreach (var observer in observers[itemId])
                observer.OnPriceChanged(itemId, newPrice, oldPrice);//옵저버들은 OnPriceChanged를 구현해서 본인의 로직에 맞게 가격을 수정해야함
    }
    



}
