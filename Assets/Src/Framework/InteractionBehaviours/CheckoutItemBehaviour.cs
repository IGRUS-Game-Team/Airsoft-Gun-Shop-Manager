using System.Collections;
using UnityEngine;

/// <summary>
/// 카운터 슬롯 위 상품이 클릭되면
///   1) 왼쪽으로 슬라이드
///   2) 스캐너 지점 통과 시 삑(넉넉한 반경 체크)
///   3) 봉투 지점에 닿으면 파괴 + 결제 완료(넉넉한 반경 체크)
/// </summary>
public class CheckoutItemBehaviour : MonoBehaviour, IInteractable
{
    CounterManager manager;
    NpcController owner;
    Transform scanner, bag;
    AudioClip beep;
    float price; // Optional: 가격 쓰고 싶으면 세팅해서 쓰면 됨

    private CountorMonitorController countorMonitorController;
    private CashRegisterUI cashRegisterUI;
    private CounterSlotData counterSlotData;

    const float speed = 12f;   // 이동 속도
    bool moving, beeped;
    bool isInitialized = false;

    [Header("넉넉한 판정 반경(인스펙터에서 조절)")]
    [SerializeField] float scanRadius = 0.6f;  // 스캐너 감지 반경
    [SerializeField] float bagRadius  = 0.8f;  // 봉투 감지 반경
    [SerializeField] bool drawGizmos  = true;  // 반경 기즈모 표시

    void Awake()
    {
        countorMonitorController = FindFirstObjectByType<CountorMonitorController>();
        cashRegisterUI           = FindFirstObjectByType<CashRegisterUI>();
        if (!countorMonitorController) Debug.Log("CountorMonitorController 없음");
    }

    public CheckoutItemBehaviour Init(
        CounterManager mgr, NpcController npc,
        Transform scan, Transform bagPos, AudioClip clip)
    {
        manager = mgr;
        owner   = npc;
        scanner = scan;
        bag     = bagPos;
        beep    = clip;
        isInitialized = true;

        counterSlotData = GetComponent<CounterSlotData>();
        if (counterSlotData) counterSlotData.itemObject = this.gameObject;

        // price = GetComponent<CounterSlotData>()?.itemData.baseCost ?? 0f;

        EnsureRaycastable();
        return this;
    }

    /* 플레이어가 클릭했을 때 자동 호출 (InputContextRouter 경유) */
    public void Interact()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("CheckoutItemBehaviour: 아직 Init되지 않음");
            return;
        }
        if (moving) return;

        moving = true;
        var col = GetComponent<Collider>();
        if (col) col.enabled = false;   // 재클릭 방지
        StartCoroutine(MoveRoutine());
    }

    IEnumerator MoveRoutine()
    {
        if (bag == null)
        {
            Debug.LogWarning("CheckoutItemBehaviour: bag Transform이 비어있음");
            yield break;
        }

        Vector3 dir = (bag.position - transform.position).normalized;

        AudioSource src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;

        while (true)
        {
            // 이동
            transform.position += dir * speed * Time.deltaTime;

            // ① 스캐너 통과하면 삑 (넉넉한 반경 + 진행방향 확인)
            if (!beeped && scanner != null)
            {
                // 수평 거리로 반경 판정(높이 차이는 무시)
                if (InsideRadius2D(transform.position, scanner.position, scanRadius))
                {
                    // 스캐너를 ‘지나쳤다’ 판정(스캐너 기준 앞으로 진행 중)
                    if (Vector3.Dot(transform.position - scanner.position, dir) > 0f)
                    {
                        if (beep) src.PlayOneShot(beep);
                        beeped = true;
                        countorMonitorController.Show(counterSlotData);
                        //cashRegisterUI.SetValues(counterSlotData.itemData.baseCost - Random.Range(0, 100), counterSlotData.itemData.baseCost);
                        Debug.Log($"{price} 계산 시퀀스");
                    }
                }
            }

            // ② 봉투 반경에 들어오면 포장 완료
            if (bag != null && InsideRadius2D(transform.position, bag.position, bagRadius))
            {
                manager.OnItemBagged(owner); // “상품 담김” 알림
                Destroy(gameObject);
                yield break;
            }

            yield return null;
        }
    }

    // 2D(수평) 반경 체크: y는 무시해서 판정 넉넉하게
    bool InsideRadius2D(Vector3 a, Vector3 b, float radius)
    {
        a.y = 0f; b.y = 0f;
        return (a - b).sqrMagnitude <= radius * radius;
    }

    void EnsureRaycastable()
    {
        // 1) 레이어를 클릭용으로 고정 (없으면 기존 레이어 유지)
        int interactableLayer = LayerMask.NameToLayer("Interactable");
        gameObject.layer = (interactableLayer == -1) ? gameObject.layer : interactableLayer;

        // 2) 콜라이더 없으면 박스 달아주고, 있으면 전부 켜기
        var cols = GetComponentsInChildren<Collider>(true);
        if (cols == null || cols.Length == 0)
        {
            var rend = GetComponentInChildren<Renderer>();
            var bc = gameObject.AddComponent<BoxCollider>();
            if (rend != null)
            {
                var b = rend.bounds; // 월드 기준
                bc.center = transform.InverseTransformPoint(b.center);
                var sizeLocal = transform.InverseTransformVector(b.size); // 월드→로컬
                bc.size = new Vector3(Mathf.Abs(sizeLocal.x), Mathf.Abs(sizeLocal.y), Mathf.Abs(sizeLocal.z));
            }
        }
        else
        {
            foreach (var c in cols) c.enabled = true;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Gizmos.matrix = Matrix4x4.identity;
        Color scanC = new Color(0f, 1f, 0.5f, 0.25f);
        Color bagC  = new Color(0.2f, 0.6f, 1f, 0.25f);

        if (scanner != null)
        {
            Gizmos.color = scanC;
            var p = scanner.position; p.y = 0f;
            Gizmos.DrawSphere(p, scanRadius);
        }

        if (bag != null)
        {
            Gizmos.color = bagC;
            var p = bag.position; p.y = 0f;
            Gizmos.DrawSphere(p, bagRadius);
        }
    }
#endif
}