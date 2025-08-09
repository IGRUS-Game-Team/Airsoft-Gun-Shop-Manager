using UnityEngine;
using TMPro;

// 결제 안내 패널을 켜고 끄는 간단한 UI 관리 클래스
public class UIManager : MonoBehaviour
{
    /* ---------- 싱글톤 접근 ---------- */

    public static UIManager Instance { get; private set; }         // 전역 접근 포인트
    public static UIManager uiManager => Instance;                 // 기존 코드 호환용 별칭

    /* ---------- 인스펙터에서 주입 ---------- */

    [SerializeField] private GameObject payPanel;                  // “E - 돈 받기” 안내 패널
    [SerializeField] private TextMeshProUGUI npcNameText;          // 패널에 표시할 NPC 이름 텍스트

    /* ---------- 초기화 ---------- */

    private void Awake()
    {
        if (Instance != null && Instance != this)                  // 이미 다른 인스턴스가 있으면
        {
            Destroy(this.gameObject);                              // 중복 인스턴스 파괴
            return;                                                // 실행 종료
        }

        Instance = this;                                           // 싱글톤 할당
    }

    /* ---------- 외부 호출 메서드 ---------- */

    // NPC가 결제를 제안할 때 호출하여 패널을 띄운다
    public void ShowPayPrompt(NpcController npcController)
    {
        npcNameText.text = npcController.name;                     // NPC 이름 텍스트 설정
        payPanel.SetActive(true);                                  // 패널 표시
    }

    // 결제가 완료되거나 취소될 때 호출하여 패널을 숨긴다
    public void HidePayPrompt()
    {
        payPanel.SetActive(false);                                 // 패널 비활성화
    }
}