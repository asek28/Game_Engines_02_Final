using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ekranın ortasında crosshair (nişangah) gösterir
/// Settings/Inventory açıkken gizlenir, oyunda görünür
/// </summary>
public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair UI")]
    [Tooltip("Crosshair Image (ekranın merkezinde olmalı) - SimpleCrosshairGenerator kullanıyorsanız boş bırakın")]
    [SerializeField] private Image crosshairImage;
    
    [Header("Settings")]
    [Tooltip("Crosshair rengi")]
    [SerializeField] private Color crosshairColor = Color.white;
    
    [Tooltip("Crosshair boyutu (width ve height)")]
    [SerializeField] private Vector2 crosshairSize = new Vector2(32f, 32f);
    
    [Tooltip("Crosshair sprite'ı (yoksa SimpleCrosshairGenerator kullanın)")]
    [SerializeField] private Sprite crosshairSprite;
    
    [Header("Visibility")]
    [Tooltip("Oyun başladığında crosshair göster")]
    [SerializeField] private bool showOnStart = true;
    
    private Canvas crosshairCanvas;
    private bool isVisible = true;
    
    private void Awake()
    {
        // SimpleCrosshairGenerator kullanılıyorsa manuel kurulum yapma
        SimpleCrosshairGenerator generator = GetComponent<SimpleCrosshairGenerator>();
        if (generator != null)
        {
            Debug.Log("[CrosshairController] SimpleCrosshairGenerator detected. Skipping manual setup.");
            return;
        }
        
        // Crosshair canvas'ını oluştur veya bul
        SetupCrosshairCanvas();
        
        // Crosshair image'ını oluştur veya ayarla
        SetupCrosshairImage();
    }
    
    private void Start()
    {
        // Başlangıçta crosshair'i göster/gizle
        SetCrosshairVisibility(showOnStart);
    }
    
    private void Update()
    {
        // SimpleCrosshairGenerator kullanılıyorsa görünürlük kontrolünü ona bırak
        SimpleCrosshairGenerator generator = GetComponent<SimpleCrosshairGenerator>();
        if (generator != null)
        {
            return;
        }
        
        // Settings veya Inventory açıksa crosshair'i gizle
        UpdateCrosshairVisibility();
    }
    
    /// <summary>
    /// Crosshair canvas'ını oluşturur veya bulur
    /// </summary>
    private void SetupCrosshairCanvas()
    {
        // Eğer crosshairImage null ise, canvas ve image oluştur
        if (crosshairImage == null)
        {
            // Canvas oluştur
            GameObject canvasObj = new GameObject("CrosshairCanvas");
            canvasObj.transform.SetParent(transform);
            
            crosshairCanvas = canvasObj.AddComponent<Canvas>();
            crosshairCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            crosshairCanvas.sortingOrder = 100; // En üstte görünsün
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Image oluştur
            GameObject imageObj = new GameObject("Crosshair");
            imageObj.transform.SetParent(canvasObj.transform);
            
            crosshairImage = imageObj.AddComponent<Image>();
            
            // RectTransform ayarları (ekranın tam ortası)
            RectTransform rect = crosshairImage.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = crosshairSize;
            
            Debug.Log("[CrosshairController] Created crosshair canvas and image.");
        }
        else
        {
            // Crosshair image zaten atanmış, canvas'ı bul
            crosshairCanvas = crosshairImage.GetComponentInParent<Canvas>();
        }
    }
    
    /// <summary>
    /// Crosshair image'ını ayarlar
    /// </summary>
    private void SetupCrosshairImage()
    {
        if (crosshairImage == null) return;
        
        // Rengi ayarla
        crosshairImage.color = crosshairColor;
        
        // Sprite'ı ayarla (eğer varsa)
        if (crosshairSprite != null)
        {
            crosshairImage.sprite = crosshairSprite;
        }
        else
        {
            // Varsayılan sprite yoksa, basit bir + işareti oluştur
            // Not: Unity'de runtime'da sprite oluşturmak karmaşık, 
            // bu yüzden kullanıcının kendi sprite'ını eklemesini öneririz
            crosshairImage.sprite = null;
            crosshairImage.color = crosshairColor;
            Debug.LogWarning("[CrosshairController] No crosshair sprite assigned! Please assign a crosshair sprite in the Inspector.");
        }
        
        // Boyutu ayarla
        RectTransform rect = crosshairImage.rectTransform;
        rect.sizeDelta = crosshairSize;
    }
    
    /// <summary>
    /// Crosshair görünürlüğünü günceller (Settings/Inventory durumuna göre)
    /// </summary>
    private void UpdateCrosshairVisibility()
    {
        // Settings veya Inventory açık mı kontrol et
        bool shouldHide = IsUIOpen();
        
        // Crosshair'in görünür olması gereken durumu hesapla
        bool shouldBeVisible = !shouldHide;
        
        // Eğer durum değiştiyse güncelle
        if (isVisible != shouldBeVisible)
        {
            SetCrosshairVisibility(shouldBeVisible);
        }
    }
    
    /// <summary>
    /// UI menülerinin açık olup olmadığını kontrol eder
    /// </summary>
    private bool IsUIOpen()
    {
        // Settings menüsü açık mı?
        SettingsMenuController settingsMenu = FindFirstObjectByType<SettingsMenuController>();
        if (settingsMenu != null && settingsMenu.IsSettingsOpen())
        {
            return true;
        }
        
        // Inventory açık mı?
        if (InventoryManager.instance != null && InventoryManager.instance.IsInventoryVisible)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Crosshair'i gösterir veya gizler
    /// </summary>
    public void SetCrosshairVisibility(bool visible)
    {
        isVisible = visible;
        
        if (crosshairImage != null)
        {
            crosshairImage.enabled = visible;
        }
        
        if (crosshairCanvas != null)
        {
            crosshairCanvas.enabled = visible;
        }
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[CrosshairController] Crosshair visibility: {visible}");
        #endif
    }
    
    /// <summary>
    /// Crosshair rengini değiştirir
    /// </summary>
    public void SetCrosshairColor(Color color)
    {
        crosshairColor = color;
        if (crosshairImage != null)
        {
            crosshairImage.color = color;
        }
    }
    
    /// <summary>
    /// Crosshair boyutunu değiştirir
    /// </summary>
    public void SetCrosshairSize(Vector2 size)
    {
        crosshairSize = size;
        if (crosshairImage != null)
        {
            crosshairImage.rectTransform.sizeDelta = size;
        }
    }
    
    /// <summary>
    /// Crosshair sprite'ını değiştirir
    /// </summary>
    public void SetCrosshairSprite(Sprite sprite)
    {
        crosshairSprite = sprite;
        if (crosshairImage != null)
        {
            crosshairImage.sprite = sprite;
        }
    }
}
