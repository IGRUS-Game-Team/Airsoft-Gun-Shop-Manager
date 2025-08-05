/// <summary>
/// 8/5 박정민
/// 세이브핸들러패턴 사용으로 세이브 로직의 가독성과 안정성을 높이기 위해
/// 인터페이스 생성함
/// </summary>
public interface ISaveable
{
    object CaptureData();         // 현재 상태 저장용
    void RestoreData(object data); // 저장된 상태 복원용  
}
