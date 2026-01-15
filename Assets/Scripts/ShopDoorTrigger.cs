using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// GameScene'de Shop kapısı - Q tuşuna basınca Shop sahnesine götürür
/// BoxCollider Trigger kullanır
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class ShopDoorTrigger : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Açılacak sahne ismi")]
    [SerializeField] private string shopSceneName = "Shop";
    
    [Header("Interaction")]
    [Tooltip("'Press Q to Enter Shop' prompt UI")]
    [SerializeField] private GameObject promptUI;
    
    [Tooltip("Prompt text component")]
    [SerializeField] private TextMeshProUGUI promptText;
    
    [Header("Audio")]
    [Tooltip("Kapı açılma sesi")]
    [SerializeField] private AudioClip doorOpenSound;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("TEST - BoxCollider Info")]
    [SerializeField] private bool showColliderInfo = true;
    
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
            Debug.LogWarning("[ShopDoorTrigger] BoxCollider.isTrigger = true olarak ayarlandı!");
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
            Debug.Log("[ShopDoorTrigger] Shop door initialized.");
        }
        
        // BoxCollider kontrolü
        if (showColliderInfo)
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null)
            {
                Debug.LogError("[ShopDoorTrigger] ❌ BoxCollider BULUNAMADI! Lütfen ekleyin!");
            }
            else
            {
                Debug.Log($"[ShopDoorTrigger] ✓ BoxCollider var. IsTrigger: {box.isTrigger}, Size: {box.size}");
            }
            
            // Player kontrolü
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("[ShopDoorTrigger] ❌ Player BULUNAMADI! Tag 'Player' olmalı!");
            }
            else
            {
                Debug.Log($"[ShopDoorTrigger] ✓ Player bulundu: {player.name}");
            }
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
                EnterShop();
            }
            
            // Debug: Her frame Q tuşunun durumunu göster
            if (showDebugLogs && keyboard != null)
            {
                if (keyboard.qKey.wasPressedThisFrame)
                {
                    Debug.Log("[ShopDoorTrigger] Q tuşuna basıldı! EnterShop() çağrılıyor...");
                }
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[ShopDoorTrigger] OnTriggerEnter! Object: {other.name}, Tag: {other.tag}");
        }
        
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            ShowPrompt();
            
            if (showDebugLogs)
            {
                Debug.Log("[ShopDoorTrigger] ✓ Player entered door trigger zone!");
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
                Debug.Log("[ShopDoorTrigger] Player left door trigger zone!");
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
                promptText.text = "Press Q to Enter Shop";
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
    /// Shop sahnesine geç
    /// </summary>
    private void EnterShop()
    {
        Debug.Log($"[ShopDoorTrigger] Loading shop scene: {shopSceneName}");
        
        // Ses efekti
        if (doorOpenSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
        }
        
        // Player'ın mevcut pozisyonunu kaydet (geri dönerken kullanılabilir)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerPrefs.SetFloat("LastPosX", player.transform.position.x);
            PlayerPrefs.SetFloat("LastPosY", player.transform.position.y);
            PlayerPrefs.SetFloat("LastPosZ", player.transform.position.z);
            PlayerPrefs.SetString("LastScene", SceneManager.GetActiveScene().name);
            PlayerPrefs.Save();
        }
        
        // Shop sahnesini yükle
        SceneManager.LoadScene(shopSceneName);
    }
    
    /// <summary>
    /// Gizmos ile BoxCollider'ı göster
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}
