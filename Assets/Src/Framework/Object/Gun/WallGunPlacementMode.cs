using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 총기를 WallGunSlot에 배치하는 모드를 관리하는 싱글턴 매니저.
/// SmallBox에서 총기를 꺼내면 이 모드에 진입하여,
/// 빈 WallGunSlot을 초록색으로 하이라이트하고 플레이어가 슬롯을 선택하면 배치한다.
/// </summary>
public class WallGunPlacementMode : MonoBehaviour
{
    public static WallGunPlacementMode Instance { get; private set; }
    public static bool IsActive { get; private set; }

    private GameObject _gun;                                    // 배치할 총기 인스턴스
    private WallGunSlot _focusedSlot;                           // 현재 바라보고 있는 슬롯
    private List<WallGunSlot> _emptySlots;                      // 하이라이트 대상
    private Dictionary<WallGunSlot, GameObject> _highlights;    // 슬롯별 하이라이트 오브젝트

    // 비네트 오버레이
    private GameObject _vignetteRoot;
    private RawImage _vignetteImage;

    // 하이라이트 머티리얼 (런타임 생성)
    private Material _normalHighlightMat;   // 초록 반투명 (빈 슬롯)
    private Material _focusedHighlightMat;  // 밝은 초록 (포커스된 슬롯)
    private Mesh _cubeMesh;

    // 총기 고스트 프리뷰
    private GameObject _ghost;
    private Material _ghostValidMat;    // 배치 가능 (초록)
    private Material _ghostInvalidMat;  // 배치 불가 (빨강)
    private Renderer[] _ghostRenderers;
    private Material _currentGhostMat;

    // ExitMode 후 같은 프레임에 InputContextRouter가 클릭을 통과시키지 않도록
    private bool _pendingDeactivate;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CreateVignetteOverlay();
    }

    // ──────────────────────────────────────────────
    // 공개 API
    // ──────────────────────────────────────────────

    /// <summary>
    /// 배치 모드 진입. 총기를 비활성화하고 빈 WallGunSlot들을 하이라이트한다.
    /// </summary>
    public void EnterMode(GameObject gun)
    {
        if (gun == null || IsActive) return;

        _gun = gun;
        _gun.SetActive(false);

        IsActive = true;

        // 빈 슬롯 검색
        _emptySlots = new List<WallGunSlot>();
        var allSlots = FindObjectsByType<WallGunSlot>(FindObjectsSortMode.None);
        foreach (var slot in allSlots)
        {
            if (slot.IsEmpty)
                _emptySlots.Add(slot);
        }

        // 하이라이트 생성
        CreateHighlights();

        // 고스트 프리뷰 생성
        CreateGunGhost();

        // 비네트 ON
        SetVignette(true);

        // 커서 해제 (배치 모드에서 자유 조준)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log($"[WallGunPlacement] EnterMode: {gun.name}, 빈 슬롯 {_emptySlots.Count}개");
    }

    /// <summary>
    /// 배치 모드 종료. placed=false면 총기를 바닥에 드롭한다.
    /// </summary>
    public void ExitMode(bool placed)
    {
        if (!IsActive) return;

        // 하이라이트 제거
        DestroyHighlights();

        // 고스트 제거
        if (_ghost != null) { Destroy(_ghost); _ghost = null; }
        _ghostRenderers = null;
        _currentGhostMat = null;

        // 비네트 OFF
        SetVignette(false);

        // 취소 시 총기 드롭
        if (!placed && _gun != null)
        {
            DropGun();
        }

        _gun = null;
        _focusedSlot = null;
        _emptySlots = null;

        // 같은 프레임에 InputContextRouter가 클릭을 통과시키지 않도록
        // IsActive를 다음 프레임에 해제
        _pendingDeactivate = true;

        Debug.Log($"[WallGunPlacement] ExitMode placed={placed}");
    }

    // ──────────────────────────────────────────────
    // Update — RaycastDetector + 입력 처리
    // ──────────────────────────────────────────────

    private void Update()
    {
        // ExitMode 다음 프레임에 IsActive 해제
        if (_pendingDeactivate)
        {
            _pendingDeactivate = false;
            IsActive = false;
        }

        if (!IsActive) return;

        // 기존 RaycastDetector가 감지한 오브젝트에서 WallGunSlot 찾기
        WallGunSlot hitSlot = null;
        var hitObj = (RaycastDetector.Instance != null) ? RaycastDetector.Instance.HitObject : null;

        if (hitObj != null)
        {
            hitSlot = hitObj.GetComponentInParent<WallGunSlot>();

            // 비어있지 않은 슬롯은 무시
            if (hitSlot != null && !hitSlot.IsEmpty)
                hitSlot = null;
        }

        // 포커스 갱신
        if (hitSlot != _focusedSlot)
        {
            // 이전 포커스 슬롯 → 기본 하이라이트로 복원
            if (_focusedSlot != null && _highlights != null && _highlights.ContainsKey(_focusedSlot))
            {
                var mr = _highlights[_focusedSlot].GetComponent<MeshRenderer>();
                if (mr != null) mr.sharedMaterial = _normalHighlightMat;
            }

            // 새 포커스 슬롯 → 밝은 하이라이트
            if (hitSlot != null && _highlights != null && _highlights.ContainsKey(hitSlot))
            {
                var mr = _highlights[hitSlot].GetComponent<MeshRenderer>();
                if (mr != null) mr.sharedMaterial = _focusedHighlightMat;
            }

            _focusedSlot = hitSlot;
        }

        // 고스트 프리뷰: 매 프레임 에임 따라 위치/머티리얼 갱신
        if (_ghost != null)
        {
            if (_focusedSlot != null)
            {
                // 빈 슬롯 위 → 스냅 위치 + 초록
                _focusedSlot.PositionAtSnap(_ghost.transform);
                ApplyGhostMaterial(_ghostValidMat);
            }
            else
            {
                // 슬롯 밖 → 카메라 앞 부유 + 빨강
                Camera cam = Camera.main;
                if (cam != null)
                {
                    _ghost.transform.position = cam.transform.position + cam.transform.forward * 3f;
                    _ghost.transform.rotation = cam.transform.rotation * Quaternion.Euler(0f, 90f, 0f);
                }
                ApplyGhostMaterial(_ghostInvalidMat);
            }
        }

        // LMB → 배치
        if (Input.GetMouseButtonDown(0) && _focusedSlot != null)
        {
            if (_focusedSlot.HangGunDirect(_gun))
            {
                _gun = null; // 소유권 이전 완료
                ExitMode(true);
            }
            return;
        }

        // RMB → 취소 (바닥 드롭)
        if (Input.GetMouseButtonDown(1))
        {
            ExitMode(false);
        }
    }

    // ──────────────────────────────────────────────
    // 총기 바닥 드롭
    // ──────────────────────────────────────────────

    private void DropGun()
    {
        if (_gun == null) return;

        _gun.SetActive(true);

        // 플레이어 앞 위치로 이동
        Camera cam = Camera.main;
        if (cam != null)
        {
            _gun.transform.position = cam.transform.position + cam.transform.forward * 1.5f;
            _gun.transform.rotation = Quaternion.identity;
        }

        // 부모 해제
        _gun.transform.SetParent(null);

        // 물리 활성화
        var blockHold = _gun.GetComponent<BlockIsHolding>();
        if (blockHold != null)
            blockHold.EnablePhysics();

        var rb = _gun.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.useGravity = true;
        }

        var col = _gun.GetComponentInChildren<Collider>();
        if (col != null) col.enabled = true;

        Debug.Log("[WallGunPlacement] 총기 바닥 드롭");
    }

    // ──────────────────────────────────────────────
    // 하이라이트 생성/제거
    // ──────────────────────────────────────────────

    private void CreateHighlights()
    {
        EnsureMaterials();

        if (_cubeMesh == null)
            _cubeMesh = CreateCubeMesh();

        _highlights = new Dictionary<WallGunSlot, GameObject>();

        foreach (var slot in _emptySlots)
        {
            var boxCol = slot.GetComponent<BoxCollider>();
            if (boxCol == null) continue;

            // 하이라이트 오브젝트 생성
            var hlObj = new GameObject("WallGunSlotHighlight");
            hlObj.transform.SetParent(slot.transform, false);
            hlObj.transform.localPosition = boxCol.center;
            hlObj.transform.localRotation = Quaternion.identity;
            hlObj.transform.localScale = boxCol.size;

            var mf = hlObj.AddComponent<MeshFilter>();
            mf.sharedMesh = _cubeMesh;

            var mr = hlObj.AddComponent<MeshRenderer>();
            mr.sharedMaterial = _normalHighlightMat;

            _highlights[slot] = hlObj;
        }
    }

    // ──────────────────────────────────────────────
    // 총기 고스트 프리뷰
    // ──────────────────────────────────────────────

    private void CreateGunGhost()
    {
        if (_gun == null) return;

        // 총기 복제 (비활성 상태에서도 메시 데이터는 유지됨)
        _ghost = Instantiate(_gun);
        _ghost.name = "GunPlacementGhost";

        // 물리/스크립트 비활성화 — 순수 비주얼 셸만 남긴다
        // (RequireComponent 의존성 때문에 Destroy 대신 disable)
        foreach (var mb in _ghost.GetComponentsInChildren<MonoBehaviour>(true))
            mb.enabled = false;
        foreach (var rb in _ghost.GetComponentsInChildren<Rigidbody>(true))
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
        foreach (var col in _ghost.GetComponentsInChildren<Collider>(true))
            col.enabled = false;

        // 렌더러 캐시 + 초기 머티리얼(빨강) 적용
        EnsureMaterials();
        _ghostRenderers = _ghost.GetComponentsInChildren<Renderer>(true);
        _currentGhostMat = null;
        ApplyGhostMaterial(_ghostInvalidMat);

        // 에임 추적용으로 바로 표시
        _ghost.SetActive(true);
    }

    private void ApplyGhostMaterial(Material mat)
    {
        if (mat == _currentGhostMat) return;   // 같으면 스킵
        _currentGhostMat = mat;
        if (_ghostRenderers == null) return;
        foreach (var rend in _ghostRenderers)
        {
            if (rend == null) continue;
            var mats = rend.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = mat;
            rend.sharedMaterials = mats;
        }
    }

    private void DestroyHighlights()
    {
        if (_highlights == null) return;

        foreach (var kv in _highlights)
        {
            if (kv.Value != null)
                Destroy(kv.Value);
        }
        _highlights.Clear();
        _highlights = null;
    }

    private void EnsureMaterials()
    {
        // 슬롯 하이라이트 — 하늘색 계열 (고스트와 색 구분)
        if (_normalHighlightMat == null)
            _normalHighlightMat = CreateHighlightMaterial(new Color(0.3f, 0.7f, 1f, 0.25f));
        if (_focusedHighlightMat == null)
            _focusedHighlightMat = CreateHighlightMaterial(new Color(0.4f, 0.85f, 1f, 0.45f));

        // 고스트 — 초록(배치 가능) / 빨강(배치 불가)
        if (_ghostValidMat == null)
            _ghostValidMat = CreateHighlightMaterial(new Color(0.2f, 0.9f, 0.3f, 0.35f));
        if (_ghostInvalidMat == null)
            _ghostInvalidMat = CreateHighlightMaterial(new Color(0.9f, 0.2f, 0.2f, 0.35f));
    }

    private static Material CreateHighlightMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        var mat = new Material(shader);

        // Surface Type = Transparent
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.SetFloat("_AlphaClip", 0f);
        mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0f);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        else
            mat.SetColor("_Color", color);

        return mat;
    }

    private static Mesh CreateCubeMesh()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var mesh = go.GetComponent<MeshFilter>().sharedMesh;
        Destroy(go);
        return mesh;
    }

    // ──────────────────────────────────────────────
    // 초록 비네트 오버레이
    // ──────────────────────────────────────────────

    private void CreateVignetteOverlay()
    {
        _vignetteRoot = new GameObject("GunPlacementVignetteOverlay");
        _vignetteRoot.transform.SetParent(transform);

        var canvas = _vignetteRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        var imgObj = new GameObject("VignetteImage");
        imgObj.transform.SetParent(_vignetteRoot.transform, false);
        _vignetteImage = imgObj.AddComponent<RawImage>();

        var rt = _vignetteImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

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
}
