# Airsoft Gun Shop Manager

에어소프트건 매장을 운영하는 **1인칭 시뮬레이션 게임**입니다.
상품 주문, 진열, 가격 설정, NPC 고객 응대, 매장 확장, 사격장 운영 등을 통해 매장을 성장시킵니다.

> 개발 기간: 2025-07-28 ~ 2025-08-31

---

## 목차

1. [시작하기](#1-시작하기)
2. [프로젝트 구조](#2-프로젝트-구조)
3. [씬 구성과 게임 흐름](#3-씬-구성과-게임-흐름)
4. [핵심 시스템 상세](#4-핵심-시스템-상세)
5. [ScriptableObject 구성 가이드](#5-scriptableobject-구성-가이드)
6. [콘텐츠 추가 매뉴얼](#6-콘텐츠-추가-매뉴얼)
7. [남은 작업 및 알려진 이슈](#7-남은-작업-및-알려진-이슈)
8. [주의사항 및 함정](#8-주의사항-및-함정)
9. [팀원 역할](#9-팀원-역할)

---

## 1. 시작하기

### 요구사항
- **Unity** : LTS `6000.0.53f1`
- **Git LFS** : 필수 (프리팹, 텍스처 등 바이너리 파일 관리)

### 클론 및 설정
```bash
git clone <repo-url>
cd Airsoft-Gun-Shop-Manager
git lfs install
git lfs pull
```

Unity Hub에서 `6000.0.53f1` 버전으로 프로젝트를 열면 됩니다.

### 빌드 설정
- **타겟 플랫폼**: Windows (Standalone)
- **기본 해상도**: 1024×768
- **렌더 파이프라인**: URP (Universal Render Pipeline)
- **Build Scenes** (순서대로):
  1. `MainMenu 1`
  2. `RealFinal 1`
  3. `NewsDeskScene`

---

## 2. 프로젝트 구조

### 전체 폴더
```
Assets/
├── Art/                          # 아트 리소스
│   ├── Animation/                #   애니메이션 (박스 오픈, 사격)
│   ├── Materials/                #   머티리얼
│   ├── Music/                    #   BGM
│   ├── Prefabs/                  #   프리팹 (아래 상세)
│   ├── Sound/Game Sounds/        #   효과음 (Box, Gun, Menu 등)
│   ├── Textures/                 #   텍스처, 렌더텍스처
│   └── UI/                       #   UI 이미지
│
├── ImportedAssets/ThirdPartyAssets/   # 서드파티 에셋
│   ├── Armory Store/             #   매장 3D 모델
│   ├── Classical_city/           #   외부 환경
│   ├── Easy Build System/        #   배치 시스템
│   ├── Military Toy/             #   에어소프트건 3D 모델
│   ├── Military/                 #   NPC 의상
│   ├── StarterAssets/            #   1인칭 컨트롤러 (커스텀됨)
│   └── Survivalist/              #   URP 세팅
│
├── Levels/                       # 씬 파일 (3개)
├── Src/                          # 소스 코드 전체
│   ├── Framework/                #   모든 게임 로직
│   └── ScriptableObject/         #   SO 에셋 (아이템, 탄약, 점원 등)
├── Settings/                     # URP 렌더 파이프라인 설정
└── Plugins/                      # RootMotion FinalIK 등
```

### 프리팹 구조 (`Art/Prefabs/`)
```
Prefabs/
├── Box/                # 배송 박스 프리팹
├── Environments/       # 환경 오브젝트
├── InteractionUI/      # 상호작용 UI 프롬프트
├── Market/
│   ├── Decoration/     # 장식 아이템 (포스터 등)
│   ├── ExtendMarket/   # 매장 확장 관련
│   ├── Furnitures/     # 가구 (선반, 테이블 등)
│   ├── Merchandise/    # 상품 박스 (AK47 Toy Box 등)
│   ├── Money/          # 화폐 오브젝트
│   └── ShootingRange/  # 사격장 오브젝트
├── Monitor/
│   ├── CounterCashMonitor/  # 계산대 화면
│   ├── CounterMonitor/      # 카운터 모니터
│   └── ManageMonitor/       # 관리 모니터 (주문, 카탈로그, 고용)
├── Npc/Npcs/           # NPC 캐릭터 프리팹 (12종)
├── Object/             # 신문, TV 등
├── Player/             # 플레이어, 카메라, 사격 관련
├── PriceCard/          # 가격표 프리팹
├── System/             # 시스템 오브젝트
└── UI/                 # UI 프리팹 (정산, 가격설정, 설정 등)
```

### 스크립트 구조 (`Src/Framework/`)
```
Framework/
├── Clerk/                       # 자동 점원 시스템
│   ├── AutoClerkController.cs   #   점원 AI (자동 스캔/결제)
│   ├── ClerkData.cs             #   점원 SO 정의
│   ├── ClerkDatabase.cs         #   점원 DB SO
│   └── ClerkInteraction.cs      #   점원 상호작용
│
├── Data/                        # 데이터 레이어
│   ├── ItemData.cs              #   아이템 SO 정의 (핵심)
│   ├── ItemCategory.cs          #   카테고리 enum
│   ├── ItemDataBase.cs          #   아이템 DB SO (인덱싱)
│   ├── ItemDisplayType.cs       #   진열 타입 enum
│   ├── ItemOverrideStore.cs     #   런타임 이름 커스텀
│   ├── SaveData/
│   │   ├── SaveDatas/           #   세이브 DTO 18종
│   │   └── SaveHandler/         #   ISaveable 구현체 18종
│   └── Setting/                 #   그래픽/입력 설정
│
├── Input/                       # 입력 시스템
│   ├── InputContextRouter.cs    #   클릭 라우팅 (우선순위 기반)
│   ├── InteractionController.cs #   상호작용 입력 처리
│   └── LookBinding.cs           #   마우스 감도 적용
│
├── Interaction/                 # 인터페이스 정의
│   ├── ISaveable.cs             #   저장/로드 인터페이스
│   ├── ISocialEventStrategy.cs  #   경제 이벤트 전략 인터페이스
│   └── FurniturePlaceable.cs    #   가구 배치 컴포넌트
│
├── InteractionBehaviours/       # 상호작용 핸들러
│   ├── BoxInteractionBehaviour.cs
│   ├── CashInteractionBehaviour.cs
│   ├── CashRegisterEnterHandler.cs
│   ├── CheckoutItemBehaviour.cs
│   ├── ESCInteractionBehaviours.cs
│   ├── MonitorInteractionBehaviours.cs
│   ├── MonitorUIModeManager.cs
│   ├── SlotFillBehaviour.cs
│   ├── TrashBinInteractionBehaviour.cs
│   └── SocialStrategy/         #   경제 이벤트 전략 구현
│       ├── BoomEventStrategy.cs
│       ├── NormalEventStrategy.cs
│       ├── RecessionEventStrategy.cs
│       └── relaxationGunRegulation.cs
│
├── Market/                      # 매장 시스템
│   ├── DoorSignInteraction.cs   #   영업 시작/종료
│   ├── MarketExtender.cs        #   매장 확장
│   └── Managers/
│       ├── PlacementManager.cs            #   가구 배치 매니저
│       └── DecorationPlacementManager.cs  #   장식 배치 매니저
│
├── Music/                       # 오디오
│   ├── AudioManager.cs          #   효과음 (DontDestroyOnLoad)
│   └── MusicPlayer.cs           #   BGM (DontDestroyOnLoad)
│
├── Npc/                         # NPC 시스템 (가장 큰 모듈)
│   ├── Core/NpcController.cs    #   NPC 메인 컨트롤러 (상태머신)
│   ├── NpcStates/               #   12개 상태 클래스
│   │   ├── NpcState_ToDoor.cs   #     입장
│   │   ├── NpcState_ToShelf.cs  #     선반으로 이동
│   │   ├── NpcState_PickItem.cs #     상품 집기
│   │   ├── NpcState_PickWait.cs #     대기
│   │   ├── NpcState_ToQueue.cs  #     대기줄로
│   │   ├── NpcState_QueueWait.cs#     줄서기
│   │   ├── NpcState_ToCounter.cs#     카운터로
│   │   ├── NpcState_OfferPayment.cs # 결제 제시
│   │   ├── NpcState_ToRange.cs  #     사격장으로
│   │   ├── NpcState_Shoot.cs    #     사격
│   │   ├── NpcState_Leave.cs    #     퇴장
│   │   └── NpcState_Wander.cs   #     배회
│   ├── Managers/
│   │   ├── NpcSpawnManager.cs   #   NPC 스폰
│   │   ├── CounterManager.cs    #   계산대 관리
│   │   ├── QueueManager.cs      #   대기줄 관리
│   │   ├── MarketPriceDataManager.cs  # 가격 변동 반영
│   │   ├── SettlementManager.cs #   일일 정산
│   │   └── Shelf/ShelfSlots.cs  #   선반 슬롯 관리
│   ├── NpcIK/                   #   IK 시스템 (손 위치)
│   ├── NpcPayment/              #   결제 처리
│   └── NpcProfile/              #   NPC 프로필
│
├── Object/
│   ├── Box/                     # 배송 박스
│   │   ├── Box Spawner.cs       #   박스 생성
│   │   ├── BoxContentVisualManager.cs  # 박스 내부 시각화
│   │   └── BoxLabel.cs          #   박스 라벨
│   └── Gun/                     # 총기
│       ├── GunInteraction.cs    #   총 상호작용
│       ├── WallGunSlot.cs       #   벽걸이 슬롯
│       └── WallGunPlacementMode.cs  # 벽걸이 배치 모드
│
├── Player/                      # 플레이어 시스템
│   ├── PlayerObjectHoldController.cs   # 오브젝트 들기
│   ├── PlayerObjectThrowBoxController.cs # 던지기
│   ├── RaycastDectator.cs       #   레이캐스트
│   ├── SmallBoxInteraction.cs   #   소형 박스 (총기 포장)
│   ├── InteractionUI/           #   상호작용 프롬프트 UI
│   └── ShootingMode/            #   사격 시스템
│       ├── ActiveGun.cs
│       ├── BBPellet.cs
│       ├── ShootingGun.cs
│       └── ShootingZoneManager.cs
│
├── PriceCard/                   # 가격표 시스템
│   ├── PriceCardController.cs   #   가격표 동작
│   └── PriceCardFactory.cs      #   가격표 생성
│
├── SocialEvents/                # 사회 이벤트
│   ├── BouncerEscort.cs         #   바운서 호송
│   ├── ProtestDirector.cs       #   시위 지휘
│   └── Protestor.cs             #   시위대 AI
│
├── System/                      # 글로벌 시스템
│   ├── SaveManager.cs           #   저장/불러오기 (ES3)
│   ├── GameState.cs             #   소지금 (DontDestroyOnLoad)
│   ├── ReputationState.cs       #   평판 (DontDestroyOnLoad)
│   ├── RevenueXPTracker.cs      #   매출/경험치
│   ├── SocialEventManager.cs    #   경제 이벤트 관리
│   ├── GameResetHelper.cs       #   새 게임 리셋
│   ├── ComplainUI.cs            #   불만 표시 UI
│   └── MoneyChangeUI.cs         #   소지금 변동 UI
│
├── Tutorial/                    # 튜토리얼 (5종)
│   ├── TutorialManager.cs       #   메인 튜토리얼
│   ├── ExpansionTutorialManager.cs
│   ├── ShootingRangeTutorialManager.cs
│   ├── BouncerTutorialManager.cs
│   └── SocialEventTutorialManager.cs
│
└── UI/                          # UI 시스템
    ├── Adjustment/              #   일일 정산 화면
    ├── CounterCashMonitor/      #   계산대 화면
    ├── Monitor/                 #   모니터 (주문, 카탈로그)
    │   ├── MonitorShopCartManager.cs  # 장바구니
    │   ├── PurchaseProcessor.cs       # 구매 처리
    │   └── UI/
    │       ├── ShopUIController.cs    # 상품 목록
    │       ├── Unlock/CatalogUIManager.cs  # 카탈로그
    │       └── Hiring/                # 점원 고용 UI
    ├── NewsDesk/                #   TV 뉴스 시스템
    ├── NewsPaper/               #   신문 UI
    ├── Overlay/Time/            #   HUD, 시간, 설정
    └── SceneUI/MainMenu/        #   메인 메뉴
```

---

## 3. 씬 구성과 게임 흐름

### 씬 목록

| 씬 | 용도 |
|---|---|
| `MainMenu 1` | 메인 메뉴 (새 게임, 불러오기, 설정, 종료) |
| `RealFinal 1` | 메인 게임 씬 (매장 운영 전체) |
| `NewsDeskScene` | 뉴스 데스크 (TV 상호작용 시 전환) |

### 씬 전환 흐름
```
[앱 시작]
  └→ MainMenu 1

[새 게임]
  MainMenu 1
    → GameResetHelper.ResetAll()
       ├ ES3SlotManager.selectedSlotPath = null
       ├ GameState.Money = 초기값
       ├ ReputationState 리셋
       ├ RevenueXPTracker 리셋
       ├ SettlementManager 리셋
       └ PlayerPrefs 삭제 (확장/튜토리얼 플래그)
    → SceneManager.LoadScene("RealFinal 1")

[불러오기]
  MainMenu 1
    → 슬롯 팝업 → 슬롯 선택
    → ES3SlotManager.selectedSlotPath = "slots/Save_yyyyMMdd_HHmmss.es3"
    → SaveManager.QueueLoadAfterSceneChange()
    → SceneManager.LoadScene("RealFinal 1")
    → OnSceneLoaded → SaveManager.InitAndMaybeLoad() → 모든 ISaveable 복원

[게임 플레이 - 하루 루프]
  아침 시작
    → SocialEventManager.ExecuteStrategy()  (경제 이벤트 결정)
    → 뉴스 화면 표시 (이벤트 내용)
    → NpcSpawnManager 스폰 시작
    → NPC 쇼핑 & 결제
    → 영업 종료 (DoorSignInteraction)
    → OnDayEnd → SettlementManager 일일 정산
    → AdjustmentUI 표시 (수익, 지출, 레벨업)
    → SaveManager.SaveGame() 자동 저장
    → 다음 날
```

### 아이템의 생애주기
```
[모니터에서 주문]
  ShopUIController → 카테고리별 상품 표시 (잠금해제된 것만)
    → MonitorShopCartManager.AddItem() → 장바구니 추가
    → PurchaseProcessor.Purchase() → 결제

[배송 도착]
  PurchaseProcessor → BoxSpawner.BoxDrop(itemId, boxCount)
    → 배송 박스 프리팹 인스턴스화
    → BoxContainer.SetContent(itemData, perBoxCount)

[박스 개봉]
  플레이어 클릭 → BoxContainer.ToggleLid()
    → BoxContentVisualManager.Refresh() → displayPrefab 3D 모델 표시

[선반 진열]
  플레이어가 아이템을 들고 선반 클릭
    → SlotFillBehaviour → ShelfSlot.RegisterNewItem()
    → ShelfSlot.OnProductPlacedToFactory 이벤트 발생
    → PriceCardFactory → PriceCardController 생성 (가격표)

[NPC 구매]
  NpcState_PickItem → ShelfSlot.PopItem() → 아이템 획득
    → NpcState_ToCounter → 카운터 이동
    → NpcState_OfferPayment → 결제
    → SettlementManager → 매출 기록
```

---

## 4. 핵심 시스템 상세

### 4.1 저장/불러오기 시스템 (ES3)

**핵심 파일**: `SaveManager.cs`, `ISaveable.cs`, `SaveHandler/` 폴더 전체

```csharp
// ISaveable 인터페이스 - 모든 저장 가능한 컴포넌트가 구현
public interface ISaveable
{
    object CaptureData();      // 현재 상태 → 직렬화 가능한 객체
    void RestoreData(object);  // 저장된 객체 → 상태 복원
}
```

- `SaveManager`는 **DontDestroyOnLoad** 싱글톤
- `FindObjectsOfType<ISaveable>()`로 씬의 모든 핸들러를 자동 수집
- ES3 키는 핸들러 클래스명: `ES3.Save("BoxSaveHandler", data, slotPath)`
- 슬롯 경로: `slots/Save_yyyyMMdd_HHmmss.es3`

**복원 순서** (이 순서를 반드시 지켜야 합니다):

| 순서 | 핸들러 | 이유 |
|:----:|--------|------|
| -2 | `MarketExpansionSaveHandler` | 매장/사격장 영역을 먼저 활성화해야 나머지 오브젝트 복원 가능 |
| -1 | `FurnitureSaveHandler` | 플레이어가 배치한 가구 복원 (선반 포함) |
| 0 | `BoxSaveHandler` | 배송 박스 상태 |
| 0 | `DoorStateSaveHandler` | 문 열림/닫힘 |
| 0 | `GameTimeSaveHandler` | 게임 내 시간 |
| 0 | `MoneySaveHandler` | 소지금 |
| 0 | `ReputationSaveHandler` | 평판 |
| 0 | `RevenueXPSaveHandler` | 매출/경험치 |
| 0 | `SettlementSaveHandler` | 일일 정산 데이터 |
| 0 | `SocialEventSaveHandler` | 현재 경제 이벤트 |
| 0 | `MarketPriceSaveHandler` | 가격 변동 데이터 |
| 0 | `ClerkSaveHandler` | 고용된 점원 |
| 0 | `PlayerSaveHandler` | 플레이어 위치 |
| 0 | `ShopCartSaveHandler` | 장바구니 |
| 0 | `SlotSaveHandler` | 슬롯 메타데이터 |
| 0 | `ItemNameOverrideSaveHandler` | 커스텀 아이템 이름 |
| 1 | `ShelfItemSaveHandler` | 선반 아이템 (선반이 먼저 존재해야 함) + 가격표 복원 |
| 2 | `WallGunSaveHandler` | 벽걸이 총 (리플렉션 사용, 마지막) |

**새 핸들러 추가 방법**:
1. `SaveDatas/`에 DTO 클래스 생성 (`[System.Serializable]`)
2. `SaveHandler/`에 `MonoBehaviour`로 핸들러 생성, `ISaveable` 구현
3. 씬에 핸들러 오브젝트 배치 (또는 기존 매니저에 컴포넌트 추가)
4. 복원 순서가 중요하면 `ISaveable`의 순서 속성 설정

---

### 4.2 NPC AI 시스템 (상태머신)

**핵심 파일**: `NpcController.cs`, `NpcStates/` 폴더

```
NPC 행동 흐름:

[입장] ToDoor
  │
  ├─→ [쇼핑] ToShelf → PickItem → PickWait → ToQueue → QueueWait → ToCounter → OfferPayment → Leave
  │
  ├─→ [사격] ToRange → Shoot → Leave
  │
  └─→ [배회] Wander → Leave
```

**NpcController 핵심 속성**:
- `NavMeshAgent Agent` — AI 길찾기
- `Animator Animator` — 애니메이션
- `StateMachine stateMachine` — 상태머신 실행기
- `targetShelfSlot` / `targetShelfGroup` — 현재 목표 선반
- `ShootingLane TargetLane` — 사격장 레인
- `Transform QueueTarget` — 대기줄 위치
- `bool PaymentDone` — 결제 완료 플래그

**NpcSpawnManager**:
- `minDelay` ~ `maxDelay` 랜덤 간격으로 NPC 스폰
- `spawnPoints[]` 배열에서 랜덤 위치 선택
- `npcPrefabs[]` 풀에서 랜덤 프리팹 선택
- `DespawnAll()` — 영업 종료 시 전체 디스폰

**새 NPC 상태 추가 방법**:
1. `NpcStates/`에 새 상태 클래스 생성
2. `Enter()`, `Update()`, `Exit()` 구현
3. `NpcController`에서 `stateMachine.SetState(new NpcState_NewState(this))` 호출

---

### 4.3 경제/이벤트 시스템 (전략 패턴)

**핵심 파일**: `SocialEventManager.cs`, `ISocialEventStrategy.cs`, `SocialStrategy/` 폴더

```csharp
// 이벤트 전략 인터페이스
public interface ISocialEventStrategy
{
    string EventName { get; }       // "사격 게임 인기" 등
    string StatusText { get; }      // "수요 급증" 등
    float MarketModifier { get; }   // 가격 배수 (-0.3 ~ +0.5)
    bool IsGunRegulation { get; }   // true면 특정 총기만 영향
    void GetEventStrategyData();    // 랜덤 이벤트 데이터 생성
}
```

**현재 구현된 4가지 전략**:

| 전략 | MarketModifier | 설명 |
|------|:-:|------|
| `NormalEventStrategy` | 0 | 변동 없음 (1일차 강제) |
| `BoomEventStrategy` | +0.1 ~ +0.3 | 호황: 수요 증가, 가격 상승 |
| `RecessionEventStrategy` | -0.2 ~ -0.3 | 불황: 수요 감소, 가격 하락 |
| `RegulationEventStrategy` | -0.1 | 규제: 1~2종 총기 판매 제한 |

**가격 계산 공식**:
```
판매가 = baseCost × (1.2 + marketModifier)
```

**일일 이벤트 선택 확률**: Normal 50% / Boom 30% / Recession 20%

**새 이벤트 전략 추가 방법**:
1. `SocialStrategy/`에 `ISocialEventStrategy` 구현 클래스 생성
2. `GetEventStrategyData()`에서 이벤트명, 수치 랜덤 생성
3. `SocialEventManager`의 전략 목록에 등록
4. 선택 확률 조정

---

### 4.4 입력 시스템

**핵심 파일**: `InputContextRouter.cs`

클릭 시 아래 우선순위로 처리됩니다:

```
1. PlacementManager.IsPlacing     → 가구 배치 처리
2. WallGunPlacementMode 활성      → 벽걸이 총 배치 처리
3. PriceCardController.IsAnyPriceUIOpen → 무시
4. UIUtility (포인터가 UI 위)     → 무시
5. 오브젝트를 들고 있음           → 해당 오브젝트 상호작용
6. SmallBox 직접 히트             → SmallBox 처리
7. 선반 슬롯 히트 + 오브젝트 보유 → SlotFillBehaviour
8. IInteractable 히트             → 표준 상호작용
```

---

### 4.5 매장 확장 시스템

**핵심 파일**: `MarketExtender.cs`, `MarketExpansionSaveHandler.cs`

- `StoreExpansionPurchased` / `ShootingRangePurchased` — PlayerPrefs 키
- 모니터에서 확장 구매 → 해당 영역 활성화
- `RestoreState()`로 로드 시 영역 복원

---

### 4.6 점원(Clerk) 시스템

**핵심 파일**: `AutoClerkController.cs`, `ClerkData.cs`, `ClerkDatabase.cs`

- 모니터 고용 탭에서 점원 고용/해고
- `AutoClerkController`가 고용된 점원의 자동 스캔/결제 처리
- 코루틴 기반 순차 아이템 처리

---

### 4.7 주요 싱글톤 매니저

| 매니저 | DontDestroy | 역할 |
|--------|:-:|------|
| `SaveManager` | ✅ | 저장/불러오기 총괄 |
| `GameState` | ✅ | 플레이어 소지금 |
| `ReputationState` | ✅ | 평판 시스템 |
| `RevenueXPTracker` | ✅ | 매출/경험치 추적 |
| `MusicPlayer` | ✅ | BGM |
| `AudioManager` | ✅ | 효과음 |
| `ItemOverrideStore` | ✅ | 아이템 이름 커스텀 |
| `SocialEventManager` | ❌ | 일일 경제 이벤트 |
| `NpcSpawnManager` | ❌ | NPC 스폰 |
| `SettlementManager` | ❌ | 일일 정산/레벨 |
| `CounterManager` | ❌ | 계산대 관리 |
| `QueueManager` | ❌ | 대기줄 관리 |
| `PlacementManager` | ❌ | 가구 배치 |
| `DecorationPlacementManager` | ❌ | 장식 배치 |
| `AutoClerkController` | ❌ | 자동 점원 |
| `PriceCardFactory` | ❌ | 가격표 생성 |
| `UnlockedItemsStore` | ❌ | 잠금해제 상태 |

---

## 5. ScriptableObject 구성 가이드

### 5.1 ItemData (아이템 정의)

**생성**: `Create > Shop > Item`

| 필드 | 타입 | 설명 | 예시 |
|------|------|------|------|
| `itemId` | int | 자동 생성 (OnValidate). 수동 변경 금지 | 1002 |
| `itemName` | string | 표시 이름 | "AK47" |
| `category` | ItemCategory | 카테고리 | MainWeapon |
| `baseCost` | float | 원가 (주문 비용 계산 기준) | 300 |
| `perBoxCount` | int | 박스당 수량 | 4 |
| `rarity` | float | 희귀도 (현재 미사용) | 0 |
| `regulationLevel` | int | 규제 레벨 | 0 |
| `BaseDemand` | int | 기본 수요 | 0 |
| `icon` | Sprite | UI 아이콘 (카탈로그, 상점) | — |
| `displayType` | DisplayType | 진열 방식 | Shelf |
| `displayPrefab` | GameObject | 박스/선반 위 3D 모델 | Ak47 Toy 프리팹 |
| `unlockCost` | float | 카탈로그 잠금해제 비용 | 3000 |
| `worldPrefab` | GameObject | 장식용 월드 배치 프리팹 (장식만) | Poster 프리팹 |

**카테고리별 필드 사용**:

| | MainWeapon | ProtectiveGear | Consumable | Furniture | Decoration |
|---|:-:|:-:|:-:|:-:|:-:|
| baseCost | ✅ 원가 | ✅ 원가 | ✅ 원가 | ❌ | ❌ |
| perBoxCount | ✅ | ✅ | ❌ (0) | ❌ | ❌ |
| unlockCost | ✅ 해제비용 | ✅ 해제비용 | ❌ (0) | ✅ 구매비용 | ✅ 구매비용 |
| displayPrefab | ✅ 3D모델 | ✅ 3D모델 | ✅ 3D모델 | ✅ 미리보기 | ✅ 미리보기 |
| worldPrefab | ❌ | ❌ | ❌ | ❌ | ✅ 배치모델 |

> **CartPrice 프로퍼티**: Decoration/Furniture이면 `unlockCost`, 그 외엔 `baseCost` 반환

**DisplayType enum**:
```
Shelf       (0) — 일반 선반
Showcase    (1) — 유리 진열장
WallMount   (2) — 벽걸이
Vending     (3) — 자판기형
LockedCase  (4) — 잠금 진열장
```

### 5.2 ItemDatabase (아이템 DB)

**생성**: `Create > Shop > Item Database`
**위치**: `Assets/Src/ScriptableObject/DataBase/ItemDatabase.asset`

- `items` 리스트에 모든 ItemData SO를 등록
- `OnEnable`/`OnValidate` 시 자동으로 인덱스 재구성
  - `_byId` — `Dictionary<int, ItemData>`
  - `_byName` — `Dictionary<string, ItemData>` (대소문자 무시)

**주요 API**:
```csharp
ItemData GetById(int id)                              // ID로 조회 (O(1))
ItemData GetByName(string name)                       // 이름으로 조회 (대소문자 무시)
List<ItemData> GetItemsByCategory(ItemCategory cat)    // 카테고리 필터
List<ItemData> GetRandomItems(ItemCategory cat, int n) // 랜덤 N개
bool TryGet(int itemId, out ItemData result)           // 안전한 조회
```

### 5.3 ClerkData (점원 정의)

**생성**: `Create > Shop > Clerk Data`

| 필드 | 타입 | 설명 |
|------|------|------|
| `clerkId` | int | 고유 ID |
| `clerkName` | string | 표시 이름 ("James") |
| `icon` | Sprite | UI 초상화 |
| `hiringCost` | float | 고용 비용 |
| `npcPrefab` | GameObject | 점원 NPC 프리팹 |

### 5.4 ClerkDatabase (점원 DB)

**위치**: `Assets/Src/ScriptableObject/DataBase/ClerkDatabase.asset`

- `clerks` 리스트에 ClerkData 등록
- `GetById(int id)` — ID로 점원 조회

### 5.5 현재 등록된 SO 에셋

```
ScriptableObject/
├── AK47.asset          (id:1002, MainWeapon, baseCost:300, unlock:3000, perBox:4)
├── MP5.asset           (MainWeapon)
├── Shotgun.asset       (MainWeapon)
├── pistol.asset        (MainWeapon)
├── sniper.asset        (MainWeapon)
├── helmet.asset        (ProtectiveGear)
├── Shelf.asset         (Furniture)
├── Poster.asset~5      (Decoration, unlock:100)
├── Ammo/
│   ├── 9mm Ammo.asset       (id:3001, Consumable, baseCost:300)
│   ├── Convel 30-06.asset
│   ├── Mosker 30-06 SPR.asset
│   └── Super Buck.asset
├── Clerk/
│   ├── Clerk 1.asset   (James, hiringCost:1000)
│   ├── Clerk 2~4.asset
├── DataBase/
│   ├── ItemDatabase.asset    ← 모든 아이템 여기에 등록
│   └── ClerkDatabase.asset   ← 모든 점원 여기에 등록
└── ShootingGun/
    └── Gun1~5.asset    (사격장 전용 총기)
```

---

## 6. 콘텐츠 추가 매뉴얼

### 6.1 새 상품(총기/장비/소모품) 추가

#### Step 1: 3D 모델 프리팹 준비
1. 3D 모델을 `Assets/Art/Prefabs/Market/Merchandise/`에 프리팹으로 저장
2. 프리팹에 **`ItemDataManager`** 컴포넌트 추가 (나중에 ItemData 연결)
3. `BoxCollider` 추가 (선반 진열/박스 시각화에 필요)

#### Step 2: ItemData SO 생성
1. `Assets/Src/ScriptableObject/`에서 우클릭 → `Create > Shop > Item`
2. 필드 설정:
   - `itemName`: 표시될 이름
   - `category`: MainWeapon / ProtectiveGear / Consumable 선택
   - `baseCost`: 원가 (이 기준으로 판매가 계산됨)
   - `perBoxCount`: 박스 하나에 들어가는 수량
   - `icon`: UI에 쓸 스프라이트 (512×512 권장)
   - `displayType`: Shelf (일반), WallMount (벽걸이) 등
   - `displayPrefab`: Step 1에서 만든 프리팹 연결
   - `unlockCost`: 카탈로그 해제 비용 (0이면 처음부터 해제)
3. `itemId`는 **자동 생성** — 수동으로 변경하지 마세요

#### Step 3: 프리팹에 ItemData 연결
1. Step 1의 프리팹 열기
2. `ItemDataManager` 컴포넌트의 `itemData` 필드에 Step 2의 SO 드래그

#### Step 4: ItemDatabase에 등록
1. `Assets/Src/ScriptableObject/DataBase/ItemDatabase.asset` 열기
2. `items` 리스트 사이즈 +1
3. 새 SO를 빈 슬롯에 드래그
4. **저장** (Ctrl+S) — 자동으로 인덱스 재구성됨

#### Step 5: 테스트
1. 게임 실행 → 모니터 → 카탈로그에서 새 아이템 확인
2. 잠금해제 → 주문 → 배송 박스 확인 → 선반 진열 → NPC 구매 확인

---

### 6.2 새 장식(Decoration) 추가

#### Step 1: 월드 배치용 프리팹 준비
1. 3D 모델을 `Assets/Art/Prefabs/Market/Decoration/`에 프리팹으로 저장
2. 프리팹에 다음 컴포넌트 추가:
   - **`FurniturePlaceable`** — 배치 시스템 연동
   - **`BoxCollider`** — 충돌 판정
   - **`Rigidbody`** — 물리 (배치 시 자동 kinematic 전환)
3. `FurniturePlaceable`의 `placementBounds` 필드에 BoxCollider 연결

#### Step 2: ItemData SO 생성
1. `Create > Shop > Item`
2. 설정:
   - `category`: **Decoration**
   - `unlockCost`: 구매 비용 (이것이 실제 가격)
   - `icon`: UI 아이콘
   - `displayPrefab`: 미리보기용 모델 (보통 worldPrefab과 같은 프리팹)
   - `worldPrefab`: **Step 1의 프리팹** (실제 월드에 배치되는 오브젝트)

#### Step 3: ItemDatabase에 등록 (6.1 Step 4와 동일)

#### 배치 흐름 (자동):
```
구매 → PurchaseProcessor → DecorationPlacementManager.EnqueuePlacement()
  → 배치 모드 진입 (녹색 비네트 + 배치 가능 구역 표시)
  → 플레이어가 위치 지정 → FurniturePlaceable.ExitPreview(placed:true)
  → PrefabName 자동 설정 → FurnitureSaveHandler가 저장
```

---

### 6.3 새 NPC 추가

#### Step 1: NPC 프리팹 생성
1. 기존 NPC 프리팹 복제 (예: `Assets/Art/Prefabs/Npc/Npcs/Npc 1.prefab`)
2. 3D 모델/머티리얼 교체
3. `NpcController` 컴포넌트 확인 (이미 포함되어 있음)
4. `NavMeshAgent` 컴포넌트 확인
5. `Animator` 컨트롤러 설정

#### Step 2: 스폰 매니저에 등록
1. 씬에서 `NpcSpawnManager` 오브젝트 선택
2. `npcPrefabs[]` 배열에 새 프리팹 추가
3. 스폰 확률은 균등 (배열 크기로 나눔)

---

### 6.4 새 경제 이벤트 추가

#### Step 1: 전략 클래스 생성
```csharp
// Assets/Src/Framework/InteractionBehaviours/SocialStrategy/NewEventStrategy.cs
using UnityEngine;

public class NewEventStrategy : MonoBehaviour, ISocialEventStrategy
{
    // 이벤트 이름 목록
    private string[] eventNames = {
        "새 이벤트 1",
        "새 이벤트 2"
    };

    private string eventName;
    private float marketModifier;

    public string EventName => eventName;
    public string StatusText => "상태 텍스트";
    public float MarketModifier => marketModifier;
    public bool IsGunRegulation => false;

    public void GetEventStrategyData()
    {
        eventName = eventNames[Random.Range(0, eventNames.Length)];
        marketModifier = Random.Range(0.1f, 0.4f);
    }
}
```

#### Step 2: SocialEventManager에 등록
1. `SocialEventManager.cs`에서 전략 배열에 새 전략 참조 추가
2. `SelectRandomStrategy()` 메서드에서 선택 확률 조정

---

### 6.5 새 점원(Clerk) 추가

#### Step 1: 점원 NPC 프리팹 생성
1. 기존 NPC 프리팹 복제 → 외형 교체
2. 점원용 애니메이터 설정

#### Step 2: ClerkData SO 생성
1. `Create > Shop > Clerk Data`
2. 설정: `clerkId`, `clerkName`, `icon`, `hiringCost`, `npcPrefab`

#### Step 3: ClerkDatabase에 등록
1. `Assets/Src/ScriptableObject/DataBase/ClerkDatabase.asset` 열기
2. `clerks` 리스트에 새 ClerkData 추가

---

### 6.6 새 튜토리얼 추가

#### Step 1: 튜토리얼 매니저 생성
1. `Assets/Src/Framework/Tutorial/`에 새 매니저 클래스 생성
2. 기존 `ExpansionTutorialManager.cs`를 참고하여 구현
3. **주의**: `Start()`에서 `root.SetActive(false)` 호출 금지 (메인 TutorialManager와 UI 공유)

#### Step 2: PlayerPrefs 키 등록
1. 완료 플래그: `PlayerPrefs.SetInt("NewTutorialDone", 1)`
2. `GameResetHelper.ResetAll()`에 새 키 삭제 추가

---

### 6.7 새 선반/가구 타입 추가

#### Step 1: 가구 프리팹 생성
1. 3D 모델에 **`FurniturePlaceable`** 컴포넌트 추가
2. 선반이면 **`ShelfSlots`** 컴포넌트 추가
   - `points[]` 배열에 아이템 배치 위치 Transform 연결 (최대 2개/슬롯)
   - `standPoint` — NPC가 서서 물건을 볼 위치
   - `priceCardParent` — 가격표 부착 위치

#### Step 2: ItemData SO 생성 (category: Furniture)
- `unlockCost`: 구매 비용
- `displayPrefab`: 미리보기 모델
- (worldPrefab은 Decoration만 사용, Furniture는 displayPrefab이 곧 배치 프리팹)

---

## 7. 남은 작업 및 알려진 이슈

### 🔴 높은 우선순위

| 항목 | 파일 | 상태 | 설명 |
|------|------|------|------|
| 결제 계산 로직 미구현 | `NpcPayment/PaymentContext.cs:18` | TODO | `Calculate()` 메서드가 비어있음. 세금/할인 로직 필요 |
| ESC로 결제 취소 불가 | `ESCInteractionBehaviours.cs:7` | TODO | 결제 진행 중 ESC 키로 탈출하는 기능 미구현 |
| 결제 메서드 위치 문제 | `NpcController.cs:117` | TODO | 결제 관련 메서드가 NpcController에 있음. NpcPayment 모듈로 이동 필요 |

### 🟡 중간 우선순위

| 항목 | 파일 | 상태 | 설명 |
|------|------|------|------|
| 잔액 부족 피드백 없음 | `ShopItemUnlockCELL.cs:107` | TODO | `Debug.Log("돈 부족")`만 있음. 사운드/트윈 애니메이션 추가 필요 |
| 개별 가격 표시 미구현 | `ItemData.cs:13` | TODO | 박스 내 개별 아이템 가격 표시 기능 |
| 가격 데이터 임시 코드 | `ProductPrice.cs:10-11` | FIXME | 변화된 가격값 제대로 받아오도록 수정 필요 |
| 가격설정 UI 임시 코드 | `PriceSettingController.cs:17` | 임시 | SO에서 데이터 가져오는 부분 임시 작성됨 |
| 진열가 0일 때 폴백 | `ShelfSlots.cs:149` | 테스트용 | 진열가가 0이면 시세를 대신 사용하는 임시 보호 로직 |

### 🟢 낮은 우선순위 / 정리 작업

| 항목 | 설명 |
|------|------|
| Debug.Log 정리 | 프로젝트 전체에 ~317개 Debug.Log. 출시 전 불필요한 것 제거 필요 |
| 디버그 도구 분리 | `Src/Debug/ShowMeTheMoney.cs`, `ProtestDirectorDebug.cs` 등 — 출시 빌드에서 제외 또는 `#if UNITY_EDITOR` 래핑 |
| Assets 루트 프리팹 정리 | `Calculator.prefab`, `Player.prefab` 등이 루트에 있음 — Unity Editor에서 적절한 폴더로 이동 |
| Untracked 파일 커밋 | 새로 추가된 기능 파일들(Clerk, Tutorial, NewsDesk 등)이 git에 아직 추가되지 않음 |
| PlayerPrefs → ES3 전환 | 확장/튜토리얼 플래그가 PlayerPrefs에 저장됨 — ES3 슬롯으로 통합하면 슬롯별 독립 관리 가능 |

### 🔵 신규 기능 (추가 개발 필요)

#### A. 매장 확장 — 2층 개방 및 다단계 확장

현재 매장 확장은 **뒷공간 개방 (Lv.2)** + **사격장 (Lv.3)** 2단계만 하드코딩되어 있음.
2층 개방 등 추가 확장 단계는 **아예 존재하지 않으며**, 구조 확장이 필요합니다.

**현재 구조의 한계**:
- `MarketExpansionSaveData`가 bool 2개뿐 (`marketPurchased`, `rangePurchased`)
- `MarketExtender`에 `PurchaseMarketExpansion()`, `PurchaseShootingRange()` 2개 메서드만 존재
- 확장 구매 UI (`MonitorUnlockUIContriller.cs`)가 빈 껍데기 (Start/Update만 있음)

**필요 작업**:
1. `MarketExpansionSaveData` 확장 — bool 배열 또는 enum 기반 단계 시스템으로 변경
2. `MarketExtender`에 단계별 확장 로직 추가 (2층 GameObject 토글, 벽 제거, 구역 활성화)
3. 2층 씬 구성 — `RealFinal 1.unity`에 2층 구조물 + NavMesh Surface 추가
4. 확장 구매 UI 구현 — `MonitorUnlockUIContriller.cs`에 단계별 카드/버튼 추가
5. `GameResetHelper`에 새 확장 키 리셋 추가
6. 튜토리얼 추가 (선택)

**참고할 기존 코드**:
- `MarketExtender.cs` — `ApplyExpansion()` 패턴 참고 (GameObject 토글 + 벽 제거 + 구역 조정)
- `ExpansionTutorialManager.cs` — 확장 튜토리얼 구조 참고

---

#### B. 업무별 알바생(점원) 시스템

현재 점원은 **1명만 고용 가능**하고, **자동 계산(스캔+결제)만** 수행.
업무에 따른 역할 분화와 다수 고용이 필요합니다.

**현재 구현 상태** (유지해야 할 것):
- ✅ 1명 고용/해고 (`ClerkHiringPanel`)
- ✅ 자동 계산 (`AutoClerkController.AutoProcessLoop`)
- ✅ 일하기/쉬기 토글 (`ClerkInteraction`)
- ✅ 세이브/로드 (`ClerkSaveHandler`)
- ✅ 하루 스폰/디스폰 연동

**필요 작업**:
1. **역할(Role) enum 추가** — `ClerkData`에 `ClerkRole` 필드 추가
   ```
   enum ClerkRole { Cashier, Restocker, Greeter }
   ```
2. **행동 분화** — 역할별 행동 클래스 생성 (전략 패턴 또는 상태머신)
   - `Cashier`: 현재 `AutoProcessLoop` 그대로
   - `Restocker`: 박스에서 아이템 꺼내 선반에 자동 진열 (`ShelfSlot.RegisterNewItem` 연동)
   - `Greeter`: NPC 안내 / 고객 만족도 버프 (선택)
3. **다수 고용** — `AutoClerkController` 싱글톤 → `List<ClerkInstance>` 기반으로 리팩터
4. **ClerkData 확장** — 능력치 (작업 속도, 효율), 일급/월급 필드 추가
5. **비용 구조** — 고용비(1회) + 일급(매일 차감) 분리 → `SettlementManager` 일일 정산에 반영
6. **ClerkSaveData 확장** — 다수 고용 상태 + 역할 저장
7. **고용 UI 확장** — `ClerkHiringPanel`에서 역할 표시 + 다수 고용 슬롯

**참고할 기존 코드**:
- `AutoClerkController.cs` — 현재 자동 계산 로직 (이것을 Cashier 행동으로 분리)
- `ClerkHiringPanel.cs` — 고용 UI 구조
- `NpcController.cs` + `NpcStates/` — NPC 상태머신 패턴을 점원에도 적용 가능

---

### 기능 완성도 요약

| 기능 | 완성도 | 비고 |
|------|:------:|------|
| 매장 운영 루프 (주문→진열→판매) | 95% | 핵심 완성, 가격 계산 세부 조정 필요 |
| NPC AI (쇼핑, 결제, 사격) | 90% | 상태머신 완성, 결제 계산 로직 빈 곳 있음 |
| 저장/불러오기 | 95% | 18개 핸들러 완성, 잘 동작함 |
| 모니터 UI (주문, 카탈로그, 고용) | 90% | 잔액 부족 피드백만 부족 |
| 경제 이벤트 시스템 | 90% | 4종 전략 완성 |
| 매장 확장 (1단계: 뒷공간+사격장) | 100% | ✅ 완성 — 로직+저장+튜토리얼 |
| **매장 확장 (2단계: 2층 등)** | **0%** | ❌ 미구현 — 구조 확장 필요 |
| 점원 — 기본 (1명, 자동 계산) | 100% | ✅ 완성 — 고용/해고/토글/저장 |
| **점원 — 업무 분화 (다수, 역할별)** | **0%** | ❌ 미구현 — Role enum + 행동 분화 필요 |
| 사격장 | 90% | BB탄 비주얼 추가됨 |
| 튜토리얼 | 85% | 5종 매니저 완성, 세부 스텝 조정 가능 |
| 가구/장식 배치 | 90% | 배치 모드 완성 |
| 뉴스 시스템 | 90% | TV 뉴스 + 신문 완성 |
| 사운드 | 80% | 기본 효과음 있음, 일부 UI 피드백 사운드 누락 |
| 밸런스 조정 | 70% | baseCost, unlockCost, 수요 등 튜닝 필요 |

---

## 8. 주의사항 및 함정

새 기능 추가나 수정 시 반드시 확인해야 할 사항들입니다.

### 세이브 시스템
- 새 `ISaveable` 핸들러 추가 시 **복원 순서** 고려 (위 테이블 참조)
- `FurnitureSaveHandler`는 `PrefabName == null`인 씬 기본 오브젝트를 건너뜀
  - `PrefabName`은 `DecorationPlacementManager`(배치 시)와 `FurnitureSaveHandler`(복원 시)에서만 설정
  - 씬에 미리 놓인 BaseShelf 등은 PrefabName이 null → 저장/복원 대상 아님
- `WallGunSaveHandler`는 `SmallBoxInteraction.gunPrefab` (private 필드)에 **리플렉션**으로 접근
  - 이 필드명을 변경하면 벽걸이 총 저장/로드가 깨짐
- `GameResetHelper.ResetAll()`은 PlayerPrefs 키도 삭제 (확장/튜토리얼 플래그)

### 튜토리얼
- 보조 튜토리얼 매니저 (Expansion, ShootingRange, Bouncer, SocialEvent)는 메인 TutorialManager와 **root/guideText UI 참조를 공유**
- 보조 매니저의 `Start()`에서 `root.SetActive(false)` 호출하면 **안 됨** — 메인 튜토리얼까지 숨겨짐

### Unity 관련
- 프리팹 내부 컴포넌트는 씬 YAML grep으로 찾을 수 없음 → **스크립트 GUID로 검색**
  - 예: `FurniturePlaceable` GUID: `e35dc408d9ab9844d8be84745ec562c1`
- `StarterAssets/FirstPersonController.cs`는 커스텀 수정됨 — 에셋 스토어 업데이트 시 주의

### 가격 시스템
- `PriceObserver`가 가격 변경을 이벤트로 전파
- `ShelfSlot`이 아이템 등록 시 `OnProductPlacedToFactory` 이벤트 발생 → `PriceCardFactory`가 가격표 생성
- 로드 시 `ShelfItemSaveHandler`가 `FirePriceCardEvent`를 호출하여 가격표 복원

### ES3 관련
- `ES3SlotManager.selectedSlotPath`는 **static** — 씬 전환 후에도 유지됨
- 새 게임 시 `GameResetHelper`가 null로 초기화
- `SaveManager.SaveGame()`에서 selectedSlotPath가 null이면 자동으로 새 슬롯 생성

---

## 9. 팀원 역할

| 이름 | 역할 | 담당 영역 |
|------|------|-----------|
| 윤여준 | PM | 프로젝트 관리 |
| 박정민 | 개발 총괄 | 데이터, 세이브/로드, 경제 시뮬, 가격 알고리즘, 평판 |
| 이준서 | 개발 | NPC AI, 결제 시스템, 매장 루프, 사회 이벤트, 시위 |
| 장지원 | 개발 | UI/UX, 진열/배치, 매장 확장, HUD, 카메라 |
| 이지연 | 개발 | 튜토리얼, 사운드, 사격장, 디버그 도구 |
