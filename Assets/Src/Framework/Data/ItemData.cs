using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 게임 내 모든 아이템(총기, 방어구, 소모품, 가구, 장식)의 데이터를 정의하는 ScriptableObject.
///
/// [사용법]
/// 1. Create > Shop > Item 으로 새 에셋 생성
/// 2. 필드 설정 후 ItemDatabase.asset의 items 리스트에 등록
/// 3. itemId는 OnValidate에서 자동 부여됨 (수동 변경 금지)
///
/// [카테고리별 주요 필드]
/// - MainWeapon/ProtectiveGear/Consumable: baseCost(원가), perBoxCount(박스당 수량), unlockCost(카탈로그 해제비)
/// - Furniture: unlockCost(구매비), displayPrefab(미리보기)
/// - Decoration: unlockCost(구매비), displayPrefab(미리보기), worldPrefab(월드 배치 프리팹)
///
/// [가격 체계]
/// - 모니터 주문 시: CartPrice 사용 (Decoration/Furniture → unlockCost, 그 외 → baseCost)
/// - NPC 판매가: baseCost × (1.2 + marketModifier) — MarketPriceDataManager에서 계산
/// </summary>
[CreateAssetMenu(menuName = "Shop/Item")]
public class ItemData : ScriptableObject
{
    public int itemId;              // 고유 ID (OnValidate에서 자동 부여, 세이브/로드 키로 사용)
    public string itemName;         // UI에 표시되는 이름 (커스텀 이름은 ItemOverrideStore에서 관리)
    public ItemCategory category;   // 카테고리 (MainWeapon, ProtectiveGear, Consumable, Furniture, Decoration)
    public float baseCost;          // 원가 — 모니터 주문 비용 + NPC 판매가 계산 기준
                                    // TODO : 개별 가격도 뜨게 해야함
    public int perBoxCount;         // 배송 박스 하나에 들어가는 수량 (BoxContainer.SetContent에서 사용)
    public float rarity;            // 희귀도 (현재 미사용 — 향후 NPC 구매 확률에 활용 가능)
    public int regulationLevel;     // 규제 레벨 (RegulationEventStrategy에서 참조)
    public int BaseDemand;          // 기본 수요 (NPC 구매 빈도에 영향)
    public Sprite icon;             // UI 아이콘 (카탈로그, 상점, 장바구니에 표시)
    public DisplayType displayType; // 진열 방식 (Shelf, WallMount 등 — ShelfSlot/WallGunSlot 배치에 사용)
    public GameObject displayPrefab;// 3D 모델 프리팹 (박스 내부 시각화 + 선반 진열 시 사용)
    public float unlockCost;        // 카탈로그 잠금해제 비용 (Decoration/Furniture는 이것이 구매가)

    /// <summary>
    /// 카트 결제에 사용할 단가. 장식품·가구는 unlockCost, 그 외는 baseCost.
    /// </summary>
    public float CartPrice =>
        (category == ItemCategory.Decoration || category == ItemCategory.Furniture)
            ? unlockCost
            : baseCost;

    [Header("Decoration Placement")]
    [Tooltip("장식품 전용: 매장에 설치될 월드 프리팹")]
    public GameObject worldPrefab;      // 장식품이 설치될 때 생성되는 프리팹
    
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        // 이미 ID가 있으면 건너뜀
        if (itemId != 0) return;

        // 모든 ItemData 에셋 찾기
        string[] guids = AssetDatabase.FindAssets("t:ItemData");
        int maxId = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData data = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (data != null && data.itemId > maxId)
                maxId = data.itemId;
        }

        // 최대값 + 1 로 ID 부여
        itemId = maxId + 1;

        // 변경 사항 저장 표시
        EditorUtility.SetDirty(this);
        Debug.Log($"[ItemData] '{name}'의 itemId를 {itemId}로 자동 부여");
    }
#endif
}

