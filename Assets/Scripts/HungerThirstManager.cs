using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Açlık ve Susuzluk sistemi
/// Zamanla azalır, koşarken daha hızlı azalır, 0 olursa can azalır
/// </summary>
public class HungerThirstManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Maksimum açlık değeri (0-100)")]
    [SerializeField] private float maxHunger = 100f;
    
    [Tooltip("Maksimum susuzluk değeri (0-100)")]
    [SerializeField] private float maxThirst = 100f;
    
    [Tooltip("Başlangıç açlık değeri")]
    [SerializeField] private float startHunger = 100f;
    
    [Tooltip("Başlangıç susuzluk değeri")]
    [SerializeField] private float startThirst = 100f;
    
    [Header("Thirst Depletion (per second)")]
    [Tooltip("Koşarken susuzluk azalma hızı (saniyede) - sadece koşarken azalır")]
    [SerializeField] private float runningThirstDepletionRate = 2.4f;
    
    [Header("Hunger Depletion (on attack)")]
    [Tooltip("Enemy'ye hasar verildiğinde açlık azalma miktarı (her vuruşta)")]
    [SerializeField] private float hungerDepletionOnAttack = 1f;
    
    [Header("Health Damage")]
    [Tooltip("Açlık veya susuzluk 0 olduğunda saniyede verilen hasar")]
    [SerializeField] private float damagePerSecond = 2f;
    
    [Tooltip("Hasar verme aralığı (saniye)")]
    [SerializeField] private float damageInterval = 1f;
    
    [Header("UI References")]
    [Tooltip("Açlık slider'ı (dikey, yukarıdan aşağıya)")]
    [SerializeField] private Slider hungerSlider;
    
    [Tooltip("Susuzluk slider'ı (dikey, yukarıdan aşağıya)")]
    [SerializeField] private Slider thirstSlider;
    
    [Header("Player References")]
    [Tooltip("Player movement script (koşma kontrolü için)")]
    [SerializeField] private SimplePlayerMovement playerMovement;
    
    [Tooltip("Player health script (hasar vermek için)")]
    [SerializeField] private PlayerHealth playerHealth;
    
    // Private variables
    private float currentHunger;
    private float currentThirst;
    private float lastDamageTime;
    private bool isRunning;
    
    // Public getters
    public float CurrentHunger => currentHunger;
    public float CurrentThirst => currentThirst;
    public float MaxHunger => maxHunger;
    public float MaxThirst => maxThirst;
    public float HungerPercentage => currentHunger / maxHunger;
    public float ThirstPercentage => currentThirst / maxThirst;
    
    private void Awake()
    {
        // Player referanslarını bul
        if (playerMovement == null)
        {
            playerMovement = GetComponent<SimplePlayerMovement>();
        }
        
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }
        
        // Başlangıç değerlerini ayarla
        currentHunger = Mathf.Clamp(startHunger, 0f, maxHunger);
        currentThirst = Mathf.Clamp(startThirst, 0f, maxThirst);
        
        // Slider'ları dikey yap (yukarıdan aşağıya)
        if (hungerSlider != null)
        {
            hungerSlider.direction = Slider.Direction.BottomToTop;
            hungerSlider.value = HungerPercentage;
        }
        
        if (thirstSlider != null)
        {
            thirstSlider.direction = Slider.Direction.BottomToTop;
            thirstSlider.value = ThirstPercentage;
        }
        
        Debug.Log($"[HungerThirstManager] Initialized - Hunger: {currentHunger}/{maxHunger}, Thirst: {currentThirst}/{maxThirst}");
    }
    
    private void Update()
    {
        // Player ölü mü?
        if (playerHealth != null && playerHealth.IsDead)
        {
            return;
        }
        
        // Koşma durumunu kontrol et
        isRunning = playerMovement != null && playerMovement.IsRunning();
        
        // Açlık ve susuzluk azaltma
        UpdateHunger();
        UpdateThirst();
        
        // UI güncelle
        UpdateUI();
        
        // 0 olursa hasar ver
        ApplyDamageIfEmpty();
    }
    
    /// <summary>
    /// Açlık değerini güncelle
    /// NOT: Açlık artık zamanla azalmaz, sadece enemy'ye hasar verince azalır
    /// </summary>
    private void UpdateHunger()
    {
        // Açlık artık zamanla azalmıyor, sadece enemy'ye hasar verince azalıyor
        // Bu metod boş bırakıldı ama Update'te çağrılıyor, silmek yerine boş bıraktık
    }
    
    /// <summary>
    /// Susuzluk değerini güncelle
    /// Sadece koşarken azalır
    /// </summary>
    private void UpdateThirst()
    {
        if (currentThirst <= 0f) return;
        
        // Sadece koşarken azal
        if (isRunning)
        {
            currentThirst -= runningThirstDepletionRate * Time.deltaTime;
            currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);
        }
    }
    
    /// <summary>
    /// Enemy'ye hasar verildiğinde açlık azalt
    /// </summary>
    public void OnEnemyHit()
    {
        if (currentHunger <= 0f) return;
        
        currentHunger -= hungerDepletionOnAttack;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        
        Debug.Log($"[HungerThirstManager] ⚔️ Enemy hit! Hunger reduced by {hungerDepletionOnAttack}. Current: {currentHunger}/{maxHunger}");
    }
    
    /// <summary>
    /// UI slider'larını güncelle
    /// </summary>
    private void UpdateUI()
    {
        if (hungerSlider != null)
        {
            hungerSlider.value = HungerPercentage;
        }
        
        if (thirstSlider != null)
        {
            thirstSlider.value = ThirstPercentage;
        }
    }
    
    /// <summary>
    /// Açlık veya susuzluk 0 olursa can azalt
    /// </summary>
    private void ApplyDamageIfEmpty()
    {
        if (playerHealth == null || playerHealth.IsDead)
        {
            return;
        }
        
        bool shouldTakeDamage = (currentHunger <= 0f || currentThirst <= 0f);
        
        if (shouldTakeDamage && Time.time - lastDamageTime >= damageInterval)
        {
            int damage = Mathf.RoundToInt(damagePerSecond * damageInterval);
            playerHealth.TakeDamage(damage);
            lastDamageTime = Time.time;
            
            Debug.Log($"[HungerThirstManager] ⚠️ Taking damage from hunger/thirst! Hunger: {currentHunger:F1}, Thirst: {currentThirst:F1}, Damage: {damage}");
        }
    }
    
    /// <summary>
    /// Açlık ekle (yemek yeme)
    /// </summary>
    public void AddHunger(float amount)
    {
        currentHunger += amount;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        Debug.Log($"[HungerThirstManager] 🍖 Added {amount} hunger! Current: {currentHunger}/{maxHunger}");
    }
    
    /// <summary>
    /// Susuzluk ekle (su içme)
    /// </summary>
    public void AddThirst(float amount)
    {
        currentThirst += amount;
        currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);
        Debug.Log($"[HungerThirstManager] 💧 Added {amount} thirst! Current: {currentThirst}/{maxThirst}");
    }
    
    /// <summary>
    /// Açlığı tam doldur
    /// </summary>
    public void FillHunger()
    {
        currentHunger = maxHunger;
        Debug.Log($"[HungerThirstManager] 🍖 Hunger filled to {maxHunger}!");
    }
    
    /// <summary>
    /// Susuzluğu tam doldur
    /// </summary>
    public void FillThirst()
    {
        currentThirst = maxThirst;
        Debug.Log($"[HungerThirstManager] 💧 Thirst filled to {maxThirst}!");
    }
    
    /// <summary>
    /// Her ikisini de tam doldur
    /// </summary>
    public void FillBoth()
    {
        FillHunger();
        FillThirst();
    }
    
    /// <summary>
    /// Açlık değerini set et (debug için)
    /// </summary>
    public void SetHunger(float value)
    {
        currentHunger = Mathf.Clamp(value, 0f, maxHunger);
    }
    
    /// <summary>
    /// Susuzluk değerini set et (debug için)
    /// </summary>
    public void SetThirst(float value)
    {
        currentThirst = Mathf.Clamp(value, 0f, maxThirst);
    }
}

