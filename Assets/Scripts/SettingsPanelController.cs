using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Settings Panel kontrolü - Ses, grafik, kontroller gibi ayarları yönetir
/// </summary>
public class SettingsPanelController : MonoBehaviour
{
    [Header("Close Button")]
    [Tooltip("Settings panelini kapatacak buton")]
    [SerializeField] private Button closeButton;
    
    [Header("Audio Settings")]
    [Tooltip("Master Volume Slider")]
    [SerializeField] private Slider masterVolumeSlider;
    
    [Tooltip("Music Volume Slider")]
    [SerializeField] private Slider musicVolumeSlider;
    
    [Tooltip("SFX Volume Slider")]
    [SerializeField] private Slider sfxVolumeSlider;
    
    [Header("Graphics Settings")]
    [Tooltip("Quality Dropdown")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    
    [Tooltip("Fullscreen Toggle")]
    [SerializeField] private Toggle fullscreenToggle;
    
    [Tooltip("Resolution Dropdown")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    
    [Header("Controls Settings")]
    [Tooltip("Sensitivity Slider (eğer varsa)")]
    [SerializeField] private Slider sensitivitySlider;
    
    [Header("Default Values")]
    [SerializeField, Range(0f, 1f)] private float defaultMasterVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float defaultMusicVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float defaultSFXVolume = 1f;
    
    private Resolution[] resolutions;
    
    private void Awake()
    {
        // Close button event'ini ayarla
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }
    
    private void Start()
    {
        // Ayarları yükle
        LoadSettings();
        
        // UI elementlerini ayarla
        SetupUI();
    }
    
    /// <summary>
    /// UI elementlerini ayarlar ve event'leri bağlar
    /// </summary>
    private void SetupUI()
    {
        // Audio Sliders
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
        
        // Graphics Settings
        if (qualityDropdown != null)
        {
            qualityDropdown.onValueChanged.RemoveAllListeners();
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            
            // Quality seviyelerini dropdown'a ekle
            qualityDropdown.ClearOptions();
            string[] qualityNames = QualitySettings.names;
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(qualityNames));
            qualityDropdown.value = QualitySettings.GetQualityLevel();
        }
        
        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveAllListeners();
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            fullscreenToggle.isOn = Screen.fullScreen;
        }
        
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveAllListeners();
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            
            // Mevcut çözünürlükleri al
            resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();
            
            System.Collections.Generic.List<string> resolutionOptions = new System.Collections.Generic.List<string>();
            int currentResolutionIndex = 0;
            
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                resolutionOptions.Add(option);
                
                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }
            
            resolutionDropdown.AddOptions(resolutionOptions);
            resolutionDropdown.value = currentResolutionIndex;
        }
        
        // Controls
        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.RemoveAllListeners();
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }
    }
    
    /// <summary>
    /// Close butonuna tıklandığında çağrılır
    /// </summary>
    public void OnCloseButtonClicked()
    {
        // MainMenuController'ı bul ve settings panelini kapat
        MainMenuController mainMenu = FindObjectOfType<MainMenuController>();
        if (mainMenu != null)
        {
            mainMenu.CloseSettingsPanel();
        }
        else
        {
            // MainMenuController yoksa direkt kapat
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Master volume değiştiğinde çağrılır
    /// </summary>
    private void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Music volume değiştiğinde çağrılır
    /// </summary>
    private void OnMusicVolumeChanged(float value)
    {
        // Music için ayrı bir AudioSource kullanıyorsanız buraya ekleyin
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// SFX volume değiştiğinde çağrılır
    /// </summary>
    private void OnSFXVolumeChanged(float value)
    {
        // SFX için ayrı bir AudioSource kullanıyorsanız buraya ekleyin
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Quality seviyesi değiştiğinde çağrılır
    /// </summary>
    private void OnQualityChanged(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Fullscreen durumu değiştiğinde çağrılır
    /// </summary>
    private void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Çözünürlük değiştiğinde çağrılır
    /// </summary>
    private void OnResolutionChanged(int resolutionIndex)
    {
        if (resolutions != null && resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            PlayerPrefs.SetInt("ResolutionWidth", resolution.width);
            PlayerPrefs.SetInt("ResolutionHeight", resolution.height);
            PlayerPrefs.Save();
        }
    }
    
    /// <summary>
    /// Sensitivity değiştiğinde çağrılır
    /// </summary>
    private void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat("Sensitivity", value);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Kaydedilmiş ayarları yükler
    /// </summary>
    private void LoadSettings()
    {
        // Audio
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", defaultMasterVolume);
        AudioListener.volume = masterVol;
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVol;
        }
        
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVol;
        }
        
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", defaultSFXVolume);
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVol;
        }
        
        // Graphics
        int qualityLevel = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(qualityLevel);
        if (qualityDropdown != null)
        {
            qualityDropdown.value = qualityLevel;
        }
        
        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        Screen.fullScreen = fullscreen;
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = fullscreen;
        }
        
        int resWidth = PlayerPrefs.GetInt("ResolutionWidth", Screen.currentResolution.width);
        int resHeight = PlayerPrefs.GetInt("ResolutionHeight", Screen.currentResolution.height);
        if (resWidth > 0 && resHeight > 0)
        {
            Screen.SetResolution(resWidth, resHeight, fullscreen);
        }
        
        // Controls
        float sensitivity = PlayerPrefs.GetFloat("Sensitivity", 1f);
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = sensitivity;
        }
    }
    
    /// <summary>
    /// Ayarları varsayılan değerlere sıfırlar
    /// </summary>
    public void ResetToDefaults()
    {
        // Audio
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = defaultMasterVolume;
        }
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = defaultMusicVolume;
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = defaultSFXVolume;
        }
        
        // Graphics
        if (qualityDropdown != null)
        {
            qualityDropdown.value = QualitySettings.GetQualityLevel();
        }
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
        }
        
        // Controls
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = 1f;
        }
    }
}

