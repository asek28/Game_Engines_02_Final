using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maksimum can")]
    [SerializeField, Min(1)] private int maxHealth = 10;
    [Tooltip("Mevcut can")]
    [SerializeField] private int currentHealth;
    
    [Header("Death Settings")]
    [Tooltip("Ölüm sonrası respawn süresi (saniye)")]
    [SerializeField] private float respawnDelay = 3f;
    
    private bool isDead = false;
    private Vector3 spawnPosition;
    
    // Events
    public System.Action<int, int> OnHealthChanged; // (currentHealth, maxHealth)
    public System.Action OnPlayerDeath;
    public System.Action OnPlayerRespawn;
    
    private void Awake()
    {
        currentHealth = maxHealth;
        spawnPosition = transform.position;
    }
    
    private void Start()
    {
        // Health değişikliğini bildir
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log($"[PlayerHealth] Took {damage} damage! Current health: {currentHealth}/{maxHealth}");
        
        // Health değişikliğini bildir
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Can 0 oldu mu kontrol et
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        if (isDead)
        {
            return;
        }
        
        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        
        Debug.Log($"[PlayerHealth] Healed {amount}! Current health: {currentHealth}/{maxHealth}");
        
        // Health değişikliğini bildir
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    private void Die()
    {
        if (isDead)
        {
            return;
        }
        
        isDead = true;
        Debug.Log($"[PlayerHealth] Player died!");
        
        // Death event'ini tetikle
        OnPlayerDeath?.Invoke();
        
        // Hareketi durdur
        SimplePlayerMovement movement = GetComponent<SimplePlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }
        
        // Respawn'ı başlat
        Invoke(nameof(Respawn), respawnDelay);
    }
    
    private void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        transform.position = spawnPosition;
        
        Debug.Log($"[PlayerHealth] Player respawned!");
        
        // Hareketi tekrar etkinleştir
        SimplePlayerMovement movement = GetComponent<SimplePlayerMovement>();
        if (movement != null)
        {
            movement.enabled = true;
        }
        
        // Health değişikliğini bildir
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Respawn event'ini tetikle
        OnPlayerRespawn?.Invoke();
    }
    
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    
    public bool IsDead()
    {
        return isDead;
    }
    
    public void SetSpawnPosition(Vector3 position)
    {
        spawnPosition = position;
    }
}

