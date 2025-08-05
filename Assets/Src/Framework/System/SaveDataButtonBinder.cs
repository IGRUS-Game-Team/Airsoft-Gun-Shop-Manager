using UnityEngine;
using UnityEngine.UI;

public class SaveDataButtonBinder : MonoBehaviour
{
    [SerializeField] private Button saveButton;

    void Start()
    {
        if (SaveManager.Instance != null)
            saveButton.onClick.AddListener(() => SaveManager.Instance.SaveGame());
        else
            Debug.LogWarning("SaveManager 인스턴스 없음");
    }
}