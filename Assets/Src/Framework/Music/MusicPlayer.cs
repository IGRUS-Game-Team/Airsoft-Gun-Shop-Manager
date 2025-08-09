using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    /// MusicPlayer.cs 이지연
    /// 메인 BGM 재생 싱글톤 패턴 이용해 구현하였습니다.
    /// 다른 씬으로 넘어가도 끊기지 않도록 하였습니다.
    void Start()
    {
        int numOfMusicPlayers = FindObjectsByType<MusicPlayer>(FindObjectsSortMode.None).Length;
        // Scene 내 모든 뮤직 플레이어 검색, 개수 검색

        if (numOfMusicPlayers > 1) // 씬이 넘어가서 뮤직 플레이어가 1개 더 생기면
        {
            Destroy(gameObject); // 그 뮤직 플레이어를 삭제
        }
        else
        {
            DontDestroyOnLoad(gameObject); // 현재 뮤직 플레이어는 유지
        }
    }
}
