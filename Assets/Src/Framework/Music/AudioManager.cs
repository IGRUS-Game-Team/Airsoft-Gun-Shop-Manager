using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 08.06 이지연 게임 내 효과음들을 관리하는 스크립트
/// </summary>

public class AudioManager : MonoBehaviour
{
    [SerializeField] ShootingGunSO shootingGunSO;

    [Header("박스 상호작용 사운드")]
    [SerializeField] AudioClip BoxPickUpSound;
    [SerializeField] AudioClip BoxDropSound;
    [SerializeField] AudioClip BoxThrowSound;

    [Header("사격장 총기별 사운드")]
    [SerializeField] AudioClip ShootingGunSound1;

    public static AudioManager Instance; // 싱글톤 선언
    public AudioSource audioSource;

    void Awake()
    {
        if (Instance == null) // AudioManager 두 개 이상 존재 방지
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject); // 두 개 이상이면 제거 
    }

    // 박스 상호작용 사운드 함수
    public void PlayBoxPickUpSound()
    {
        if (BoxPickUpSound != null)
            audioSource.PlayOneShot(BoxPickUpSound);
    }
    public void PlayBoxDropSound()
    {
        if (BoxDropSound != null)
            audioSource.PlayOneShot(BoxDropSound);
    }
    public void PlayBoxThrowSound()
    {
        if (BoxThrowSound != null)
            audioSource.PlayOneShot(BoxThrowSound);
    }
}
