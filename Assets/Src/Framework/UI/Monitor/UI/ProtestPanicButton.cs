using UnityEngine;
using UnityEngine.UI;

public class ProtestPanicButton : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] Image  cooldownFill; // UI 이미지의 fillAmount로 쿨다운 표현(선택)

    ProtestDirector _director;

    void Awake()
    {
        if (!button) button = GetComponent<Button>();
        _director = FindFirstObjectByType<ProtestDirector>();
        if (button) button.onClick.AddListener(OnClick);
    }

    void Update()
    {
        if (!_director)
        {
            if (!_director) _director = FindFirstObjectByType<ProtestDirector>();
            if (button) button.interactable = false;
            return;
        }

        var cd = _director.CooldownRemaining;
        var can = _director.IsProtestOn && cd <= 0f;

        if (button) button.interactable = can;

        if (cooldownFill)
        {
            // 0~1로 보이게 하려면 기준 시간 필요. 여기선 대략 10초 기준으로 노멀라이즈(원하면 조절)
            float baseSec = Mathf.Max(1f, 10f);
            cooldownFill.fillAmount = Mathf.Clamp01(cd / baseSec);
        }
    }

    void OnClick()
    {
        if (_director) _director.CallBouncer();
    }
}