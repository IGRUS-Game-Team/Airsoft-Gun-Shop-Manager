using UnityEngine;

public class BoxContentVisualManager : MonoBehaviour
{
    [SerializeField] private BoxContainer box;
    [SerializeField] private Transform contentRoot;   // 박스 내부 바닥에 빈 오브젝트
    [SerializeField] private int columns = 4;
    [SerializeField] private float cellSize = 0.12f;
    [SerializeField] private float padding = 0.02f;
    [SerializeField] private int cap = 12;            // 너무 많을 때 상한
    [SerializeField] private Vector2 randomYawPitch = new Vector2(8f, 3f);

    [Header("Prefab Resolve")]
    [SerializeField] private GameObject fallbackCube; // 매핑 없을 때 대체 프리팹

    private GameObject[] spawned = System.Array.Empty<GameObject>();

    private void Awake()
    {
        if (!box) box = GetComponent<BoxContainer>();
    }

    private void OnEnable()
    {
        if (!box) return;
        box.OnChanged += OnBoxChanged;
        box.OnLidChanged += OnLidToggled;
    }

    private void OnDisable()
    {
        if (box)
        {
            box.OnChanged -= OnBoxChanged;
            box.OnLidChanged -= OnLidToggled;
        }
        Clear();
    }

    private void OnLidToggled(bool open)
    {
        if (open) Refresh();
        else SetActiveAll(false); // 닫힐 땐 비표시(필요하면 Destroy로 바꿔도 됨)
    }

    private void OnBoxChanged()
    {
        if (box.IsOpen) Refresh(); // 열려 있을 때만 동기화
    }

    private void Refresh()
    {
        if (!box || !box.IsOpen) return;

        int want = Mathf.Min(box.Remaining, cap);
        int have = spawned.Length;

        // 개수 맞추기
        if (want != have)
        {
            Clear();
            spawned = new GameObject[want];

            var start = GetStartPos(want);
            for (int i = 0; i < want; i++)
            {
                var pos = SlotToLocal(start, i);
                spawned[i] = SpawnOne(box.Item, pos);
            }
        }
        else
        {
            // 위치만 재정렬
            var start = GetStartPos(want);
            for (int i = 0; i < want; i++)
                if (spawned[i]) spawned[i].transform.localPosition = SlotToLocal(start, i);
        }

        SetActiveAll(true);
    }

    private void SetActiveAll(bool on)
    {
        foreach (var go in spawned) if (go) go.SetActive(on);
    }

    private void Clear()
    {
        foreach (var go in spawned) if (go) Destroy(go);
        spawned = System.Array.Empty<GameObject>();
    }

    private Vector3 GetStartPos(int total)
    {
        int rows = Mathf.CeilToInt((float)total / columns);
        float w = columns * cellSize;
        float h = rows * cellSize;
        return new Vector3(-w * 0.5f + padding, 0f, -h * 0.5f + padding);
    }

    private Vector3 SlotToLocal(Vector3 start, int index)
    {
        int cx = index % columns;
        int cy = index / columns;
        return start + new Vector3(cx * cellSize, 0f, cy * cellSize);
    }

    private GameObject SpawnOne(ItemData item, Vector3 localPos)
    {
        // ItemData에 displayPrefab 필드 추가했다고 했으니까 그거 우선 사용
        var prefab = (item != null && item.displayPrefab != null) ? item.displayPrefab : fallbackCube;

        var go = Instantiate(prefab, contentRoot);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity; // 필요하면 랜덤 틸트로 바꿔도 됨
        go.transform.localScale    = Vector3.one * 0.9f;

        // 박스 안에서 클릭 레이 방해 안 하도록
        var col = go.GetComponent<Collider>();
        if (col) col.enabled = false;

        return go;
    }

    private GameObject ResolvePrefab(ItemData item)
    {
        if (!item) return null;
        return item.displayPrefab != null ? item.displayPrefab : null;
    }
}
