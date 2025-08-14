// UnlockedItemsStore.cs
using System.Collections.Generic;
using UnityEngine;

public class UnlockedItemsStore : MonoBehaviour, ISaveable
{
    public static UnlockedItemsStore Instance { get; private set; }
    private readonly HashSet<int> unlocked = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static bool IsUnlocked(int itemId) =>
        Instance != null && Instance.unlocked.Contains(itemId);

    public static void MarkUnlocked(int itemId) =>
        Instance?.unlocked.Add(itemId);

    // === Save ===
    public object CaptureData() => new List<int>(unlocked);
    public void RestoreData(object data)
    {
        unlocked.Clear();
        if (data is List<int> list) foreach (var id in list) unlocked.Add(id);
    }
}
