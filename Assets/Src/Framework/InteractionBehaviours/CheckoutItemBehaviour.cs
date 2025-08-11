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
    Debug.Log($"[MoveRoutine] START for {gameObject.name}");
    Debug.Log($"bag={bag}, scanner={scanner}, owner={owner}, manager={manager}, counterSlotData={counterSlotData}");

    if (bag != null)
        Debug.Log($"Start distance to bag: {Vector3.Distance(transform.position, bag.position)}");

    // 시작 시 봉투 안에 이미 있는 경우
    if (bag != null && InsideRadius2D(transform.position, bag.position, bagRadius))
    {
        var usedSlot = transform.parent;
        Debug.Log($"[BAG-IMMEDIATE] owner={owner?.name}, item={gameObject.name}, parentSlot={usedSlot?.name}");

        if (usedSlot == null)
            Debug.LogWarning("[BAG-IMMEDIATE] usedSlot is NULL!");
        else
            CounterManager.Instance?.ReturnSlot(usedSlot);

        // 다음 아이템 시도
        if (owner == null)
            Debug.LogWarning("[BAG-IMMEDIATE] owner is NULL!");
        else
            owner.GetComponent<CarriedItemHandler>()?.PlaceToCounter();

        manager?.OnItemBagged(owner);
        Destroy(gameObject);
        yield break;
    }

    Vector3 dir = (bag != null) ? (bag.position - transform.position).normalized : Vector3.zero;
    Debug.Log($"[MOVE] dir={dir}");

    AudioSource src = gameObject.AddComponent<AudioSource>();
    src.playOnAwake = false;

    while (true)
    {
        transform.position += dir * speed * Time.deltaTime;

        // 스캐너 판정
        if (!beeped)
        {
            if (scanner == null)
            {
                Debug.LogWarning("[SCANNER] scanner is NULL!");
            }
            else
            {
                if (InsideRadius2D(transform.position, scanner.position, scanRadius))
                {
                    Debug.Log("[SCANNER] Inside scan radius");
                    if (Vector3.Dot(transform.position - scanner.position, dir) > 0f)
                    {
                        Debug.Log("[SCANNER] Passed scanner, triggering beep");
                        if (beep != null) src.PlayOneShot(beep);
                        else Debug.LogWarning("[SCANNER] beep clip is NULL");

                        beeped = true;

                        if (countorMonitorController == null)
                            Debug.LogWarning("[SCANNER] countorMonitorController is NULL!");
                        else
                            countorMonitorController.Show(counterSlotData);

                        if (cashRegisterUI == null)
                            Debug.LogWarning("[SCANNER] cashRegisterUI is NULL!");
                        else if (counterSlotData?.itemData == null)
                            Debug.LogWarning("[SCANNER] counterSlotData.itemData is NULL!");
                        else
                            cashRegisterUI.SetValues(
                                counterSlotData.itemData.baseCost - Random.Range(0, 100),
                                counterSlotData.itemData.baseCost
                            );

                        Debug.Log($"{price} 계산 시퀀스");
                    }
                }
            }
        }

        // 봉투 판정
        // if (bag == null)
        // {
        //     Debug.LogWarning("[BAG] bag is NULL!");
        // }
        // else if (InsideRadius2D(transform.position, bag.position, bagRadius))
        // {
        //     Debug.Log($"[BAG] Packed item={gameObject.name}, owner={owner?.name}");

        //     manager?.OnItemBagged(owner);
        //     Destroy(gameObject);
        //     yield break;
        // }
        // else
        // {
        //     Debug.Log($"[BAG] Not in radius yet. Dist={Vector3.Distance(transform.position, bag.position)}");
        // }
                // ② 봉투 반경 도착 → 슬롯 반납 + 다음 아이템 시도
        if (bag != null && InsideRadius2D(transform.position, bag.position, bagRadius))
        {
            var usedSlot = transform.parent;
            Debug.Log($"[BAG] Packed item={gameObject.name}, owner={owner?.name}, parentSlot={usedSlot?.name}");

            if (usedSlot != null)
                CounterManager.Instance.ReturnSlot(usedSlot);

            manager.OnItemBagged(owner);

            owner?.GetComponent<CarriedItemHandler>()?.PlaceToCounter();

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
}