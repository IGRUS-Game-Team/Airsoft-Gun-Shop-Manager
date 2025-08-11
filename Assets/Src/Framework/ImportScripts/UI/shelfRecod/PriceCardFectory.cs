// using UnityEngine;
// /// <summary>
// /// 장지원 8.10
// /// 가격표 생성 전담
// /// </summary>
// public class PriceCardFectory : MonoBehaviour
// {
//     ProductCatalogManager productCatalogManager;
//     public static PriceCardFectory Instance { get; private set; }

//     [Header("가격표 설정")]
//     [SerializeField] GameObject priceCard; //가격표 프리팹
//     [SerializeField] Transform priceCardPosition;//가격표 생성될 위치?

//     void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//             Debug.Log("PriceCardFactory Instance 설정 완료"); // 디버그 추가
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     //가격표 만들기
//     // productcatalogmanager이 준 그룹데이터를 이용해 카드 프리팹을 생성한다.
//     public void CreatePriceCard(ProductCatalogManager.ProductGroup itemDataGroup)
//     {


//         // 이미 이 ItemID의 가격표가 있는지 체크
//         if (HasPriceCardForItem(itemDataGroup.itemId))
//         {
//             Debug.Log($"ItemID {itemDataGroup.itemId} 가격표가 이미 존재함");
//             return; // 중복 생성 방지
//         }
//         // 가격표 생성
//         GameObject newCard = Instantiate(priceCard, priceCardPosition.position, priceCardPosition.rotation, priceCardPosition);

//         // 생성된 가격표에 직접 데이터 설정
//         var cardController = newCard.GetComponent<PriceCardController>();
//         var settingController = newCard.GetComponentInChildren<PriceSettingController>(true);
//         if (cardController != null)
//         {
//             Debug.Log("가격표 데이터 전달");
//             cardController.SetItemData(itemDataGroup);//가격표에 데이터그룹 전달
//             Debug.Log("세팅창 데이터 전달");
//             settingController.SetItemData(itemDataGroup); //세팅창에 데이터그룹 전달
//             //ㄴ이시발것이 눌이란다!

//         }
        
//          Debug.Log("=== CreatePriceCard 시작 ===");
//     Debug.Log($"itemDataGroup이 null인가? {itemDataGroup == null}");
//     Debug.Log($"priceCard가 null인가? {priceCard == null}");
//     Debug.Log($"priceCardPosition이 null인가? {priceCardPosition == null}");
//     }
        
//     private bool HasPriceCardForItem(int itemId)
//     {
//     // ProductCatalogManager에서 확인
//     var group = ProductCatalogManager.Instance.GetGroup(itemId);
//     return group != null && group.sharedPriceCard != null;
//     }
//     //가격표 생성 위치.. 를 여기서 관리해야하나
// }
