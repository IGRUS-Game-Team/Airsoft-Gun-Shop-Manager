using System;

public static class TutorialEvents
{
    // ── 기존 이벤트 ──
    public static event Action OnMoved;
    public static event Action OnPickedUp;
    public static event Action OnThrew;
    public static event Action OnItemPlacedOnShelf;
    public static event Action OnPriceSet;
    public static event Action OnShopOpened;
    public static event Action OnScanned;
    public static event Action OnCardPaid;
    public static event Action OnCashPaid;

    // ── 추가 이벤트 ──
    public static event Action OnBoxTrashed;
    public static event Action OnEnteredMonitor;
    public static event Action OnExitedMonitor;
    public static event Action<int> OnMonitorTabOpened;
    public static event Action OnItemUnlocked;
    public static event Action OnOrderAmountSet;
    public static event Action OnAddedToCart;
    public static event Action OnPurchased;
    public static event Action OnOrderBoxSpawned;
    public static event Action<int> OnScanTargetSet;
    public static event Action<int, int> OnScanProgressChanged;

    // ── 확장 튜토리얼 이벤트 ──
    public static event Action OnMarketExpanded;
    public static event Action OnAllFurniturePlaced;
    public static event Action OnShootingRangeExpanded;

    // ── 사회 이벤트 튜토리얼 이벤트 ──
    public static event Action OnSocialEventNewsDismissed;
    public static event Action OnBouncerCalled;

    // ── 기존 Raise ──
    public static void RaiseMoved() => OnMoved?.Invoke();
    public static void RaisePickedUp() => OnPickedUp?.Invoke();
    public static void RaiseThrew() => OnThrew?.Invoke();
    public static void RaiseItemPlacedOnShelf() => OnItemPlacedOnShelf?.Invoke();
    public static void RaisePriceSet() => OnPriceSet?.Invoke();
    public static void RaiseShopOpened() => OnShopOpened?.Invoke();
    public static void RaiseScanned() => OnScanned?.Invoke();
    public static void RaiseCardPaid() => OnCardPaid?.Invoke();
    public static void RaiseCashPaid() => OnCashPaid?.Invoke();

    // ── 추가 Raise ──
    public static void RaiseBoxTrashed() => OnBoxTrashed?.Invoke();
    public static void RaiseEnteredMonitor() => OnEnteredMonitor?.Invoke();
    public static void RaiseExitedMonitor() => OnExitedMonitor?.Invoke();
    public static void RaiseMonitorTabOpened(int index) => OnMonitorTabOpened?.Invoke(index);
    public static void RaiseItemUnlocked() => OnItemUnlocked?.Invoke();
    public static void RaiseOrderAmountSet() => OnOrderAmountSet?.Invoke();
    public static void RaiseAddedToCart() => OnAddedToCart?.Invoke();
    public static void RaisePurchased() => OnPurchased?.Invoke();
    public static void RaiseOrderBoxSpawned() => OnOrderBoxSpawned?.Invoke();
    public static void RaiseScanTargetSet(int total) => OnScanTargetSet?.Invoke(total);
    public static void RaiseScanProgressChanged(int current, int target) => OnScanProgressChanged?.Invoke(current, target);

    // ── 확장 튜토리얼 Raise ──
    public static void RaiseMarketExpanded() => OnMarketExpanded?.Invoke();
    public static void RaiseAllFurniturePlaced() => OnAllFurniturePlaced?.Invoke();
    public static void RaiseShootingRangeExpanded() => OnShootingRangeExpanded?.Invoke();

    // ── 사회 이벤트 튜토리얼 Raise ──
    public static void RaiseSocialEventNewsDismissed() => OnSocialEventNewsDismissed?.Invoke();
    public static void RaiseBouncerCalled() => OnBouncerCalled?.Invoke();
}
