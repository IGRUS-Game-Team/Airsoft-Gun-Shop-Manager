using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 결제 완료된 장식품들을 큐에 넣고 순차적으로 설치 모드를 실행한다.
/// PurchaseProcessor에서 호출 → 모니터 종료 후 설치 모드 진입.
/// </summary>
public class DecorationPlacementManager : MonoBehaviour
{
    public static DecorationPlacementManager Instance { get; private set; }
    public static bool IsPlacementActive { get; private set; }

    [Header("설치 가능 영역")]
    [SerializeField] private GameObject[] zones;           // DecoZone 오브젝트들
    [SerializeField] private LayerMask zoneMask;           // DecoZone 레이어
    [SerializeField] private LayerMask blockingMask;       // 겹침 체크용 (DecoZone 제외!)

    [Header("프리뷰 머티리얼")]
    [SerializeField] private Material previewGhostMaterial;

    // 설치 대기 큐: (프리팹, 수량, decoMode, 단가)
    private readonly Queue<(GameObject prefab, int count, bool decoMode, float unitPrice)> _queue = new();
    private bool _placing;

    // Zone 하이라이트용 런타임 머티리얼 / 메시
    private Material _zoneHighlightMat;
    private Mesh _cubeMesh;

    // 초록 비네트 오버레이
    private GameObject _vignetteRoot;
    private RawImage _vignetteImage;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CreateVignetteOverlay();
    }

    // ──────────────────────────────────────────────
    // PurchaseProcessor에서 호출
    // ──────────────────────────────────────────────

    /// <summary>
    /// 설치형 프리팹을 설치 큐에 등록한다.
    /// decoMode=true → 장식품(Zone 벽면 배치), false → 가구/선반(바닥 배치).
    /// </summary>
    public void EnqueuePlacement(GameObject prefab, int count, bool decoMode, float unitPrice = 0f)
    {
        if (prefab == null || count <= 0) return;
        _queue.Enqueue((prefab, count, decoMode, unitPrice));
    }

    /// <summary>
    /// 큐에 대기 중인 장식품이 있는지 확인.
    /// PurchaseProcessor가 결제 후 이 값으로 분기한다.
    /// </summary>
    public bool HasPending => _queue.Count > 0;

    /// <summary>
    /// 설치 모드 시작. 모니터 종료 후 호출한다.
    /// Zone들을 초록색으로 표시하고, 첫 번째 장식품 프리뷰를 생성.
    /// </summary>
    public void StartPlacing()
    {
        if (_placing || _queue.Count == 0) return;
        _placing = true;
        IsPlacementActive = true;

        SetVignette(true);
        SpawnNext();
    }

    // ──────────────────────────────────────────────
    // 내부 로직
    // ──────────────────────────────────────────────

    private void SpawnNext()
    {
        if (_queue.Count == 0)
        {
            FinishAll();
            return;
        }

        var (prefab, count, decoMode, unitPrice) = _queue.Dequeue();

        // 장식품이면 Zone 시각화 ON, 가구면 OFF
        ShowZones(decoMode);

        // 프리팹 인스턴스 생성
        Camera cam = Camera.main;
        if (cam == null)
        {
            FinishAll();
            return;
        }

        Vector3 spawnPos = cam.transform.position + cam.transform.forward * 3f;
        GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity);

        // FurniturePlaceable 있으면 사용, 없으면 동적 추가
        var placeable = instance.GetComponent<FurniturePlaceable>();
        if (placeable == null)
            placeable = instance.AddComponent<FurniturePlaceable>();

        // 장식품 여부 + 단가 + 프리팹 이름 기록 (세이브/로드용)
        placeable.SetIsDecoration(decoMode);
        placeable.SetUnitPrice(unitPrice);
        placeable.PrefabName = prefab.name;

        // 프리뷰 머티리얼 주입
        if (previewGhostMaterial != null)
            placeable.SetPreviewGhostMaterial(previewGhostMaterial);

        // 나머지 수량은 다시 큐에 넣기
        if (count > 1)
            _queue.Enqueue((prefab, count - 1, decoMode, unitPrice));

        // PlacementManager로 설치 모드 시작
        if (PlacementManager.Instance == null) return;
        PlacementManager.Instance.BeginPlacement(placeable, decoMode: decoMode);
    }

    void Update()
    {
        if (!_placing) return;

        // PlacementManager가 설치/취소를 완료하면 다음 아이템으로
        if (!PlacementManager.Instance.IsPlacing)
        {
            SpawnNext();
        }
    }

    /// <summary>
    /// URP Lit 반투명 초록 머티리얼을 런타임 생성.
    /// </summary>
    private Material CreateZoneHighlightMaterial()
    {
        // URP Lit 셰이더 사용 (없으면 Standard 폴백)
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        var mat = new Material(shader);

        // Surface Type = Transparent
        mat.SetFloat("_Surface", 1f);         // 0=Opaque, 1=Transparent
        mat.SetFloat("_Blend", 0f);           // 0=Alpha
        mat.SetFloat("_AlphaClip", 0f);
        mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0f);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        // 반투명 초록색
        Color highlightColor = new Color(0.2f, 0.9f, 0.3f, 0.25f);
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", highlightColor);
        else
            mat.SetColor("_Color", highlightColor);

        return mat;
    }

    /// <summary>
    /// 빌트인 Cube 메시를 런타임 생성. MeshFilter가 없는 Zone 보정용.
    /// </summary>
    private static Mesh CreateCubeMesh()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var mesh = go.GetComponent<MeshFilter>().sharedMesh;
        Destroy(go);
        return mesh;
    }

    /// <summary>
    /// Zone 오브젝트들의 시각화(초록 반투명) ON/OFF.
    /// </summary>
    public void ShowZones(bool show)
    {
        if (zones == null) return;

        // 필요 시 하이라이트 머티리얼 생성
        if (show && _zoneHighlightMat == null)
            _zoneHighlightMat = CreateZoneHighlightMaterial();

        // 빌트인 Cube 메시 캐시 (MeshFilter 보정용)
        if (show && _cubeMesh == null)
            _cubeMesh = CreateCubeMesh();

        for (int i = 0; i < zones.Length; i++)
        {
            if (zones[i] == null) continue;
            zones[i].SetActive(show);

            // MeshFilter가 없으면 추가 + Cube 메시 할당 (BoxCollider와 동일 크기)
            var mf = zones[i].GetComponent<MeshFilter>();
            if (mf == null)
                mf = zones[i].AddComponent<MeshFilter>();
            if (mf.sharedMesh == null)
                mf.sharedMesh = _cubeMesh;

            var mr = zones[i].GetComponent<MeshRenderer>();
            if (mr == null)
                mr = zones[i].AddComponent<MeshRenderer>();

            if (show)
            {
                mr.sharedMaterial = _zoneHighlightMat;
                mr.enabled = true;
            }
            else
            {
                mr.sharedMaterial = null;
                mr.enabled = false;
            }
        }
    }

    /// <summary>
    /// 모든 장식품 설치 완료. Zone 숨기고, 커서 잠금 복원, 플레이어 조작 복원.
    /// </summary>
    private void FinishAll()
    {
        _placing = false;
        IsPlacementActive = false;
        ShowZones(false);
        SetVignette(false);
        TutorialEvents.RaiseAllFurniturePlaced();
    }

    // ──────────────────────────────────────────────
    // 초록 비네트 오버레이 (배치 모드 표시)
    // ──────────────────────────────────────────────

    private void CreateVignetteOverlay()
    {
        // 런타임 Canvas (항상 최상단에 표시)
        _vignetteRoot = new GameObject("DecoVignetteOverlay");
        _vignetteRoot.transform.SetParent(transform);

        var canvas = _vignetteRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        // RawImage (전체 화면)
        var imgObj = new GameObject("VignetteImage");
        imgObj.transform.SetParent(_vignetteRoot.transform, false);
        _vignetteImage = imgObj.AddComponent<RawImage>();

        var rt = _vignetteImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // 방사형 그라데이션 텍스처 생성
        _vignetteImage.texture = GenerateVignetteTexture(256);
        _vignetteImage.raycastTarget = false;

        _vignetteRoot.SetActive(false);
    }

    private Texture2D GenerateVignetteTexture(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float half = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - half) / half;
                float dy = (y - half) / half;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // 중심(0) → 투명, 최외곽만 살짝 초록 반투명
                float alpha = Mathf.Clamp01((dist - 0.8f) / 0.2f) * 0.10f;
                tex.SetPixel(x, y, new Color(0.2f, 0.9f, 0.3f, alpha));
            }
        }

        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    private void SetVignette(bool on)
    {
        if (_vignetteRoot != null)
            _vignetteRoot.SetActive(on);
    }

    // ──────────────────────────────────────────────
    // Scene뷰 Gizmos — 배치 상태 시각화
    // ──────────────────────────────────────────────
    void OnDrawGizmos()
    {
        // Zone 영역 항상 표시 (배치 중이면 초록, 아니면 회색)
        if (zones != null)
        {
            foreach (var z in zones)
            {
                if (z == null) continue;
                Gizmos.color = _placing ? new Color(0f, 1f, 0f, 0.3f) : new Color(0.5f, 0.5f, 0.5f, 0.15f);

                var col = z.GetComponent<Collider>();
                if (col is BoxCollider box)
                {
                    Gizmos.matrix = z.transform.localToWorldMatrix;
                    Gizmos.DrawCube(box.center, box.size);
                    Gizmos.color = _placing ? Color.green : Color.gray;
                    Gizmos.DrawWireCube(box.center, box.size);
                    Gizmos.matrix = Matrix4x4.identity;
                }
                else if (col is MeshCollider)
                {
                    Gizmos.DrawWireSphere(z.transform.position, 0.5f);
                }
                else
                {
                    Gizmos.DrawWireSphere(z.transform.position, 0.3f);
                }
            }
        }

        // 배치 모드 상태 텍스트 (Scene뷰에서 빨간 구)
        if (_placing)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}
