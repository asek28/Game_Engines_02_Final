using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Tüm scene'lerde çalan background müzik yöneticisi
/// DontDestroyOnLoad kullanarak scene değişimlerinde müzik kesilmez
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    
    [Header("Music Settings")]
    [Tooltip("Oynatılacak müzik clip'i")]
    [SerializeField] private AudioClip musicClip;
    
    [Tooltip("Müzik ses seviyesi (0-1)")]
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f;
    
    [Tooltip("Oyun başladığında otomatik çal")]
    [SerializeField] private bool playOnStart = true;
    
    [Header("Audio Mixer (Optional)")]
    [Tooltip("AudioMixer referansı (Settings'ten kontrol için)")]
    [SerializeField] private AudioMixer audioMixer;
    
    [Tooltip("AudioMixer Group (MusicVol parametresine bağlı)")]
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    
    private AudioSource audioSource;
    
    private void Awake()
    {
        // Singleton pattern - sadece bir tane olsun
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // AudioSource ayarları
            audioSource.clip = musicClip;
            audioSource.volume = volume;
            audioSource.loop = true; // Loop açık
            audioSource.playOnAwake = false; // Manuel kontrol
            
            // AudioMixerGroup bağla (eğer varsa)
            if (musicMixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = musicMixerGroup;
            }
            
            Debug.Log("[MusicManager] Initialized! Music will play across all scenes.");
        }
        else
        {
            // Zaten bir MusicManager var - bu instance'ı sil
            Debug.Log("[MusicManager] Another MusicManager already exists. Destroying duplicate.");
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // PlayerPrefs'ten volume'u yükle
        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", volume);
        SetVolume(savedVolume);
        
        if (playOnStart && musicClip != null)
        {
            PlayMusic();
        }
    }
    
    /// <summary>
    /// Müziği çal
    /// </summary>
    public void PlayMusic()
    {
        if (audioSource != null && musicClip != null)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
                Debug.Log("[MusicManager] Music started playing.");
            }
        }
    }
    
    /// <summary>
    /// Müziği durdur
    /// </summary>
    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("[MusicManager] Music stopped.");
        }
    }
    
    /// <summary>
    /// Müzik ses seviyesini ayarla
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
    
    /// <summary>
    /// Müzik çalıyor mu?
    /// </summary>
    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }
}
