using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Kod ile basit bir crosshair (nişangah) oluşturur
/// Sprite'a gerek kalmadan + şeklinde crosshair yapar
/// </summary>
[RequireComponent(typeof(CrosshairController))]
public class SimpleCrosshairGenerator : MonoBehaviour
{
    [Header("Crosshair Style")]
    [Tooltip("Crosshair tipi")]
    [SerializeField] private CrosshairType crosshairType = CrosshairType.Cross;
    
    [Tooltip("Crosshair rengi")]
    [SerializeField] private Color crosshairColor = Color.white;
    
    [Tooltip("Çizgi kalınlığı (pixel)")]
    [SerializeField, Range(1f, 10f)] private float lineThickness = 2f;
    
    [Tooltip("Çizgi uzunluğu (pixel)")]
    [SerializeField, Range(5f, 50f)] private float lineLength = 15f;
    
    [Tooltip("Merkezdeki boşluk (pixel)")]
    [SerializeField, Range(0f, 20f)] private float centerGap = 5f;
    
    [Tooltip("Nokta çapı (sadece Dot tipi için)")]
    [SerializeField, Range(2f, 20f)] private float dotSize = 4f;
    
    [Header("PlayerPrefs Keys")]
    [SerializeField] private string crosshairTypeKey = "CrosshairType";
    [SerializeField] private string crosshairColorRKey = "CrosshairColorR";
    [SerializeField] private string crosshairColorGKey = "CrosshairColorG";
    [SerializeField] private string crosshairColorBKey = "CrosshairColorB";
    [SerializeField] private string crosshairColorAKey = "CrosshairColorA";
    [SerializeField] private string lineThicknessKey = "CrosshairThickness";
    [SerializeField] private string lineLengthKey = "CrosshairLength";
    [SerializeField] private string centerGapKey = "CrosshairGap";
    [SerializeField] private string dotSizeKey = "CrosshairDotSize";
    
    public enum CrosshairType
    {
        Cross,      // + şekli
        Dot,        // • nokta
        Circle,     // ○ daire
        Hybrid      // + ve • kombine
    }
    
    private GameObject crosshairContainer;
    private Canvas crosshairCanvas;
    private CrosshairController crosshairController;
    private bool isVisible = true;
    
    private void Awake()
    {
        crosshairController = GetComponent<CrosshairController>();
        
        // Parent'tan ayır (Player child'ı ise Player yok olduğunda bu kalır)
        if (transform.parent != null)
        {
            Debug.Log($"[SimpleCrosshairGenerator] Detaching from parent: {transform.parent.name}");
            transform.SetParent(null);
        }
        
        // SimpleCrosshairGenerator'ı sahne değişimlerinde korumak için DontDestroyOnLoad
        // Ama sadece tek instance olsun (duplicate önle)
        SimpleCrosshairGenerator[] generators = FindObjectsByType<SimpleCrosshairGenerator>(FindObjectsSortMode.None);
        if (generators.Length == 1)
        {
            DontDestroyOnLoad(gameObject);
            Debug.Log("[SimpleCrosshairGenerator] Set as DontDestroyOnLoad.");
        }
        else if (generators.Length > 1)
        {
            // Duplicate var, en eskisini tut yenisini sil
            bool isOldest = true;
            foreach (SimpleCrosshairGenerator gen in generators)
            {
                if (gen != this && gen.gameObject.scene.buildIndex == -1) // DontDestroyOnLoad'da mı?
                {
                    isOldest = false;
                    break;
                }
            }
            
            if (!isOldest)
            {
                Debug.LogWarning("[SimpleCrosshairGenerator] Duplicate found! Destroying this instance.");
                Destroy(gameObject);
                return;
            }
            else
            {
                DontDestroyOnLoad(gameObject);
                Debug.Log("[SimpleCrosshairGenerator] Kept as the oldest instance.");
            }
        }
    }
    
    private void OnEnable()
    {
        // Sahne değişiminde veya GameObject enable olduğunda
        // crosshairContainer yok olduysa yeniden oluştur
        if (crosshairContainer == null || crosshairCanvas == null)
        {
            Debug.Log("[SimpleCrosshairGenerator] Crosshair missing, regenerating...");
            crosshairCanvas = null; // Canvas referansını sıfırla
            LoadSettings();
            GenerateCrosshair();
            SetVisibility(true);
        }
    }
    
    private void Start()
    {
        // Kaydedilmiş ayarları yükle
        LoadSettings();
        
        // Crosshair'i oluştur
        GenerateCrosshair();
        
        // Başlangıç görünürlüğünü ayarla
        SetVisibility(true);
        
        Debug.Log("[SimpleCrosshairGenerator] Crosshair initialized.");
    }
    
    private void Update()
    {
        // Crosshair container yok olmuş mu kontrol et (sahne değişimi sonrası)
        if (crosshairContainer == null || crosshairCanvas == null)
        {
            // Crosshair'i yeniden oluştur
            Debug.LogWarning("[SimpleCrosshairGenerator] Crosshair container lost! Recreating...");
            crosshairCanvas = null;
            GenerateCrosshair();
            SetVisibility(true);
            return;
        }
        
        // UI durumuna göre görünürlüğü kontrol et
        bool shouldBeVisible = !IsUIOpen();
        
        // Durum değiştiyse güncelle
        if (isVisible != shouldBeVisible)
        {
            SetVisibility(shouldBeVisible);
            Debug.Log($"[SimpleCrosshairGenerator] Visibility changed: {shouldBeVisible} (UI Open: {IsUIOpen()})");
        }
    }
    
    /// <summary>
    /// UI menülerinin açık olup olmadığını veya MainMenu sahnesinde olup olmadığını kontrol eder
    /// </summary>
    private bool IsUIOpen()
    {
        // MainMenu sahnesinde mi kontrol et
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName.Contains("MainMenu") || currentSceneName.Contains("Menu"))
        {
            // MainMenu'deyiz, crosshair'i gizle
            return true;
        }
        
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
    /// Crosshair'i oluşturur
    /// </summary>
    public void GenerateCrosshair()
    {
        // Eski crosshair'i temizle
        if (crosshairContainer != null)
        {
            Destroy(crosshairContainer);
        }
        
        // Canvas bul veya oluştur
        if (crosshairCanvas == null)
        {
            // Tüm Canvas'ları bul
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            
            // ÖNCE CrosshairCanvas isimli canvas'ı ara (varsa)
            foreach (Canvas canvas in allCanvases)
            {
                if (canvas.name.Contains("CrosshairCanvas") || canvas.name.Contains("PlayerUI"))
                {
                    crosshairCanvas = canvas;
                    Debug.Log($"[SimpleCrosshairGenerator] ✅ Found preferred Canvas: {canvas.name}");
                    break;
                }
            }
            
            // Bulunamadıysa ScreenSpaceOverlay Canvas'ı tercih et AMA SettingsCanvas değil!
            if (crosshairCanvas == null)
            {
                foreach (Canvas canvas in allCanvases)
                {
                    // SettingsCanvas, InventoryCanvas gibi UI Canvas'larını ATLA
                    if (canvas.name.Contains("Settings") || canvas.name.Contains("Inventory") || 
                        canvas.name.Contains("Menu") || canvas.name.Contains("Death"))
                    {
                        continue; // Bu Canvas'ları atla
                    }
                    
                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        crosshairCanvas = canvas;
                        Debug.Log($"[SimpleCrosshairGenerator] Using ScreenSpaceOverlay Canvas: {canvas.name}");
                        break;
                    }
                }
            }
            
            // Hala bulunamadıysa YENİ BİR CROSSHAIR CANVAS OLUŞTUR
            if (crosshairCanvas == null)
            {
                Debug.LogWarning("[SimpleCrosshairGenerator] No suitable Canvas found! Creating CrosshairCanvas...");
                GameObject canvasObj = new GameObject("CrosshairCanvas");
                crosshairCanvas = canvasObj.AddComponent<Canvas>();
                crosshairCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                crosshairCanvas.sortingOrder = 9999; // En üstte
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                
                Debug.Log("[SimpleCrosshairGenerator] ✅ Created new CrosshairCanvas!");
            }
        }
        
        if (crosshairCanvas == null)
        {
            Debug.LogError("[SimpleCrosshairGenerator] ❌ No Canvas found in scene!");
            return;
        }
        
        // Container oluştur
        crosshairContainer = new GameObject("GeneratedCrosshair");
        crosshairContainer.transform.SetParent(crosshairCanvas.transform, false);
        
        RectTransform containerRect = crosshairContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(100f, 100f);
        
        // Crosshair tipine göre oluştur
        switch (crosshairType)
        {
            case CrosshairType.Cross:
                CreateCrossLines();
                break;
            case CrosshairType.Dot:
                CreateDot();
                break;
            case CrosshairType.Circle:
                CreateCircle();
                break;
            case CrosshairType.Hybrid:
                CreateCrossLines();
                CreateDot();
                break;
        }
        
        Debug.Log($"[SimpleCrosshairGenerator] ✅ Crosshair generated! Type: {crosshairType}, Canvas: {crosshairCanvas.name}");
        
        // Crosshair yeniden oluşturulduktan sonra mevcut duruma göre visibility'yi ayarla
        bool shouldBeVisible = !IsUIOpen();
        SetVisibility(shouldBeVisible);
        Debug.Log($"[SimpleCrosshairGenerator] Post-generation visibility set to: {shouldBeVisible}");
    }
    
    /// <summary>
    /// + şeklinde crosshair oluşturur
    /// </summary>
    private void CreateCrossLines()
    {
        // Üst çizgi
        CreateLine("Top", new Vector2(0, centerGap + lineLength/2), new Vector2(lineThickness, lineLength));
        
        // Alt çizgi
        CreateLine("Bottom", new Vector2(0, -(centerGap + lineLength/2)), new Vector2(lineThickness, lineLength));
        
        // Sol çizgi
        CreateLine("Left", new Vector2(-(centerGap + lineLength/2), 0), new Vector2(lineLength, lineThickness));
        
        // Sağ çizgi
        CreateLine("Right", new Vector2(centerGap + lineLength/2, 0), new Vector2(lineLength, lineThickness));
    }
    
    /// <summary>
    /// Tek bir çizgi oluşturur
    /// </summary>
    private void CreateLine(string name, Vector2 position, Vector2 size)
    {
        GameObject line = new GameObject($"Line_{name}");
        line.transform.SetParent(crosshairContainer.transform, false);
        
        RectTransform rect = line.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        Image img = line.AddComponent<Image>();
        img.color = crosshairColor;
        img.raycastTarget = false;
    }
    
    /// <summary>
    /// Merkeze nokta oluşturur
    /// </summary>
    private void CreateDot()
    {
        GameObject dot = new GameObject("Dot_Center");
        dot.transform.SetParent(crosshairContainer.transform, false);
        
        RectTransform rect = dot.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(dotSize, dotSize);
        
        Image img = dot.AddComponent<Image>();
        img.color = crosshairColor;
        img.raycastTarget = false;
        
        // Yuvarlak yapmak için sprite kullanmalıyız, ama basit kare de çalışır
        // Eğer yuvarlak istiyorsanız Unity'nin varsayılan UI sprite'larını kullanın
    }
    
    /// <summary>
    /// Daire şeklinde crosshair oluşturur
    /// </summary>
    private void CreateCircle()
    {
        GameObject circle = new GameObject("Circle");
        circle.transform.SetParent(crosshairContainer.transform, false);
        
        RectTransform rect = circle.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(lineLength * 2, lineLength * 2);
        
        Image img = circle.AddComponent<Image>();
        img.color = new Color(crosshairColor.r, crosshairColor.g, crosshairColor.b, crosshairColor.a * 0.5f); // Yarı şeffaf
        img.raycastTarget = false;
        img.type = Image.Type.Simple;
        
        // Unity'nin varsayılan UI sprite'ı (Circle) kullanılabilir
        // Inspector'dan sprite atanmazsa düz kare görünür
        Debug.LogWarning("[SimpleCrosshairGenerator] Circle type requires a circle sprite. Assign 'UI/Skin/UISprite' from Unity's default resources.");
    }
    
    /// <summary>
    /// Crosshair rengini değiştirir
    /// </summary>
    public void SetColor(Color color)
    {
        crosshairColor = color;
        
        if (crosshairContainer != null)
        {
            foreach (Image img in crosshairContainer.GetComponentsInChildren<Image>())
            {
                img.color = color;
            }
        }
        
        SaveSettings();
    }
    
    /// <summary>
    /// Crosshair tipini değiştirir
    /// </summary>
    public void SetCrosshairType(int typeIndex)
    {
        crosshairType = (CrosshairType)typeIndex;
        GenerateCrosshair();
        SaveSettings();
    }
    
    /// <summary>
    /// Crosshair tipini değiştirir (enum ile)
    /// </summary>
    public void SetCrosshairType(CrosshairType type)
    {
        crosshairType = type;
        GenerateCrosshair();
        SaveSettings();
    }
    
    /// <summary>
    /// Çizgi kalınlığını değiştirir
    /// </summary>
    public void SetLineThickness(float thickness)
    {
        lineThickness = Mathf.Clamp(thickness, 1f, 10f);
        GenerateCrosshair();
        SaveSettings();
    }
    
    /// <summary>
    /// Çizgi uzunluğunu değiştirir
    /// </summary>
    public void SetLineLength(float length)
    {
        lineLength = Mathf.Clamp(length, 5f, 50f);
        GenerateCrosshair();
        SaveSettings();
    }
    
    /// <summary>
    /// Merkez boşluğunu değiştirir
    /// </summary>
    public void SetCenterGap(float gap)
    {
        centerGap = Mathf.Clamp(gap, 0f, 20f);
        GenerateCrosshair();
        SaveSettings();
    }
    
    /// <summary>
    /// Nokta boyutunu değiştirir
    /// </summary>
    public void SetDotSize(float size)
    {
        dotSize = Mathf.Clamp(size, 2f, 20f);
        GenerateCrosshair();
        SaveSettings();
    }
    
    /// <summary>
    /// Ayarları PlayerPrefs'e kaydeder
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.SetInt(crosshairTypeKey, (int)crosshairType);
        PlayerPrefs.SetFloat(crosshairColorRKey, crosshairColor.r);
        PlayerPrefs.SetFloat(crosshairColorGKey, crosshairColor.g);
        PlayerPrefs.SetFloat(crosshairColorBKey, crosshairColor.b);
        PlayerPrefs.SetFloat(crosshairColorAKey, crosshairColor.a);
        PlayerPrefs.SetFloat(lineThicknessKey, lineThickness);
        PlayerPrefs.SetFloat(lineLengthKey, lineLength);
        PlayerPrefs.SetFloat(centerGapKey, centerGap);
        PlayerPrefs.SetFloat(dotSizeKey, dotSize);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Ayarları PlayerPrefs'ten yükler
    /// </summary>
    public void LoadSettings()
    {
        crosshairType = (CrosshairType)PlayerPrefs.GetInt(crosshairTypeKey, (int)crosshairType);
        crosshairColor = new Color(
            PlayerPrefs.GetFloat(crosshairColorRKey, crosshairColor.r),
            PlayerPrefs.GetFloat(crosshairColorGKey, crosshairColor.g),
            PlayerPrefs.GetFloat(crosshairColorBKey, crosshairColor.b),
            PlayerPrefs.GetFloat(crosshairColorAKey, crosshairColor.a)
        );
        lineThickness = PlayerPrefs.GetFloat(lineThicknessKey, lineThickness);
        lineLength = PlayerPrefs.GetFloat(lineLengthKey, lineLength);
        centerGap = PlayerPrefs.GetFloat(centerGapKey, centerGap);
        dotSize = PlayerPrefs.GetFloat(dotSizeKey, dotSize);
    }
    
    /// <summary>
    /// Varsayılan ayarlara sıfırlar
    /// </summary>
    public void ResetToDefaults()
    {
        crosshairType = CrosshairType.Cross;
        crosshairColor = Color.white;
        lineThickness = 2f;
        lineLength = 15f;
        centerGap = 5f;
        dotSize = 4f;
        
        GenerateCrosshair();
        SaveSettings();
    }
    
    // Getter fonksiyonları (UI'ın mevcut değerleri alması için)
    public CrosshairType GetCrosshairType() => crosshairType;
    public Color GetCrosshairColor() => crosshairColor;
    public float GetLineThickness() => lineThickness;
    public float GetLineLength() => lineLength;
    public float GetCenterGap() => centerGap;
    public float GetDotSize() => dotSize;
    
    /// <summary>
    /// Crosshair'in görünürlüğünü değiştirir (SetActive yerine enabled kullanır - daha güvenli)
    /// </summary>
    public void SetVisibility(bool visible)
    {
        isVisible = visible;
        
        if (crosshairContainer != null)
        {
            // SetActive yerine Canvas Group veya Image.enabled kullan
            CanvasGroup canvasGroup = crosshairContainer.GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
            {
                // Canvas Group yoksa ekle
                canvasGroup = crosshairContainer.AddComponent<CanvasGroup>();
                Debug.Log("[SimpleCrosshairGenerator] Added CanvasGroup to crosshair container.");
            }
            
            // CanvasGroup ile görünürlüğü kontrol et (daha güvenli)
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
            
            Debug.Log($"[SimpleCrosshairGenerator] SetVisibility({visible}) - Alpha: {canvasGroup.alpha}");
        }
        else
        {
            Debug.LogWarning("[SimpleCrosshairGenerator] ⚠️ Cannot set visibility - crosshairContainer is NULL!");
        }
    }
}
