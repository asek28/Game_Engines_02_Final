using UnityEngine;
using TMPro;

/// <summary>
/// Shopkeeper NPC - Oturarak rastgele animasyonlar oynatır
/// Player yaklaşınca "Press E to Shop" gösterir
/// E tuşuna basılınca Shop UI açılır
/// </summary>
public class ShopkeeperController : MonoBehaviour
{
    [Header("Animator")]
    [Tooltip("Shopkeeper'ın Animator component'i")]
    [SerializeField] private Animator animator;
    
    [Header("Sitting Animations")]
    [Tooltip("Oturma animasyon state isimleri (Sitting Angry, Sitting Disbelief vb.)")]
    [SerializeField] private string[] sittingAnimationStates = new string[]
    {
        "Sitting Angry",
        "Sitting Disbelief"
    };
    
    [Tooltip("Animasyonlar arası geçiş süresi (saniye)")]
    [SerializeField, Range(3f, 15f)] private float animationSwitchInterval = 8f;
    
    [Tooltip("Rastgele geçiş yapılsın mı?")]
    [SerializeField] private bool randomSwitching = true;
    
    [Header("Interaction")]
    [Tooltip("Player'ın shop'a erişebileceği mesafe")]
    [SerializeField, Min(1f)] private float interactionRange = 3f;
    
    [Tooltip("'Press E' prompt UI (Canvas > Text)")]
    [SerializeField] private GameObject pressEPrompt;
    
    [Tooltip("Prompt text component (opsiyonel - otomatik bulunur)")]
    [SerializeField] private TextMeshProUGUI promptText;
    
    [Header("Shop UI")]
    [Tooltip("Açılacak Shop Canvas")]
    [SerializeField] private GameObject shopCanvas;
    
    [Tooltip("Shop açıldığında oyun duracak mı?")]
    [SerializeField] private bool pauseGameWhenShopOpen = true;
    
    [Tooltip("Shop açıldığında cursor gösterilsin mi?")]
    [SerializeField] private bool showCursorInShop = true;
    
    [Header("Audio (Opsiyonel)")]
    [Tooltip("Shop açılış sesi")]
    [SerializeField] private AudioClip shopOpenSound;
    
    [Tooltip("Shop kapanış sesi")]
    [SerializeField] private AudioClip shopCloseSound;
    
    // Private variables
    private Transform playerTransform;
    private float animationTimer = 0f;
    private int currentAnimationIndex = 0;
    private bool isPlayerNearby = false;
    private bool isShopOpen = false;
    private AudioSource audioSource;
    
    private void Awake()
    {
        // Animator'ı bul (yoksa)
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Prompt text'i bul (yoksa)
        if (pressEPrompt != null && promptText == null)
        {
            promptText = pressEPrompt.GetComponentInChildren<TextMeshProUGUI>();
        }
        
        // AudioSource ekle (yoksa)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    
    private void Start()
    {
        // Player'ı bul
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("[ShopkeeperController] Player not found! Make sure Player has 'Player' tag.");
        }
        
        // Prompt'u gizle
        if (pressEPrompt != null)
        {
            pressEPrompt.SetActive(false);
        }
        
        // Shop Canvas'ı gizle
        if (shopCanvas != null)
        {
            shopCanvas.SetActive(false);
        }
        
        // İlk animasyonu başlat
        if (animator != null && sittingAnimationStates.Length > 0)
        {
            PlayAnimation(0);
        }
        
        Debug.Log("[ShopkeeperController] Shopkeeper initialized.");
    }
    
    private void Update()
    {
        // Shop açıkken input kontrolü yapma (sadece ESC ile kapatma)
        if (isShopOpen)
        {
            // ESC ile shop'u kapat
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseShop();
            }
            return;
        }
        
        // Player mesafe kontrolü
        CheckPlayerProximity();
        
        // E tuşu ile shop açma
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            OpenShop();
        }
        
        // Rastgele animasyon geçişleri
        if (randomSwitching && animator != null)
        {
            animationTimer += Time.deltaTime;
            
            if (animationTimer >= animationSwitchInterval)
            {
                animationTimer = 0f;
                PlayRandomAnimation();
            }
        }
    }
    
    /// <summary>
    /// Player'ın shopkeeper'a yakınlığını kontrol eder
    /// </summary>
    private void CheckPlayerProximity()
    {
        if (playerTransform == null) return;
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool wasNearby = isPlayerNearby;
        isPlayerNearby = distance <= interactionRange;
        
        // Player yaklaştı
        if (isPlayerNearby && !wasNearby)
        {
            ShowPrompt();
        }
        // Player uzaklaştı
        else if (!isPlayerNearby && wasNearby)
        {
            HidePrompt();
        }
    }
    
    /// <summary>
    /// "Press E" prompt'unu gösterir
    /// </summary>
    private void ShowPrompt()
    {
        if (pressEPrompt != null)
        {
            pressEPrompt.SetActive(true);
            
            // Text'i güncelle
            if (promptText != null)
            {
                promptText.text = "Press E to Shop";
            }
            
            Debug.Log("[ShopkeeperController] Player nearby - showing prompt.");
        }
    }
    
    /// <summary>
    /// "Press E" prompt'unu gizler
    /// </summary>
    private void HidePrompt()
    {
        if (pressEPrompt != null)
        {
            pressEPrompt.SetActive(false);
            Debug.Log("[ShopkeeperController] Player left - hiding prompt.");
        }
    }
    
    /// <summary>
    /// Shop UI'ı açar
    /// </summary>
    public void OpenShop()
    {
        if (shopCanvas == null)
        {
            Debug.LogWarning("[ShopkeeperController] Shop Canvas is not assigned!");
            return;
        }
        
        isShopOpen = true;
        shopCanvas.SetActive(true);
        
        // Prompt'u gizle
        HidePrompt();
        
        // Oyunu durdur (opsiyonel)
        if (pauseGameWhenShopOpen)
        {
            Time.timeScale = 0f;
        }
        
        // Cursor'u göster
        if (showCursorInShop)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
        // Ses efekti
        if (shopOpenSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shopOpenSound);
        }
        
        Debug.Log("[ShopkeeperController] Shop opened!");
    }
    
    /// <summary>
    /// Shop UI'ı kapatır
    /// </summary>
    public void CloseShop()
    {
        if (shopCanvas == null) return;
        
        isShopOpen = false;
        shopCanvas.SetActive(false);
        
        // Oyunu devam ettir
        if (pauseGameWhenShopOpen)
        {
            Time.timeScale = 1f;
        }
        
        // Cursor'u gizle
        if (showCursorInShop)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        // Player hala yakınsa prompt'u tekrar göster
        if (isPlayerNearby)
        {
            ShowPrompt();
        }
        
        // Ses efekti
        if (shopCloseSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shopCloseSound);
        }
        
        Debug.Log("[ShopkeeperController] Shop closed!");
    }
    
    /// <summary>
    /// Belirtilen index'teki animasyonu oynatır (Parameter-based)
    /// </summary>
    private void PlayAnimation(int index)
    {
        if (animator == null || sittingAnimationStates.Length == 0) return;
        
        index = Mathf.Clamp(index, 0, sittingAnimationStates.Length - 1);
        currentAnimationIndex = index;
        
        // TÜM parametreleri false yap
        ResetAllAnimationParameters();
        
        // Seçilen parametreyi true yap
        string paramName = sittingAnimationStates[index];
        
        // Animator'da bu isimde bool parameter var mı kontrol et
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(paramName, true);
                Debug.Log($"[ShopkeeperController] Set parameter '{paramName}' = TRUE");
                return;
            }
        }
        
        // Eğer parameter yoksa eski yöntemle (Play)
        Debug.LogWarning($"[ShopkeeperController] Parameter '{paramName}' not found! Using animator.Play() instead.");
        animator.Play(paramName);
    }
    
    /// <summary>
    /// Rastgele bir animasyon oynatır
    /// </summary>
    private void PlayRandomAnimation()
    {
        if (sittingAnimationStates.Length == 0) return;
        
        // Rastgele bir animasyon seç
        int newIndex = Random.Range(0, sittingAnimationStates.Length);
        
        PlayAnimation(newIndex);
    }
    
    /// <summary>
    /// Tüm animasyon parametrelerini false yapar (Sitting'e geri döner)
    /// </summary>
    private void ResetAllAnimationParameters()
    {
        if (animator == null) return;
        
        foreach (string paramName in sittingAnimationStates)
        {
            // Parameter varsa false yap
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
                {
                    animator.SetBool(paramName, false);
                }
            }
        }
    }
    
    /// <summary>
    /// Interaction range'i Gizmos ile göster (Editor'da)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
