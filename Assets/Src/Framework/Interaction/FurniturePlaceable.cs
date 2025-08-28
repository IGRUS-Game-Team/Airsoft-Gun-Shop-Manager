using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))] // 루트에 가벼운 콜라이더 1개(Trigger 권장)
public class FurniturePlaceable : MonoBehaviour
{
    [Header("프리뷰(유령)용")]
    [SerializeField] private Material previewGhostMaterial; // M_PreviewGhost (URP Transparent)
    [SerializeField] private float holdSeconds = 0.6f;

    [Header("필수 참조")]
    [SerializeField] private BoxCollider placementBounds;   // 자식 "PlacementBounds"의 BoxCollider

    [Header("선택(있으면 더 정확)")]
    [SerializeField] private BlockOutLiner outliner;        // 하이라이트(있으면 자동 연결)

    // ── 내부 캐시 ─────────────────────────────────────────────
    private Renderer[] _renderers;
    private readonly List<Material[]> _originalMats = new();
    private Collider[] _allColliders;
    private Rigidbody _rb;
    private MaterialPropertyBlock _mpb; // ★ MPB 캐시

    private bool _isPreview;
    private float _holdTimer;
    private bool _mouseOver;

    public bool IsPreviewing => _isPreview; // 외부에서 프리뷰 여부 확인용

    void Awake()
    {
        if (outliner == null) outliner = GetComponent<BlockOutLiner>();
        if (placementBounds == null)
        {
            var child = transform.Find("PlacementBounds");
            if (child) placementBounds = child.GetComponent<BoxCollider>();
        }

        _renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in _renderers) _originalMats.Add(r.sharedMaterials);
        _allColliders = GetComponentsInChildren<Collider>(true);
        _rb = GetComponent<Rigidbody>();
        _mpb = new MaterialPropertyBlock();
    }

    void OnMouseEnter() => _mouseOver = true;
    void OnMouseExit()  { _mouseOver = false; _holdTimer = 0f; }

    void Update()
    {
        if (_isPreview) return; // 배치 모드 중에는 홀드 금지

        // 커서가 얹힌 상태에서 좌클릭 홀드 시간 누적
        if (_mouseOver && Input.GetMouseButton(0))
        {
            _holdTimer += Time.deltaTime;
            if (_holdTimer >= holdSeconds)
            {
                _holdTimer = 0f;
                PlacementManager.Instance.BeginPlacement(this);
            }
        }
        else
        {
            if (_holdTimer > 0f) _holdTimer -= Time.deltaTime * 2f;
        }
    }

    // ───────── 프리뷰/배치 제어(API) ─────────
    public void EnterPreview()
    {
        _isPreview = true;

        // Outliner 잠시 OFF (MPB 충돌 방지)
        if (outliner)
        {
            outliner.SetSelected(false);
            outliner.enabled = false;
        }

        // 물리/충돌 비활성
        if (_rb) { _rb.isKinematic = true; _rb.useGravity = false; }
        foreach (var c in _allColliders) c.enabled = false; // 겹침 체크는 OverlapBox로만

        // 프리뷰 머티리얼로 전환 (슬롯 수 유지)
        for (int i = 0; i < _renderers.Length; i++)
        {
            var r = _renderers[i];
            var mats = new List<Material>(_originalMats[i]);
            for (int m = 0; m < mats.Count; m++) mats[m] = previewGhostMaterial;
            r.sharedMaterials = mats.ToArray();
        }
    }

    public void ExitPreview(bool placed)
    {
        // 1) 프리뷰 색(MPB) 제거
        ClearPreviewTint();

        // 2) 원래 머티리얼 복구
        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].sharedMaterials = _originalMats[i];

        // 3) 물리/충돌 복구
        foreach (var c in _allColliders) c.enabled = true;
        if (_rb) { _rb.isKinematic = false; _rb.useGravity = true; }

        // 4) Outliner 다시 ON
        if (outliner)
        {
            outliner.enabled = true;
            outliner.SetSelected(false);
        }

        _isPreview = false;
    }

    // 프리뷰 색 적용(초록/빨강) — 모든 머티리얼 슬롯에 MPB 세팅
    public void SetPreviewTint(Color c)
    {
        if (_renderers == null) return;

        foreach (var r in _renderers)
        {
            _mpb.Clear();

            // 프리뷰용 머티 기준으로 속성명 선택 (_BaseColor or _Color)
            if (previewGhostMaterial && previewGhostMaterial.HasProperty("_BaseColor"))
                _mpb.SetColor("_BaseColor", c);
            else
                _mpb.SetColor("_Color", c);

            ApplyMPBToAllSlots(r, _mpb); // ★ 슬롯마다 적용
        }
    }

    // MPB 제거(원래 색 복귀) — 모든 슬롯에서 해제
    private void ClearPreviewTint()
    {
        if (_renderers == null) return;
        foreach (var r in _renderers)
        {
            int count = r.sharedMaterials != null ? r.sharedMaterials.Length : 1;
            for (int i = 0; i < count; i++)
                r.SetPropertyBlock(null, i);
        }
    }

    // 헬퍼: 모든 머티리얼 슬롯에 동일 MPB 적용
    private void ApplyMPBToAllSlots(Renderer r, MaterialPropertyBlock mpb)
    {
        int count = r.sharedMaterials != null ? r.sharedMaterials.Length : 1;
        for (int i = 0; i < count; i++)
            r.SetPropertyBlock(mpb, i);
    }

    public BoxCollider GetPlacementBounds() => placementBounds;
}