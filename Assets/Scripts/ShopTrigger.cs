using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Shop masasına yaklaşınca E tuşu prompt'u gösterir
/// E'ye basınca Shop Canvas açılır
/// </summary>
public class ShopTrigger : MonoBehaviour
{
    [Header("Shop UI")]
    [Tooltip("Açılacak Shop Canvas")]
    [SerializeField] private GameObject shopCanvas;
    
    /// <summary>
    /// Shop Canvas'ı döndür (public getter)
    /// </summary>
    public GameObject GetShopCanvas()
    {
        return shopCanvas;
    }
    
    [Header("Prompt UI")]
    [Tooltip("'Press E to Shop' yazısı (Canvas > Text)")]
    [SerializeField] private GameObject promptUI;
    
    [Tooltip("Prompt text component (opsiyonel)")]
    [SerializeField] private TextMeshProUGUI promptText;
    
    [Header("Settings")]
    [Tooltip("Açma tuşu")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    
    [Tooltip("Shop açıldığında oyun duracak mı?")]
    [SerializeField] private bool pauseGame = true;
    
    [Tooltip("Shop açıldığında cursor gösterilsin mi?")]
    [SerializeField] private bool showCursor = true;
    
    [Header("Audio (Opsiyonel)")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    // Private
    private bool isPlayerInRange = false;
    private bool isShopOpen = false;
    private AudioSource audioSource;
    
    private void Start()
    {
        // Prompt'u gizle
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
        
        // Shop'u gizle
        if (shopCanvas != null)
        {
            shopCanvas.SetActive(false);
        }
        
        // AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (openSound != null || closeSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Text bul
        if (promptUI != null && promptText == null)
        {
            promptText = promptUI.GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (showDebugLogs)
        {
            Debug.Log("[ShopTrigger] Initialized - Collider should be set to Trigger!");
        }
    }
    
    private void Update()
    {
        // Keyboard kontrolü (Input System)
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Shop açıkken
        if (isShopOpen)
        {
            // ESC ile kapat
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                CloseShop();
            }
            return;
        }
        
        // Player range içindeyse E tuşunu dinle
        if (isPlayerInRange && keyboard.eKey.wasPressedThisFrame)
        {
            OpenShop();
        }
    }
    
    /// <summary>
    /// Player trigger'a girdiğinde
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Debug: Her şeyi logla
        Debug.Log($"[ShopTrigger] OnTriggerEnter! Object: {other.name}, Tag: {other.tag}");
        
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            ShowPrompt();
            
            Debug.Log("[ShopTrigger] ✓ Player entered trigger zone!");
        }
        else
        {
            Debug.LogWarning($"[ShopTrigger] ✗ Not Player! Tag is: {other.tag}");
        }
    }
    
    /// <summary>
    /// Player trigger'dan çıktığında
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            HidePrompt();
            
            if (showDebugLogs)
            {
                Debug.Log("[ShopTrigger] Player left trigger zone!");
            }
        }
    }
    
    /// <summary>
    /// Prompt göster
    /// </summary>
    private void ShowPrompt()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(true);
            
            if (promptText != null)
            {
                promptText.text = $"Press {interactionKey} to Shop";
            }
        }
    }
    
    /// <summary>
    /// Prompt gizle
    /// </summary>
    private void HidePrompt()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }
    
    /// <summary>
    /// Shop'u aç
    /// </summary>
    private void OpenShop()
    {
        if (shopCanvas == null)
        {
            Debug.LogError("[ShopTrigger] Shop Canvas not assigned!");
            return;
        }
        
        isShopOpen = true;
        shopCanvas.SetActive(true);
        HidePrompt();
        
        // Oyunu durdur
        if (pauseGame)
        {
            Time.timeScale = 0f;
        }
        
        // Player movement'ı devre dışı bırak
        DisablePlayerMovement();
        
        // Kamera hareketini durdur
        DisableCameraMovement();
        
        // Cursor göster
        if (showCursor)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
        // Ses
        if (openSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(openSound);
        }
        
        if (showDebugLogs)
        {
            Debug.Log("[ShopTrigger] Shop opened!");
        }
    }
    
    /// <summary>
    /// Shop'u kapat
    /// </summary>
    public void CloseShop()
    {
        if (shopCanvas == null) return;
        
        isShopOpen = false;
        shopCanvas.SetActive(false);
        
        // Oyunu devam ettir
        if (pauseGame)
        {
            Time.timeScale = 1f;
        }
        
        // Player movement'ı tekrar aktif et
        EnablePlayerMovement();
        
        // Kamera hareketini tekrar aktif et
        EnableCameraMovement();
        
        // Cursor gizle
        if (showCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        // Player hala range içindeyse prompt'u tekrar göster
        if (isPlayerInRange)
        {
            ShowPrompt();
        }
        
        // Ses
        if (closeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(closeSound);
        }
        
        if (showDebugLogs)
        {
            Debug.Log("[ShopTrigger] Shop closed!");
        }
    }
    
    /// <summary>
    /// Gizmos ile trigger alanını göster
    /// </summary>
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f); // Yeşil şeffaf
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (col is BoxCollider box)
            {
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }
    }
    
    /// <summary>
    /// Player movement sistemlerini devre dışı bırak
    /// </summary>
    private void DisablePlayerMovement()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        // SimplePlayerMovement'ı devre dışı bırak
        SimplePlayerMovement simpleMovement = player.GetComponent<SimplePlayerMovement>();
        if (simpleMovement != null)
        {
            simpleMovement.enabled = false;
        }
        
        // ShopPlayerMovement'ı devre dışı bırak
        ShopPlayerMovement shopMovement = player.GetComponent<ShopPlayerMovement>();
        if (shopMovement != null)
        {
            shopMovement.enabled = false;
        }
        
        // CharacterController'ı devre dışı bırak
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
    }
    
    /// <summary>
    /// Player movement sistemlerini tekrar aktif et
    /// </summary>
    private void EnablePlayerMovement()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        // SimplePlayerMovement'ı aktif et
        SimplePlayerMovement simpleMovement = player.GetComponent<SimplePlayerMovement>();
        if (simpleMovement != null)
        {
            simpleMovement.enabled = true;
        }
        
        // ShopPlayerMovement'ı aktif et
        ShopPlayerMovement shopMovement = player.GetComponent<ShopPlayerMovement>();
        if (shopMovement != null)
        {
            shopMovement.enabled = true;
        }
        
        // CharacterController'ı aktif et
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = true;
        }
    }
    
    /// <summary>
    /// Kamera hareketini devre dışı bırak
    /// </summary>
    private void DisableCameraMovement()
    {
        RightMouseOrbit cameraOrbit = FindFirstObjectByType<RightMouseOrbit>();
        if (cameraOrbit != null)
        {
            cameraOrbit.enabled = false;
        }
    }
    
    /// <summary>
    /// Kamera hareketini tekrar aktif et
    /// </summary>
    private void EnableCameraMovement()
    {
        RightMouseOrbit cameraOrbit = FindFirstObjectByType<RightMouseOrbit>();
        if (cameraOrbit != null)
        {
            cameraOrbit.enabled = true;
        }
    }
}
