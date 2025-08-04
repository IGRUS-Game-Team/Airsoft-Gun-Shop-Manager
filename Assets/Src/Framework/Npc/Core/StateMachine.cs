// 간단한 상태 전환 관리기.
// - SetState : 다른 상태로 바꾸면서 Enter/Exit 호출을 보장.
// - Tick : 현재 상태가 존재하면 매 프레임 실행.
public class StateMachine
{
    IState current;                 // 현재 활성 상태

    public IState Current => current;   // 외부에서 읽기 전용

    // 새로운 상태로 전환한다.
    // 같은 상태를 다시 넣거나 null을 넣으면 아무 것도 하지 않는다.
    public void SetState(IState next)
    {
        if (next == null || next == current) return;

        current?.Exit();            // 이전 상태 정리
        current = next;
        current.Enter();            // 새 상태 초기화
    }

    // 매 프레임 호출해 현재 상태의 Tick을 실행.
    public void Tick()
    {
        current?.Tick();
    }
}