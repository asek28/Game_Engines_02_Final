using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Player health system
/// Can yönetimi, hasar alma, ölüm
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maksimum can")]
    [SerializeField] private int maxHealth = 100;
    
    [Tooltip("Başlangıç canı (maxHealth'den farklı olabilir)")]
    [SerializeField] private int startHealth = 100;
    
    [Header("Damage Feedback")]
    [Tooltip("Hasar aldığında ekran kırmızı yanıp söner mi?")]
    [SerializeField] private bool enableDamageFlash = true;
    
    [Tooltip("Hasar efekti süresi")]
    [SerializeField] private float damageFlashDuration = 0.2f;
    
    [Tooltip("Hasar sesi")]
    [SerializeField] private AudioClip damageSound;
    
    [Tooltip("Ölüm sesi")]
    [SerializeField] private AudioClip deathSound;
    
    [Header("Regeneration (Opsiyonel)")]
    [Tooltip("Can yenilensin mi?")]
    [SerializeField] private bool enableRegeneration = false;
    
    [Tooltip("Saniyede yenilenen can")]
    [SerializeField] private float regenPerSecond = 1f;
    
    [Tooltip("Hasar aldıktan kaç saniye sonra yenilenme başlar")]
    [SerializeField] private float regenDelay = 5f;
    
    [Header("Events")]
    [Tooltip("Can değiştiğinde tetiklenir (currentHealth, maxHealth)")]
    public UnityEvent<int, int> OnHealthChanged;
    
    [Tooltip("Hasar aldığında tetiklenir (damage)")]
    public UnityEvent<int> OnDamageTaken;
    
    [Tooltip("Öldüğünde tetiklenir")]
    public UnityEvent OnDeath;
    
    // Private variables
    private int currentHealth;
    private bool isDead = false;
    private AudioSource audioSource;
    private float lastDamageTime;
    
    // Public getters
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;
    public float HealthPercentage => (float)currentHealth / maxHealth;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    
    private void Start()
    {
        // Başlangıç canı
        currentHealth = Mathf.Clamp(startHealth, 0, maxHealth);
        
        // Event tetikle (healthbar güncellenir)
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log($"[PlayerHealth] Initialized: {currentHealth}/{maxHealth} HP");
    }
    
    private void Update()
    {
        // Can yenilenme
        if (enableRegeneration && !isDead && currentHealth < maxHealth)
        {
            if (Time.time - lastDamageTime >= regenDelay)
            {
                Heal((int)(regenPerSecond * Time.deltaTime));
            }
        }
    }
    
    /// <summary>
    /// Hasar al
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        lastDamageTime = Time.time;
        
        Debug.Log($"<color=red>[PlayerHealth] Took {damage} damage! Health: {currentHealth}/{maxHealth}</color>");
        
        // Event'leri tetikle
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamageTaken?.Invoke(damage);
        
        // Ses efekti
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
        
        // Damage flash (ekran kırmızı yanıp söner)
        if (enableDamageFlash)
        {
            StartCoroutine(DamageFlashCoroutine());
        }
        
        // Ölüm kontrolü
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Can kazan (heal)
    /// </summary>
    public void Heal(int amount)
    {
        if (isDead) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        Debug.Log($"<color=green>[PlayerHealth] Healed {amount}! Health: {currentHealth}/{maxHealth}</color>");
        
        // Event tetikle
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// Canı tam doldur
    /// </summary>
    public void HealFull()
    {
        Heal(maxHealth - currentHealth);
    }
    
    /// <summary>
    /// Öldür
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        Debug.Log("<color=red>[PlayerHealth] Player DIED!</color>");
        
        // Ölüm sesi
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Event tetikle
        OnDeath?.Invoke();
        
        // Burada ölüm animasyonu, game over ekranı, vb. eklenebilir
    }
    
    /// <summary>
    /// Damage flash coroutine (ekran kırmızı yanıp söner)
    /// </summary>
    private System.Collections.IEnumerator DamageFlashCoroutine()
    {
        // Burada CanvasGroup veya Image kullanarak ekran kırmızı yapılabilir
        // Şimdilik basit log
        Debug.Log("<color=red>[PlayerHealth] 💥 Damage Flash!</color>");
        
        yield return new WaitForSeconds(damageFlashDuration);
        
        // Flash bitişi
    }
    
    /// <summary>
    /// Canı set et (debug/cheat için)
    /// </summary>
    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// Maksimum canı değiştir
    /// </summary>
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
