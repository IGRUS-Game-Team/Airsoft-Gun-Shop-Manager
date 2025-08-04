// 모든 상태 클래스가 반드시 따라야 하는 기본 규칙.

public interface IState
{
    void Enter(); // Enter : 상태가 처음 시작될 때 한 번 실행. 초기화 작업에 사용.
    void Tick();  // Tick : 상태가 유지되는 동안 매 프레임 실행. 핵심 동작을 넣는다.
    void Exit();  // Exit : 다른 상태로 바뀌기 직전에 한 번 실행. 정리, 리셋 작업에 사용.
}