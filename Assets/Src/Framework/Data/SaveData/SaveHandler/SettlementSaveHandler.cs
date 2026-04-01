using UnityEngine;

public class SettlementSaveHandler : MonoBehaviour, ISaveable
{
    private SettlementManager settlement;

    private void Awake()
    {
        settlement = SettlementManager.Instance ?? FindFirstObjectByType<SettlementManager>();
    }

    private void EnsureRefs()
    {
        if (settlement == null)
            settlement = SettlementManager.Instance ?? FindFirstObjectByType<SettlementManager>(FindObjectsInactive.Include);
    }

    public object CaptureData()
    {
        EnsureRefs();
        if (settlement == null) return null;

        return new SettlementSaveData
        {
            dayNumber            = settlement.DayNumber,
            satisfiedCustomers   = settlement.SatisfiedCustomers,
            dissatisfiedCustomers= settlement.DissatisfiedCustomers,
            shopLevel            = settlement.ShopLevel,
            expensiveComplaints  = settlement.ExpensiveComplaints,
            totalCustomersToday  = settlement.TotalCustomersToday,
            grossProfitToday     = settlement.GrossProfitToday,
            purchaseCostToday    = settlement.PurchaseCostToday,
            netProfitToday       = settlement.NetProfitToday
        };
    }

    public void RestoreData(object data)
    {
        EnsureRefs();
        var loaded = data as SettlementSaveData;
        if (loaded == null || settlement == null) return;

        settlement.RestoreState(loaded);
    }
}
