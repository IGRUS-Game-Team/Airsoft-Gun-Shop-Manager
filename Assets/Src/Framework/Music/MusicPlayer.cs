using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    /// MusicPlayer.cs 이지연
    /// 메인 BGM 재생 싱글톤 패턴 이용해 구현하였습니다.
    /// 다른 씬으로 넘어가도 끊기지 않도록 하였습니다.

    public static MusicPlayer Instance { get; private set; }

    private AudioSource source;
    private static int muteRefCount;

    void Start()
    {
        int numOfMusicPlayers = FindObjectsByType<MusicPlayer>(FindObjectsSortMode.None).Length;

        if (numOfMusicPlayers > 1)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        source = GetComponent<AudioSource>();
        muteRefCount = 0;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>레퍼런스 카운팅 뮤트. 여러 UI가 동시에 Mute해도 안전.</summary>
    public static void Mute()
    {
        muteRefCount++;
        ApplyMute();
    }

    /// <summary>Mute 호출 수만큼 Unmute 해야 실제로 소리가 복원됨.</summary>
    public static void Unmute()
    {
        muteRefCount = Mathf.Max(0, muteRefCount - 1);
        ApplyMute();
    }

    private static void ApplyMute()
    {
        if (Instance != null && Instance.source != null)
            Instance.source.mute = muteRefCount > 0;
    }
}
