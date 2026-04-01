using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PromptEntryUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image keyImage;          // 키캡 배경(스프라이트)
    [SerializeField] private TMP_Text keyText;        // 키 문자 (E, R, G, etc)
    [SerializeField] private TMP_Text descText;       // 설명 텍스트

    [Header("Sprites")]
    [SerializeField] private Sprite keycapSprite;     // 네가 가진 “키보드 배경”
    [SerializeField] private Sprite mouseSprite;      // 마우스 모양(있으면)

    public void Setup(InteractionPrompt prompt)
    {
        // 키캡/마우스 선택
        bool isMouse = (prompt.input == InputHint.LMB || prompt.input == InputHint.RMB);
        if (keyImage != null)
            keyImage.sprite = isMouse ? mouseSprite : keycapSprite;

        if (keyText != null) keyText.text = GetKeyLabel(prompt.input);
        if (descText != null) descText.text = prompt.description;
    }

    private string GetKeyLabel(InputHint hint)
    {
        return hint switch
        {
            InputHint.LMB => "LMB",
            InputHint.RMB => "RMB",
            InputHint.E   => "E",
            InputHint.R   => "R",
            InputHint.G   => "G",
            InputHint.ESC => "ESC",
            InputHint.QE  => "Q/E",
            _ => hint.ToString()
        };
    }
}
