using UnityEngine;

public static class GlobalInteractionFlagS
{
    public static int ModalDepth = 0;
    public static bool IsInModal => ModalDepth > 0;

    // 도메인 리로드가 꺼져 있어도 실행 시작 시 1회 호출되어 정적 필드 초기화
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        ModalDepth = 0;
    }
}
