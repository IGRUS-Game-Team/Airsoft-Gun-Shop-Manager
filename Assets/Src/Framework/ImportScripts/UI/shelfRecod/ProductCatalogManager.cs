// using System;
// using System.Collections.Generic;
// using Unity.VisualScripting;
// using UnityEngine;
// /// <summary>
// /// 
// /// 상품의 ItemDataManager(SO정보) 를 전송받아서 ( SlotFillBehaviour에게 )
// /// 정보를 같은 상품 끼리 그룹화 해서 공유하도록 한다. -> 가격을 변경하면 동일상품의 모든 가격표가 변동되도록
// /// 
// /// 전송받은 ItemDataManager(SO정보)를 PriceCardFactory에 다시 전송하도록 한다.
// /// 
// /// </summary>
// public class ProductCatalogManager : MonoBehaviour
// {

//     public static ProductCatalogManager Instance { get; private set; }//싱글톤 안된다

//     private Dictionary<int, ProductGroup> productGroups = new Dictionary<int, ProductGroup>(); //그룹 찾기에 사용할 딕셔너리
//                                                                                                // 키값, 상품 그룹
//                                                                                                //상품정보 통합관리 : 상품이 같은 가격표가 아래 값을 공유한다.
//     public class ProductGroup
//     {
//         public int itemId; //해당 상품 아이디
//         //public List<int> occupiedSlots; // 해당 상품이 차지하고 있는 슬롯 
//         public GameObject sharedPriceCard; // 해당 상품 가격표
//         public string productName; //상품 이름
//         public float basePrice;// 상품 원가
//         public float currentPrice; //상품의 정가

//     }

//     void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }



//     //ItemData를 전달받아 동일 상품의 그룹이 존재하는지 확인한다. -> slotfillbehaviour에서 호출
//     public void ProcessProduct(ItemDataManager itemDataManager)
//     //상품 프리팹에서 so 빼와야하네
//     {

//         if (productGroups.ContainsKey(itemDataManager.ItemID)) // 이미 존재
//         {
//             AddSlotToExistingGroup(itemDataManager);
//         }
//         else // 존재 안함
//         {
//             RegisterProduct(itemDataManager);
//         }
//     }

//     //SO 상품을 기반으로 한 그룹 생성
//     private void RegisterProduct(ItemDataManager itemDataManager)
//     {
//         ProductGroup newGroup = new ProductGroup();
//         newGroup.itemId = itemDataManager.ItemID;
//         //newGroup.occupiedSlots = new List<int>(); //슬롯 리스트 생성
//         newGroup.basePrice = itemDataManager.BaseCost;
//         newGroup.productName = itemDataManager.DisplayName;

//         productGroups[itemDataManager.ItemID] = newGroup;//상품 id 따라 순서대로 그룹 정보가 저장됨

//         PriceCardFactory.Instance.CreatePriceCard(productGroups[itemDataManager.ItemID]);//그룹을 정보를 통째로 전달
//         //팩토리에게 가격표 생성 요청
//     }


//     //이미 그룹이 딕셔너리 안에 존재한다면?
//     private void AddSlotToExistingGroup(ItemDataManager itemDataManager)
//     {
//         Debug.Log("이미 그룹 존재");
//         // 기존 그룹에 추가할 때는 가격표 생성 안함
//         // 필요하다면 재고 수량 증가 등의 로직만
//     }
    
//     // ProductCatalogManager에서
//     public ProductGroup GetGroup(int itemId)
//     {
//         productGroups.TryGetValue(itemId, out ProductGroup group);
//         return group; 
//     }

// }
