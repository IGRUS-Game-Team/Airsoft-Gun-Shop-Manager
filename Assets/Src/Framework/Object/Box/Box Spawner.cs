using UnityEditor;
using UnityEngine;


public class BoxSpawner : MonoBehaviour
{
    [SerializeField] SelectionManager selectionManager;
    [SerializeField] private float spawnRange = 3f;
    [SerializeField] private int spawnHeight = 7;
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
    public void BoxDrop(string prefabName, int count)
    {
        GameObject prefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");
        if (prefab == null)
        {
            Debug.LogError($"[BoxSpawner] 프리팹 로드 실패: {prefabName}");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-spawnRange, spawnRange), spawnHeight, Random.Range(-spawnRange, spawnRange));
            Vector3 spawnPos = basePosition + offset;

            GameObject box = Instantiate(prefab, spawnPos, Quaternion.identity);

        }


    }

}
