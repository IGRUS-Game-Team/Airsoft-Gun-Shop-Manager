using System.Collections;
using UnityEngine;

/// <summary>
/// 카운터 슬롯 위 상품이 클릭되면
///   1) 왼쪽으로 슬라이드
///   2) 스캐너 지점 통과 시 삑
///   3) 봉투 지점에 닿으면 파괴 + 결제 완료
/// </summary>
public class CheckoutItemBehaviour : MonoBehaviour, IInteractable
{
    CounterManager manager;
    NpcController owner;
    Transform scanner, bag;
    AudioClip beep;
    float price;         // ★추가

    const float speed = 12f;   // 이동 속도
    bool moving, beeped;
    bool isInitialized = false;

    public CheckoutItemBehaviour Init(
        CounterManager mgr, NpcController npc,
        Transform scan, Transform bagPos, AudioClip clip)
    {
        manager = mgr;
        owner = npc;
        scanner = scan;
        bag = bagPos;
        beep = clip;
        isInitialized = true;
        price = GetComponent<ProductPrice>()?.Price ?? 0f;
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
        Debug.Log("ㅎㅇ 시발롬아");
        if (moving) return;
        moving = true;
        GetComponent<Collider>().enabled = false;   // 재클릭 방지
        StartCoroutine(MoveRoutine());
    }

    IEnumerator MoveRoutine()
    {
        Vector3 dir = (bag.position - transform.position).normalized;

        AudioSource src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;

        while (true)
        {
            transform.position += dir * speed * Time.deltaTime;

            // ① 스캐너 통과하면 삑 (한 번만)
            if (!beeped &&
                Vector3.Dot(transform.position - scanner.position, dir) > 0f)
            {
                src.PlayOneShot(beep);
                beeped = true;

                // CashRegister.Instance.AddPrice(price);   // ← 계산기 담당이 만든 메서드명으로 교체
            }

            // ② 봉투에 닿으면 파괴 + “상품 담김” 알림
            if (Vector3.Distance(transform.position, bag.position) < 0.05f)
            {
                manager.OnItemBagged(owner);      // ★변경: 상품만 담겼다고 알림
                Destroy(gameObject);
                yield break;
            }

            yield return null;
        }
    }
    
    void EnsureRaycastable()
{
    // 1) 레이어를 클릭용으로 고정 (없으면 Default로)
    int interactableLayer = LayerMask.NameToLayer("Interactable");
    gameObject.layer = (interactableLayer == -1) ? gameObject.layer : interactableLayer;

    // 2) 콜라이더가 없으면 붙이고, 있으면 전부 켜기
    var cols = GetComponentsInChildren<Collider>(true);
    if (cols == null || cols.Length == 0)
    {
        // 렌더 바운드 기반 BoxCollider 생성(대충이라도 클릭 가능하게)
        var rend = GetComponentInChildren<Renderer>();
        var bc = gameObject.AddComponent<BoxCollider>();
        if (rend != null)
        {
            var b = rend.bounds; // 월드 기준
            bc.center = transform.InverseTransformPoint(b.center);
            // 월드 사이즈를 로컬로 변환
            var sizeLocal = transform.InverseTransformVector(b.size);
            bc.size = new Vector3(Mathf.Abs(sizeLocal.x), Mathf.Abs(sizeLocal.y), Mathf.Abs(sizeLocal.z));
        }
    }
    else
    {
        foreach (var c in cols) c.enabled = true;
    }
}
}