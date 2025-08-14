// ItemNameDialog.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ItemNameDialog : MonoBehaviour
{
    public TMP_InputField input;
    public Button btnApply; //btnCancel;
    private ItemData bound;
    private Action<string> onApply;

    public void Open(ItemData item, Action<string> onApply)
    {
        this.bound = item;
        this.onApply = onApply;
        input.text = ItemOverrideStore.Instance.GetDisplayName(item); // 현재 표시명
        gameObject.SetActive(true);
    }

    void Awake()
    {
        btnApply.onClick.AddListener(() =>
        {
            onApply?.Invoke(input.text);
            Destroy(gameObject);
        });
        //btnCancel.onClick.AddListener(() => Destroy(gameObject));
    }
}
