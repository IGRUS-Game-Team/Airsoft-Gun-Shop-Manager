using UnityEngine;

// NPC를 출입문으로 이동시키는 상태
public class NpcState_ToDoor : IState
{
    private readonly NpcController npcController;   // 대상 NPC
    private readonly Transform doorTransform;       // NPC가 향할 출입문 위치

    private const string WalkingAnim = "Walking";   // 걷기 애니메이션 이름

    // 생성자: NPC와 출입문 Transform을 받아 초기 설정
    public NpcState_ToDoor(NpcController npcController, Transform doorTransform)
    {
        this.npcController = npcController;         // 필드에 NPC 저장
        this.doorTransform  = doorTransform;        // 필드에 출입문 위치 저장
    }

    // 상태 진입 시 호출: 이동 재개, 목적지 설정, 애니메이션 실행
    public void Enter()
    {
        npcController.Agent.updateRotation = true; // 이동 중엔 에이전트가 회전 맡음
        npcController.Agent.isStopped = false;                          // 이동 정지 해제
        npcController.Agent.SetDestination(doorTransform.position);     // 목적지: 출입문
        npcController.Animator.Play(WalkingAnim);                       // 걷기 애니메이션 재생
    }

    // 매 프레임 호출: 이 상태는 이동만 담당하므로 별도 처리 없음
    public void Tick()
    {
    }

    // 상태 종료 시 호출: 추가 정리 작업 없음
    public void Exit()
    {
    }
}