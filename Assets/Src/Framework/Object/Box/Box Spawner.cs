using UnityEngine;

/// <summary>
/// 모니터에서 주문한 상품의 배송 박스를 스폰하는 매니저.
///
/// [스폰 흐름]
/// PurchaseProcessor.Purchase() → BoxSpawner.BoxDrop(itemId, boxCount)
///   → ItemDatabase에서 ItemData 조회
///   → deliveryBoxPrefab 인스턴스 생성 (basePosition 근처 랜덤 위치, spawnHeight 높이에서 낙하)
///   → BoxContainer.SetContent(itemData, perBoxCount) → 내용물 설정
///
/// [세이브/로드]
/// BoxSaveHandler가 RestoreBox(BoxSaveData)를 호출하여 저장된 위치/내용물로 복원.
///
/// [새 상품 추가 시]
/// ItemData SO만 잘 만들면 이 스크립트는 수정 불필요 — ItemDatabase.GetById()로 자동 조회.
/// </summary>
public class BoxSpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private float spawnRange = 3f;
    [SerializeField] private int spawnHeight = 7;
    [SerializeField] private GameObject deliveryBoxPrefab;
    [SerializeField] private Transform parentsBox;

    [Header("DB")]
    [SerializeField] private ItemDatabase itemDB; // id/name → ItemData

    private Vector3 basePosition;

    private void Awake()
    {
        basePosition = transform.position;
    }

    // 장바구니: amount = "박스 개수"
    // 표시명/카테고리는 넘기지 않음 (표시는 런타임에 OverrideStore로 해결)
    public void BoxDrop(int itemId, int boxCount)
    {
        var itemData = ResolveItem(itemId, null);
        if (itemData == null)
        {
            Debug.LogWarning($"[BoxSpawner] ItemData 못 찾음: id={itemId}");
            return;
        }

        int perBox = Mathf.Max(1, itemData.perBoxCount); // 한 박스당 내용물 수

        for (int i = 0; i < boxCount; i++)
        {
            Vector3 offset   = new Vector3(Random.Range(-spawnRange, spawnRange), spawnHeight, Random.Range(-spawnRange, spawnRange));
            Vector3 spawnPos = basePosition + offset;

            var box = Instantiate(deliveryBoxPrefab, spawnPos, Quaternion.identity, parentsBox);
            box.name = $"Box_{itemId}";

            var container = box.GetComponent<BoxContainer>();
            if (container != null)
            {
                container.SetContent(itemData, perBox);
            }
            else
            {
                Debug.LogWarning("[BoxSpawner] BoxContainer 컴포넌트가 없음");
            }
        }

        TutorialEvents.RaiseOrderBoxSpawned();
    }

    // === 세이브 로드용: SaveData 한 건 복구 ===
    public GameObject RestoreBox(BoxSaveData data)
    {
        if (deliveryBoxPrefab == null)
        {
            Debug.LogError("[BoxSpawner] deliveryBoxPrefab 미지정");
            return null;
        }

        var itemData = ResolveItem(data.itemId, null /*이름 폴백 필요 시 넣어도 됨*/);
        if (itemData == null)
        {
            Debug.LogWarning($"[BoxSpawner] ItemData 못 찾음: id={data.itemId}");
            return null;
        }

        // 저장된 정확 위치/회전으로 스폰 (원하면 랜덤 오프셋 제거/유지 선택)
        var box = Instantiate(deliveryBoxPrefab, data.position, data.rotation, parentsBox);
        box.name = $"Box_{data.itemId}";

        var container = box.GetComponent<BoxContainer>();
        if (container != null)
        {
            container.SetContent(itemData, Mathf.Max(0, data.amount));
            // if (data.isOpen) container.SetOpenImmediate(true); // 필요 시 사용
        }
        else
        {
            Debug.LogWarning("[BoxSpawner] BoxContainer 컴포넌트가 없음");
        }

        return box;
    }

    // 선택: 위치/회전만 복구하는 헬퍼 (원하면 사용)
    public GameObject RestoreBoxTransform(Vector3 pos, Quaternion rot)
    {
        if (deliveryBoxPrefab == null)
        {
            Debug.LogError("[BoxSpawner] deliveryBoxPrefab 미지정");
            return null;
        }
        return Instantiate(deliveryBoxPrefab, pos, rot, parentsBox);
    }

    // DB에서 ItemData 찾기 (id 우선)
    private ItemData ResolveItem(int itemId, string fallbackName)
    {
        if (itemDB == null) return null;

        var byId = itemDB.GetById(itemId);
        if (byId != null) return byId;

        if (!string.IsNullOrEmpty(fallbackName))
        {
            var byName = itemDB.GetByName(fallbackName);
            if (byName != null) return byName;
        }

        return null;
    }
}
