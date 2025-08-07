using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("박스 상호작용 사운드")]
    [SerializeField] AudioClip BoxPickUpSound;
    [SerializeField] AudioClip BoxDropSound;
    [SerializeField] AudioClip BoxThrowSound;

    public static AudioManager Instance; // AudioManager = 싱글톤 선언
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
