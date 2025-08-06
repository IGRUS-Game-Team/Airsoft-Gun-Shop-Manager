using UnityEditor;
using UnityEngine;


public class BoxSpawner : MonoBehaviour
{
    [SerializeField] SelectionManager selectionManager;
    [SerializeField] private float spawnRange = 3f;
    [SerializeField] private int spawnHeight = 7;
    [SerializeField] GameObject deliveryBoxPrefab;
    [SerializeField] Transform parentsBox;
    private Vector3 basePosition;


    private void Awake()
    {
        basePosition = transform.position;
    }


    /// <summary>
    /// 외부에서 호출하는 박스 생성 함수
    /// </summary>
    /// <param name="prefabName">Resources/Prefabs/ 안의 프리팹 이름</param>
    /// <param name="count">몇 개 생성할지</param>
    public void BoxDrop(int itemId, int amount, ItemCategory category, string itemName)
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-spawnRange, spawnRange), spawnHeight, Random.Range(-spawnRange, spawnRange));
            Vector3 spawnPos = basePosition + offset;

            GameObject box = Instantiate(deliveryBoxPrefab, spawnPos, Quaternion.identity, parentsBox);
            box.name = $"Box_{itemId}_{itemName}_{category}";

            var container = box.GetComponent<BoxItemContainer>();
            if (container != null)
                container.Setup(itemId, category, itemName);
        }
    }

    public void RestoreBox(BoxSaveData data)
    {
        Vector3 offset = new Vector3(Random.Range(-spawnRange, spawnRange), 0, Random.Range(-spawnRange, spawnRange));
        Vector3 spawnPos = data.position + offset;

        GameObject box = Instantiate(deliveryBoxPrefab, spawnPos, data.rotation, parentsBox);
        box.name = $"Box_{data.itemId}_{data.itemName}_{data.category}";

        var container = box.GetComponent<BoxItemContainer>();
        if (container != null)
            container.Setup(data.itemId, data.category, data.itemName);
    }
}
