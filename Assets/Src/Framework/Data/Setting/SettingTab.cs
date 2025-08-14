using UnityEngine;
using UnityEngine.UI;

public class SettingTab : MonoBehaviour
{
    [SerializeField] GameObject _graphics;
    [SerializeField] GameObject _audio;
    [SerializeField] GameObject _controls;
    [SerializeField] GameObject _access;

    void Start()
    {
        SetTab(1);
    }
    public void SetTab(int index)
    {
        _graphics.SetActive(index == 1);
        _audio.SetActive(index == 2);
        _controls.SetActive(index == 3);
        _access.SetActive(index == 4);
    }

    
}
