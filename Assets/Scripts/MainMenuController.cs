using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Main Menu kontrolü - Play, Quit, Settings butonlarını yönetir
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Button References")]
    [Tooltip("Play butonu (Raw Image)")]
    [SerializeField] private RawImage playButton;
    
    [Tooltip("Quit butonu (Raw Image)")]
    [SerializeField] private RawImage quitButton;
    
    [Tooltip("Settings butonu (Raw Image)")]
    [SerializeField] private RawImage settingsButton;
    
    [Header("Settings Panel")]
    [Tooltip("Settings panel GameObject (açılıp kapanacak)")]
    [SerializeField] private GameObject settingsPanel;
    
    [Tooltip("Settings panel açık mı?")]
    [SerializeField] private bool isSettingsOpen = false;
    
    [Header("Scene Settings")]
    [Tooltip("Oyun başladığında yüklenecek scene adı (örnek: 'GameScene', 'MainGame')")]
    [SerializeField] private string gameSceneName = "GameScene";
    
    [Header("Audio (Optional)")]
    [Tooltip("Buton tıklama sesi")]
    [SerializeField] private AudioClip buttonClickSound;
    
    private AudioSource audioSource;
    
    private void Awake()
    {
        // AudioSource ekle (eğer yoksa)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Settings panelini başlangıçta kapat
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            isSettingsOpen = false;
        }
    }
    
    private void Start()
    {
        // Buton event'lerini ayarla
        SetupButtons();
        
        // Debug: Button'ların durumunu kontrol et
        DebugButtonStates();
    }
    
    /// <summary>
    /// Button durumlarını debug eder
    /// </summary>
    private void DebugButtonStates()
    {
        Debug.Log("=== MainMenu Button States ===");
        
        Canvas canvas = FindObjectOfType<Canvas>();
        RectTransform canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;
        float canvasWidth = canvasRect != null ? canvasRect.rect.width : Screen.width;
        float canvasHeight = canvasRect != null ? canvasRect.rect.height : Screen.height;
        
        if (playButton != null)
        {
            Button btn = playButton.GetComponent<Button>();
            RectTransform rect = playButton.rectTransform;
            Vector2 anchorMin = rect.anchorMin;
            Vector2 anchorMax = rect.anchorMax;
            Vector2 sizeDelta = rect.sizeDelta;
            
            // Gerçek dünya boyutunu hesapla
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            float worldWidth = Vector2.Distance(corners[0], corners[2]);
            float worldHeight = Vector2.Distance(corners[0], corners[1]);
            
            Debug.Log($"Play Button: " +
                      $"RaycastTarget={playButton.raycastTarget}, " +
                      $"Button={btn != null}, " +
                      $"Interactable={btn != null && btn.interactable}, " +
                      $"Anchors=({anchorMin.x:F2},{anchorMin.y:F2}) to ({anchorMax.x:F2},{anchorMax.y:F2}), " +
                      $"SizeDelta={sizeDelta}, " +
                      $"WorldSize={worldWidth:F0}x{worldHeight:F0}, " +
                      $"CoversCanvas={(worldWidth >= canvasWidth * 0.9f || worldHeight >= canvasHeight * 0.9f)}");
        }
        
        if (quitButton != null)
        {
            Button btn = quitButton.GetComponent<Button>();
            RectTransform rect = quitButton.rectTransform;
            Vector2 anchorMin = rect.anchorMin;
            Vector2 anchorMax = rect.anchorMax;
            Vector2 sizeDelta = rect.sizeDelta;
            
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            float worldWidth = Vector2.Distance(corners[0], corners[2]);
            float worldHeight = Vector2.Distance(corners[0], corners[1]);
            
            Debug.Log($"Quit Button: " +
                      $"RaycastTarget={quitButton.raycastTarget}, " +
                      $"Button={btn != null}, " +
                      $"Interactable={btn != null && btn.interactable}, " +
                      $"Anchors=({anchorMin.x:F2},{anchorMin.y:F2}) to ({anchorMax.x:F2},{anchorMax.y:F2}), " +
                      $"SizeDelta={sizeDelta}, " +
                      $"WorldSize={worldWidth:F0}x{worldHeight:F0}, " +
                      $"CoversCanvas={(worldWidth >= canvasWidth * 0.9f || worldHeight >= canvasHeight * 0.9f)}");
        }
        
        if (settingsButton != null)
        {
            Button btn = settingsButton.GetComponent<Button>();
            RectTransform rect = settingsButton.rectTransform;
            Vector2 anchorMin = rect.anchorMin;
            Vector2 anchorMax = rect.anchorMax;
            Vector2 sizeDelta = rect.sizeDelta;
            
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            float worldWidth = Vector2.Distance(corners[0], corners[2]);
            float worldHeight = Vector2.Distance(corners[0], corners[1]);
            
            Debug.Log($"Settings Button: " +
                      $"RaycastTarget={settingsButton.raycastTarget}, " +
                      $"Button={btn != null}, " +
                      $"Interactable={btn != null && btn.interactable}, " +
                      $"Anchors=({anchorMin.x:F2},{anchorMin.y:F2}) to ({anchorMax.x:F2},{anchorMax.y:F2}), " +
                      $"SizeDelta={sizeDelta}, " +
                      $"WorldSize={worldWidth:F0}x{worldHeight:F0}, " +
                      $"CoversCanvas={(worldWidth >= canvasWidth * 0.9f || worldHeight >= canvasHeight * 0.9f)}");
        }
        
        // Hierarchy sırasını kontrol et
        Debug.Log("=== Button Hierarchy Order (Top to Bottom) ===");
        if (canvas != null)
        {
            int playIndex = -1, quitIndex = -1, settingsIndex = -1;
            for (int i = 0; i < canvas.transform.childCount; i++)
            {
                Transform child = canvas.transform.GetChild(i);
                if (playButton != null && child == playButton.transform)
                    playIndex = i;
                if (quitButton != null && child == quitButton.transform)
                    quitIndex = i;
                if (settingsButton != null && child == settingsButton.transform)
                    settingsIndex = i;
            }
            Debug.Log($"Play Index: {playIndex}, Quit Index: {quitIndex}, Settings Index: {settingsIndex} (Higher = Top, receives clicks first)");
        }
    }
    
    /// <summary>
    /// Buton event'lerini ayarlar
    /// </summary>
    private void SetupButtons()
    {
        // Önce canvas'taki tüm RawImage'ların RaycastTarget'ını kapat
        // (Sadece button olanlar açık kalacak)
        DisableNonButtonRaycastTargets();
        
        // Play Button
        if (playButton != null)
        {
            // Raw Image'ın RaycastTarget'ını açık tut (button olarak çalışması için)
            playButton.raycastTarget = true;
            
            // Raw Image'a Button component ekle (eğer yoksa)
            Button playBtn = playButton.GetComponent<Button>();
            if (playBtn == null)
            {
                playBtn = playButton.gameObject.AddComponent<Button>();
            }
            
            // Button'ın target graphic'ini ayarla
            playBtn.targetGraphic = playButton;
            
            // Button'ın transition'ını None yap (RawImage kullanıyorsak)
            playBtn.transition = Selectable.Transition.None;
            
            // Event listener ekle
            playBtn.onClick.RemoveAllListeners();
            playBtn.onClick.AddListener(OnPlayButtonClicked);
            
            // RectTransform'u kontrol et ve düzelt
            FixButtonRectTransform(playButton.rectTransform);
        }
        
        // Quit Button
        if (quitButton != null)
        {
            quitButton.raycastTarget = true;
            
            Button quitBtn = quitButton.GetComponent<Button>();
            if (quitBtn == null)
            {
                quitBtn = quitButton.gameObject.AddComponent<Button>();
            }
            
            quitBtn.targetGraphic = quitButton;
            quitBtn.transition = Selectable.Transition.None;
            
            quitBtn.onClick.RemoveAllListeners();
            quitBtn.onClick.AddListener(OnQuitButtonClicked);
            
            FixButtonRectTransform(quitButton.rectTransform);
        }
        
        // Settings Button
        if (settingsButton != null)
        {
            settingsButton.raycastTarget = true;
            
            Button settingsBtn = settingsButton.GetComponent<Button>();
            if (settingsBtn == null)
            {
                settingsBtn = settingsButton.gameObject.AddComponent<Button>();
            }
            
            settingsBtn.targetGraphic = settingsButton;
            settingsBtn.transition = Selectable.Transition.None;
            
            settingsBtn.onClick.RemoveAllListeners();
            settingsBtn.onClick.AddListener(OnSettingsButtonClicked);
            
            FixButtonRectTransform(settingsButton.rectTransform);
        }
    }
    
    /// <summary>
    /// Button olmayan tüm RawImage'ların RaycastTarget'ını kapatır
    /// </summary>
    private void DisableNonButtonRaycastTargets()
    {
        // Canvas'ı bul
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            return;
        }
        
        // Canvas'taki tüm RawImage'ları bul
        RawImage[] allRawImages = canvas.GetComponentsInChildren<RawImage>(true);
        
        foreach (RawImage rawImg in allRawImages)
        {
            // Eğer bu RawImage button değilse, RaycastTarget'ını kapat
            if (rawImg != playButton && rawImg != quitButton && rawImg != settingsButton)
            {
                rawImg.raycastTarget = false;
                Debug.Log($"[MainMenu] Disabled RaycastTarget for: {rawImg.gameObject.name}");
            }
        }
        
        // Ayrıca tüm Image component'lerini de kontrol et (eğer varsa)
        UnityEngine.UI.Image[] allImages = canvas.GetComponentsInChildren<UnityEngine.UI.Image>(true);
        foreach (UnityEngine.UI.Image img in allImages)
        {
            // Eğer bu Image bir button'un child'ı değilse, RaycastTarget'ını kapat
            if (img.GetComponent<Button>() == null && 
                img.transform.parent != null && 
                img.transform.parent.GetComponent<Button>() == null)
            {
                img.raycastTarget = false;
            }
        }
    }
    
    /// <summary>
    /// Button'ın RectTransform'unu düzeltir (eğer canvas'ı kaplıyorsa)
    /// </summary>
    private void FixButtonRectTransform(RectTransform rectTransform)
    {
        if (rectTransform == null)
        {
            return;
        }
        
        Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            return;
        }
        
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect == null)
        {
            return;
        }
        
        // Canvas'ın gerçek boyutunu al (Screen space için)
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        
        // Eğer canvas boyutu 0 ise, Screen boyutunu kullan
        if (canvasWidth <= 0 || canvasHeight <= 0)
        {
            canvasWidth = Screen.width;
            canvasHeight = Screen.height;
        }
        
        RawImage rawImage = rectTransform.GetComponent<RawImage>();
        
        // Anchor'ları kontrol et - eğer stretch (0-1) ise, bunu düzelt
        Vector2 anchorMin = rectTransform.anchorMin;
        Vector2 anchorMax = rectTransform.anchorMax;
        
        // Eğer anchor'lar canvas'ı tamamen kaplıyorsa (stretch), center'a çevir
        if (anchorMin.x < 0.1f && anchorMax.x > 0.9f && anchorMin.y < 0.1f && anchorMax.y > 0.9f)
        {
            Debug.LogWarning($"[MainMenu] Button {rectTransform.gameObject.name} has stretch anchors! Fixing...");
            
            // Center anchor'a çevir
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            
            // Texture boyutuna göre size ayarla
            if (rawImage != null && rawImage.texture != null)
            {
                float textureWidth = rawImage.texture.width;
                float textureHeight = rawImage.texture.height;
                float aspectRatio = textureWidth / textureHeight;
                
                // Canvas'ın %30'undan büyük olmamalı (button için)
                float maxWidth = canvasWidth * 0.3f;
                float maxHeight = canvasHeight * 0.3f;
                
                float newWidth = Mathf.Min(textureWidth, maxWidth);
                float newHeight = newWidth / aspectRatio;
                
                if (newHeight > maxHeight)
                {
                    newHeight = maxHeight;
                    newWidth = newHeight * aspectRatio;
                }
                
                rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
                Debug.Log($"[MainMenu] Fixed button {rectTransform.gameObject.name}: Size={newWidth}x{newHeight}, Anchors=Center");
            }
            else
            {
                // Texture yoksa, varsayılan boyut kullan
                rectTransform.sizeDelta = new Vector2(200f, 100f);
                Debug.Log($"[MainMenu] Fixed button {rectTransform.gameObject.name}: Default size 200x100");
            }
        }
        else
        {
            // Anchor'lar doğru ama size çok büyükse düzelt
            Vector2 currentSize = rectTransform.sizeDelta;
            
            if (currentSize.x >= canvasWidth * 0.9f || currentSize.y >= canvasHeight * 0.9f)
            {
                if (rawImage != null && rawImage.texture != null)
                {
                    float textureWidth = rawImage.texture.width;
                    float textureHeight = rawImage.texture.height;
                    float aspectRatio = textureWidth / textureHeight;
                    
                    float maxWidth = canvasWidth * 0.3f;
                    float maxHeight = canvasHeight * 0.3f;
                    
                    float newWidth = Mathf.Min(textureWidth, maxWidth);
                    float newHeight = newWidth / aspectRatio;
                    
                    if (newHeight > maxHeight)
                    {
                        newHeight = maxHeight;
                        newWidth = newHeight * aspectRatio;
                    }
                    
                    rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
                    Debug.Log($"[MainMenu] Fixed button size: {rectTransform.gameObject.name} -> {newWidth}x{newHeight}");
                }
            }
        }
        
        // Debug: Button'ın gerçek pozisyonunu ve boyutunu göster
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        Debug.Log($"[MainMenu] Button {rectTransform.gameObject.name} - " +
                  $"Anchors: Min={anchorMin}, Max={anchorMax}, " +
                  $"SizeDelta={rectTransform.sizeDelta}, " +
                  $"WorldSize={Vector2.Distance(corners[0], corners[2])}x{Vector2.Distance(corners[0], corners[1])}");
    }
    
    /// <summary>
    /// Play butonuna tıklandığında çağrılır
    /// </summary>
    public void OnPlayButtonClicked()
    {
        PlayButtonSound();
        
        Debug.Log($"[MainMenu] Play button clicked! Loading scene: {gameSceneName}");
        
        // Oyun scene'ini yükle
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            // Scene'in Build Settings'te olup olmadığını kontrol et
            if (IsSceneInBuildSettings(gameSceneName))
            {
                SceneManager.LoadScene(gameSceneName);
            }
            else
            {
                Debug.LogError($"[MainMenu] Scene '{gameSceneName}' is not in Build Settings!\n" +
                              "Please add it via: File > Build Settings > Add Open Scenes\n\n" +
                              "Available scenes in Build Settings:");
                ListAvailableScenes();
            }
        }
        else
        {
            Debug.LogError("[MainMenu] Game scene name is not set! Please assign a scene name in the Inspector.");
        }
    }
    
    /// <summary>
    /// Scene'in Build Settings'te olup olmadığını kontrol eder
    /// </summary>
    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            if (sceneNameFromPath == sceneName)
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Build Settings'teki tüm scene'leri listeler
    /// </summary>
    private void ListAvailableScenes()
    {
        if (SceneManager.sceneCountInBuildSettings == 0)
        {
            Debug.LogWarning("[MainMenu] No scenes in Build Settings! Please add scenes via File > Build Settings");
            return;
        }
        
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"  [{i}] {sceneName} ({scenePath})");
        }
    }
    
    /// <summary>
    /// Quit butonuna tıklandığında çağrılır
    /// </summary>
    public void OnQuitButtonClicked()
    {
        PlayButtonSound();
        
        Debug.Log("[MainMenu] Quit button clicked!");
        
        #if UNITY_EDITOR
        // Editor'da oynatıyorsak Play Mode'u durdur
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // Build'de oyundan çık
        Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Settings butonuna tıklandığında çağrılır
    /// </summary>
    public void OnSettingsButtonClicked()
    {
        PlayButtonSound();
        
        // Settings panelini aç/kapat
        ToggleSettingsPanel();
    }
    
    /// <summary>
    /// Settings panelini açar/kapatır
    /// </summary>
    public void ToggleSettingsPanel()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("[MainMenu] Settings panel is not assigned!");
            return;
        }
        
        isSettingsOpen = !isSettingsOpen;
        settingsPanel.SetActive(isSettingsOpen);
        
        Debug.Log($"[MainMenu] Settings panel {(isSettingsOpen ? "opened" : "closed")}.");
    }
    
    /// <summary>
    /// Settings panelini açar
    /// </summary>
    public void OpenSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            isSettingsOpen = true;
        }
    }
    
    /// <summary>
    /// Settings panelini kapatır
    /// </summary>
    public void CloseSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            isSettingsOpen = false;
        }
    }
    
    /// <summary>
    /// Buton tıklama sesini çalar
    /// </summary>
    private void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
    
    /// <summary>
    /// Game scene adını ayarlar (kod ile değiştirmek için)
    /// </summary>
    public void SetGameSceneName(string sceneName)
    {
        gameSceneName = sceneName;
    }
    
    private void OnValidate()
    {
        // Inspector'da değişiklik yapıldığında settings panel durumunu güncelle
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(isSettingsOpen);
        }
    }
}

