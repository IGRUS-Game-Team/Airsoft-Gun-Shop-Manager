using UnityEngine;
using TMPro;

/// <summary>
/// 박스 외면에 아이템 이름을 표시.
/// 프리팹에 직접 배치한 TMP_Text를 참조하여, BoxContainer 내용물에 따라 텍스트만 갱신.
/// </summary>
public class BoxLabel : MonoBehaviour
{
    [SerializeField] private BoxContainer box;
    [SerializeField] private TMP_Text label;

    private void Awake()
    {
        if (box == null) box = GetComponent<BoxContainer>();
        if (label == null) label = GetComponentInChildren<TMP_Text>();
    }

    private void OnEnable()
    {
        if (box != null)
            box.OnChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (box != null)
            box.OnChanged -= Refresh;
    }

    private void Refresh()
    {
        if (label == null) return;

        if (box == null || box.Item == null)
        {
            label.text = string.Empty;
            return;
        }

        string displayName = box.Item.itemName;
        if (string.IsNullOrEmpty(displayName))
            displayName = box.Item.name;

        label.text = displayName;
    }
}
