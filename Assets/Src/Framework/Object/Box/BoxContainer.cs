using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoxContainer : MonoBehaviour
{
    [Header("Lid (Left / Right)")]
    [SerializeField] Transform lid1;              // 왼쪽 뚜껑
    [SerializeField] Transform lid2;              // 오른쪽 뚜껑
    [SerializeField] Vector3 lidClosedEuler = Vector3.zero;
    [SerializeField] float lidLeftOpenZ  = 228f;  // 왼쪽 열림 Z 각도
    [SerializeField] float lidRightOpenZ = -228f; // 오른쪽 열림 Z 각도
    [SerializeField] float lidAnimTime = 0.2f;

    private readonly Dictionary<ItemData,int> remaining = new();
    public bool IsOpen { get; private set; }
    public event Action OnChanged;

    public void SetContents(Dictionary<ItemData,int> contents)
    {
        remaining.Clear();
        foreach (var kv in contents)
            if (kv.Key != null && kv.Value > 0) remaining[kv.Key] = kv.Value;
        OnChanged?.Invoke();
    }

    public IReadOnlyDictionary<ItemData,int> GetRemaining() => remaining;

    public bool TakeOne(ItemData item)
    {
        if (item == null || !remaining.TryGetValue(item, out var cnt) || cnt <= 0) return false;
        remaining[item] = cnt - 1;
        OnChanged?.Invoke();
        return true;
    }

    public void ToggleLid()
    {
        if (lid1 == null && lid2 == null) return;
        StopAllCoroutines();
        StartCoroutine(RotateLids(!IsOpen));
        IsOpen = !IsOpen;
    }

    private System.Collections.IEnumerator RotateLids(bool open)
    {
        // 시작 회전값
        var start1 = lid1 ? lid1.localRotation : Quaternion.identity;
        var start2 = lid2 ? lid2.localRotation : Quaternion.identity;

        // 목표 회전값
        Quaternion target1, target2;
        if (open)
        {
            // 열 때: 왼쪽은 Z=+228°, 오른쪽은 Z=-228°
            var leftEuler  = new Vector3(lidClosedEuler.x, lidClosedEuler.y, lidLeftOpenZ);
            var rightEuler = new Vector3(lidClosedEuler.x, lidClosedEuler.y, lidRightOpenZ);
            target1 = Quaternion.Euler(leftEuler);
            target2 = Quaternion.Euler(rightEuler);
        }
        else
        {
            // 닫을 때: 둘 다 닫힘 각도
            var closed = Quaternion.Euler(lidClosedEuler);
            target1 = closed;
            target2 = closed;
        }

        float t = 0f;
        while (t < lidAnimTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / lidAnimTime);
            if (lid1) lid1.localRotation = Quaternion.Slerp(start1, target1, k);
            if (lid2) lid2.localRotation = Quaternion.Slerp(start2, target2, k);
            yield return null;
        }
        if (lid1) lid1.localRotation = target1;
        if (lid2) lid2.localRotation = target2;
    }

    // ==== 저장/로드 ====
    [Serializable]
    public struct SaveData
    {
        public string prefabName;
        public Vector3 pos;
        public Quaternion rot;
        public bool isOpen;
        public List<string> itemKeys;
        public List<int> counts;
    }

    public SaveData Capture()
    {
        return new SaveData {
            prefabName = name.Replace("(Clone)", ""),
            pos = transform.position,
            rot = transform.rotation,
            isOpen = IsOpen,
            itemKeys = remaining.Keys.Select(k => k.name).ToList(),
            counts = remaining.Values.ToList()
        };
    }

    public void Restore(SaveData data, Func<string, ItemData> resolve)
    {
        transform.SetPositionAndRotation(data.pos, data.rot);
        IsOpen = data.isOpen;

        // 복원 시 즉시 각도 적용
        if (lid1 || lid2)
        {
            if (IsOpen)
            {
                var leftEuler  = new Vector3(lidClosedEuler.x, lidClosedEuler.y, lidLeftOpenZ);
                var rightEuler = new Vector3(lidClosedEuler.x, lidClosedEuler.y, lidRightOpenZ);
                if (lid1) lid1.localRotation = Quaternion.Euler(leftEuler);
                if (lid2) lid2.localRotation = Quaternion.Euler(rightEuler);
            }
            else
            {
                var closed = Quaternion.Euler(lidClosedEuler);
                if (lid1) lid1.localRotation = closed;
                if (lid2) lid2.localRotation = closed;
            }
        }

        remaining.Clear();
        for (int i = 0; i < data.itemKeys.Count; i++)
        {
            var item = resolve?.Invoke(data.itemKeys[i]);
            var cnt  = data.counts[i];
            if (item != null && cnt > 0) remaining[item] = cnt;
        }
        OnChanged?.Invoke();
    }
}
