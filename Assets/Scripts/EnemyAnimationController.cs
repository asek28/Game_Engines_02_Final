using System.Collections;
using UnityEngine;

/// <summary>
/// Enemy animasyon kontrolü için script
/// Animator parametrelerini yönetir: Damaged, Loot, Damage, Hitting, Death
/// </summary>
[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maksimum can")]
    [SerializeField, Min(1)] private int maxHealth = 100;
    
    [Tooltip("Mevcut can")]
    [SerializeField] private int currentHealth;
    
    [Header("Animation Settings")]
    [Tooltip("Hasar animasyonu süresi (saniye) - Damage bool'unun true kalacağı süre")]
    [SerializeField, Min(0.1f)] private float damageAnimationDuration = 0.3f;
    
    [Tooltip("Saldırı animasyonu süresi (saniye) - Hitting bool'unun true kalacağı süre")]
    [SerializeField, Min(0.1f)] private float attackAnimationDuration = 1f;
    
    [Tooltip("Yaralı yürüme eşiği (can yüzdesi) - Bu değerin altına düşünce Damaged = true")]
    [SerializeField, Range(0f, 100f)] private float woundedThresholdPercent = 50f;
    
    // Animator referansı
    private Animator animator;
    
    // Animator parametre isimleri (exact names)
    private const string PARAM_DAMAGED = "Damaged";
    private const string PARAM_LOOT = "Loot";
    private const string PARAM_DAMAGE = "Damage";
    private const string PARAM_HITTING = "Hitting";
    private const string PARAM_DEATH = "Death"; // NOT: Artık Trigger parametresi!
    // NOT: Animator'da "Walking" parametresi YOK!
    // Walking bir state, default state olarak Entry'den direkt geçiş yapıyor
    // Diğer parametreler (Hitting, Damage, Loot, Damaged) false olduğunda otomatik Walking state'ine döner
    
    // Coroutine referansları (çoklu çağrıları önlemek için)
    private Coroutine damageAnimationCoroutine;
    private Coroutine attackAnimationCoroutine;
    
    private void Awake()
    {
        // Animator'ı al
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError($"[EnemyAnimationController] {name}: Animator component not found! Please add an Animator component.");
        }
        
        // Health'i başlat
        currentHealth = maxHealth;
    }
    
    private void Start()
    {
        // Animator parametrelerini başlangıç değerlerine ayarla
        InitializeAnimatorParameters();
    }
    
    /// <summary>
    /// Animator parametrelerini başlangıç değerlerine ayarla
    /// </summary>
    private void InitializeAnimatorParameters()
    {
        if (animator == null) return;
        
        animator.SetBool(PARAM_DAMAGED, false);
        animator.SetBool(PARAM_LOOT, false);
        animator.SetBool(PARAM_DAMAGE, false);
        animator.SetBool(PARAM_HITTING, false);
        // NOT: Death artık Trigger parametresi, Initialize'da set etmeye gerek yok
        // NOT: Walking parametresi yok, Walking bir state (default state)
        // Diğer parametreler false olduğunda otomatik Walking state'ine döner
        
        Debug.Log($"[EnemyAnimationController] {name}: Animator parameters initialized.");
    }
    
    /// <summary>
    /// Walking animasyonunu ayarla (Enemy.cs'den çağrılır)
    /// NOT: Animator'da "Walking" parametresi YOK! Walking bir state.
    /// Diğer parametreler (Hitting, Damage, Loot, Damaged) false olduğunda otomatik Walking state'ine döner.
    /// Bu metod sadece diğer animasyonların false olduğundan emin olur.
    /// </summary>
    /// <param name="isWalking">Yürüyor mu? (sadece diğer animasyonları kontrol etmek için)</param>
    public void SetWalking(bool isWalking)
    {
        // Walking parametresi yok, Walking bir state (default state)
        // Diğer parametreler false olduğunda otomatik Walking state'ine döner
        // Bu yüzden burada sadece diğer animasyonların false olduğundan emin oluyoruz
        
        if (animator == null || currentHealth <= 0)
        {
            return; // Ölüyse veya animator yoksa işlem yapma
        }
        
        if (isWalking)
        {
            // Yürüyorsa, diğer animasyonları false yap ki Walking state'ine dönsün
            // NOT: Death trigger aktifse Walking'e geçiş yapma (ölüyse yürümesin)
            // Trigger parametreleri otomatik reset olur, bu yüzden kontrol etmeye gerek yok
            // Ama ölüyse zaten yürümemeli (currentHealth <= 0 kontrolü zaten var)
            
            if (HasAnimatorParameter(PARAM_HITTING))
            {
                animator.SetBool(PARAM_HITTING, false);
            }
            if (HasAnimatorParameter(PARAM_DAMAGE))
            {
                animator.SetBool(PARAM_DAMAGE, false);
            }
            if (HasAnimatorParameter(PARAM_LOOT))
            {
                animator.SetBool(PARAM_LOOT, false);
            }
            // Damaged parametresini false yapma, çünkü can yüzdesine bağlı
            // Death parametresini false yapma, çünkü ölüm durumuna bağlı
        }
        // isWalking = false ise hiçbir şey yapma, çünkü diğer animasyonlar zaten aktif olabilir
    }
    
    /// <summary>
    /// Animator'da parametre var mı kontrol et
    /// </summary>
    private bool HasAnimatorParameter(string paramName)
    {
        if (animator == null) return false;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Enemy'ye hasar verir (animasyon kontrolü için - health Enemy.cs tarafından yönetiliyor)
    /// </summary>
    /// <param name="amount">Hasar miktarı (sadece animasyon için, health Enemy.cs'de azaltılıyor)</param>
    public void TakeDamage(int amount)
    {
        // NOT: Health Enemy.cs tarafından yönetiliyor, burada sadece animasyon kontrolü yapıyoruz
        // Enemy.cs'den SetHealth() çağrıldığında health güncelleniyor
        
        Debug.Log($"[EnemyAnimationController] {name}: Damage animation triggered! Amount: {amount}");
        
        // Damage bool'unu true yap (kısa süreli impact animasyonu için)
        SetDamageBool(true);
        
        // Eğer önceki coroutine çalışıyorsa durdur
        if (damageAnimationCoroutine != null)
        {
            StopCoroutine(damageAnimationCoroutine);
        }
        
        // Damage bool'unu belirli süre sonra false yap
        damageAnimationCoroutine = StartCoroutine(ResetDamageBoolAfterDelay());
        
        // Can yüzdesi kontrolü - %50'nin altına düştü mü?
        // Bu kontrol SetHealth() içinde yapılıyor, ama burada da kontrol edelim
        float healthPercent = (float)currentHealth / maxHealth * 100f;
        if (healthPercent < woundedThresholdPercent)
        {
            SetDamagedBool(true);
            Debug.Log($"[EnemyAnimationController] {name}: Health below {woundedThresholdPercent}%! Setting Damaged = true (Wounded Walk).");
        }
        
        // Ölüm kontrolü SetHealth() içinde yapılıyor
    }
    
    /// <summary>
    /// Saldırı animasyonunu başlatır
    /// </summary>
    public void PerformAttack()
    {
        if (currentHealth <= 0)
        {
            Debug.LogWarning($"[EnemyAnimationController] {name}: Cannot attack, enemy is dead.");
            return;
        }
        
        Debug.Log($"[EnemyAnimationController] {name}: Performing attack animation.");
        
        // Hitting bool'unu true yap
        SetHittingBool(true);
        
        // Eğer önceki coroutine çalışıyorsa durdur
        if (attackAnimationCoroutine != null)
        {
            StopCoroutine(attackAnimationCoroutine);
        }
        
        // Hitting bool'unu belirli süre sonra false yap
        attackAnimationCoroutine = StartCoroutine(ResetHittingBoolAfterDelay());
    }
    
    /// <summary>
    /// Loot bulunduğunda çağrılır
    /// </summary>
    public void FoundLoot()
    {
        if (currentHealth <= 0)
        {
            Debug.LogWarning($"[EnemyAnimationController] {name}: Cannot find loot, enemy is dead.");
            return;
        }
        
        Debug.Log($"[EnemyAnimationController] {name}: Found loot! Setting Loot = true.");
        SetLootBool(true);
    }
    
    /// <summary>
    /// Loot bool'unu false yapar (loot toplama animasyonu bittiğinde çağrılabilir)
    /// </summary>
    public void ResetLootBool()
    {
        SetLootBool(false);
    }
    
    // ========== Animator Parametre Setter Metodları ==========
    
    /// <summary>
    /// Damaged bool'unu ayarla (Wounded Walk state'i için)
    /// </summary>
    private void SetDamagedBool(bool value)
    {
        if (animator != null)
        {
            animator.SetBool(PARAM_DAMAGED, value);
        }
    }
    
    /// <summary>
    /// Loot bool'unu ayarla
    /// </summary>
    private void SetLootBool(bool value)
    {
        if (animator != null)
        {
            animator.SetBool(PARAM_LOOT, value);
        }
    }
    
    /// <summary>
    /// Damage bool'unu ayarla (Impact reaction için)
    /// </summary>
    private void SetDamageBool(bool value)
    {
        if (animator != null)
        {
            animator.SetBool(PARAM_DAMAGE, value);
        }
    }
    
    /// <summary>
    /// Hitting bool'unu ayarla (Attack animasyonu için)
    /// </summary>
    private void SetHittingBool(bool value)
    {
        if (animator != null)
        {
            animator.SetBool(PARAM_HITTING, value);
        }
    }
    
    /// <summary>
    /// Death trigger'ını tetikle (Death state için)
    /// NOT: Death artık Trigger parametresi, Int değil!
    /// </summary>
    private void SetDeathTrigger()
    {
        if (animator != null && HasAnimatorParameter(PARAM_DEATH))
        {
            animator.SetTrigger(PARAM_DEATH);
            Debug.Log($"[EnemyAnimationController] {name}: Death trigger set!");
        }
        else
        {
            Debug.LogWarning($"[EnemyAnimationController] {name}: Death parameter not found or animator is null!");
        }
    }
    
    // ========== Coroutines ==========
    
    /// <summary>
    /// Damage bool'unu belirli süre sonra false yapar
    /// </summary>
    private IEnumerator ResetDamageBoolAfterDelay()
    {
        yield return new WaitForSeconds(damageAnimationDuration);
        SetDamageBool(false);
        Debug.Log($"[EnemyAnimationController] {name}: Damage bool reset to false after {damageAnimationDuration}s.");
    }
    
    /// <summary>
    /// Hitting bool'unu belirli süre sonra false yapar
    /// </summary>
    private IEnumerator ResetHittingBoolAfterDelay()
    {
        yield return new WaitForSeconds(attackAnimationDuration);
        SetHittingBool(false);
        Debug.Log($"[EnemyAnimationController] {name}: Hitting bool reset to false after {attackAnimationDuration}s.");
    }
    
    // ========== Public Getter Metodları ==========
    
    /// <summary>
    /// Mevcut canı döndürür
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    /// <summary>
    /// Maksimum canı döndürür
    /// </summary>
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    
    /// <summary>
    /// Enemy'nin ölü olup olmadığını döndürür
    /// </summary>
    public bool IsDead()
    {
        return currentHealth <= 0;
    }
    
    /// <summary>
    /// Canı manuel olarak ayarlar (heal için kullanılabilir)
    /// </summary>
    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        
        // Can yüzdesi kontrolü
        float healthPercent = (float)currentHealth / maxHealth * 100f;
        if (healthPercent >= woundedThresholdPercent)
        {
            SetDamagedBool(false);
        }
        
        // Ölüm kontrolü
        if (currentHealth <= 0)
        {
            SetDeathTrigger(); // Death trigger'ını tetikle
            Debug.Log($"[EnemyAnimationController] {name}: Death trigger activated!");
            
            // Ölüm animasyonu başladığında diğer tüm animasyonları false yap
            SetHittingBool(false);
            SetDamageBool(false);
            SetLootBool(false);
            // Damaged'ı false yapma, çünkü ölüm animasyonu sırasında wounded walk gösterebilir
        }
        // NOT: Trigger parametreleri otomatik reset olur, else bloğuna gerek yok
    }
}
