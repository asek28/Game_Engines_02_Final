using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WeaponHitDetector : MonoBehaviour
{
    [Header("Weapon Settings")]
    [Tooltip("Hasar miktarı")]
    [SerializeField, Min(1)] private int damage = 1;
    [Tooltip("Saldırı aktifken hasar ver (ComboSystem isAttacking kontrolü)")]
    [SerializeField] private bool requireActiveAttack = true;

    [Header("Juice Settings - Hit Feedback")]
    [Tooltip("Hit Stop süresi (saniye) - Vuruş anında oyunu dondurur")]
    [SerializeField, Min(0.01f)] private float hitStopDuration = 0.1f;
    [Tooltip("Screen shake kullanılsın mı?")]
    [SerializeField] private bool useScreenShake = true;
    [Tooltip("Screen shake gücü")]
    [SerializeField, Min(0.01f)] private float screenShakePower = 0.15f;
    [Tooltip("Screen shake süresi")]
    [SerializeField, Min(0.01f)] private float screenShakeDuration = 0.2f;

    private Collider weaponCollider;
    private readonly System.Collections.Generic.List<Enemy> enemiesInRange = new System.Collections.Generic.List<Enemy>();
    private readonly System.Collections.Generic.List<EnemyAIController> enemyAIControllersInRange = new System.Collections.Generic.List<EnemyAIController>();
    private readonly System.Collections.Generic.List<LootBox> lootBoxesInRange = new System.Collections.Generic.List<LootBox>();
    private readonly System.Collections.Generic.List<AnimatedBox> animatedBoxesInRange = new System.Collections.Generic.List<AnimatedBox>();
    private CameraShake cameraShake;
    private Transform playerTransform;

    private void Awake()
    {
        weaponCollider = GetComponent<Collider>();
        if (weaponCollider == null)
        {
            Debug.LogError($"WeaponHitDetector on {name}: Collider component not found!");
            return;
        }

        // Collider'ı trigger yap
        weaponCollider.isTrigger = true;
        
        // Player transform'unu bul
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            SimplePlayerMovement playerMovement = FindFirstObjectByType<SimplePlayerMovement>();
            if (playerMovement != null)
            {
                playerTransform = playerMovement.transform;
            }
        }
        else
        {
            playerTransform = player.transform;
        }
    }

    private void Start()
    {
        // ComboSystem eventini dinle
        ComboSystem.OnAttackPerformed += OnAttackPerformed;
        
        // Camera shake'i bul
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraShake = mainCamera.GetComponent<CameraShake>();
            if (cameraShake == null)
            {
                cameraShake = mainCamera.gameObject.AddComponent<CameraShake>();
            }
        }
        else
        {
            Debug.LogWarning("[WeaponHitDetector] Main camera not found! Screen shake will not work.");
        }
    }

    private void OnDestroy()
    {
        // Event dinleyicisini kaldır
        ComboSystem.OnAttackPerformed -= OnAttackPerformed;
    }

    private void OnAttackPerformed()
    {
        bool hitEnemy = false;
        bool hitLootBox = false;
        bool hitAnimatedBox = false;
        Vector3 hitPoint = Vector3.zero;
        
        // Saldırı yapıldığında, menzildeki tüm Enemy'lere hasar ver (eski sistem)
        foreach (Enemy enemy in enemiesInRange)
        {
            if (enemy != null && !enemy.IsDead)
            {
                // Hit point'i hesapla (enemy pozisyonu)
                hitPoint = enemy.transform.position;
                
                // Enemy'ye hasar ver (knockback ve visual feedback dahil)
                Vector3 knockbackDirection = playerTransform != null 
                    ? (enemy.transform.position - playerTransform.position).normalized 
                    : Vector3.forward;
                
                enemy.TakeDamage(damage, hitPoint, knockbackDirection);
                hitEnemy = true;
                
                // Açlık azalt (enemy'ye hasar verildiğinde)
                NotifyEnemyHit();
                
                // Debug.Log($"[WeaponHitDetector] {name}: Hit enemy {enemy.name} for {damage} damage!"); // Gereksiz log
            }
        }
        
        // Saldırı yapıldığında, menzildeki tüm EnemyAIController'lere hasar ver (yeni sistem)
        foreach (EnemyAIController enemyAI in enemyAIControllersInRange)
        {
            if (enemyAI != null && !enemyAI.IsDead)
            {
                // Hit point'i hesapla (enemy pozisyonu)
                hitPoint = enemyAI.transform.position;
                
                // Enemy'ye hasar ver (knockback ve visual feedback dahil)
                Vector3 knockbackDirection = playerTransform != null 
                    ? (enemyAI.transform.position - playerTransform.position).normalized 
                    : Vector3.forward;
                
                enemyAI.TakeDamage(damage, hitPoint, knockbackDirection);
                hitEnemy = true;
                
                // Açlık azalt (enemy'ye hasar verildiğinde)
                NotifyEnemyHit();
                
                // Debug.Log($"[WeaponHitDetector] {name}: Hit enemy AI {enemyAI.name} for {damage} damage!"); // Gereksiz log
            }
        }
        
        // Saldırı yapıldığında, menzildeki tüm LootBox'lara hasar ver
        foreach (LootBox lootBox in lootBoxesInRange)
        {
            if (lootBox != null)
            {
                hitPoint = lootBox.transform.position;
                lootBox.TakeDamage(damage);
                hitLootBox = true;
                
                // Debug.Log($"[WeaponHitDetector] {name}: Hit lootbox {lootBox.name} for {damage} damage!"); // Gereksiz log
            }
        }
        
        // Saldırı yapıldığında, menzildeki tüm AnimatedBox'lara hasar ver
        foreach (AnimatedBox animatedBox in animatedBoxesInRange)
        {
            if (animatedBox != null)
            {
                hitPoint = animatedBox.transform.position;
                animatedBox.TakeDamage(damage);
                hitAnimatedBox = true;
                
                // Debug.Log($"[WeaponHitDetector] {name}: Hit animated box {animatedBox.name} for {damage} damage!"); // Gereksiz log
            }
        }
        
        // ALTERNATIF HIT DETECTION: OverlapSphere ile yakındaki EnemyAIController'ları bul
        // OnTriggerEnter çalışmadıysa bu yöntem devreye girer
        if (!hitEnemy && weaponCollider != null)
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(weaponCollider.bounds.center, weaponCollider.bounds.size.magnitude * 1.5f);
            foreach (Collider col in nearbyColliders)
            {
                // EnemyAIController kontrolü
                EnemyAIController nearbyEnemyAI = col.GetComponent<EnemyAIController>();
                if (nearbyEnemyAI != null && !enemyAIControllersInRange.Contains(nearbyEnemyAI) && !nearbyEnemyAI.IsDead)
                {
                    hitPoint = nearbyEnemyAI.transform.position;
                    Vector3 knockbackDirection = playerTransform != null 
                        ? (nearbyEnemyAI.transform.position - playerTransform.position).normalized 
                        : Vector3.forward;
                    nearbyEnemyAI.TakeDamage(damage, hitPoint, knockbackDirection);
                    hitEnemy = true;
                    enemyAIControllersInRange.Add(nearbyEnemyAI);
                    Debug.Log($"[WeaponHitDetector] {name}: Hit EnemyAIController {nearbyEnemyAI.name} via OverlapSphere!");
                }
                
                // LootBox kontrolü
                LootBox nearbyLootBox = col.GetComponent<LootBox>();
                if (nearbyLootBox != null && !lootBoxesInRange.Contains(nearbyLootBox))
                {
                    hitPoint = nearbyLootBox.transform.position;
                    nearbyLootBox.TakeDamage(damage);
                    hitLootBox = true;
                    lootBoxesInRange.Add(nearbyLootBox);
                }
            }
        }

        // Temizle (null referansları kaldır)
        enemiesInRange.RemoveAll(e => e == null);
        enemyAIControllersInRange.RemoveAll(e => e == null);
        lootBoxesInRange.RemoveAll(lb => lb == null);
        animatedBoxesInRange.RemoveAll(ab => ab == null);
        
        // Eğer enemy'ye, lootbox'a veya animated box'a vurulduysa, juice efektlerini uygula
        if (hitEnemy || hitLootBox || hitAnimatedBox)
        {
            ApplyHitJuice(hitPoint);
        }
    }
    
    /// <summary>
    /// Hit Stop ve Screen Shake gibi "juice" efektlerini uygular
    /// </summary>
    private void ApplyHitJuice(Vector3 hitPoint)
    {
        // Hit Stop: Vuruş anında oyunu kısa süreliğine dondur (impact hissi)
        StartCoroutine(HitStopCoroutine());
        
        // Screen Shake: Kamerayı sars (impact hissi)
        if (useScreenShake && cameraShake != null)
        {
            cameraShake.Shake(screenShakeDuration, screenShakePower);
        }
    }
    
    /// <summary>
    /// Hit Stop Coroutine - Vuruş anında Time.timeScale'i 0 yaparak freeze frame efekti verir
    /// </summary>
    private IEnumerator HitStopCoroutine()
    {
        // Oyunu dondur
        Time.timeScale = 0f;
        
        // Gerçek zamanı bekle (Time.timeScale = 0 olduğu için Time.deltaTime çalışmaz)
        float realTime = 0f;
        while (realTime < hitStopDuration)
        {
            realTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // Oyunu normale döndür
        Time.timeScale = 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Enemy algılama (eski sistem)
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Add(enemy);
            // Debug.Log($"[WeaponHitDetector] {name}: Enemy {enemy.name} entered weapon range."); // Gereksiz log
        }
        
        // EnemyAIController algılama (yeni sistem)
        EnemyAIController enemyAI = other.GetComponent<EnemyAIController>();
        if (enemyAI != null && !enemyAIControllersInRange.Contains(enemyAI))
        {
            enemyAIControllersInRange.Add(enemyAI);
            // Debug.Log($"[WeaponHitDetector] {name}: EnemyAIController {enemyAI.name} entered weapon range."); // Gereksiz log
        }
        
        LootBox lootBox = other.GetComponent<LootBox>();
        if (lootBox != null && !lootBoxesInRange.Contains(lootBox))
        {
            lootBoxesInRange.Add(lootBox);
            // Debug.Log($"[WeaponHitDetector] {name}: LootBox {lootBox.name} entered weapon range (OnTriggerEnter)."); // Gereksiz log
        }
        
        AnimatedBox animatedBox = other.GetComponent<AnimatedBox>();
        if (animatedBox != null && !animatedBoxesInRange.Contains(animatedBox))
        {
            animatedBoxesInRange.Add(animatedBox);
            // Debug.Log($"[WeaponHitDetector] {name}: AnimatedBox {animatedBox.name} entered weapon range."); // Gereksiz log
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        // OnTriggerStay ile her frame kontrol et (daha güvenilir)
        LootBox lootBox = other.GetComponent<LootBox>();
        if (lootBox != null && !lootBoxesInRange.Contains(lootBox))
        {
            lootBoxesInRange.Add(lootBox);
            // Debug.Log($"[WeaponHitDetector] {name}: LootBox {lootBox.name} detected via OnTriggerStay."); // Gereksiz log
        }
        
        AnimatedBox animatedBox = other.GetComponent<AnimatedBox>();
        if (animatedBox != null && !animatedBoxesInRange.Contains(animatedBox))
        {
            animatedBoxesInRange.Add(animatedBox);
            // Debug.Log($"[WeaponHitDetector] {name}: AnimatedBox {animatedBox.name} detected via OnTriggerStay."); // Gereksiz log
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Enemy çıkış (eski sistem)
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Remove(enemy);
            // Debug.Log($"[WeaponHitDetector] {name}: Enemy {enemy.name} left weapon range."); // Gereksiz log
        }
        
        // EnemyAIController çıkış (yeni sistem)
        EnemyAIController enemyAI = other.GetComponent<EnemyAIController>();
        if (enemyAI != null && enemyAIControllersInRange.Contains(enemyAI))
        {
            enemyAIControllersInRange.Remove(enemyAI);
            // Debug.Log($"[WeaponHitDetector] {name}: EnemyAIController {enemyAI.name} left weapon range."); // Gereksiz log
        }
        
        LootBox lootBox = other.GetComponent<LootBox>();
        if (lootBox != null && lootBoxesInRange.Contains(lootBox))
        {
            lootBoxesInRange.Remove(lootBox);
        }
        
        AnimatedBox animatedBox = other.GetComponent<AnimatedBox>();
        if (animatedBox != null && animatedBoxesInRange.Contains(animatedBox))
        {
            animatedBoxesInRange.Remove(animatedBox);
        }
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

