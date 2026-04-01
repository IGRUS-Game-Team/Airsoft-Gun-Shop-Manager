using System.Collections.Generic;
using UnityEngine;

public class BoxSaveHandler : MonoBehaviour, ISaveable
{
    [SerializeField] private BoxSpawner spawner;
    [SerializeField] private ItemDatabase database; // 복원 때 필요

    public object CaptureData()
    {
        var boxes = new List<BoxSaveData>();

        // 1) 기존 BoxItemContainer 박스 캡처
        foreach (var box in FindObjectsOfType<BlockIsHolding>())
        {
            var container = box.GetComponent<BoxItemContainer>();
            if (container == null || container.ItemId == 0) continue;

            // BoxContainer가 있으면 배달 박스이므로 아래에서 처리
            if (box.GetComponent<BoxContainer>() != null) continue;

            boxes.Add(new BoxSaveData
            {
                position = box.transform.position,
                rotation = box.transform.rotation,
                itemId   = container.ItemId,
                amount   = container.Amount,
                isDeliveryBox = false
            });
        }

        // 2) BoxContainer (배달 박스) 캡처
        foreach (var dc in FindObjectsOfType<BoxContainer>())
        {
            if (dc.Item == null || dc.Item.itemId == 0) continue;

            boxes.Add(new BoxSaveData
            {
                position = dc.transform.position,
                rotation = dc.transform.rotation,
                itemId   = dc.Item.itemId,
                amount   = dc.Remaining,
                isOpen   = dc.IsOpen,
                isDeliveryBox = true
            });
        }

        return boxes;
    }

    public void RestoreData(object data)
    {
        var boxes = data as List<BoxSaveData>;
        if (boxes == null || spawner == null) return;

        // 기존 배달 박스 제거 (중복 방지)
        foreach (var existing in FindObjectsOfType<BoxContainer>())
            Destroy(existing.gameObject);

        foreach (var b in boxes)
        {
            if (b.isDeliveryBox)
            {
                // 배달 박스: BoxSpawner.RestoreBox 사용 (BoxContainer + SetContent)
                var go = spawner.RestoreBox(b);
                if (go == null) continue;

                // 열림 상태 복원
                if (b.isOpen)
                {
                    var container = go.GetComponent<BoxContainer>();
                    if (container != null && !container.IsOpen)
                        container.ToggleLid();
                }
            }
            else
            {
                // 기존 BoxItemContainer 박스 복원
                var go = spawner.RestoreBoxTransform(b.position, b.rotation);
                if (!go) continue;

                var container = go.GetComponent<BoxItemContainer>();
                if (container == null)
                    container = go.AddComponent<BoxItemContainer>();

                container.SetupById(b.itemId, database, b.amount);
            }
        }
    }
}
