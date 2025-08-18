using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 들고 있는 '박스' 안의 '아이템 모델' 크기에 맞춰 Green/Red 프리뷰를 띄우고,
/// 초록 위치일 때만 실제 배치(TryPlaceHeld)까지 처리한다.
///
/// - 콘텐츠 루트가 있으면 그 하위 렌더러/콜라이더만 사용
/// - 박스 외피는 레이어/태그/이름 힌트로 제외
/// - 월드 크기를 로컬 스케일로 변환(부모 스케일 보정)해 과대 스케일링 방지
/// - 배치 전 OverlapBox로 장애물 검사(본인/프리뷰/플레이어 레이어는 제외)
/// </summary>
public class PlacePreview : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerObjectHoldController holdController;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private GameObject previewGreen;   // 투명 초록 박스 프리팹
    [SerializeField] private GameObject previewRed;     // 투명 빨강 박스 프리팹

    [Header("Raycast & Range")]
    [SerializeField] private float placeRange = 3f;
    [Tooltip("바닥/선반 등 맞춰서 쏠 레이어 마스크(들고있는 것/프리뷰/플레이어는 제외)")]
    [SerializeField] private LayerMask groundMask;
    [Tooltip("정확한 표면 판정을 위해 태그까지 확인(예: SettableSurface)")]
    [SerializeField] private string settableTag = "SettableSurface";

    [Header("Inner Bounds Source")]
    [Tooltip("아이템 모델 루트(있으면 가장 정확). 없으면 자동 탐색")]
    [SerializeField] private Transform contentRootOverride;
    [Tooltip("가능하면 BoxCollider 크기를 우선 사용(더 타이트)")]
    [SerializeField] private bool useBoxColliderFirst = true;

    [Header("Exclude (Box Outer) Filters")]
    [Tooltip("박스 외형으로 간주할 레이어(제외)")]
    [SerializeField] private LayerMask boxLayerMask;
    [Tooltip("박스 외형으로 간주할 태그(제외)")]
    [SerializeField] private string[] boxLikeTags = new[] { "Box", "Crate", "Package" };
    [Tooltip("박스 외형으로 간주할 이름 힌트(제외)")]
    [SerializeField] private string[] boxNameHints = new[] { "Box", "Crate", "Lid", "Cap", "Case", "Package" };

    [Header("Sizing & Look")]
    [Tooltip("프리뷰 크기에 곱하는 미세 조정 계수(1=원본)")]
    [Range(0.8f, 1.2f)] [SerializeField] private float sizeScale = 1.00f;
    [Tooltip("바닥에 반쯤 파묻히지 않게 올리는 비율(0.5 = 높이의 절반)")]
    [Range(0f, 1f)] [SerializeField] private float liftByHeightRatio = 0.5f;

    [Header("Rotation")]
    [Tooltip("표면 법선 정렬(수평면은 Y-up 유지)")]
    [SerializeField] private bool alignToSurface = true;

    [Header("Placement")]
    [Tooltip("장애물 검사에 사용할 레이어 마스크(본인/프리뷰/플레이어는 제외)")]
    [SerializeField] private LayerMask obstacleMask;
    [Tooltip("들고있는 동안 임시로 적용할 레이어")]
    [SerializeField] private string heldLayerName = "HeldItem";
    [Tooltip("프리뷰 오브젝트 레이어")]
    [SerializeField] private string previewLayerName = "Preview";

    public bool   CanPlace        { get; private set; }
    public Vector3 PreviewPosition { get; private set; }

    // 캐시
    Transform cachedHeldRoot;
    Bounds   cachedInnerBounds;
    bool     cachedHasBounds;

    // =======================
    //   Public API
    // =======================

    public void UpdatePreview()
    {
        if (holdController == null || holdController.heldObject == null)
        {
            Hide();
            return;
        }

        // 1) 레이캐스트 (자기/프리뷰/플레이어 레이어는 groundMask에서 제외되어 있어야 함)
        Ray ray = new Ray(holdPoint.position, holdPoint.forward);
        bool hitOk = Physics.Raycast(ray, out RaycastHit hit, placeRange, groundMask, QueryTriggerInteraction.Ignore);
        if (!hitOk)
        {
            Hide();
            return;
        }

        // 2) 배치 가능 판정
        bool validSurface = string.IsNullOrEmpty(settableTag) ? true : hit.collider.CompareTag(settableTag);
        CanPlace = validSurface;
        PreviewPosition = hit.point;

        // 3) 프리뷰 토글
        if (previewGreen) previewGreen.SetActive(validSurface);
        if (previewRed)   previewRed.SetActive(!validSurface);

        // 4) 내부 모델 Bounds(월드) 계산 & 캐시
        Transform heldRoot = holdController.heldObject.transform;
        if (cachedHeldRoot != heldRoot)
        {
            cachedHeldRoot = heldRoot;
            cachedHasBounds = TryGetInnerBounds(heldRoot, out cachedInnerBounds);
        }
        if (!cachedHasBounds)
        {
            // 폴백: 아무것도 못 찾으면 전체 렌더러 하나라도 사용
            var anyR = heldRoot.GetComponentInChildren<Renderer>();
            if (anyR != null) { cachedInnerBounds = anyR.bounds; cachedHasBounds = true; }
        }
        if (!cachedHasBounds) return;

        // 5) 월드 크기/위치/회전
        Vector3 worldSize = cachedInnerBounds.size * sizeScale;
        Vector3 lifted    = PreviewPosition + Vector3.up * (worldSize.y * liftByHeightRatio);

        Quaternion rot = Quaternion.Euler(0f, heldRoot.eulerAngles.y, 0f);
        if (alignToSurface)
        {
            Vector3 n = hit.normal;
            if (Vector3.Angle(n, Vector3.up) >= 5f)
                rot = Quaternion.FromToRotation(Vector3.up, n) * Quaternion.Euler(0f, heldRoot.eulerAngles.y, 0f);
        }

        // 6) 적용(월드→로컬 스케일 변환 포함)
        ApplyPreview(previewGreen, worldSize, lifted, rot);
        ApplyPreview(previewRed,   worldSize, lifted, rot);
    }

    public void Hide()
    {
        if (previewGreen) previewGreen.SetActive(false);
        if (previewRed)   previewRed.SetActive(false);
        CanPlace = false;
    }

    /// <summary>
    /// 초록 프리뷰 위치로 실제 배치 시도.
    /// obstacleMask에 걸리면 실패, 성공 시 heldObject를 해당 위치/회전으로 이동.
    /// </summary>
    public bool TryPlaceHeld()
    {
        var held = holdController?.heldObject;
        if (!CanPlace || held == null || !cachedHasBounds) return false;

        // 최종 포즈는 프리뷰 기준
        Vector3 center = previewGreen.activeSelf ? previewGreen.transform.position
                                                 : previewRed.transform.position;
        Quaternion rot = previewGreen.activeSelf ? previewGreen.transform.rotation
                                                 : previewRed.transform.rotation;

        // OverlapBox 용 절반 크기 (살짝 축소하여 여유)
        Vector3 half = (cachedInnerBounds.size * 0.5f) * 0.98f;

        // 1) 자기 자신/프리뷰가 충돌 검사에 끼지 않게 임시 비활성화 + 레이어 전환
        var selfCols = held.GetComponentsInChildren<Collider>(includeInactive: true);
        var disabled = new List<Collider>(selfCols.Length);
        foreach (var c in selfCols) if (c.enabled) { c.enabled = false; disabled.Add(c); }

        int previewLayer = LayerMask.NameToLayer(previewLayerName);
        int heldLayer    = LayerMask.NameToLayer(heldLayerName);
        int prevGreenOld = previewGreen ? previewGreen.layer : 0;
        int prevRedOld   = previewRed   ? previewRed.layer   : 0;
        int heldOld      = held.gameObject.layer;

        if (previewLayer >= 0)
        {
            if (previewGreen) previewGreen.layer = previewLayer;
            if (previewRed)   previewRed.layer   = previewLayer;
        }
        if (heldLayer >= 0) held.gameObject.layer = heldLayer;

        // 2) 장애물 겹침 검사
        var hits = Physics.OverlapBox(center, half, rot, obstacleMask, QueryTriggerInteraction.Ignore);

        bool blocked = false;
        foreach (var h in hits)
        {
            if (!h) continue;
            // 혹시라도 본인/프리뷰가 들어오면 스킵
            if (h.attachedRigidbody && h.attachedRigidbody.gameObject == held.gameObject) continue;
            if (previewLayer >= 0 && h.gameObject.layer == previewLayer) continue;

            blocked = true; break;
        }

        // 3) 임시 조치 복구
        foreach (var c in disabled) c.enabled = true;
        if (previewLayer >= 0)
        {
            if (previewGreen) previewGreen.layer = prevGreenOld;
            if (previewRed)   previewRed.layer   = prevRedOld;
        }
        if (heldLayer >= 0) held.gameObject.layer = heldOld;

        if (blocked) return false;

        // 4) 실제 배치
        var rb = held.GetComponent<Rigidbody>();
        if (rb) { rb.isKinematic = true; rb.detectCollisions = false; } // 먼저 위치/회전 고정

        held.transform.SetParent(null, true);
        held.transform.SetPositionAndRotation(center, rot);

        if (rb) { rb.isKinematic = false; rb.detectCollisions = true; rb.useGravity = true; }

        Hide();
        holdController.heldObject = null;
        return true;
    }

    // =======================
    //   Internal
    // =======================

    void ApplyPreview(GameObject go, Vector3 worldSize, Vector3 pos, Quaternion rot)
    {
        if (!go) return;

        go.transform.position = pos;
        go.transform.rotation = rot;

        // 월드 크기를 로컬 스케일로 변환(부모 스케일 보정) — 과대 스케일링 방지
        Vector3 parentLossy = (go.transform.parent != null) ? go.transform.parent.lossyScale : Vector3.one;
        go.transform.localScale = new Vector3(
            SafeDiv(worldSize.x, Mathf.Abs(parentLossy.x)),
            SafeDiv(worldSize.y, Mathf.Abs(parentLossy.y)),
            SafeDiv(worldSize.z, Mathf.Abs(parentLossy.z))
        );
    }

    static float SafeDiv(float a, float b) => Mathf.Approximately(b, 0f) ? 0f : a / b;

    bool TryGetInnerBounds(Transform heldRoot, out Bounds sum)
    {
        // 우선 콘텐츠 루트
        Transform root = contentRootOverride != null ? contentRootOverride : heldRoot;

        // 1) BoxCollider 우선
        if (useBoxColliderFirst)
        {
            var cols = ListPool<BoxCollider>.Get();
            root.GetComponentsInChildren(true, cols);

            bool anyCol = false;
            sum = default;
            foreach (var c in cols)
            {
                if (!c || !c.enabled) continue;
                if (IsBoxLike(c.gameObject)) continue; // 외피 제외
                if (!anyCol) { sum = c.bounds; anyCol = true; }
                else sum.Encapsulate(c.bounds);
            }
            ListPool<BoxCollider>.Release(cols);

            if (anyCol) return true;
        }

        // 2) Renderer 합산
        var rs = ListPool<Renderer>.Get();
        root.GetComponentsInChildren(true, rs);

        bool any = false;
        sum = default;
        foreach (var r in rs)
        {
            if (!r || !r.enabled) continue;
            if (IsBoxLike(r.gameObject)) continue; // 외피 제외
            if (!any) { sum = r.bounds; any = true; }
            else sum.Encapsulate(r.bounds);
        }
        ListPool<Renderer>.Release(rs);

        return any;
    }

    bool IsBoxLike(GameObject go)
    {
        // 레이어 제외
        if (((1 << go.layer) & boxLayerMask.value) != 0) return true;

        // 태그 제외
        if (!string.IsNullOrEmpty(go.tag))
        {
            for (int i = 0; i < boxLikeTags.Length; i++)
                if (go.CompareTag(boxLikeTags[i])) return true;
        }

        // 이름 힌트 제외
        string n = go.name;
        for (int i = 0; i < boxNameHints.Length; i++)
            if (n.IndexOf(boxNameHints[i], StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

        return false;
    }

    // 간단 리스트 풀(할당 최소화)
    static class ListPool<T>
    {
        static readonly Stack<List<T>> pool = new();
        public static List<T> Get() => pool.Count > 0 ? pool.Pop() : new List<T>(32);
        public static void Release(List<T> list) { list.Clear(); pool.Push(list); }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!cachedHasBounds) return;
        // 프리뷰 기준으로 OverlapBox 범위 시각화
        GameObject refObj = previewGreen && previewGreen.activeInHierarchy ? previewGreen : previewRed;
        if (!refObj) return;

        Gizmos.color = Color.cyan;
        Matrix4x4 m = Matrix4x4.TRS(refObj.transform.position, refObj.transform.rotation, Vector3.one);
        Gizmos.matrix = m;
        Gizmos.DrawWireCube(Vector3.zero, cachedInnerBounds.size * 0.98f);
    }
#endif
}
