using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour, ISaveable
{
    public static TutorialManager Instance { get; private set; }

    // ════════════════════════════════════════════
    //  Step 정의
    // ════════════════════════════════════════════
    public enum Step
    {
        Move = 0,        // WASD + Shift + Space
        Pickup,          // 박스 집기 (LMB)
        Throw,           // 던지기 (R)
        PlaceOnShelf,    // 선반 진열
        TrashBox,        // 빈 박스 휴지통에 버리기
        SetPrice,        // 가격표 설정
        OpenShop,        // 문 표지판 OPEN 전환
        Scan,            // 카운터 스캔
        Pay,             // 결제 (카드 + 현금 둘 다)
        StockOrderFlow,  // 모니터 주문 플로우 (8개 체크포인트)
        PickupOrderBox,  // 배달 박스 줍기
        OpenOrderBox,    // 박스 열기
        StockShelf,      // 진열
        Done
    }

    // ════════════════════════════════════════════
    //  StockOrderFlow 서브 체크포인트 (모니터 작업만)
    // ════════════════════════════════════════════
    enum OrderCheckpoint
    {
        EnterMonitor = 0,
        OpenUnlockTab,   // Unlock 탭 열기 (index 5)
        UnlockItem,
        OpenStockTab,    // Stock Order 탭 열기 (index 2)
        SetAmount,
        AddToCart,
        Purchase,
        ExitMonitor
    }
    const int OrderCheckpointCount = 8;

    // ════════════════════════════════════════════
    //  Inspector 필드
    // ════════════════════════════════════════════
    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI guideText;
    [Header("Arrow")]
    [SerializeField] private TutorialArrowIndicator arrowIndicator;

    [Header("Targets — Inspector 할당")]
    [SerializeField] private Transform trashBinTransform;
    [SerializeField] private Transform doorSignTransform;
    [SerializeField] private Transform counterTransform;
    [SerializeField] private Transform monitorTransform;
    [SerializeField] private Transform shelfTransform;

    [Header("Options")]
    [SerializeField] private bool enableTutorial = true;
    [SerializeField] private float advanceDelay = 0.6f;

    // ════════════════════════════════════════════
    //  내부 상태
    // ════════════════════════════════════════════
    private Step current = Step.Move;
    private bool isAdvancing;
    private bool uiDirty;

    // Move
    private bool didMove, didRun, didJump;

    // Pickup / Throw
    private bool didPickup;
    private bool didThrew;

    // PlaceOnShelf — BoxContainer 바인딩
    private bool didPlaced;
    private BoxContainer boundBox;
    private int boundBoxPrevRemaining;
    private int boundBoxInitialCount;

    // TrashBox
    private bool didTrashed;

    // SetPrice
    private bool didPriceSet;

    // OpenShop
    private bool didOpened;

    // Scan
    private int scanTarget, scanCurrent;

    // Pay
    private bool didPaidCard, didPaidCash;

    // StockOrderFlow
    private int orderProgress;

    // PickupOrderBox / OpenOrderBox / StockShelf
    private bool didPickupOrder;
    private bool didOpenedOrderBox;
    private bool didStockedShelf;


    // ════════════════════════════════════════════
    //  싱글톤 / 외부 조회
    // ════════════════════════════════════════════
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 튜토리얼 Pay 단계에서 아직 안 깬 결제 수단을 강제 반환한다.
    /// 카드 완료 → Cash 강제, 현금 완료 → Card 강제.
    /// 둘 다 안 깼거나 둘 다 깼으면 false (랜덤 허용).
    /// </summary>
    public bool TryGetForcedPaymentType(out PaymentType type)
    {
        type = PaymentType.Cash;
        if (!enableTutorial || current != Step.Pay) return false;

        if (didPaidCard && !didPaidCash) { type = PaymentType.Cash; return true; }
        if (didPaidCash && !didPaidCard) { type = PaymentType.Card; return true; }
        return false;
    }

    // ════════════════════════════════════════════
    //  생명주기
    // ════════════════════════════════════════════
    void OnEnable()
    {
        if (!enableTutorial) return;

        TutorialEvents.OnMoved += HandleMoved;
        TutorialEvents.OnPickedUp += HandlePickedUp;
        TutorialEvents.OnThrew += HandleThrew;
        TutorialEvents.OnItemPlacedOnShelf += HandlePlaced;
        TutorialEvents.OnPriceSet += HandlePriceSet;
        TutorialEvents.OnShopOpened += HandleShopOpened;
        TutorialEvents.OnScanned += HandleScanned;
        TutorialEvents.OnCardPaid += HandleCardPaid;
        TutorialEvents.OnCashPaid += HandleCashPaid;

        TutorialEvents.OnBoxTrashed += HandleBoxTrashed;
        TutorialEvents.OnEnteredMonitor += HandleEnteredMonitor;
        TutorialEvents.OnExitedMonitor += HandleExitedMonitor;
        TutorialEvents.OnMonitorTabOpened += HandleMonitorTabOpened;
        TutorialEvents.OnItemUnlocked += HandleItemUnlocked;
        TutorialEvents.OnOrderAmountSet += HandleOrderAmountSet;
        TutorialEvents.OnAddedToCart += HandleAddedToCart;
        TutorialEvents.OnPurchased += HandlePurchased;
        TutorialEvents.OnOrderBoxSpawned += HandleOrderBoxSpawned;
        TutorialEvents.OnScanTargetSet += HandleScanTargetSet;
        TutorialEvents.OnScanProgressChanged += HandleScanProgress;
    }

    void OnDisable()
    {
        if (!enableTutorial) return;

        TutorialEvents.OnMoved -= HandleMoved;
        TutorialEvents.OnPickedUp -= HandlePickedUp;
        TutorialEvents.OnThrew -= HandleThrew;
        TutorialEvents.OnItemPlacedOnShelf -= HandlePlaced;
        TutorialEvents.OnPriceSet -= HandlePriceSet;
        TutorialEvents.OnShopOpened -= HandleShopOpened;
        TutorialEvents.OnScanned -= HandleScanned;
        TutorialEvents.OnCardPaid -= HandleCardPaid;
        TutorialEvents.OnCashPaid -= HandleCashPaid;

        TutorialEvents.OnBoxTrashed -= HandleBoxTrashed;
        TutorialEvents.OnEnteredMonitor -= HandleEnteredMonitor;
        TutorialEvents.OnExitedMonitor -= HandleExitedMonitor;
        TutorialEvents.OnMonitorTabOpened -= HandleMonitorTabOpened;
        TutorialEvents.OnItemUnlocked -= HandleItemUnlocked;
        TutorialEvents.OnOrderAmountSet -= HandleOrderAmountSet;
        TutorialEvents.OnAddedToCart -= HandleAddedToCart;
        TutorialEvents.OnPurchased -= HandlePurchased;
        TutorialEvents.OnOrderBoxSpawned -= HandleOrderBoxSpawned;
        TutorialEvents.OnScanTargetSet -= HandleScanTargetSet;
        TutorialEvents.OnScanProgressChanged -= HandleScanProgress;

        UnbindStockBox();
    }

    void Start()
    {
        if (!enableTutorial)
        {
            if (root) root.SetActive(false);
            return;
        }

        // 튜토리얼 Canvas는 InGameSettingUI 블러보다 뒤에 표시
        Canvas parentCanvas = root != null ? root.GetComponentInParent<Canvas>() : null;
        if (parentCanvas != null)
        {
            parentCanvas.overrideSorting = true;
            parentCanvas.sortingOrder = 0;
        }

        if (root) root.SetActive(true);
        ResetFlagsForStep(current);
        RefreshUI();
        UpdateArrowTarget();
    }

    void Update()
    {
        if (!enableTutorial || isAdvancing) return;

        if (current == Step.Move)
        {
            if (!didMove && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                             Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)))
            {
                didMove = true;
                TutorialEvents.RaiseMoved();
                uiDirty = true;
                TryAdvance();
            }

            if (!didRun && Input.GetKey(KeyCode.LeftShift))
            {
                didRun = true;
                uiDirty = true;
                TryAdvance();
            }

            if (!didJump && Input.GetKeyDown(KeyCode.Space))
            {
                didJump = true;
                uiDirty = true;
                TryAdvance();
            }
        }

        // PlaceOnShelf: 들고 있는 박스 바인딩 시도
        if (current == Step.PlaceOnShelf)
        {
            TryBindStockBox();
        }

        // 모니터 이후 단계: 박스 바인딩
        if (current == Step.PickupOrderBox || current == Step.OpenOrderBox
            || current == Step.StockShelf)
        {
            TryBindStockBox();
        }
    }

    void LateUpdate()
    {
        if (!enableTutorial) return;

        if (uiDirty)
        {
            uiDirty = false;
            RefreshUI();
            UpdateArrowTarget();
        }
    }

    // ════════════════════════════════════════════
    //  UI 갱신
    // ════════════════════════════════════════════
    void RefreshUI()
    {
        if (!guideText) return;
        guideText.text = BuildSingleLineObjective();
    }

    string BuildSingleLineObjective()
    {
        switch (current)
        {
            case Step.Move:
                string m = didMove ? "<color=#00FF00>V</color>" : "□";
                string r = didRun ? "<color=#00FF00>V</color>" : "□";
                string j = didJump ? "<color=#00FF00>V</color>" : "□";
                return $"Move (WASD) {m}  Run (Shift) {r}  Jump (Space) {j}";

            case Step.Pickup:
                return "Pick up a box (Left Click)";

            case Step.Throw:
                return "Throw the box (R)";

            case Step.PlaceOnShelf:
                return BuildPlaceOnShelfText();

            case Step.TrashBox:
                return "Discard empty box in trash bin";

            case Step.SetPrice:
                return "Set a price on the white price tag";

            case Step.OpenShop:
                return "Switch the door sign to OPEN";

            case Step.Scan:
                if (scanTarget <= 0)
                    return "Wait for a customer, then scan items (Click)";
                return $"Scan items on the counter (Click) [{scanCurrent}/{scanTarget}]";

            case Step.Pay:
                string c = didPaidCard ? "<color=#00FF00>V</color>" : "□";
                string ca = didPaidCash ? "<color=#00FF00>V</color>" : "□";
                return $"Complete a payment — Card: {c}  Cash: {ca}";

            case Step.StockOrderFlow:
                return BuildOrderFlowText();

            case Step.PickupOrderBox:
                return "Pick up the delivered box";

            case Step.OpenOrderBox:
                return BuildOpenOrderBoxText();

            case Step.StockShelf:
                return BuildStockShelfText();

            case Step.Done:
                return "Tutorial complete! Keep serving customers to grow your shop.";
        }
        return "";
    }

    string BuildPlaceOnShelfText()
    {
        var hold = PlayerObjectHoldController.Instance;
        if (hold == null || hold.heldObject == null)
            return "Pick up a box";

        var box = hold.heldObject.GetComponent<BoxContainer>();
        if (box == null)
            return "Pick up a box";

        if (!box.IsOpen)
            return "Open the box (Left Click)";

        int total = boundBoxInitialCount;
        int placed = total - (boundBox != null ? boundBox.Remaining : 0);
        return $"Place items on a shelf [{placed}/{total}]";
    }

    string BuildOpenOrderBoxText()
    {
        var hold = PlayerObjectHoldController.Instance;
        if (hold == null || hold.heldObject == null)
            return "Pick up the box first";

        return "Open the box (Left Click)";
    }

    string BuildStockShelfText()
    {
        int total = boundBoxInitialCount;
        int placed = total - (boundBox != null ? boundBox.Remaining : 0);
        return $"Stock items on a shelf [{placed}/{total}]";
    }

    string BuildOrderFlowText()
    {
        int step = orderProgress + 1;
        switch ((OrderCheckpoint)orderProgress)
        {
            case OrderCheckpoint.EnterMonitor:  return $"[{step}/{OrderCheckpointCount}] Go to the monitor (Left Click)";
            case OrderCheckpoint.OpenUnlockTab: return $"[{step}/{OrderCheckpointCount}] Open the Unlock tab";
            case OrderCheckpoint.UnlockItem:    return $"[{step}/{OrderCheckpointCount}] Unlock an item";
            case OrderCheckpoint.OpenStockTab:  return $"[{step}/{OrderCheckpointCount}] Open the Stock Order tab";
            case OrderCheckpoint.SetAmount:     return $"[{step}/{OrderCheckpointCount}] Set order amount";
            case OrderCheckpoint.AddToCart:      return $"[{step}/{OrderCheckpointCount}] Add to cart";
            case OrderCheckpoint.Purchase:      return $"[{step}/{OrderCheckpointCount}] Purchase";
            case OrderCheckpoint.ExitMonitor:   return $"[{step}/{OrderCheckpointCount}] Exit the monitor (ESC)";
        }
        return $"[{OrderCheckpointCount}/{OrderCheckpointCount}] Complete!";
    }

    // ════════════════════════════════════════════
    //  화살표
    // ════════════════════════════════════════════
    void UpdateArrowTarget()
    {
        if (arrowIndicator == null) return;

        switch (current)
        {
            case Step.Pickup:
            case Step.PickupOrderBox:
                var box = Object.FindFirstObjectByType<BoxInteractionBehaviour>();
                if (box != null)
                    arrowIndicator.SetTarget(box.transform);
                else
                    arrowIndicator.ClearTarget();
                break;
            case Step.PlaceOnShelf:
                // 박스를 열어서 진열할 때만 선반 화살표 표시
                if (boundBox != null && boundBox.IsOpen)
                    arrowIndicator.SetTarget(shelfTransform);
                else
                    arrowIndicator.ClearTarget();
                break;
            case Step.TrashBox:
                arrowIndicator.SetTarget(trashBinTransform);
                break;
            case Step.SetPrice:
                var priceCard = Object.FindFirstObjectByType<PriceCardController>();
                if (priceCard != null)
                    arrowIndicator.SetTarget(priceCard.transform);
                else
                    arrowIndicator.ClearTarget();
                break;
            case Step.OpenShop:
                arrowIndicator.SetTarget(doorSignTransform);
                break;
            case Step.Scan:
                arrowIndicator.SetTarget(counterTransform);
                break;
            case Step.Pay:
                arrowIndicator.ClearTarget();
                break;
            case Step.StockOrderFlow:
                arrowIndicator.SetTarget(monitorTransform);
                break;
            case Step.StockShelf:
                arrowIndicator.SetTarget(shelfTransform);
                break;
            default:
                arrowIndicator.ClearTarget();
                break;
        }
    }

    // ════════════════════════════════════════════
    //  단계 진행
    // ════════════════════════════════════════════
    void TryAdvance()
    {
        if (isAdvancing) return;
        if (IsStepCompleted(current))
            StartCoroutine(AdvanceAfterDelay());
    }

    bool IsStepCompleted(Step step)
    {
        switch (step)
        {
            case Step.Move:            return didMove && didRun && didJump;
            case Step.Pickup:          return didPickup;
            case Step.Throw:           return didThrew;
            case Step.PlaceOnShelf:    return didPlaced;
            case Step.TrashBox:        return didTrashed;
            case Step.SetPrice:        return didPriceSet;
            case Step.OpenShop:        return didOpened;
            case Step.Scan:            return scanCurrent >= scanTarget && scanTarget > 0;
            case Step.Pay:             return didPaidCard && didPaidCash;
            case Step.StockOrderFlow:  return orderProgress >= OrderCheckpointCount;
            case Step.PickupOrderBox:  return didPickupOrder;
            case Step.OpenOrderBox:    return didOpenedOrderBox;
            case Step.StockShelf:      return didStockedShelf;
            case Step.Done:            return true;
        }
        return false;
    }

    IEnumerator AdvanceAfterDelay()
    {
        isAdvancing = true;
        yield return new WaitForSeconds(advanceDelay);

        UnbindStockBox();
        current = (Step)Mathf.Min((int)current + 1, (int)Step.Done);
        ResetFlagsForStep(current);
        RefreshUI();
        UpdateArrowTarget();

        isAdvancing = false;

        // Done 단계면 3초 후 숨김
        if (current == Step.Done)
            StartCoroutine(HideAfterDelay(3f));
    }

    IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (root != null) root.SetActive(false);
    }

    void ResetFlagsForStep(Step step)
    {
        switch (step)
        {
            case Step.Move:
                didMove = didRun = didJump = false;
                break;
            case Step.Pickup:
                didPickup = false;
                break;
            case Step.Throw:
                didThrew = false;
                break;
            case Step.PlaceOnShelf:
                didPlaced = false;
                break;
            case Step.TrashBox:
                didTrashed = false;
                break;
            case Step.SetPrice:
                didPriceSet = false;
                break;
            case Step.OpenShop:
                didOpened = false;
                break;
            case Step.Scan:
                scanTarget = 0;
                scanCurrent = 0;
                break;
            case Step.Pay:
                didPaidCard = didPaidCash = false;
                break;
            case Step.StockOrderFlow:
                orderProgress = 0;
                break;
            case Step.PickupOrderBox:
                didPickupOrder = false;
                break;
            case Step.OpenOrderBox:
                didOpenedOrderBox = false;
                break;
            case Step.StockShelf:
                didStockedShelf = false;
                break;
        }
    }

    // ════════════════════════════════════════════
    //  BoxContainer 바인딩 (PlaceOnShelf / 모니터 이후 단계)
    // ════════════════════════════════════════════
    void TryBindStockBox()
    {
        var hold = PlayerObjectHoldController.Instance;
        if (hold == null || hold.heldObject == null) { UnbindStockBox(); return; }

        var box = hold.heldObject.GetComponent<BoxContainer>();
        if (box == null) { UnbindStockBox(); return; }

        if (box == boundBox) return; // 이미 바인딩됨

        UnbindStockBox();
        boundBox = box;
        boundBoxPrevRemaining = box.Remaining;
        boundBoxInitialCount = box.Remaining;
        box.OnChanged += OnStockBoxChanged;
        box.OnLidChanged += OnStockBoxLidChanged;
        uiDirty = true;
    }

    void UnbindStockBox()
    {
        if (boundBox != null)
        {
            boundBox.OnChanged -= OnStockBoxChanged;
            boundBox.OnLidChanged -= OnStockBoxLidChanged;
            boundBox = null;
        }
    }

    void OnStockBoxChanged()
    {
        if (boundBox == null) return;

        // remaining이 줄었으면 진열했다는 뜻
        if (boundBox.Remaining < boundBoxPrevRemaining)
        {
            if (current == Step.PlaceOnShelf)
            {
                uiDirty = true;
                if (boundBox.Remaining <= 0)
                {
                    didPlaced = true;
                    TryAdvance();
                }
            }
            else if (current == Step.StockShelf)
            {
                uiDirty = true;
                if (boundBox.Remaining <= 0)
                {
                    didStockedShelf = true;
                    TryAdvance();
                }
            }
        }
        boundBoxPrevRemaining = boundBox.Remaining;
    }

    void OnStockBoxLidChanged(bool isOpen)
    {
        // OpenOrderBox 단계
        if (current == Step.OpenOrderBox && isOpen)
        {
            didOpenedOrderBox = true;
            TryAdvance();
        }
        uiDirty = true;
    }

    // ════════════════════════════════════════════
    //  StockOrderFlow 체크포인트 진행
    // ════════════════════════════════════════════
    void AdvanceOrderCheckpoint()
    {
        if (current != Step.StockOrderFlow) return;
        if (orderProgress >= OrderCheckpointCount) return;

        orderProgress++;
        uiDirty = true;
        TryAdvance();
    }

    // ════════════════════════════════════════════
    //  이벤트 핸들러
    // ════════════════════════════════════════════

    // --- 기존 이벤트 ---
    void HandleMoved() { }  // Move는 Update에서 처리

    void HandlePickedUp()
    {
        if (current == Step.Pickup)
        {
            didPickup = true;
            uiDirty = true;
            TryAdvance();
        }
        else if (current == Step.PickupOrderBox)
        {
            didPickupOrder = true;
            uiDirty = true;
            TryAdvance();
        }
    }

    void HandleThrew()
    {
        if (current != Step.Throw) return;
        didThrew = true;
        uiDirty = true;
        TryAdvance();
    }

    void HandlePlaced()
    {
        if (current == Step.PlaceOnShelf)
        {
            uiDirty = true;
            // didPlaced는 OnStockBoxChanged()에서 Remaining==0 체크로 처리
            if (boundBox != null && boundBox.Remaining <= 0)
            {
                didPlaced = true;
                TryAdvance();
            }
        }
        else if (current == Step.StockShelf)
        {
            uiDirty = true;
            if (boundBox != null && boundBox.Remaining <= 0)
            {
                didStockedShelf = true;
                TryAdvance();
            }
        }
    }

    void HandlePriceSet()
    {
        if (current != Step.SetPrice) return;
        didPriceSet = true;
        uiDirty = true;
        TryAdvance();
    }

    void HandleShopOpened()
    {
        if (current != Step.OpenShop) return;
        didOpened = true;
        uiDirty = true;
        TryAdvance();
    }

    void HandleScanned()
    {
        // 단일 스캔 이벤트(레거시) — ScanProgressChanged로 대체됨
    }

    void HandleCardPaid()
    {
        if (current != Step.Pay) return;
        didPaidCard = true;
        uiDirty = true;
        TryAdvance();
    }

    void HandleCashPaid()
    {
        if (current != Step.Pay) return;
        didPaidCash = true;
        uiDirty = true;
        TryAdvance();
    }

    // --- 추가 이벤트 ---
    void HandleBoxTrashed()
    {
        if (current != Step.TrashBox) return;
        didTrashed = true;
        uiDirty = true;
        TryAdvance();
    }

    void HandleEnteredMonitor()
    {
        if (current == Step.StockOrderFlow
            && orderProgress == (int)OrderCheckpoint.EnterMonitor)
        {
            AdvanceOrderCheckpoint();
        }
    }

    void HandleExitedMonitor()
    {
        if (current == Step.StockOrderFlow
            && orderProgress == (int)OrderCheckpoint.ExitMonitor)
        {
            AdvanceOrderCheckpoint();
        }
    }

    void HandleMonitorTabOpened(int index)
    {
        if (current != Step.StockOrderFlow) return;

        // index == 5 → Unlock 탭
        if (orderProgress == (int)OrderCheckpoint.OpenUnlockTab && index == 5)
        {
            AdvanceOrderCheckpoint();
        }
        // index == 2 → Stock Order 탭
        else if (orderProgress == (int)OrderCheckpoint.OpenStockTab && index == 2)
        {
            AdvanceOrderCheckpoint();
        }
    }

    void HandleItemUnlocked()
    {
        if (current == Step.StockOrderFlow
            && orderProgress == (int)OrderCheckpoint.UnlockItem)
        {
            AdvanceOrderCheckpoint();
        }
    }

    void HandleOrderAmountSet()
    {
        if (current == Step.StockOrderFlow
            && orderProgress == (int)OrderCheckpoint.SetAmount)
        {
            AdvanceOrderCheckpoint();
        }
    }

    void HandleAddedToCart()
    {
        if (current == Step.StockOrderFlow
            && orderProgress == (int)OrderCheckpoint.AddToCart)
        {
            AdvanceOrderCheckpoint();
        }
    }

    void HandlePurchased()
    {
        if (current == Step.StockOrderFlow
            && orderProgress == (int)OrderCheckpoint.Purchase)
        {
            AdvanceOrderCheckpoint();
        }
    }

    void HandleOrderBoxSpawned()
    {
        // 박스 스폰은 Purchase 시점에 일어남 — 별도 처리 불필요
    }

    void HandleScanTargetSet(int total)
    {
        if (current != Step.Scan) return;
        scanTarget = total;
        scanCurrent = 0;
        uiDirty = true;
    }

    void HandleScanProgress(int cur, int tot)
    {
        if (current != Step.Scan) return;
        scanCurrent = cur;
        scanTarget = tot;
        uiDirty = true;

        if (scanCurrent >= scanTarget && scanTarget > 0)
            TryAdvance();
    }

    // ════════════════════════════════════════════
    //  ISaveable
    // ════════════════════════════════════════════
    [System.Serializable]
    public class TutorialSaveData
    {
        public int stepIndex;
    }

    public object CaptureData()
    {
        return new TutorialSaveData { stepIndex = (int)current };
    }

    public void RestoreData(object data)
    {
        var loaded = data as TutorialSaveData;
        if (loaded == null) return;

        current = (Step)loaded.stepIndex;
        ResetFlagsForStep(current);
        uiDirty = true;

        if (current == Step.Done)
        {
            if (root != null) root.SetActive(false);
        }
    }
}
