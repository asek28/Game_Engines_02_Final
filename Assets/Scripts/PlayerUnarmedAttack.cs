using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Boş elle vuruş (isHitting animasyonu)
/// Hiçbir silah aktif değilken sol tık yapınca çalışır
/// </summary>
public class PlayerUnarmedAttack : MonoBehaviour
{
    [Header("Unarmed Attack Settings")]
    [Tooltip("Boş elle vuruş hasarı (düşük)")]
    [SerializeField] private int unarmedDamage = 2;
    
    [Tooltip("Boş elle vuruş menzili (OverlapSphere radius)")]
    [SerializeField] private float unarmedRange = 2f;
    
    [Tooltip("Vuruş noktası offset (karakterin önü)")]
    [SerializeField] private float attackOffsetDistance = 1f;
    
    [Tooltip("Vuruşlar arası cooldown")]
    [SerializeField] private float attackCooldown = 0.5f;
    
    [Tooltip("Animasyon süresi (isHitting true kalma süresi)")]
    [SerializeField] private float animationDuration = 0.5f;
    
    [Header("Audio")]
    [Tooltip("Boş elle vuruş sesi")]
    [SerializeField] private AudioClip punchSound;
    
    private PlayerAnimationController playerAnimController;
    private WeaponSlotSystem weaponSystem;
    private AudioSource audioSource;
    private float nextAttackTime = 0f;
    private Camera mainCamera;
    
    private void Awake()
    {
        playerAnimController = GetComponent<PlayerAnimationController>();
        weaponSystem = GetComponent<WeaponSlotSystem>();
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        mainCamera = Camera.main;
    }
    
    private void Update()
    {
        // Settings veya Inventory açıksa input alma
        if (IsUIOpen())
        {
            return;
        }
        
        // Mouse left click kontrolü
        var mouse = Mouse.current;
        bool attackPressed = mouse != null && mouse.leftButton.wasPressedThisFrame;
        
        if (!attackPressed)
        {
            return; // Sol tık yapılmadıysa çık
        }
        
        // Debug: Sol tık yapıldı
        Debug.Log($"<color=orange>[PlayerUnarmedAttack] Left click detected!</color>");
        
        // WeaponSlotSystem kontrolü
        if (weaponSystem != null)
        {
            IWeapon currentWeapon = weaponSystem.GetCurrentWeapon();
            if (currentWeapon != null)
            {
                Debug.Log($"<color=yellow>[PlayerUnarmedAttack] Weapon active: {currentWeapon.WeaponName}, ignoring unarmed attack</color>");
                return; // Silah varken boş elle vurma
            }
            else
            {
                Debug.Log($"<color=cyan>[PlayerUnarmedAttack] No weapon active, performing unarmed attack!</color>");
            }
        }
        else
        {
            Debug.LogWarning($"<color=yellow>[PlayerUnarmedAttack] WeaponSlotSystem is NULL! Performing unarmed attack anyway.</color>");
        }
        
        // Cooldown kontrolü
        if (Time.time >= nextAttackTime)
        {
            PerformUnarmedAttack();
            nextAttackTime = Time.time + attackCooldown;
        }
        else
        {
            Debug.Log($"<color=yellow>[PlayerUnarmedAttack] Cooldown active, wait {nextAttackTime - Time.time:F2}s</color>");
        }
    }
    
    /// <summary>
    /// Boş elle vuruş yap
    /// </summary>
    private void PerformUnarmedAttack()
    {
        Debug.Log("<color=orange>👊 [PlayerUnarmedAttack] Unarmed attack!</color>");
        
        // isHitting animasyonunu başlat
        if (playerAnimController != null)
        {
            playerAnimController.SetHitting(true);
            StartCoroutine(ResetHittingAnimation());
        }
        
        // Ses efekti
        if (audioSource != null && punchSound != null)
        {
            audioSource.PlayOneShot(punchSound);
        }
        
        // OverlapSphere ile yakındaki enemy'leri bul (Melee attack için daha güvenilir)
        Vector3 attackCenter = transform.position + transform.forward * attackOffsetDistance + Vector3.up * 1f;
        
        Debug.Log($"<color=cyan>[PlayerUnarmedAttack] Attack center: {attackCenter}, Range: {unarmedRange}</color>");
        
        // OverlapSphere ile tüm collider'ları bul
        Collider[] hitColliders = Physics.OverlapSphere(attackCenter, unarmedRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
        
        Debug.Log($"<color=cyan>[PlayerUnarmedAttack] Found {hitColliders.Length} colliders in range</color>");
        
        // Debug: OverlapSphere'i görselleştir
        Debug.DrawLine(transform.position, attackCenter, Color.yellow, 1f);
        
        bool hitEnemy = false;
        
        foreach (Collider col in hitColliders)
        {
            // Kendini vurma
            if (col.transform == transform || col.transform.IsChildOf(transform))
            {
                continue;
            }
            
            Debug.Log($"<color=orange>[PlayerUnarmedAttack] Checking collider: {col.name} (Tag: {col.tag})</color>");
            
            // Enemy'ye hasar ver
            EnemyAIController enemy = col.GetComponent<EnemyAIController>();
            if (enemy == null)
            {
                enemy = col.GetComponentInParent<EnemyAIController>();
            }
            
            if (enemy != null && !enemy.IsDead)
            {
                Vector3 knockbackDir = (enemy.transform.position - transform.position).normalized;
                Vector3 hitPoint = enemy.transform.position;
                
                enemy.TakeDamage(unarmedDamage, hitPoint, knockbackDir);
                
                // Açlık azalt (enemy'ye hasar verildiğinde)
                NotifyEnemyHit();
                
                Debug.Log($"<color=green>✅ [PlayerUnarmedAttack] HIT ENEMY! Dealt {unarmedDamage} damage to {enemy.name}! Health: {enemy.GetCurrentHealth()}/{enemy.GetMaxHealth()}</color>");
                
                hitEnemy = true;
                break; // Sadece ilk enemy'ye vur
            }
        }
        
        if (!hitEnemy)
        {
            Debug.Log($"<color=yellow>⚠️ [PlayerUnarmedAttack] No enemy found in range! (Center: {attackCenter}, Range: {unarmedRange})</color>");
        }
    }
    
    /// <summary>
    /// isHitting animasyonunu resetle
    /// </summary>
    private IEnumerator ResetHittingAnimation()
    {
        yield return new WaitForSeconds(animationDuration);
        
        if (playerAnimController != null)
        {
            playerAnimController.SetHitting(false);
        }
    }
    
    /// <summary>
    /// UI menülerinin açık olup olmadığını kontrol et
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
    /// Gizmos ile attack range'i görselleştir (Editor'da görünür)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Attack range sphere'i çiz
        Vector3 attackCenter = transform.position + transform.forward * attackOffsetDistance + Vector3.up * 1f;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackCenter, unarmedRange);
        
        // Karakterden attack center'a çizgi
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + Vector3.up * 1f, attackCenter);
    }
    
    /// <summary>
    /// Enemy'ye hasar verildiğinde HungerThirstManager'a bildir
    /// </summary>
    private void NotifyEnemyHit()
    {
        HungerThirstManager hungerThirstManager = FindFirstObjectByType<HungerThirstManager>();
        if (hungerThirstManager != null)
        {
            hungerThirstManager.OnEnemyHit();
        }
    }
}
