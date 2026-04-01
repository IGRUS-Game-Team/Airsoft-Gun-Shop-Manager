using UnityEngine;

/// <summary>
/// 아이템 카테고리. ItemData.category에서 사용.
/// 카테고리에 따라 가격 계산, UI 필터, 배치 방식이 달라짐.
///
/// - MainWeapon/ProtectiveGear/Consumable: 박스로 배송 → 선반에 진열 → NPC가 구매
/// - Furniture: 카탈로그에서 구매 → 매장에 가구로 배치 (PlacementManager)
/// - Decoration: 카탈로그에서 구매 → 매장 벽/바닥에 장식으로 배치 (DecorationPlacementManager)
/// </summary>
public enum ItemCategory
{
    MainWeapon,      // 에어소프트건 (AK47, M16, MP5, Shotgun, Sniper, Pistol)
    ProtectiveGear,  // 보호 장비 (고글, 헬멧, 장갑)
    Consumable,      // 소모품 (BB탄, 가스 캡슐) — 항상 잠금해제 상태
    Furniture,       // 가구 (선반, 테이블 등) — unlockCost가 구매가
    Decoration       // 장식품 (포스터 등) — unlockCost가 구매가, worldPrefab 필요
}
