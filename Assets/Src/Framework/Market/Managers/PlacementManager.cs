using System.Collections.Generic;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance { get; private set; }

    [Header("필수 설정")]
    [SerializeField] private Collider storeArea;            // StoreArea의 BoxCollider
    [SerializeField] private LayerMask groundMask;          // Ground 레이어
    [SerializeField] private LayerMask blockerMask;         // PlacementBlocker | Furniture

    [Header("이동/회전")]
    [SerializeField] private float rotateSnap = 15f;        // Q/E 회전 단위(Shift 누르면 1도)
    [SerializeField] private float followHeightOffset = 0f; // 필요하면 살짝 띄움

    [Header("검증")]
    [SerializeField] private float groundCheckDown = 0.2f;  // 바닥 샘플 캐스트 길이
    [SerializeField] private float cornerProbeInset = 0.01f;// 모서리 샘플 살짝 안쪽

    private FurniturePlaceable _current;
    private bool _placing;
    private bool _mouseReleasedSinceBegin;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void BeginPlacement(FurniturePlaceable f)
    {
        if (_placing) return;
        _current = f;
        _placing = true;
        _mouseReleasedSinceBegin = false;
        _current.EnterPreview();
    }

    void Update()
    {
        if (!_placing || _current == null) return;

        // 1) 마우스 위치 → 바닥 레이캐스트
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 1000f, groundMask, QueryTriggerInteraction.Ignore))
        {
            var pb = _current.GetPlacementBounds();
            if (pb == null) return;

            // 프리뷰 오브젝트의 위치/높이 맞추기: 바닥 위에 '딱' 놓이도록
            // pb의 월드 높이(half)
            var half = Vector3.Scale(pb.size, pb.transform.lossyScale) * 0.5f;
            var worldCenter = pb.transform.TransformPoint(pb.center);

            // 가구 루트의 현재 회전 유지한 채, Y는 바닥 히트 + 반높이 + 오프셋
            var targetPos = new Vector3(hit.point.x, hit.point.y + half.y + followHeightOffset, hit.point.z);
            _current.transform.position = targetPos;

            // 회전 입력(Q/E, 휠)
            float step = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 1f : rotateSnap;
            if (Input.GetKeyDown(KeyCode.Q)) _current.transform.Rotate(0f, -step, 0f, Space.World);
            if (Input.GetKeyDown(KeyCode.E)) _current.transform.Rotate(0f,  step, 0f, Space.World);
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f) _current.transform.Rotate(0f, wheel * step, 0f, Space.World);
        }

        // 2) 조건 검증
        bool valid = IsValidPlacement(_current);

        // 3) 프리뷰 색: 초록/빨강
        _current.SetPreviewTint(valid ? new Color(0.3f, 1f, 0.4f, 0.5f) : new Color(1f, 0.3f, 0.3f, 0.5f));

        // 4) 확정/취소 입력
        if (!Input.GetMouseButton(0)) _mouseReleasedSinceBegin = true; // 홀드에서 손 뗀 뒤만 확정 허용
        if (_mouseReleasedSinceBegin && Input.GetMouseButtonDown(0))
        {
            if (valid) Confirm();
            else Bump(); // 무효면 살짝 튕김 효과 등(선택). 여기선 무시.
        }
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            Cancel();
        }
    }

    private void Confirm()
    {
        _current.ExitPreview(placed: true);
        _current = null;
        _placing = false;
    }

    private void Cancel()
    {
        _current.ExitPreview(placed: false);
        _current = null;
        _placing = false;
    }

    private void Bump() { /* 필요시 미세 진동/사운드 */ }

    // ───────── 검증 로직 ─────────
    private bool IsValidPlacement(FurniturePlaceable f)
    {
        var pb = f.GetPlacementBounds();
        if (pb == null || storeArea == null) return false;

        // 1) 매장 내부(AABB로 코너 검사)
        if (!AllCornersInsideStore(pb, storeArea)) return false;

        // 2) 바닥에 닿아있나(중앙+4코너 아래로 짧은 레이)
        if (!GroundedEverywhere(pb)) return false;

        // 3) 다른 콜라이더와 겹치지 않나(PlacementBounds의 OBB Overlap)
        if (OverlapsOthers(pb, f.gameObject)) return false;

        return true;
    }

    private bool AllCornersInsideStore(BoxCollider pb, Collider store)
    {
        var corners = GetWorldCorners(pb, cornerProbeInset);
        var bounds = store.bounds;
        foreach (var c in corners)
        {
            if (!bounds.Contains(c)) return false;
        }
        return true;
    }

    private bool GroundedEverywhere(BoxCollider pb)
    {
        var corners = GetFootprintPoints(pb, cornerProbeInset);
        foreach (var p in corners)
        {
            var origin = p + Vector3.up * 0.01f;
            if (!Physics.Raycast(origin, Vector3.down, out var hit, 0.05f + groundCheckDown, groundMask, QueryTriggerInteraction.Ignore))
                return false;
        }
        return true;
    }

    private bool OverlapsOthers(BoxCollider pb, GameObject owner)
    {
        var center = pb.transform.TransformPoint(pb.center);
        var half = Vector3.Scale(pb.size, pb.transform.lossyScale) * 0.5f;
        var hits = Physics.OverlapBox(center, half, pb.transform.rotation, blockerMask, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            if (!h || !h.enabled) continue;
            if (h.transform.IsChildOf(owner.transform)) continue; // 자기 자신은 무시
            return true;
        }
        return false;
    }

    // 바닥 면적 위 5점(중앙+4코너) 월드 좌표
    private IEnumerable<Vector3> GetFootprintPoints(BoxCollider pb, float inset)
    {
        var hl = Vector3.Scale(pb.size, pb.transform.lossyScale) * 0.5f;
        var rot = pb.transform.rotation;
        var center = pb.transform.TransformPoint(pb.center);
        // 바닥면(로컬로 -Y) 쪽에서 살짝 안쪽 샘플
        Vector3 right = rot * Vector3.right;
        Vector3 forward = rot * Vector3.forward;

        var p0 = center; // 중앙
        var p1 = center + ( right * (hl.x - inset)) + ( forward * (hl.z - inset));
        var p2 = center + (-right * (hl.x - inset)) + ( forward * (hl.z - inset));
        var p3 = center + ( right * (hl.x - inset)) + (-forward * (hl.z - inset));
        var p4 = center + (-right * (hl.x - inset)) + (-forward * (hl.z - inset));

        yield return p0;
        yield return p1;
        yield return p2;
        yield return p3;
        yield return p4;
    }

    // OBB의 8개 코너(상/하 4개) — 매장 AABB 내 포함 검사 용
    private Vector3[] GetWorldCorners(BoxCollider pb, float inset)
    {
        var list = new List<Vector3>();
        var hl = Vector3.Scale(pb.size, pb.transform.lossyScale) * 0.5f;
        var rot = pb.transform.rotation;
        var center = pb.transform.TransformPoint(pb.center);

        Vector3 right = rot * Vector3.right * (hl.x - inset);
        Vector3 up = rot * Vector3.up * (hl.y - inset);
        Vector3 fwd = rot * Vector3.forward * (hl.z - inset);

        list.Add(center + (right) + (up) + (fwd));
        list.Add(center + (right) + (up) + (-fwd));
        list.Add(center + (right) + (-up) + (fwd));
        list.Add(center + (right) + (-up) + (-fwd));
        list.Add(center + (-right) + (up) + (fwd));
        list.Add(center + (-right) + (up) + (-fwd));
        list.Add(center + (-right) + (-up) + (fwd));
        list.Add(center + (-right) + (-up) + (-fwd));
        return list.ToArray();
    }
    
    // 에디터에서 시각화(디버그)
    void OnDrawGizmosSelected()
    {
        if (_current == null) return;
        var pb = _current.GetPlacementBounds();
        if (pb == null) return;

        Gizmos.matrix = Matrix4x4.TRS(pb.transform.TransformPoint(pb.center), pb.transform.rotation, pb.transform.lossyScale);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, pb.size);
        Gizmos.matrix = Matrix4x4.identity;
    }
}