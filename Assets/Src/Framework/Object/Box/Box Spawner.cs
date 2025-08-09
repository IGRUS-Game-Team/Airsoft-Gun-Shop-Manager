using UnityEngine;

public class BoxSpawner : MonoBehaviour
{
    [SerializeField] private SelectionManager selectionManager;
    [SerializeField] private float spawnRange = 3f;
    [SerializeField] private int spawnHeight = 7;
    [SerializeField] private GameObject deliveryBoxPrefab;
    [SerializeField] private Transform parentsBox;
    [SerializeField] private ItemDatabase itemDB; // id/name → ItemData

    private Vector3 basePosition;

    private void Awake()
    {
        basePosition = transform.position;
    }

    // 장바구니 amount = "박스 개수"
    public void BoxDrop(int itemId, int amount, ItemCategory category, string itemName)
    {
        Debug.Log($"[BoxSpawner] drop id={itemId}, boxes={amount}, name={itemName}");
        var itemData = ResolveItem(itemId, category, itemName);
        if (itemData == null)
        {
            Debug.LogWarning($"[BoxSpawner] ItemData 못 찾음: id={itemId}, name={itemName}, cat={category}");
            return;
        }

        int perBox = Mathf.Max(1, itemData.perBoxCount); // 한 박스당 내용물 수

        for (int i = 0; i < amount; i++) // amount = 박스 개수
        {
            Vector3 offset   = new Vector3(Random.Range(-spawnRange, spawnRange), spawnHeight, Random.Range(-spawnRange, spawnRange));
            Vector3 spawnPos = basePosition + offset;

            GameObject box = Instantiate(deliveryBoxPrefab, spawnPos, Quaternion.identity, parentsBox);
            box.name = $"Box_{itemId}_{itemName}_{category}";

            var container = box.GetComponent<BoxContainer>(); // 단일 품목 컨테이너
            if (container != null)
            {
                container.SetContent(itemData, perBox); // 이 박스 안엔 perBox개가 들어감
            }
            else
            {
                Debug.LogWarning("[BoxSpawner] BoxContainer 컴포넌트가 없음");
            }

        }
    }

    public void RestoreBox(BoxSaveData data)
    {
        // 저장된 amount는 "남은 개수"로 해석
        Vector3 offset   = new Vector3(Random.Range(-spawnRange, spawnRange), 0, Random.Range(-spawnRange, spawnRange));
        Vector3 spawnPos = data.position + offset;

        GameObject box = Instantiate(deliveryBoxPrefab, spawnPos, data.rotation, parentsBox);
        box.name = $"Box_{data.itemId}_{data.itemName}_{data.category}";

        var container = box.GetComponent<BoxContainer>();
        if (container != null)
        {
            var itemData = ResolveItem(data.itemId, data.category, data.itemName);
            if (itemData != null)
            {
                container.SetContent(itemData, Mathf.Max(0, data.amount)); // 저장된 남은 수량 복원
                // if (data.isOpen && !container.IsOpen) container.ToggleLid();
                // else if (!data.isOpen && container.IsOpen) container.ToggleLid();
            }
        }
    }

    private ItemData ResolveItem(int itemId, ItemCategory category, string itemName)
    {
        if (itemDB != null)
        {
            var a = itemDB.GetById(itemId);
            if (a) return a;
            var b = itemDB.GetByName(itemName);
            if (b) return b;
        }
        return null;
    }
}
