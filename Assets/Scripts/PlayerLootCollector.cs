using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using TMPro;

/// <summary>
/// Player'ın yakındaki loot'ları toplamasını sağlar
/// E tuşuna basınca menzildeki en yakın loot'u toplar
/// </summary>
public class PlayerLootCollector : MonoBehaviour
{
    [Header("Collection Settings")]
    [Tooltip("Loot toplama menzili (birim)")]
    [SerializeField, Min(0.5f)] private float collectionRange = 3f;
    [Tooltip("Loot arama sıklığı (saniye)")]
    [SerializeField, Min(0.1f)] private float searchInterval = 0.1f;
    
    [Header("Visual Feedback")]
    [Tooltip("Yakındaki loot'ları highlight et")]
    [SerializeField] private bool highlightNearbyLoot = true;
    
    [Header("UI Settings")]
    [Tooltip("E tuşu göstergesi için UI Text (TextMeshProUGUI). Eğer boşsa otomatik oluşturulur.")]
    [SerializeField] private TextMeshProUGUI interactPromptText;
    [Tooltip("E tuşu göstergesi metni")]
    [SerializeField] private string interactPromptMessage = "Press E to Collect";
    [Tooltip("UI'ın ekrandaki pozisyonu (0-1 arası, 0.5 = ortada)")]
    [SerializeField] private Vector2 uiPosition = new Vector2(0.5f, 0.2f);
    
    [Header("Audio Settings")]
    [Tooltip("Loot toplama ses efektleri (rastgele seçilir). Boşsa Resources klasöründen yüklenir.")]
    [SerializeField] private AudioClip[] lootCollectSounds;
    [Tooltip("Resources klasöründen otomatik yükle (Resources/SFX/ klasöründen)")]
    [SerializeField] private bool autoLoadFromResources = true;
    [Tooltip("Resources klasöründeki alt klasör yolu (örnek: 'SFX' veya 'SFX/Loot')")]
    [SerializeField] private string resourcesPath = "SFX";
    [Tooltip("Ses çalma hacmi (0-1 arası)")]
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    
    private List<Loot> nearbyLoots = new List<Loot>();
    private float searchTimer = 0f;
    private Transform playerTransform;
    private Canvas uiCanvas;
    private GameObject interactPromptObject;
    private AudioSource audioSource;
    
    private void Awake()
    {
        playerTransform = transform;
        SetupUI();
        SetupAudio();
    }
    
    /// <summary>
    /// Audio sistemini ayarlar
    /// </summary>
    private void SetupAudio()
    {
        // AudioSource'u bul veya oluştur
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D ses
        }
        
        // Eğer sesler atanmamışsa ve otomatik yükleme aktifse, Resources'tan yükle
        if ((lootCollectSounds == null || lootCollectSounds.Length == 0) && autoLoadFromResources)
        {
            LoadSoundsFromResources();
        }
        
        // Eğer hala ses yoksa uyarı ver
        if (lootCollectSounds == null || lootCollectSounds.Length == 0)
        {
            Debug.LogWarning("[PlayerLootCollector] No loot collect sounds assigned! Please assign AudioClips in the Inspector or place them in Resources/SFX/ folder.");
        }
    }
    
    /// <summary>
    /// Resources klasöründen ses efektlerini yükler
    /// </summary>
    private void LoadSoundsFromResources()
    {
        if (string.IsNullOrEmpty(resourcesPath))
        {
            resourcesPath = "SFX";
        }
        
        // Resources klasöründen tüm AudioClip'leri yükle
        AudioClip[] loadedClips = Resources.LoadAll<AudioClip>(resourcesPath);
        
        if (loadedClips != null && loadedClips.Length > 0)
        {
            lootCollectSounds = loadedClips;
            Debug.Log($"[PlayerLootCollector] Loaded {loadedClips.Length} audio clips from Resources/{resourcesPath}/");
        }
        else
        {
            Debug.LogWarning($"[PlayerLootCollector] No audio clips found in Resources/{resourcesPath}/. Make sure your SFX files are in Resources/SFX/ folder.");
        }
    }
    
    private void OnDestroy()
    {
        // UI'ı temizle
        if (interactPromptObject != null)
        {
            Destroy(interactPromptObject);
        }
    }
    
    /// <summary>
    /// UI elementlerini oluşturur veya bulur
    /// </summary>
    private void SetupUI()
    {
        // Canvas'ı bul veya oluştur
        uiCanvas = FindObjectOfType<Canvas>();
        if (uiCanvas == null)
        {
            // Canvas yoksa oluştur
            GameObject canvasObject = new GameObject("LootCollectorCanvas");
            uiCanvas = canvasObject.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // Eğer interactPromptText atanmamışsa oluştur
        if (interactPromptText == null)
        {
            // Canvas içinde mevcut bir text var mı kontrol et
            interactPromptText = uiCanvas.GetComponentInChildren<TextMeshProUGUI>();
            
            if (interactPromptText == null)
            {
                // Yeni bir GameObject oluştur
                interactPromptObject = new GameObject("InteractPrompt");
                interactPromptObject.transform.SetParent(uiCanvas.transform, false);
                
                // RectTransform ayarla
                RectTransform rectTransform = interactPromptObject.AddComponent<RectTransform>();
                rectTransform.anchorMin = uiPosition;
                rectTransform.anchorMax = uiPosition;
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = new Vector2(300f, 50f);
                
                // TextMeshProUGUI ekle
                interactPromptText = interactPromptObject.AddComponent<TextMeshProUGUI>();
                interactPromptText.text = interactPromptMessage;
                interactPromptText.fontSize = 24f;
                interactPromptText.alignment = TextAlignmentOptions.Center;
                interactPromptText.color = Color.white;
                
                // Outline ekle (görünürlük için)
                UnityEngine.UI.Outline outline = interactPromptObject.AddComponent<UnityEngine.UI.Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(2f, -2f);
            }
            else
            {
                interactPromptObject = interactPromptText.gameObject;
            }
        }
        else
        {
            interactPromptObject = interactPromptText.gameObject;
        }
        
        // Başlangıçta gizle
        if (interactPromptObject != null)
        {
            interactPromptObject.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Loot arama
        searchTimer += Time.deltaTime;
        if (searchTimer >= searchInterval)
        {
            searchTimer = 0f;
            SearchForNearbyLoots();
        }
        
        // UI'ı güncelle
        UpdateInteractPrompt();
        
        // E tuşu kontrolü
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
        {
            TryCollectNearestLoot();
        }
    }
    
    /// <summary>
    /// E tuşu göstergesini günceller
    /// </summary>
    private void UpdateInteractPrompt()
    {
        if (interactPromptObject == null || interactPromptText == null)
        {
            return;
        }
        
        // Yakında loot varsa göster, yoksa gizle
        bool shouldShow = nearbyLoots.Count > 0;
        
        if (interactPromptObject.activeSelf != shouldShow)
        {
            interactPromptObject.SetActive(shouldShow);
        }
        
        // Eğer birden fazla loot varsa, en yakın olanı göster
        if (shouldShow && nearbyLoots.Count > 0)
        {
            Loot nearestLoot = nearbyLoots
                .Where(loot => loot != null && loot.gameObject != null)
                .OrderBy(loot => Vector3.Distance(playerTransform.position, loot.transform.position))
                .FirstOrDefault();
            
            if (nearestLoot != null)
            {
                string itemName = nearestLoot.GetItemDisplayName();
                if (!string.IsNullOrEmpty(itemName))
                {
                    interactPromptText.text = $"{interactPromptMessage} ({itemName})";
                }
                else
                {
                    interactPromptText.text = interactPromptMessage;
                }
            }
        }
    }
    
    /// <summary>
    /// Yakındaki loot'ları bulur
    /// </summary>
    private void SearchForNearbyLoots()
    {
        // Önceki yakındaki loot'ların highlight'ını kapat
        foreach (Loot oldLoot in nearbyLoots)
        {
            if (oldLoot != null && oldLoot.gameObject != null)
            {
                oldLoot.SetPlayerInRange(false);
            }
        }
        
        nearbyLoots.Clear();
        
        // Tüm loot'ları bul
        Loot[] allLoots = FindObjectsOfType<Loot>();
        
        foreach (Loot loot in allLoots)
        {
            if (loot == null || loot.gameObject == null)
            {
                continue;
            }
            
            float distance = Vector3.Distance(playerTransform.position, loot.transform.position);
            
            if (distance <= collectionRange)
            {
                nearbyLoots.Add(loot);
                
                // Highlight et (eğer aktifse)
                if (highlightNearbyLoot)
                {
                    loot.SetPlayerInRange(true);
                }
            }
        }
    }
    
    /// <summary>
    /// En yakın loot'u toplamaya çalışır
    /// </summary>
    private void TryCollectNearestLoot()
    {
        if (nearbyLoots.Count == 0)
        {
            return;
        }
        
        // En yakın loot'u bul
        Loot nearestLoot = nearbyLoots
            .Where(loot => loot != null && loot.gameObject != null)
            .OrderBy(loot => Vector3.Distance(playerTransform.position, loot.transform.position))
            .FirstOrDefault();
        
        if (nearestLoot != null)
        {
            // Loot'u topla (Loot script'indeki TryCollect metodunu çağır)
            CollectLoot(nearestLoot);
        }
    }
    
    /// <summary>
    /// Loot'u toplar (Loot script'indeki TryCollect metodunu çağırır)
    /// </summary>
    private void CollectLoot(Loot loot)
    {
        if (loot == null || loot.gameObject == null)
        {
            return;
        }
        
        float distance = Vector3.Distance(playerTransform.position, loot.transform.position);
        Debug.Log($"[PlayerLootCollector] Collecting loot from distance: {distance:F2}");
        
        // Rastgele ses efekti çal
        PlayRandomCollectSound();
        
        // Loot'un kendi TryCollect metodunu çağır
        loot.TryCollect();
    }
    
    /// <summary>
    /// Rastgele bir loot toplama sesi çalar
    /// </summary>
    private void PlayRandomCollectSound()
    {
        if (audioSource == null || lootCollectSounds == null || lootCollectSounds.Length == 0)
        {
            return;
        }
        
        // Rastgele bir ses seç
        AudioClip randomClip = lootCollectSounds[UnityEngine.Random.Range(0, lootCollectSounds.Length)];
        
        if (randomClip != null)
        {
            audioSource.PlayOneShot(randomClip, volume);
            Debug.Log($"[PlayerLootCollector] Playing collect sound: {randomClip.name}");
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Collection range'ı görselleştir
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, collectionRange);
        
        // Yakındaki loot'ları göster
        if (nearbyLoots != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Loot loot in nearbyLoots)
            {
                if (loot != null && loot.gameObject != null)
                {
                    Gizmos.DrawLine(transform.position, loot.transform.position);
                }
            }
        }
    }
}

