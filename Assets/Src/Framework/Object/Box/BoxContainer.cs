using System;
using UnityEngine;

public class BoxContainer : MonoBehaviour
{
    [Header("Lid (Left / Right)")]
    [SerializeField] private Transform lidLeft;
    [SerializeField] private Transform lidRight;
    [SerializeField] private Vector3 lidClosedEuler = Vector3.zero;
    [SerializeField] private float lidLeftOpenZ  = 228f;    // 왼쪽 뚜껑 Z
    [SerializeField] private float lidRightOpenZ = -228f;   // 오른쪽 뚜껑 Z
    [SerializeField] private float lidAnimTime   = 0.2f;

    [Header("Content (Single Item)")]
    [SerializeField] private ItemData item;  // 이 박스에 담긴 단일 품목
    [SerializeField] private int remaining;  // 남은 개수

    public bool IsOpen { get; private set; }
    public ItemData Item => item;
    public int Remaining => remaining;

    /// <summary>박스 내용물/상태가 바뀔 때(UI/비주얼 갱신용)</summary>
    public event Action OnChanged;

    /// <summary>뚜껑이 열리거나 닫힐 때. 인자: true=열림, false=닫힘</summary>
    public event Action<bool> OnLidChanged;

    // ====== 런타임 API ======

    /// <summary>장바구니 생성 직후 박스에 담길 품목/수량 주입</summary>
    public void SetContent(ItemData contentItem, int count)
    {
        item = contentItem;
        remaining = Mathf.Max(0, count);
        OnChanged?.Invoke();
        Debug.Log($"[BoxContainer] SetContent {contentItem?.name} x{count} (open={IsOpen})");
    }

    /// <summary>박스에서 1개 꺼내기(진열 등)</summary>
    public bool TakeOne()
    {
        if (item == null || remaining <= 0) return false;
        remaining--;
        OnChanged?.Invoke();
        return true;
    }

    /// <summary>뚜껑 열기/닫기 토글 (양쪽 회전 동기화)</summary>
    public void ToggleLid()
    {
        if (!lidLeft && !lidRight) return;

        StopAllCoroutines();
        StartCoroutine(RotateLids(!IsOpen));
        IsOpen = !IsOpen;

        // 외부(비주얼 매니저 등)에서 열림/닫힘 트리거로 쓰도록 이벤트 발행
        OnLidChanged?.Invoke(IsOpen);
    }

    private System.Collections.IEnumerator RotateLids(bool open)
    {
        var startL = lidLeft  ? lidLeft.localRotation  : Quaternion.identity;
        var startR = lidRight ? lidRight.localRotation : Quaternion.identity;

        Quaternion targetL, targetR;
        if (open)
        {
            targetL = Quaternion.Euler(lidClosedEuler.x, lidClosedEuler.y,  lidLeftOpenZ);
            targetR = Quaternion.Euler(lidClosedEuler.x, lidClosedEuler.y, lidRightOpenZ);
        }
        else
        {
            var closed = Quaternion.Euler(lidClosedEuler);
            targetL = closed; targetR = closed;
        }

        float t = 0f;
        while (t < lidAnimTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / lidAnimTime);
            if (lidLeft)  lidLeft.localRotation  = Quaternion.Slerp(startL, targetL, k);
            if (lidRight) lidRight.localRotation = Quaternion.Slerp(startR, targetR, k);
            yield return null;
        }
        if (lidLeft)  lidLeft.localRotation  = targetL;
        if (lidRight) lidRight.localRotation = targetR;
    }

    /// <summary>복원 시 즉시 각도 반영이 필요할 때 호출</summary>
    private void ApplyLidImmediate(bool open)
    {
        if (!lidLeft && !lidRight) return;

        if (open)
        {
            if (lidLeft)  lidLeft.localRotation  = Quaternion.Euler(lidClosedEuler.x, lidClosedEuler.y,  lidLeftOpenZ);
            if (lidRight) lidRight.localRotation = Quaternion.Euler(lidClosedEuler.x, lidClosedEuler.y, lidRightOpenZ);
        }
        else
        {
            var closed = Quaternion.Euler(lidClosedEuler);
            if (lidLeft)  lidLeft.localRotation  = closed;
            if (lidRight) lidRight.localRotation = closed;
        }
    }

    // ====== 저장/로드 (GUID 기반 내부 SaveData) ======

    [Serializable]
    public struct SaveData
    {
        public string   prefabName;
        public Vector3  pos;
        public Quaternion rot;
        public bool     isOpen;
        public string   itemGuid;  // ItemData 식별자 (GUID/키)
        public int      amount;    // 남은 개수
    }

    /// <summary>
    /// 박스 상태 캡처. getGuid: ItemData → GUID/키 변환 함수 주입
    /// </summary>
    public SaveData Capture(Func<ItemData, string> getGuid)
    {
        return new SaveData
        {
            prefabName = name.Replace("(Clone)", ""),
            pos        = transform.position,
            rot        = transform.rotation,
            isOpen     = IsOpen,
            itemGuid   = (item != null && getGuid != null) ? getGuid(item) : string.Empty,
            amount     = remaining
        };
    }

    /// <summary>
    /// 박스 상태 복원. resolve: GUID/키 → ItemData 변환 함수 주입
    /// </summary>
    public void Restore(SaveData data, Func<string, ItemData> resolve)
    {
        transform.SetPositionAndRotation(data.pos, data.rot);
        IsOpen = data.isOpen;
        ApplyLidImmediate(IsOpen);

        item = (resolve != null && !string.IsNullOrEmpty(data.itemGuid)) ? resolve(data.itemGuid) : null;
        remaining = Mathf.Max(0, data.amount);

        // 상태 변경 알림
        OnChanged?.Invoke();
        OnLidChanged?.Invoke(IsOpen);
    }

    // ====== (선택) 기존 BoxSaveData 스키마도 써야 할 때용 어댑터 ======
    // 프로젝트에 기존 BoxSaveData 클래스가 있고, 그 스키마를 유지해야 한다면
    // 아래 두 메서드 이름을 여러분의 기존 Capture/Restore로 "바꿔치기" 해서 쓰세요.
    // 필요 없으면 이 영역은 지워도 됩니다.

    /*
    public BoxSaveData CaptureLegacy(
        Func<ItemData,int>            getId,
        Func<ItemData,string>         getName,
        Func<ItemData,ItemCategory>   getCategory
    )
    {
        var data = new BoxSaveData();
        data.prefabName = name.Replace("(Clone)", "");
        data.position   = transform.position;
        data.rotation   = transform.rotation;

        // 기존 스키마에 isOpen 필드가 있다면 채우고, 없다면 무시됨(컴파일 에러 시 이 줄 삭제)
        data.isOpen     = IsOpen;

        data.itemId     = (item != null && getId       != null) ? getId(item)       : 0;
        data.itemName   = (item != null && getName     != null) ? getName(item)     : string.Empty;
        data.category   = (item != null && getCategory != null) ? getCategory(item) : default;

        data.amount     = Mathf.Max(0, remaining);
        data.isHeld     = false; // 로드시 꼬임 방지

        return data;
    }

    public void RestoreLegacy(
        BoxSaveData data,
        Func<int,ItemData>      resolveById,
        Func<string,ItemData>   resolveByName
    )
    {
        transform.SetPositionAndRotation(data.position, data.rotation);

        // 기존 스키마에 isOpen이 있으면 사용(없으면 이 줄 주석처리)
        IsOpen = data.isOpen;
        ApplyLidImmediate(IsOpen);

        ItemData resolved = null;
        if (resolveById   != null && data.itemId   != 0)                resolved = resolveById(data.itemId);
        if (resolved == null && resolveByName != null && !string.IsNullOrEmpty(data.itemName))
            resolved = resolveByName(data.itemName);

        item = resolved;
        remaining = Mathf.Max(0, data.amount);

        OnChanged?.Invoke();
        OnLidChanged?.Invoke(IsOpen);
    }
    */
}
