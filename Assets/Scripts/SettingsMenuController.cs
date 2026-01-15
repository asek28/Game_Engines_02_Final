using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Settings Menu Controller - AudioMixer kullanarak ses ayarlarını, fullscreen modunu ve scene navigasyonunu yönetir
/// ESC tuşu ile oyunu durdurup settings menüsünü açar
/// </summary>
public class SettingsMenuController : MonoBehaviour
{
    [Header("Settings Panel")]
    [Tooltip("Settings Panel GameObject (ESC ile açılıp kapanacak)")]
    [SerializeField] private GameObject settingsPanel;
    
    [Header("ESC Key Settings")]
    [Tooltip("ESC tuşu ile menü açılsın mı? (Oyun scene'lerinde true, MainMenu'de false)")]
    [SerializeField] private bool enableESCKey = true;
    
    [Tooltip("Menü açıldığında oyunu duraklatsın mı? (Oyun scene'lerinde true, MainMenu'de false)")]
    [SerializeField] private bool pauseGameWhenOpen = true;
    
    [Header("Audio Mixer")]
    [Tooltip("AudioMixer referansı (MasterVol, MusicVol, SFXVol parametreleri expose edilmiş olmalı)")]
    [SerializeField] private AudioMixer audioMixer;
    
    [Header("Audio Sliders")]
    [Tooltip("Master Volume Slider (0-1 arası değer)")]
    [SerializeField] private Slider masterVolumeSlider;
    
    [Tooltip("Music Volume Slider (0-1 arası değer)")]
    [SerializeField] private Slider musicVolumeSlider;
    
    [Tooltip("SFX Volume Slider (0-1 arası değer)")]
    [SerializeField] private Slider sfxVolumeSlider;
    
    [Header("Graphics Settings")]
    [Tooltip("Fullscreen Toggle")]
    [SerializeField] private Toggle fullscreenToggle;
    
    [Header("Mute Button")]
    [Tooltip("Mute/Unmute butonu (Image component'i)")]
    [SerializeField] private UnityEngine.UI.Image muteButtonImage;
    
    [Tooltip("Ses açık sprite (ON görseli)")]
    [SerializeField] private Sprite soundOnSprite;
    
    [Tooltip("Ses kapalı sprite (OFF görseli)")]
    [SerializeField] private Sprite soundOffSprite;
    
    [Header("Crosshair Settings")]
    [Tooltip("Crosshair Type Dropdown (Cross, Dot, Circle, Hybrid) - TMP_Dropdown veya Dropdown")]
    [SerializeField] private TMP_Dropdown crosshairTypeDropdown;
    
    [Tooltip("Crosshair Color R (Kırmızı) Slider")]
    [SerializeField] private Slider crosshairColorRSlider;
    
    [Tooltip("Crosshair Color G (Yeşil) Slider")]
    [SerializeField] private Slider crosshairColorGSlider;
    
    [Tooltip("Crosshair Color B (Mavi) Slider")]
    [SerializeField] private Slider crosshairColorBSlider;
    
    [Tooltip("Crosshair Thickness Slider")]
    [SerializeField] private Slider crosshairThicknessSlider;
    
    [Tooltip("Crosshair Length Slider")]
    [SerializeField] private Slider crosshairLengthSlider;
    
    [Tooltip("Crosshair Gap Slider")]
    [SerializeField] private Slider crosshairGapSlider;
    
    [Header("Default Values")]
    [Tooltip("Varsayılan Master Volume (0-1)")]
    [SerializeField, Range(0f, 1f)] private float defaultMasterVolume = 1f;
    
    [Tooltip("Varsayılan Music Volume (0-1)")]
    [SerializeField, Range(0f, 1f)] private float defaultMusicVolume = 0.8f;
    
    [Tooltip("Varsayılan SFX Volume (0-1)")]
    [SerializeField, Range(0f, 1f)] private float defaultSFXVolume = 1f;
    
    [Header("PlayerPrefs Keys")]
    [Tooltip("PlayerPrefs key'leri (değiştirmek isterseniz)")]
    [SerializeField] private string masterVolumeKey = "MasterVolume";
    [SerializeField] private string musicVolumeKey = "MusicVolume";
    [SerializeField] private string sfxVolumeKey = "SFXVolume";
    [SerializeField] private string fullscreenKey = "Fullscreen";
    [SerializeField] private string isMutedKey = "IsMuted";
    
    // AudioMixer exposed parameter names
    private const string MASTER_VOL_PARAM = "MasterVol";
    private const string MUSIC_VOL_PARAM = "MusicVol";
    private const string SFX_VOL_PARAM = "SFXVol";
    
    // Internal state
    private bool isSettingsOpen = false;
    private bool isMuted = false;
    
    private void Awake()
    {
        // AudioMixer kontrolü
        if (audioMixer == null)
        {
            Debug.LogWarning("[SettingsMenuController] AudioMixer is not assigned! Audio settings will not work.");
        }
        
        // Settings panel başlangıçta kapalı olmalı
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            isSettingsOpen = false;
        }
        
        // Slider event'lerini ayarla
        SetupSliderEvents();
        
        // Toggle event'ini ayarla
        SetupToggleEvents();
        
        // Crosshair event'lerini ayarla
        SetupCrosshairEvents();
    }
    
    private void Start()
    {
        // Kaydedilmiş ayarları yükle ve UI'ı güncelle
        LoadSettings();
    }
    
    private void Update()
    {
        // ESC tuşu kontrolü (eğer enable ise)
        // Input System kullanarak ESC tuşunu kontrol et
        if (enableESCKey && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleSettingsPanel();
        }
    }
    
    /// <summary>
    /// Slider event'lerini ayarlar
    /// </summary>
    private void SetupSliderEvents()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderChanged);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeSliderChanged);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeSliderChanged);
        }
    }
    
    /// <summary>
    /// Toggle event'ini ayarlar
    /// </summary>
    private void SetupToggleEvents()
    {
        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveAllListeners();
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);
        }
    }
    
    /// <summary>
    /// Crosshair event'lerini ayarlar
    /// </summary>
    private void SetupCrosshairEvents()
    {
        if (crosshairTypeDropdown != null)
        {
            crosshairTypeDropdown.onValueChanged.RemoveAllListeners();
            crosshairTypeDropdown.onValueChanged.AddListener(OnCrosshairTypeChanged);
        }
        
        if (crosshairColorRSlider != null)
        {
            crosshairColorRSlider.onValueChanged.RemoveAllListeners();
            crosshairColorRSlider.onValueChanged.AddListener(OnCrosshairColorChanged);
        }
        
        if (crosshairColorGSlider != null)
        {
            crosshairColorGSlider.onValueChanged.RemoveAllListeners();
            crosshairColorGSlider.onValueChanged.AddListener(OnCrosshairColorChanged);
        }
        
        if (crosshairColorBSlider != null)
        {
            crosshairColorBSlider.onValueChanged.RemoveAllListeners();
            crosshairColorBSlider.onValueChanged.AddListener(OnCrosshairColorChanged);
        }
        
        if (crosshairThicknessSlider != null)
        {
            crosshairThicknessSlider.onValueChanged.RemoveAllListeners();
            crosshairThicknessSlider.onValueChanged.AddListener(OnCrosshairThicknessChanged);
        }
        
        if (crosshairLengthSlider != null)
        {
            crosshairLengthSlider.onValueChanged.RemoveAllListeners();
            crosshairLengthSlider.onValueChanged.AddListener(OnCrosshairLengthChanged);
        }
        
        if (crosshairGapSlider != null)
        {
            crosshairGapSlider.onValueChanged.RemoveAllListeners();
            crosshairGapSlider.onValueChanged.AddListener(OnCrosshairGapChanged);
        }
    }
    
    /// <summary>
    /// Master Volume Slider değiştiğinde çağrılır
    /// </summary>
    private void OnMasterVolumeSliderChanged(float value)
    {
        SetMasterVolume(value);
    }
    
    /// <summary>
    /// Music Volume Slider değiştiğinde çağrılır
    /// </summary>
    private void OnMusicVolumeSliderChanged(float value)
    {
        SetMusicVolume(value);
    }
    
    /// <summary>
    /// SFX Volume Slider değiştiğinde çağrılır
    /// </summary>
    private void OnSFXVolumeSliderChanged(float value)
    {
        SetSFXVolume(value);
    }
    
    /// <summary>
    /// Fullscreen Toggle değiştiğinde çağrılır
    /// </summary>
    private void OnFullscreenToggleChanged(bool value)
    {
        SetFullscreen(value);
    }
    
    /// <summary>
    /// Master Volume'u ayarlar (0-1 arası değer)
    /// Slider değerini decibel'e çevirir (-80 to 0) ve AudioMixer'a uygular
    /// </summary>
    /// <param name="volume">Volume değeri (0-1 arası, linear)</param>
    public void SetMasterVolume(float volume)
    {
        // Volume'u clamp et (0-1 arası)
        volume = Mathf.Clamp01(volume);
        
        // Linear değeri decibel'e çevir: Mathf.Log10(sliderValue) * 20
        // 0 değeri için -80dB, 1 değeri için 0dB
        float volumeInDecibels = volume > 0f ? Mathf.Log10(volume) * 20f : -80f;
        
        // AudioMixer'a uygula
        if (audioMixer != null)
        {
            bool success = audioMixer.SetFloat(MASTER_VOL_PARAM, volumeInDecibels);
            if (!success)
            {
                Debug.LogWarning($"[SettingsMenuController] Failed to set '{MASTER_VOL_PARAM}' parameter in AudioMixer. Make sure it's exposed!");
            }
        }
        
        // PlayerPrefs'e kaydet
        PlayerPrefs.SetFloat(masterVolumeKey, volume);
        PlayerPrefs.Save();
        
        // Slider'ı güncelle (eğer farklı bir kaynaktan çağrıldıysa)
        if (masterVolumeSlider != null && Mathf.Abs(masterVolumeSlider.value - volume) > 0.001f)
        {
            masterVolumeSlider.value = volume;
        }
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[SettingsMenuController] Master Volume set to {volume:F2} ({volumeInDecibels:F2} dB)");
        #endif
    }
    
    /// <summary>
    /// Music Volume'u ayarlar (0-1 arası değer)
    /// Slider değerini decibel'e çevirir (-80 to 0) ve AudioMixer'a uygular
    /// </summary>
    /// <param name="volume">Volume değeri (0-1 arası, linear)</param>
    public void SetMusicVolume(float volume)
    {
        // Volume'u clamp et (0-1 arası)
        volume = Mathf.Clamp01(volume);
        
        // Linear değeri decibel'e çevir: Mathf.Log10(sliderValue) * 20
        float volumeInDecibels = volume > 0f ? Mathf.Log10(volume) * 20f : -80f;
        
        // AudioMixer'a uygula
        if (audioMixer != null)
        {
            bool success = audioMixer.SetFloat(MUSIC_VOL_PARAM, volumeInDecibels);
            if (!success)
            {
                Debug.LogWarning($"[SettingsMenuController] Failed to set '{MUSIC_VOL_PARAM}' parameter in AudioMixer. Make sure it's exposed!");
            }
        }
        
        // PlayerPrefs'e kaydet
        PlayerPrefs.SetFloat(musicVolumeKey, volume);
        PlayerPrefs.Save();
        
        // Slider'ı güncelle
        if (musicVolumeSlider != null && Mathf.Abs(musicVolumeSlider.value - volume) > 0.001f)
        {
            musicVolumeSlider.value = volume;
        }
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[SettingsMenuController] Music Volume set to {volume:F2} ({volumeInDecibels:F2} dB)");
        #endif
    }
    
    /// <summary>
    /// SFX Volume'u ayarlar (0-1 arası değer)
    /// Slider değerini decibel'e çevirir (-80 to 0) ve AudioMixer'a uygular
    /// </summary>
    /// <param name="volume">Volume değeri (0-1 arası, linear)</param>
    public void SetSFXVolume(float volume)
    {
        // Volume'u clamp et (0-1 arası)
        volume = Mathf.Clamp01(volume);
        
        // Linear değeri decibel'e çevir: Mathf.Log10(sliderValue) * 20
        float volumeInDecibels = volume > 0f ? Mathf.Log10(volume) * 20f : -80f;
        
        // AudioMixer'a uygula
        if (audioMixer != null)
        {
            bool success = audioMixer.SetFloat(SFX_VOL_PARAM, volumeInDecibels);
            if (!success)
            {
                Debug.LogWarning($"[SettingsMenuController] Failed to set '{SFX_VOL_PARAM}' parameter in AudioMixer. Make sure it's exposed!");
            }
        }
        
        // PlayerPrefs'e kaydet
        PlayerPrefs.SetFloat(sfxVolumeKey, volume);
        PlayerPrefs.Save();
        
        // Slider'ı güncelle
        if (sfxVolumeSlider != null && Mathf.Abs(sfxVolumeSlider.value - volume) > 0.001f)
        {
            sfxVolumeSlider.value = volume;
        }
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[SettingsMenuController] SFX Volume set to {volume:F2} ({volumeInDecibels:F2} dB)");
        #endif
    }
    
    /// <summary>
    /// Fullscreen modunu ayarlar
    /// </summary>
    /// <param name="isFullscreen">Fullscreen modu açık mı?</param>
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        
        // PlayerPrefs'e kaydet
        PlayerPrefs.SetInt(fullscreenKey, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
        
        // Toggle'ı güncelle
        if (fullscreenToggle != null && fullscreenToggle.isOn != isFullscreen)
        {
            fullscreenToggle.isOn = isFullscreen;
        }
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[SettingsMenuController] Fullscreen set to: {isFullscreen}");
        #endif
    }
    
    /// <summary>
    /// Settings panelinin açık olup olmadığını döndürür
    /// </summary>
    public bool IsSettingsOpen()
    {
        return isSettingsOpen;
    }
    
    /// <summary>
    /// Settings panelini aç/kapat (Toggle)
    /// </summary>
    public void ToggleSettingsPanel()
    {
        if (isSettingsOpen)
        {
            CloseSettingsPanel();
        }
        else
        {
            OpenSettingsPanel();
        }
    }
    
    /// <summary>
    /// Settings panelini açar
    /// </summary>
    public void OpenSettingsPanel()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("[SettingsMenuController] Settings Panel is not assigned!");
            return;
        }
        
        settingsPanel.SetActive(true);
        isSettingsOpen = true;
        
        // Cursor'ı görünür yap ve kilidi aç (UI ile etkileşim için)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Eğer pause modu aktifse, oyunu durdur
        if (pauseGameWhenOpen)
        {
            Time.timeScale = 0f;
            Debug.Log("[SettingsMenuController] Game paused (Time.timeScale = 0)");
        }
        
        Debug.Log("[SettingsMenuController] Settings panel opened.");
    }
    
    /// <summary>
    /// Settings panelini kapatır
    /// </summary>
    public void CloseSettingsPanel()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("[SettingsMenuController] Settings Panel is not assigned!");
            return;
        }
        
        settingsPanel.SetActive(false);
        isSettingsOpen = false;
        
        // Cursor'ı kilitle ve gizle (oyuna geri dönüş için, eğer pause modu aktifse)
        if (pauseGameWhenOpen)
        {
            Time.timeScale = 1f;
            
            // Cursor'ı kilitle
            LockCursor();
            
            Debug.Log("[SettingsMenuController] Game resumed (Time.timeScale = 1), cursor locked.");
        }
        
        Debug.Log("[SettingsMenuController] Settings panel closed.");
    }
    
    /// <summary>
    /// Cursor'ı kilitle (oyun modu için)
    /// </summary>
    private void LockCursor()
    {
        // RightMouseOrbit script'ini bul ve cursor'ı kilitle
        RightMouseOrbit cameraOrbit = FindFirstObjectByType<RightMouseOrbit>();
        if (cameraOrbit != null)
        {
            cameraOrbit.LockCursorPublic();
        }
        else
        {
            // RightMouseOrbit yoksa manuel kilitle
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    /// <summary>
    /// Resume butonu için - Oyunu devam ettirir ve paneli kapatır
    /// </summary>
    public void ResumeGame()
    {
        CloseSettingsPanel();
    }
    
    /// <summary>
    /// Back butonu fonksiyonu - Scene'e göre farklı davranır
    /// </summary>
    public void OnBackButtonClicked()
    {
        Debug.Log($"[SettingsMenuController] Back button clicked. Current scene: {SceneManager.GetActiveScene().name}");
        
        if (IsMainMenuScene())
        {
            // MainMenu'deysek sadece Settings'i kapat
            CloseSettingsPanel();
            Debug.Log("[SettingsMenuController] Closed settings panel (MainMenu).");
        }
        else
        {
            // Oyun scene'indeyse MainMenu'ye dön
            Debug.Log("[SettingsMenuController] Returning to MainMenu from game scene...");
            BackToMainMenu();
        }
    }
    
    /// <summary>
    /// Back butonu - Her zaman MainMenu'ye git (alternatif)
    /// </summary>
    public void BackToMainMenuAlways()
    {
        BackToMainMenu();
    }
    
    /// <summary>
    /// Main Menu scene'ine geri döner
    /// </summary>
    public void BackToMainMenu()
    {
        // ÖNEMLI: Scene geçişinden önce oyun durumunu sıfırla
        ResetGameState();
        
        Debug.Log("[SettingsMenuController] Loading MainMenu scene...");
        
        // Scene'in Build Settings'te olup olmadığını kontrol et
        if (IsSceneInBuildSettings("MainMenu"))
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            Debug.LogError("[SettingsMenuController] 'MainMenu' scene is not in Build Settings!\n" +
                          "Please add it via: File > Build Settings > Add Open Scenes");
            ListAvailableScenes();
        }
    }
    
    /// <summary>
    /// Oyun durumunu sıfırlar (scene geçişi için)
    /// </summary>
    private void ResetGameState()
    {
        // Time.timeScale'i normale çevir (pause durumundan çık)
        Time.timeScale = 1f;
        
        // Cursor'ı serbest bırak (MainMenu'de cursor görünür olmalı)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("[SettingsMenuController] Game state reset: Time.timeScale=1, Cursor unlocked.");
    }
    
    /// <summary>
    /// MainMenu scene'inde olup olmadığını kontrol eder
    /// </summary>
    private bool IsMainMenuScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        return sceneName.ToLower().Contains("menu");
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
            Debug.LogWarning("[SettingsMenuController] No scenes in Build Settings!");
            return;
        }
        
        Debug.Log("[SettingsMenuController] Available scenes in Build Settings:");
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"  [{i}] {sceneName}");
        }
    }
    
    /// <summary>
    /// Kaydedilmiş ayarları yükler ve UI elementlerini günceller
    /// </summary>
    private void LoadSettings()
    {
        // Master Volume yükle
        float masterVol = PlayerPrefs.GetFloat(masterVolumeKey, defaultMasterVolume);
        SetMasterVolume(masterVol);
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVol;
        }
        
        // Music Volume yükle
        float musicVol = PlayerPrefs.GetFloat(musicVolumeKey, defaultMusicVolume);
        SetMusicVolume(musicVol);
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVol;
        }
        
        // SFX Volume yükle
        float sfxVol = PlayerPrefs.GetFloat(sfxVolumeKey, defaultSFXVolume);
        SetSFXVolume(sfxVol);
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVol;
        }
        
        // Fullscreen yükle
        bool fullscreen = PlayerPrefs.GetInt(fullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        SetFullscreen(fullscreen);
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = fullscreen;
        }
        
        Debug.Log("[SettingsMenuController] Settings loaded from PlayerPrefs.");
        
        // Crosshair ayarlarını yükle
        LoadCrosshairSettings();
        
        // Mute durumunu yükle
        LoadMuteState();
    }
    
    /// <summary>
    /// Ayarları varsayılan değerlere sıfırlar
    /// </summary>
    public void ResetToDefaults()
    {
        SetMasterVolume(defaultMasterVolume);
        SetMusicVolume(defaultMusicVolume);
        SetSFXVolume(defaultSFXVolume);
        SetFullscreen(false);
        
        Debug.Log("[SettingsMenuController] Settings reset to defaults.");
    }
    
    /// <summary>
    /// Decibel değerini linear değere çevirir (0-1 arası)
    /// PlayerPrefs'ten yüklerken kullanılabilir (şu an kullanılmıyor ama utility olarak ekledim)
    /// </summary>
    private float DecibelToLinear(float decibels)
    {
        // -80dB = 0, 0dB = 1
        return Mathf.Pow(10f, decibels / 20f);
    }
    
    // ==================== CROSSHAIR AYARLARI ====================
    
    /// <summary>
    /// Crosshair ayarlarını yükler ve UI'ı günceller
    /// </summary>
    private void LoadCrosshairSettings()
    {
        SimpleCrosshairGenerator crosshair = FindFirstObjectByType<SimpleCrosshairGenerator>();
        if (crosshair == null)
        {
            // Crosshair yok (MainMenu gibi), UI'ı gizle
            HideCrosshairUI();
            Debug.LogWarning("[SettingsMenuController] SimpleCrosshairGenerator not found in scene. Crosshair UI hidden.");
            return;
        }
        
        // Dropdown'u ayarla
        if (crosshairTypeDropdown != null)
        {
            // Dropdown option'ları oluştur (eğer boşsa)
            if (crosshairTypeDropdown.options.Count == 0)
            {
                crosshairTypeDropdown.ClearOptions();
                System.Collections.Generic.List<string> options = new System.Collections.Generic.List<string> 
                { 
                    "Cross (+)", 
                    "Dot (•)", 
                    "Circle (○)", 
                    "Hybrid (+•)" 
                };
                crosshairTypeDropdown.AddOptions(options);
            }
            crosshairTypeDropdown.value = (int)crosshair.GetCrosshairType();
        }
        
        // Renk slider'larını ayarla
        Color currentColor = crosshair.GetCrosshairColor();
        if (crosshairColorRSlider != null)
        {
            crosshairColorRSlider.minValue = 0f;
            crosshairColorRSlider.maxValue = 1f;
            crosshairColorRSlider.value = currentColor.r;
        }
        if (crosshairColorGSlider != null)
        {
            crosshairColorGSlider.minValue = 0f;
            crosshairColorGSlider.maxValue = 1f;
            crosshairColorGSlider.value = currentColor.g;
        }
        if (crosshairColorBSlider != null)
        {
            crosshairColorBSlider.minValue = 0f;
            crosshairColorBSlider.maxValue = 1f;
            crosshairColorBSlider.value = currentColor.b;
        }
        
        // Boyut slider'larını ayarla
        if (crosshairThicknessSlider != null)
        {
            crosshairThicknessSlider.minValue = 1f;
            crosshairThicknessSlider.maxValue = 10f;
            crosshairThicknessSlider.value = crosshair.GetLineThickness();
        }
        if (crosshairLengthSlider != null)
        {
            crosshairLengthSlider.minValue = 5f;
            crosshairLengthSlider.maxValue = 50f;
            crosshairLengthSlider.value = crosshair.GetLineLength();
        }
        if (crosshairGapSlider != null)
        {
            crosshairGapSlider.minValue = 0f;
            crosshairGapSlider.maxValue = 20f;
            crosshairGapSlider.value = crosshair.GetCenterGap();
        }
        
        Debug.Log("[SettingsMenuController] Crosshair settings loaded.");
    }
    
    /// <summary>
    /// Crosshair UI'ını gizler (MainMenu'de crosshair olmadığı için)
    /// </summary>
    private void HideCrosshairUI()
    {
        // Crosshair dropdown'unu gizle
        if (crosshairTypeDropdown != null && crosshairTypeDropdown.gameObject != null)
        {
            crosshairTypeDropdown.gameObject.SetActive(false);
        }
        
        // Crosshair slider'larını gizle
        if (crosshairColorRSlider != null && crosshairColorRSlider.gameObject != null)
        {
            crosshairColorRSlider.gameObject.SetActive(false);
        }
        if (crosshairColorGSlider != null && crosshairColorGSlider.gameObject != null)
        {
            crosshairColorGSlider.gameObject.SetActive(false);
        }
        if (crosshairColorBSlider != null && crosshairColorBSlider.gameObject != null)
        {
            crosshairColorBSlider.gameObject.SetActive(false);
        }
        if (crosshairThicknessSlider != null && crosshairThicknessSlider.gameObject != null)
        {
            crosshairThicknessSlider.gameObject.SetActive(false);
        }
        if (crosshairLengthSlider != null && crosshairLengthSlider.gameObject != null)
        {
            crosshairLengthSlider.gameObject.SetActive(false);
        }
        if (crosshairGapSlider != null && crosshairGapSlider.gameObject != null)
        {
            crosshairGapSlider.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Crosshair tipi değiştiğinde çağrılır
    /// </summary>
    private void OnCrosshairTypeChanged(int typeIndex)
    {
        SimpleCrosshairGenerator crosshair = FindFirstObjectByType<SimpleCrosshairGenerator>();
        if (crosshair != null)
        {
            crosshair.SetCrosshairType(typeIndex);
        }
    }
    
    /// <summary>
    /// Crosshair rengi değiştiğinde çağrılır
    /// </summary>
    private void OnCrosshairColorChanged(float value)
    {
        SimpleCrosshairGenerator crosshair = FindFirstObjectByType<SimpleCrosshairGenerator>();
        if (crosshair == null) return;
        
        float r = crosshairColorRSlider != null ? crosshairColorRSlider.value : 1f;
        float g = crosshairColorGSlider != null ? crosshairColorGSlider.value : 1f;
        float b = crosshairColorBSlider != null ? crosshairColorBSlider.value : 1f;
        
        Color newColor = new Color(r, g, b, 1f);
        crosshair.SetColor(newColor);
    }
    
    /// <summary>
    /// Crosshair kalınlığı değiştiğinde çağrılır
    /// </summary>
    private void OnCrosshairThicknessChanged(float value)
    {
        SimpleCrosshairGenerator crosshair = FindFirstObjectByType<SimpleCrosshairGenerator>();
        if (crosshair != null)
        {
            crosshair.SetLineThickness(value);
        }
    }
    
    /// <summary>
    /// Crosshair uzunluğu değiştiğinde çağrılır
    /// </summary>
    private void OnCrosshairLengthChanged(float value)
    {
        SimpleCrosshairGenerator crosshair = FindFirstObjectByType<SimpleCrosshairGenerator>();
        if (crosshair != null)
        {
            crosshair.SetLineLength(value);
        }
    }
    
    /// <summary>
    /// Crosshair boşluğu değiştiğinde çağrılır
    /// </summary>
    private void OnCrosshairGapChanged(float value)
    {
        SimpleCrosshairGenerator crosshair = FindFirstObjectByType<SimpleCrosshairGenerator>();
        if (crosshair != null)
        {
            crosshair.SetCenterGap(value);
        }
    }
    
    /// <summary>
    /// Crosshair ayarlarını varsayılana sıfırlar
    /// </summary>
    public void ResetCrosshairToDefaults()
    {
        SimpleCrosshairGenerator crosshair = FindFirstObjectByType<SimpleCrosshairGenerator>();
        if (crosshair != null)
        {
            crosshair.ResetToDefaults();
            LoadCrosshairSettings(); // UI'ı güncelle
        }
    }
    
    // ==================== MUTE BUTTON ====================
    
    /// <summary>
    /// Mute/Unmute butonuna tıklandığında çağrılır
    /// </summary>
    public void ToggleMute()
    {
        isMuted = !isMuted;
        ApplyMuteState();
        SaveMuteState();
        UpdateMuteButtonVisual();
        
        Debug.Log($"[SettingsMenuController] Sound {(isMuted ? "muted" : "unmuted")}.");
    }
    
    /// <summary>
    /// Mute durumunu uygular (AudioMixer'a)
    /// </summary>
    private void ApplyMuteState()
    {
        if (audioMixer == null) return;
        
        if (isMuted)
        {
            // Sesi kapat (-80dB)
            audioMixer.SetFloat(MASTER_VOL_PARAM, -80f);
        }
        else
        {
            // Önceki volume'u geri yükle
            float savedVolume = PlayerPrefs.GetFloat(masterVolumeKey, defaultMasterVolume);
            float volumeInDecibels = savedVolume > 0f ? Mathf.Log10(savedVolume) * 20f : -80f;
            audioMixer.SetFloat(MASTER_VOL_PARAM, volumeInDecibels);
        }
    }
    
    /// <summary>
    /// Mute button görselini günceller
    /// </summary>
    private void UpdateMuteButtonVisual()
    {
        if (muteButtonImage == null) return;
        
        if (isMuted)
        {
            // Ses kapalı görseli
            if (soundOffSprite != null)
            {
                muteButtonImage.sprite = soundOffSprite;
            }
        }
        else
        {
            // Ses açık görseli
            if (soundOnSprite != null)
            {
                muteButtonImage.sprite = soundOnSprite;
            }
        }
    }
    
    /// <summary>
    /// Mute durumunu kaydeder
    /// </summary>
    private void SaveMuteState()
    {
        PlayerPrefs.SetInt(isMutedKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Mute durumunu yükler
    /// </summary>
    private void LoadMuteState()
    {
        isMuted = PlayerPrefs.GetInt(isMutedKey, 0) == 1;
        ApplyMuteState();
        UpdateMuteButtonVisual();
        
        Debug.Log($"[SettingsMenuController] Mute state loaded: {(isMuted ? "muted" : "unmuted")}");
    }
    
    /// <summary>
    /// Sesi aç (unmute)
    /// </summary>
    public void Unmute()
    {
        if (!isMuted) return;
        
        isMuted = false;
        ApplyMuteState();
        SaveMuteState();
        UpdateMuteButtonVisual();
    }
    
    /// <summary>
    /// Sesi kapat (mute)
    /// </summary>
    public void Mute()
    {
        if (isMuted) return;
        
        isMuted = true;
        ApplyMuteState();
        SaveMuteState();
        UpdateMuteButtonVisual();
    }
    
    /// <summary>
    /// Mute durumunu döndürür
    /// </summary>
    public bool IsMuted()
    {
        return isMuted;
    }
}
