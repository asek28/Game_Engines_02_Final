using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Player healthbar UI controller
/// Slider veya Image fill ile çalışır
/// </summary>
public class PlayerHealthbarUI : MonoBehaviour
{
    [Header("Healthbar Type")]
    [Tooltip("Slider kullan (basit) veya Image Fill (özelleştirilebilir)")]
    [SerializeField] private HealthbarType healthbarType = HealthbarType.ImageFill;
    
    [Header("Slider Settings (Healthbar Type = Slider)")]
    [Tooltip("Healthbar Slider component")]
    [SerializeField] private Slider healthbarSlider;
    
    [Header("Image Fill Settings (Healthbar Type = ImageFill)")]
    [Tooltip("Fill Image (kendi sprite'ınızı atayın)")]
    [SerializeField] private Image healthbarFillImage;
    
    [Tooltip("Background Image (arka plan)")]
    [SerializeField] private Image healthbarBackgroundImage;
    
    [Header("Text Display (Opsiyonel)")]
    [Tooltip("Health text (örn: '80/100')")]
    [SerializeField] private TextMeshProUGUI healthText;
    
    [Tooltip("Text formatı (örn: '{0}/{1}' veya '{0} HP')")]
    [SerializeField] private string healthTextFormat = "{0}/{1}";
    
    [Header("Color Settings")]
    [Tooltip("Can yüksekken renk (yeşil)")]
    [SerializeField] private Color highHealthColor = Color.green;
    
    [Tooltip("Can ortayken renk (sarı)")]
    [SerializeField] private Color mediumHealthColor = Color.yellow;
    
    [Tooltip("Can düşükken renk (kırmızı)")]
    [SerializeField] private Color lowHealthColor = Color.red;
    
    [Tooltip("Can yüksek threshold (örn: 0.6 = %60)")]
    [SerializeField, Range(0f, 1f)] private float highHealthThreshold = 0.6f;
    
    [Tooltip("Can düşük threshold (örn: 0.3 = %30)")]
    [SerializeField, Range(0f, 1f)] private float lowHealthThreshold = 0.3f;
    
    [Header("Animation (Opsiyonel)")]
    [Tooltip("Healthbar yumuşak azalır mı?")]
    [SerializeField] private bool smoothTransition = true;
    
    [Tooltip("Yumuşak geçiş hızı")]
    [SerializeField] private float smoothSpeed = 5f;
    
    [Header("Player Reference")]
    [Tooltip("PlayerHealth component (otomatik bulunur)")]
    [SerializeField] private PlayerHealth playerHealth;
    
    private float targetFillAmount = 1f;
    private float currentFillAmount = 1f;
    
    public enum HealthbarType
    {
        Slider,
        ImageFill
    }
    
    private void Awake()
    {
        // PlayerHealth'i bul
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }
        
        if (playerHealth == null)
        {
            Debug.LogError("[PlayerHealthbarUI] PlayerHealth component not found!");
            return;
        }
        
        // Event'lere subscribe ol
        playerHealth.OnHealthChanged.AddListener(UpdateHealthbar);
    }
    
    private void Start()
    {
        // Başlangıç değerini set et
        if (playerHealth != null)
        {
            UpdateHealthbar(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }
    
    private void Update()
    {
        // Smooth transition
        if (smoothTransition && Mathf.Abs(currentFillAmount - targetFillAmount) > 0.01f)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);
            ApplyFillAmount(currentFillAmount);
        }
    }
    
    /// <summary>
    /// Healthbar'ı güncelle
    /// </summary>
    private void UpdateHealthbar(int currentHealth, int maxHealth)
    {
        float healthPercentage = (float)currentHealth / maxHealth;
        targetFillAmount = healthPercentage;
        
        if (!smoothTransition)
        {
            currentFillAmount = targetFillAmount;
            ApplyFillAmount(currentFillAmount);
        }
        
        // Text güncelle
        if (healthText != null)
        {
            healthText.text = string.Format(healthTextFormat, currentHealth, maxHealth);
        }
        
        // Renk güncelle
        UpdateHealthbarColor(healthPercentage);
        
        Debug.Log($"[PlayerHealthbarUI] Updated: {currentHealth}/{maxHealth} ({healthPercentage:P0})");
    }
    
    /// <summary>
    /// Fill amount'u uygula (slider veya image)
    /// </summary>
    private void ApplyFillAmount(float fillAmount)
    {
        if (healthbarType == HealthbarType.Slider && healthbarSlider != null)
        {
            healthbarSlider.value = fillAmount;
        }
        else if (healthbarType == HealthbarType.ImageFill && healthbarFillImage != null)
        {
            healthbarFillImage.fillAmount = fillAmount;
        }
    }
    
    /// <summary>
    /// Healthbar rengini güncelle
    /// </summary>
    private void UpdateHealthbarColor(float healthPercentage)
    {
        Color targetColor;
        
        if (healthPercentage >= highHealthThreshold)
        {
            targetColor = highHealthColor;
        }
        else if (healthPercentage >= lowHealthThreshold)
        {
            targetColor = mediumHealthColor;
        }
        else
        {
            targetColor = lowHealthColor;
        }
        
        // Rengi uygula
        if (healthbarType == HealthbarType.Slider && healthbarSlider != null)
        {
            Image fillImage = healthbarSlider.fillRect?.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = targetColor;
            }
        }
        else if (healthbarType == HealthbarType.ImageFill && healthbarFillImage != null)
        {
            healthbarFillImage.color = targetColor;
        }
    }
    
    private void OnDestroy()
    {
        // Event subscription'ları temizle
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.RemoveListener(UpdateHealthbar);
        }
    }
}
