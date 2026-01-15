using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Shop sahnesinden çıkış - Q tuşuna basınca GameScene'e döner
/// BoxCollider Trigger kullanır
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class ShopExitTrigger : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Dönülecek sahne ismi")]
    [SerializeField] private string returnSceneName = "GameScene";
    
    [Tooltip("Kayıtlı pozisyona geri dön")]
    [SerializeField] private bool returnToSavedPosition = true;
    
    [Header("Interaction")]
    [Tooltip("'Press Q to Exit' prompt UI")]
    [SerializeField] private GameObject promptUI;
    
    [Tooltip("Prompt text component")]
    [SerializeField] private TextMeshProUGUI promptText;
    
    [Header("Audio")]
    [Tooltip("Kapı açılma sesi")]
    [SerializeField] private AudioClip doorOpenSound;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    // Private variables
    private bool isPlayerInRange = false;
    private AudioSource audioSource;
    
    private void Awake()
    {
        // AudioSource ekle
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Prompt text'i otomatik bul
        if (promptUI != null && promptText == null)
        {
            promptText = promptUI.GetComponentInChildren<TextMeshProUGUI>();
        }
        
        // BoxCollider kontrolü
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null && !boxCollider.isTrigger)
        {
            boxCollider.isTrigger = true;
            Debug.LogWarning("[ShopExitTrigger] BoxCollider.isTrigger = true olarak ayarlandı!");
        }
    }
    
    private void Start()
    {
        // Prompt'u gizle
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
        
        if (showDebugLogs)
        {
            Debug.Log("[ShopExitTrigger] Shop exit initialized.");
        }
    }
    
    private void Update()
    {
        // Q tuşu kontrolü (Input System)
        if (isPlayerInRange)
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.qKey.wasPressedThisFrame)
            {
                ExitShop();
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[ShopExitTrigger] OnTriggerEnter! Object: {other.name}, Tag: {other.tag}");
        }
        
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            ShowPrompt();
            
            if (showDebugLogs)
            {
                Debug.Log("[ShopExitTrigger] ✓ Player entered exit trigger zone!");
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            HidePrompt();
            
            if (showDebugLogs)
            {
                Debug.Log("[ShopExitTrigger] Player left exit trigger zone!");
            }
        }
    }
    
    /// <summary>
    /// Prompt'u göster
    /// </summary>
    private void ShowPrompt()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(true);
            
            if (promptText != null)
            {
                promptText.text = "Press Q to Exit Shop";
            }
        }
    }
    
    /// <summary>
    /// Prompt'u gizle
    /// </summary>
    private void HidePrompt()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }
    
    /// <summary>
    /// GameScene'e geri dön
    /// </summary>
    private void ExitShop()
    {
        Debug.Log($"[ShopExitTrigger] Exiting shop, returning to: {returnSceneName}");
        
        // Ses efekti
        if (doorOpenSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
        }
        
        // Geri dönülecek pozisyonu kaydet
        if (returnToSavedPosition)
        {
            // PlayerSpawnManager için pozisyon bilgisini kaydet
            PlayerPrefs.SetInt("ShouldRestorePosition", 1);
            PlayerPrefs.Save();
        }
        
        // GameScene'i yükle
        SceneManager.LoadScene(returnSceneName);
    }
    
    /// <summary>
    /// Gizmos ile BoxCollider'ı göster
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}
