// Assets/Src/Framework/Debug/CheatMoney.cs
using UnityEngine;

public class ShowMeTheMoney : MonoBehaviour
{
    [Header("얼마를 줄까요?")]
    [SerializeField] private float amount = 1_000_000f;

    [Header("플레이 중 이 키를 누르면 지급")]
    [SerializeField] private KeyCode hotkey = KeyCode.F10;

    [Tooltip("Ctrl(또는 Cmd)와 함께 눌러야 작동하도록 보호")]
    [SerializeField] private bool requireCtrl = true;

    void Update()
    {
        if (!Input.GetKeyDown(hotkey)) return;
        if (requireCtrl && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                             Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)))
            return;

        GiveMoney();
    }

    // UI 버튼에 바로 연결해도 됨
    public void GiveMoneyButton() => GiveMoney();

    // 인스펙터에서 우클릭 > 메뉴로도 가능
    [ContextMenu("Give Huge Money Now")]
    public void GiveMoneyContext() => GiveMoney();

    private void GiveMoney()
    {
        if (GameState.Instance != null)
        {
            // 프로젝트에 AddMoney(float)가 있는 것으로 보였음.
            // 만약 SetMoney만 있다면 아래 두 줄 중 하나로 바꿔 쓰면 됨.
            GameState.Instance.AddMoney(amount);
            Debug.Log($"[Cheat] +${amount:n0} 지급. 현재 소지금: {GameState.Instance.Money:n0}");
        }
        else
        {
            Debug.LogWarning("[Cheat] GameState.Instance 가 없습니다. 씬에 GameState가 있는지 확인하세요.");
        }
    }

#if UNITY_EDITOR
    // 에디터 상단 메뉴: Debug/Give $$$
    [UnityEditor.MenuItem("Debug/Give $$$ (1,000,000)", priority = 200)]
    private static void MenuGiveMoney()
    {
        if (!Application.isPlaying) { Debug.LogWarning("플레이 모드에서만 작동합니다."); return; }
        var gs = GameState.Instance;
        if (gs == null) { Debug.LogWarning("GameState.Instance 없음"); return; }
        gs.AddMoney(1_000_000f);
        Debug.Log($"[Cheat] 메뉴로 +$1,000,000 지급. 현재 소지금: {gs.Money:n0}");
    }
#endif
}
