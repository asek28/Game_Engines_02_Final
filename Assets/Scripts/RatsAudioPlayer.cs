using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RatsAudioPlayer : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("Audio Source component. Eğer boşsa otomatik bulunur.")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Ses çalma aralığı (saniye)")]
    [SerializeField, Min(0.1f)] private float playInterval = 5f;
    [Tooltip("Oyun başladığında hemen çal")]
    [SerializeField] private bool playOnStart = true;

    private float timer = 0f;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError($"RatsAudioPlayer on {name}: AudioSource component not found!");
            }
        }
    }

    private void Start()
    {
        if (playOnStart && audioSource != null)
        {
            audioSource.Play();
            timer = 0f;
        }
    }

    private void Update()
    {
        if (audioSource == null)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= playInterval)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
            timer = 0f;
        }
    }
}

