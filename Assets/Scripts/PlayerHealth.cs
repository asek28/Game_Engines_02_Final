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
    
    [Tooltip("Hunger ve Thirst full olduğunda regen aktif olsun mu?")]
    [SerializeField] private bool enableHungerThirstRegen = true;
    
    [Tooltip("Hunger ve Thirst full olduğunda regen süresi (saniye)")]
    [SerializeField] private float hungerThirstRegenDuration = 3f;
    
    [Header("Events")]
    [Tooltip("Can değiştiğinde tetiklenir (currentHealth, maxHealth)")]
    public UnityEvent<int, int> OnHealthChanged;
    
    [Tooltip("Hasar aldığında tetiklenir (damage)")]
    public UnityEvent<int> OnDamageTaken;
    
    [Tooltip("Öldüğünde tetiklenir")]
    public UnityEvent OnDeath;
    
    // Static event for Death Screen UI
    public static event System.Action OnPlayerDied;
    
    // Private variables
    private int currentHealth;
    private bool isDead = false;
    private AudioSource audioSource;
    private float lastDamageTime;
    private HungerThirstManager hungerThirstManager;
    private float hungerThirstRegenStartTime = -1f;
    private bool isHungerThirstRegenActive = false;
    
    // Public getters
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;
    public float HealthPercentage => (float)currentHealth / maxHealth;
    
    private void Awake()
    {
        // UnityEvent'leri initialize et
        if (OnHealthChanged == null)
            OnHealthChanged = new UnityEvent<int, int>();
        if (OnDamageTaken == null)
            OnDamageTaken = new UnityEvent<int>();
        if (OnDeath == null)
            OnDeath = new UnityEvent();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        Debug.Log("[PlayerHealth] Initialized UnityEvents");
    }
    
    private void Start()
    {
        // Başlangıç canı
        currentHealth = Mathf.Clamp(startHealth, 0, maxHealth);
        
        // Event tetikle (healthbar güncellenir)
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // HungerThirstManager'ı bul
        hungerThirstManager = FindFirstObjectByType<HungerThirstManager>();
        if (hungerThirstManager == null)
        {
            hungerThirstManager = GetComponent<HungerThirstManager>();
        }
        
        Debug.Log($"[PlayerHealth] Initialized: {currentHealth}/{maxHealth} HP");
    }
    
    private void Update()
    {
        // Hunger ve Thirst full olduğunda regen kontrolü
        if (enableHungerThirstRegen && hungerThirstManager != null && !isDead && currentHealth < maxHealth)
        {
            bool isHungerFull = hungerThirstManager.CurrentHunger >= hungerThirstManager.MaxHunger;
            bool isThirstFull = hungerThirstManager.CurrentThirst >= hungerThirstManager.MaxThirst;
            
            // Her ikisi de full ise regen başlat
            if (isHungerFull && isThirstFull)
            {
                // Regen başlamadıysa başlat
                if (!isHungerThirstRegenActive)
                {
                    hungerThirstRegenStartTime = Time.time;
                    isHungerThirstRegenActive = true;
                    Debug.Log($"[PlayerHealth] 🍖💧 Hunger and Thirst are full! Starting regen for {hungerThirstRegenDuration} seconds.");
                }
                
                // Regen süresi içindeyse can yenile (delay yok, hemen başlar)
                if (Time.time - hungerThirstRegenStartTime <= hungerThirstRegenDuration)
                {
                    Heal((int)(regenPerSecond * Time.deltaTime));
                }
                else
                {
                    // Süre doldu, regen'i durdur
                    isHungerThirstRegenActive = false;
                    Debug.Log($"[PlayerHealth] ⏱️ Hunger/Thirst regen duration ended.");
                }
            }
            else
            {
                // Hunger veya Thirst full değilse regen'i durdur
                if (isHungerThirstRegenActive)
                {
                    isHungerThirstRegenActive = false;
                    hungerThirstRegenStartTime = -1f;
                    Debug.Log($"[PlayerHealth] ⚠️ Hunger or Thirst is not full anymore. Regen stopped.");
                }
            }
        }
        
        // Normal can yenilenme (Hunger/Thirst regen aktif değilse)
        if (enableRegeneration && !isDead && currentHealth < maxHealth && !isHungerThirstRegenActive)
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
        if (isDead)
        {
            Debug.LogWarning("[PlayerHealth] Already dead! Ignoring Die() call.");
            return;
        }
        
        isDead = true;
        
        Debug.Log("<color=red>[PlayerHealth] ☠️ Player DIED! Health: " + currentHealth + "</color>");
        
        // Hareketi durdur
        DisableMovement();
        
        // Ölüm sesi
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Event tetikle
        Debug.Log("[PlayerHealth] Invoking OnDeath event...");
        OnDeath?.Invoke();
        
        Debug.Log($"[PlayerHealth] Invoking OnPlayerDied static event... (Subscribers: {(OnPlayerDied != null ? OnPlayerDied.GetInvocationList().Length : 0)})");
        OnPlayerDied?.Invoke(); // Static event (DeathScreenUI için)
        
        Debug.Log("[PlayerHealth] Die() completed!");
    }
    
    /// <summary>
    /// Player'ın hareket sistemini devre dışı bırak
    /// </summary>
    private void DisableMovement()
    {
        Debug.Log("<color=orange>[PlayerHealth] ⏸️ Disabling movement systems...</color>");
        
        // Movement scriptlerini ÖNCE devre dışı bırak (Update çalışmasın)
        SimplePlayerMovement simpleMovement = GetComponent<SimplePlayerMovement>();
        if (simpleMovement != null)
        {
            simpleMovement.enabled = false;
            Debug.Log($"[PlayerHealth] SimplePlayerMovement disabled. Was enabled: {simpleMovement.enabled}");
        }
        
        ShopPlayerMovement shopMovement = GetComponent<ShopPlayerMovement>();
        if (shopMovement != null)
        {
            shopMovement.enabled = false;
            Debug.Log($"[PlayerHealth] ShopPlayerMovement disabled. Was enabled: {shopMovement.enabled}");
        }
        
        // NavMeshAgent'ı devre dışı bırak (eğer varsa)
        UnityEngine.AI.NavMeshAgent navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.enabled = false;
            Debug.Log("[PlayerHealth] NavMeshAgent disabled.");
        }
        
        // CharacterController'ı SONRA devre dışı bırak
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            Debug.Log($"[PlayerHealth] CharacterController disabled. IsEnabled: {controller.enabled}");
        }
        
        Debug.Log("<color=orange>[PlayerHealth] ✓ All movement systems disabled!</color>");
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
