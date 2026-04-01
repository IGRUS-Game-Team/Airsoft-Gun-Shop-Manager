/// <summary>
/// 세이브/로드 가능한 컴포넌트가 구현하는 인터페이스.
/// SaveManager가 FindObjectsOfType으로 자동 수집하여 ES3를 통해 저장/복원.
///
/// [구현 방법]
/// 1. SaveDatas/ 에 직렬화 가능한 DTO 클래스 생성 ([System.Serializable])
/// 2. SaveHandler/ 에 MonoBehaviour를 만들고 ISaveable 구현
/// 3. CaptureData()에서 DTO 인스턴스 반환
/// 4. RestoreData()에서 object를 DTO로 캐스팅 후 상태 복원
/// 5. 씬에 핸들러 오브젝트 배치
///
/// [복원 순서]
/// SaveManager.GetRestoreOrder()에서 핸들러별 순서를 지정.
/// 새 핸들러가 다른 핸들러에 의존하면 순서를 설정해야 함.
/// 예: ShelfItemSaveHandler(1)은 FurnitureSaveHandler(-1)보다 나중에 복원
///
/// [ES3 키]
/// 핸들러의 클래스명이 ES3 키로 사용됨 (예: "BoxSaveHandler")
/// 클래스명 변경 시 기존 세이브 파일과 호환 안 됨에 주의.
/// </summary>
public interface ISaveable
{
    /// <summary>현재 상태를 직렬화 가능한 DTO 객체로 반환</summary>
    object CaptureData();

    /// <summary>저장된 DTO 객체로부터 상태를 복원 (캐스팅 필요)</summary>
    void RestoreData(object data);
}
