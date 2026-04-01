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
    [SerializeField] private float rotateSnap = 90f;        // Q/E 회전 단위(Shift 누르면 1도)
    [SerializeField] private float followHeightOffset = 0f; // 필요하면 살짝 띄움

    [Header("검증")]
    [SerializeField] private float groundCheckDown = 0.2f;  // 바닥 샘플 캐스트 길이
    [SerializeField] private float cornerProbeInset = 0.01f;// 모서리 샘플 살짝 안쪽

    [Header("장식품 Zone 설치")]
    [SerializeField] private LayerMask zoneMask;  // DecoZone 레이어
    private bool _isDecoMode;
    private Quaternion _decoLocalRotation = Quaternion.identity; // 벽면 위 추가 회전

    private FurniturePlaceable _current;
    private bool _placing;
    private bool _mouseReleasedSinceBegin;
    public bool IsPlacing => _placing;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // 기존 가구 + 장식품 공용. decoMode=true면 Zone 레이캐스트 사용.

    void Update()
    {
        if (!_placing || _current == null) return;

        // 1) 화면 중앙(에임) → 바닥/존 레이캐스트
        if (Camera.main == null) return;
        var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));

        LayerMask targetMask = _isDecoMode ? zoneMask : groundMask;
        QueryTriggerInteraction triggerMode = _isDecoMode
        ? QueryTriggerInteraction.Collide
        : QueryTriggerInteraction.Ignore;

        bool didHit = Physics.Raycast(ray, out var hit, 1000f, targetMask, triggerMode);

        // Q/E 회전 입력 — 히트 여부와 무관하게 항상 처리
        float step = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 1f : rotateSnap;
        float wheel = Input.mouseScrollDelta.y;

        if (_isDecoMode)
        {
            if (Input.GetKeyDown(KeyCode.Q)) _decoLocalRotation *= Quaternion.Euler(0f, -step, 0f);
            if (Input.GetKeyDown(KeyCode.E)) _decoLocalRotation *= Quaternion.Euler(0f,  step, 0f);
            if (Mathf.Abs(wheel) > 0.01f) _decoLocalRotation *= Quaternion.Euler(0f, wheel * step, 0f);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Q)) _current.transform.Rotate(0f, -step, 0f, Space.World);
            if (Input.GetKeyDown(KeyCode.E)) _current.transform.Rotate(0f,  step, 0f, Space.World);
            if (Mathf.Abs(wheel) > 0.01f) _current.transform.Rotate(0f, wheel * step, 0f, Space.World);
        }

        // 위치 갱신
        if (didHit)
        {
            var pb = _current.GetPlacementBounds();
            if (pb == null) return;

            var half = Vector3.Scale(pb.size, pb.transform.lossyScale) * 0.5f;

            if (_isDecoMode)
            {
                // ── 장식품: 벽 기준 배치 ──
                float depthOffset = half.z;
                Vector3 targetPos = hit.point + hit.normal * depthOffset;
                _current.transform.position = targetPos;

                Quaternion wallFacing = Quaternion.LookRotation(hit.normal, Vector3.up);
                _current.transform.rotation = wallFacing * _decoLocalRotation;
            }
            else
            {
                // ── 선반/가구: 바닥 기준 배치 ──
                var targetPos = new Vector3(hit.point.x, hit.point.y + half.y + followHeightOffset, hit.point.z);
                _current.transform.position = targetPos;
            }
        }
        else
        {
            // 레이캐스트가 아무것도 못 맞춤 → 카메라 전방 기본 위치로 (프리뷰가 항상 보이도록)
            Camera cam = Camera.main;
            _current.transform.position = cam.transform.position + cam.transform.forward * 3f;

            // 장식품: 벽 히트 없어도 회전은 반영
            if (_isDecoMode)
                _current.transform.rotation = _decoLocalRotation;
        }

        // 2) 조건 검증
        bool valid = _isDecoMode
        ? IsValidDecoPlacement(_current)
        : IsValidPlacement(_current);

        // 3) 프리뷰 색: 초록/빨강
        _current.SetPreviewTint(valid ? new Color(0.3f, 1f, 0.4f, 0.5f) : new Color(1f, 0.3f, 0.3f, 0.5f));

        // 4) 확정/취소 입력
        if (!Input.GetMouseButton(0)) _mouseReleasedSinceBegin = true; // 홀드에서 손 뗀 뒤만 확정 허용
        if (_mouseReleasedSinceBegin && Input.GetMouseButtonDown(0))
        {
            if (valid) Confirm();
            else Bump(); // 무효면 살짝 튕김 효과 등(선택). 여기선 무시.
        }
        if (Input.GetMouseButtonDown(1))
        {
            Cancel();
        }
    }

    private void Confirm()
    {
        _current.ExitPreview(placed: true);
        _current = null;
        _placing = false;

        // 단독 재편집 완료 시 Zone 숨김 (큐 배치 중에는 DecorationPlacementManager가 관리)
        if (_isDecoMode && !DecorationPlacementManager.IsPlacementActive
            && DecorationPlacementManager.Instance != null)
            DecorationPlacementManager.Instance.ShowZones(false);
    }

    private void Cancel()
    {
        // 환불: 단가가 기록되어 있으면 돌려줌
        float refund = _current.UnitPrice;
        if (refund > 0f && GameState.Instance != null)
            GameState.Instance.AddMoney(refund);

        Destroy(_current.gameObject);
        _current = null;
        _placing = false;

        // 단독 재편집 취소 시 Zone 숨김
        if (_isDecoMode && !DecorationPlacementManager.IsPlacementActive
            && DecorationPlacementManager.Instance != null)
            DecorationPlacementManager.Instance.ShowZones(false);
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

    // 에디터에서 시각화(디버그) — 배치 중이면 항상 표시
    void OnDrawGizmos()
    {
        if (_current == null) return;

        // 현재 오브젝트 위치에 라벨
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_current.transform.position, 0.3f);

        var pb = _current.GetPlacementBounds();
        if (pb != null)
        {
            // PlacementBounds 박스
            Gizmos.matrix = Matrix4x4.TRS(pb.transform.TransformPoint(pb.center), pb.transform.rotation, pb.transform.lossyScale);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, pb.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
        else
        {
            // PlacementBounds가 없으면 빨간 X 표시
            Gizmos.color = Color.red;
            var p = _current.transform.position;
            Gizmos.DrawLine(p + Vector3.left * 0.5f + Vector3.up * 0.5f, p + Vector3.right * 0.5f + Vector3.down * 0.5f);
            Gizmos.DrawLine(p + Vector3.left * 0.5f + Vector3.down * 0.5f, p + Vector3.right * 0.5f + Vector3.up * 0.5f);
        }

        // 레이캐스트 시각화
        if (Camera.main != null)
        {
            var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
            Gizmos.color = Color.green;
            Gizmos.DrawRay(ray.origin, ray.direction * 20f);
        }
    }

    // BeginPlacement 오버로드
    public void BeginPlacement(FurniturePlaceable f, bool decoMode = false)
    {
        if (_placing) return;
        _current = f;
        _placing = true;
        _isDecoMode = decoMode;
        _decoLocalRotation = Quaternion.identity;
        _mouseReleasedSinceBegin = false;

        // 장식품 재편집 시 Zone 하이라이트 표시
        if (decoMode && DecorationPlacementManager.Instance != null)
            DecorationPlacementManager.Instance.ShowZones(true);

        _current.EnterPreview();
    }

    // 장식품 전용 검증
    private bool IsValidDecoPlacement(FurniturePlaceable f)
    {
        var pb = f.GetPlacementBounds();
        if (pb == null) return false;

        // 1) Zone 위에 있는지
        var center = pb.transform.TransformPoint(pb.center);
        if (!Physics.CheckBox(center, Vector3.one * 0.01f,
            pb.transform.rotation, zoneMask, QueryTriggerInteraction.Collide))
            return false;

        // 2) 방향 검증: 장식품 뒷면(-forward)이 Zone(벽)에 닿아 있고, 벽 법선과 정렬되어야 함
        Vector3 back = -f.transform.forward;
        if (Physics.Raycast(center, back, out var wallHit, 0.5f, zoneMask, QueryTriggerInteraction.Collide))
        {
            // 벽 법선과 장식품 forward 사이 각도 체크 (정렬됐으면 ~0도)
            float angle = Vector3.Angle(f.transform.forward, wallHit.normal);
            if (angle > 15f) return false;
        }
        else
        {
            // 뒤에 Zone이 없으면 invalid (벽에 붙어있지 않음)
            return false;
        }

        // 3) 다른 장식품과 겹치지 않는지
        if (OverlapsOthers(pb, f.gameObject)) return false;

        return true;
    }
}
