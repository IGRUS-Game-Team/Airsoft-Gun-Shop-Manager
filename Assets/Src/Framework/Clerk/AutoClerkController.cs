using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AutoClerkController : MonoBehaviour
{
    public static AutoClerkController Instance { get; private set; }

    [SerializeField] GameObject clerkNpcPrefab;
    [SerializeField] Transform clerkSpawnPoint;
    [SerializeField] float scanIntervalSeconds = 1.0f;
    [SerializeField] float paymentDelaySeconds = 0.5f;
    [SerializeField] float nextCustomerDelay = 1.0f;
    [SerializeField] ClerkDatabase clerkDatabase;

    bool isHired;
    bool isWorking;
    int hiredClerkId = -1;
    GameObject clerkInstance;
    Coroutine autoProcessCoroutine;
    NpcController currentAutoNpc;

    public bool IsHired => isHired;
    public bool IsWorking => isWorking;
    public int HiredClerkId => hiredClerkId;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void HireClerk(ClerkData clerkData)
    {
        isHired = true;
        hiredClerkId = clerkData.clerkId;
        clerkNpcPrefab = clerkData.npcPrefab;
    }

    public void FireClerk()
    {
        isHired = false;
        isWorking = false;
        hiredClerkId = -1;
        StopAutoProcessing();
        DespawnClerk();
    }

    public void RestoreHiredClerk(int clerkId)
    {
        if (clerkDatabase == null) return;

        var data = clerkDatabase.GetById(clerkId);
        if (data != null)
            HireClerk(data);
    }

    public void OnNewDayStarted()
    {
        if (!isHired) return;

        SpawnClerkVisual();
        isWorking = true;
        StartAutoProcessing();
    }

    public void OnDayEnded()
    {
        StopAutoProcessing();
        isWorking = false;
        currentAutoNpc = null;
        DespawnClerk();
    }

    public void ToggleWorking()
    {
        if (clerkInstance == null) return;

        isWorking = !isWorking;

        if (isWorking)
            StartAutoProcessing();
        else
            StopAutoProcessing();
    }

    void SpawnClerkVisual()
    {
        if (clerkInstance != null) return;
        if (clerkNpcPrefab == null || clerkSpawnPoint == null) return;

        clerkInstance = Instantiate(clerkNpcPrefab, clerkSpawnPoint.position, clerkSpawnPoint.rotation);

        // NavMeshAgent 비활성화 (정적 비주얼)
        var agent = clerkInstance.GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        // NpcController 비활성화 (상태머신 비활성)
        var npcCtrl = clerkInstance.GetComponent<NpcController>();
        if (npcCtrl != null) npcCtrl.enabled = false;

        // CarriedItemHandler 비활성화
        var carried = clerkInstance.GetComponent<CarriedItemHandler>();
        if (carried != null) carried.enabled = false;

        // ClerkInteraction 컴포넌트 추가
        clerkInstance.AddComponent<ClerkInteraction>();

        // 레이어를 Interaction으로 변경
        int interactionLayer = LayerMask.NameToLayer("Interaction");
        if (interactionLayer != -1)
            SetLayerRecursive(clerkInstance, interactionLayer);

        // 콜라이더 활성화 확인
        var cols = clerkInstance.GetComponentsInChildren<Collider>(true);
        foreach (var c in cols) c.enabled = true;

        // Animator: Root Motion 끄기 + Standing 애니메이션 강제 재생
        var animator = clerkInstance.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.Play("Standing", 0, 0f);
            animator.Update(0f);
        }
    }

    void DespawnClerk()
    {
        if (clerkInstance != null)
        {
            Destroy(clerkInstance);
            clerkInstance = null;
        }
    }

    void StartAutoProcessing()
    {
        if (autoProcessCoroutine != null) return;
        autoProcessCoroutine = StartCoroutine(AutoProcessLoop());
    }

    void StopAutoProcessing()
    {
        if (autoProcessCoroutine != null)
        {
            StopCoroutine(autoProcessCoroutine);
            autoProcessCoroutine = null;
        }
        currentAutoNpc = null;
    }

    IEnumerator AutoProcessLoop()
    {
        while (true)
        {
            yield return null;

            if (!isWorking) continue;

            // 1) 줄 맨 앞 NPC 가져오기
            var frontNpc = QueueManager.Instance?.GetFrontNpc();
            if (frontNpc == null) continue;

            // 2) 체크아웃이 시작되었는지 확인
            if (CounterManager.Instance == null || !CounterManager.Instance.HasCheckoutStarted(frontNpc))
                continue;

            currentAutoNpc = frontNpc;

            // AUTO-SCAN: 모든 아이템 스캔
            while (true)
            {
                if (!isWorking || currentAutoNpc == null) break;
                if (frontNpc == null || frontNpc.PaymentDone) break;

                var item = FindNextCheckoutItem(frontNpc);
                if (item == null) break;

                item.Interact();
                yield return new WaitForSeconds(scanIntervalSeconds);
            }

            // WAIT FOR READY: 결제 준비 대기
            while (currentAutoNpc != null && !CounterManager.Instance.IsReadyToPay(frontNpc))
            {
                if (!isWorking || frontNpc == null) break;
                yield return null;
            }

            if (!isWorking || frontNpc == null)
            {
                currentAutoNpc = null;
                continue;
            }

            // NPC가 결제수단 꺼내는 시간 대기
            yield return new WaitForSeconds(paymentDelaySeconds);

            // FINAL GUARD
            if (frontNpc == null || frontNpc.PaymentDone)
            {
                currentAutoNpc = null;
                continue;
            }

            // 자동 결제 완료
            CounterManager.Instance.AutoCompletePayment(frontNpc);
            currentAutoNpc = null;

            yield return new WaitForSeconds(nextCustomerDelay);
        }
    }

    CheckoutItemBehaviour FindNextCheckoutItem(NpcController npc)
    {
        var items = FindObjectsOfType<CheckoutItemBehaviour>();
        foreach (var item in items)
        {
            if (item.Owner == npc && !item.IsMoving)
                return item;
        }
        return null;
    }

    void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }
}
