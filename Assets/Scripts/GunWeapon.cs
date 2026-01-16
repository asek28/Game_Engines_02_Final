using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Ateşli silah - Gun
/// Mouse tıklaması ile ateş eder, Enemy'lere hasar verir
/// </summary>
public class GunWeapon : MonoBehaviour, IWeapon
{
    [Header("Weapon Info")]
    [SerializeField] private string weaponName = "Gun";
    
    [Header("Gun Settings")]
    [Tooltip("Hasar miktarı")]
    [SerializeField] private int damage = 10;
    
    [Tooltip("Menzil (raycast mesafesi)")]
    [SerializeField] private float range = 50f;
    
    [Tooltip("Hasar verme yöntemi: Raycast (ekran merkezi) veya OverlapSphere (player önünde alan taraması)")]
    [SerializeField] private bool useOverlapSphere = true;
    
    [Tooltip("OverlapSphere yarıçapı (sadece useOverlapSphere true ise)")]
    [SerializeField] private float overlapSphereRadius = 2f;
    
    [Tooltip("Enemy tag'i (boş bırakılırsa 'Enemy' kullanılır)")]
    [SerializeField] private string enemyTag = "Enemy";
    
    [Tooltip("Ateş etme cooldown (saniye)")]
    [SerializeField] private float fireRate = 0.25f;
    
    [Tooltip("Mermi çıkış noktası (barrel)")]
    [SerializeField] private Transform firePoint;
    
    [Header("Visual Effects")]
    [Tooltip("Mermi izi için LineRenderer")]
    [SerializeField] private LineRenderer bulletTrail;
    
    [Tooltip("Ateş efekti (muzzle flash) - ParticleSystem VEYA SimpleMuzzleFlash")]
    [SerializeField] private ParticleSystem muzzleFlashParticle;
    
    [Tooltip("Ateş efekti (muzzle flash) - SimpleMuzzleFlash component")]
    [SerializeField] private SimpleMuzzleFlash muzzleFlash;
    
    [Tooltip("Vuruş efekti (impact) - Prefab")]
    [SerializeField] private GameObject impactEffect;
    
    [Header("Audio")]
    [Tooltip("Ateş sesi")]
    [SerializeField] private AudioClip fireSound;
    
    [Tooltip("Boş şarjör sesi")]
    [SerializeField] private AudioClip emptySound;
    
    [Header("Debug")]
    [Tooltip("Raycast'i görselleştir (Debug.DrawLine)")]
    [SerializeField] private bool showRaycastDebug = true;
    
    [Tooltip("Debug raycast çizgi rengi")]
    [SerializeField] private Color raycastDebugColor = Color.yellow;
    
    private AudioSource audioSource;
    private bool isEquipped = false;
    private float nextFireTime = 0f;
    private Camera mainCamera;
    private PlayerAnimationController playerAnimController;
    
    public string WeaponName => weaponName;
    public bool IsEquipped => isEquipped;
    public GameObject WeaponObject => gameObject;
    
    private void Awake()
    {
        // enemyTag null ise varsayılan değeri ata
        if (string.IsNullOrEmpty(enemyTag))
        {
            enemyTag = "Enemy";
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        mainCamera = Camera.main;
        
        // PlayerAnimationController'ı bul (parent'ta olmalı)
        playerAnimController = GetComponentInParent<PlayerAnimationController>();
        if (playerAnimController == null)
        {
            Debug.LogWarning("[GunWeapon] PlayerAnimationController not found in parent! Shooting animation won't play.");
        }
        
        // LineRenderer'ı konfigüre et
        if (bulletTrail != null)
        {
            bulletTrail.enabled = false;
        }
    }
    
    private void Update()
    {
        if (!isEquipped)
        {
            return;
        }
        
        // Settings veya Inventory açıksa ateş etme
        if (IsUIOpen())
        {
            return;
        }
        
        // Mouse left click ile ateş et
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            Use();
        }
    }
    
    public void Equip()
    {
        isEquipped = true;
        gameObject.SetActive(true);
        
        Debug.Log($"[GunWeapon] Equipped: {weaponName}");
    }
    
    public void Unequip()
    {
        isEquipped = false;
        
        Debug.Log($"[GunWeapon] Unequipped: {weaponName}");
    }
    
    public void Use()
    {
        if (!isEquipped)
        {
            return;
        }
        
        // Cooldown kontrolü
        if (Time.time < nextFireTime)
        {
            return;
        }
        
        Fire();
        nextFireTime = Time.time + fireRate;
    }
    
    /// <summary>
    /// Ateş et
    /// </summary>
    private void Fire()
    {
        Debug.Log($"<color=cyan>🔫 [GunWeapon] FIRING! Range: {range}, Damage: {damage}</color>");
        
        // Shooting animasyonunu başlat
        if (playerAnimController != null)
        {
            playerAnimController.SetShooting(true);
            // Animasyonu kısa süre sonra kapat (fire rate kadar)
            StartCoroutine(ResetShootingAnimation(fireRate));
        }
        
        // Ses efekti
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
        
        // Muzzle flash (ParticleSystem)
        if (muzzleFlashParticle != null)
        {
            muzzleFlashParticle.Play();
        }
        
        // Muzzle flash (SimpleMuzzleFlash)
        if (muzzleFlash != null)
        {
            muzzleFlash.ShowFlash();
        }
        
        // Player transform'unu bul (OverlapSphere için gerekli)
        Transform playerTransform = null;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            SimplePlayerMovement playerMovement = FindFirstObjectByType<SimplePlayerMovement>();
            if (playerMovement != null)
            {
                playerTransform = playerMovement.transform;
            }
        }
        
        bool hitEnemy = false;
        Vector3 hitPoint = Vector3.zero;
        
        // YÖNTEM 1: OverlapSphere (Player'ın önünde alan taraması - daha güvenilir)
        if (useOverlapSphere && playerTransform != null)
        {
            // Player'ın önünde bir nokta hesapla
            Vector3 forwardPosition = playerTransform.position + playerTransform.forward * (range * 0.5f);
            
            // OverlapSphere ile enemy'leri bul
            Collider[] colliders = Physics.OverlapSphere(forwardPosition, overlapSphereRadius, ~0, QueryTriggerInteraction.Collide);
            
            Debug.Log($"[GunWeapon] 🔍 OverlapSphere at {forwardPosition}, Radius: {overlapSphereRadius}, Found {colliders.Length} colliders");
            
            foreach (Collider col in colliders)
            {
                if (col == null) continue;
                
                GameObject hitObject = col.gameObject;
                bool isEnemy = false;
                
                // 1. Tag kontrolü
                if (!string.IsNullOrEmpty(enemyTag) && 
                    (col.CompareTag(enemyTag) || hitObject.CompareTag(enemyTag)))
                {
                    isEnemy = true;
                }
                
                // 2. Component kontrolü
                Enemy enemy = hitObject.GetComponent<Enemy>();
                EnemyAIController enemyAI = hitObject.GetComponent<EnemyAIController>();
                
                if (enemy == null)
                {
                    enemy = hitObject.GetComponentInParent<Enemy>();
                }
                if (enemyAI == null)
                {
                    enemyAI = hitObject.GetComponentInParent<EnemyAIController>();
                }
                
                if (enemy != null || enemyAI != null)
                {
                    isEnemy = true;
                }
                
                // Enemy'ye hasar ver
                if (isEnemy)
                {
                    hitPoint = hitObject.transform.position;
                    Vector3 knockbackDir = playerTransform != null 
                        ? (hitObject.transform.position - playerTransform.position).normalized 
                        : Vector3.forward;
                    
                    // EnemyAIController'a hasar ver
                    if (enemyAI != null && !enemyAI.IsDead)
                    {
                        enemyAI.TakeDamage(damage, hitPoint, knockbackDir);
                        NotifyEnemyHit();
                        hitEnemy = true;
                        Debug.Log($"<color=green>✅ [GunWeapon] HIT ENEMY AI via OverlapSphere! Dealt {damage} damage to {enemyAI.name}! Health: {enemyAI.GetCurrentHealth()}/{enemyAI.GetMaxHealth()}</color>");
                    }
                    // Enemy.cs'ye hasar ver
                    else if (enemy != null && !enemy.IsDead)
                    {
                        enemy.TakeDamage(damage, hitPoint, knockbackDir);
                        NotifyEnemyHit();
                        hitEnemy = true;
                        Debug.Log($"<color=green>✅ [GunWeapon] HIT ENEMY via OverlapSphere! Dealt {damage} damage to {enemy.name}! Health: {enemy.GetCurrentHealth()}/{enemy.GetMaxHealth()}</color>");
                    }
                }
            }
            
            // Visual feedback için raycast yap (mermi izi için)
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            
            if (mainCamera != null)
            {
                Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
                Ray ray = mainCamera.ScreenPointToRay(screenCenter);
                Vector3 rayOrigin = ray.origin;
                Vector3 rayDirection = ray.direction;
                
                RaycastHit hit;
                bool didHit = Physics.Raycast(rayOrigin, rayDirection, out hit, range, ~0, QueryTriggerInteraction.Collide);
                Vector3 visualHitPoint = didHit ? hit.point : rayOrigin + rayDirection * range;
                
                // Mermi izi göster
                if (bulletTrail != null)
                {
                    StartCoroutine(ShowBulletTrail(rayOrigin, visualHitPoint));
                }
                
                // Impact efekti
                if (didHit && impactEffect != null)
                {
                    GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impact, 2f);
                }
            }
        }
        // YÖNTEM 2: Raycast (ekran merkezi - eski yöntem)
        else
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            
            if (mainCamera == null)
            {
                Debug.LogError("[GunWeapon] Camera.main is null! Cannot fire.");
                return;
            }
            
            // Ekranın tam ortasından raycast
            Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
            Ray ray = mainCamera.ScreenPointToRay(screenCenter);
            Vector3 rayOrigin = ray.origin;
            Vector3 rayDirection = ray.direction;
            
            RaycastHit hit;
            int layerMask = ~0;
            bool didHit = Physics.Raycast(rayOrigin, rayDirection, out hit, range, layerMask, QueryTriggerInteraction.Collide);
            
            hitPoint = didHit ? hit.point : rayOrigin + rayDirection * range;
            
            Debug.Log($"[GunWeapon] 🎯 Raycast from screen center - Hit: {didHit}, Range: {range}");
            
            // Mermi izi göster
            if (bulletTrail != null)
            {
                StartCoroutine(ShowBulletTrail(rayOrigin, hitPoint));
            }
            
            // Enemy detection ve hasar verme
            if (didHit && hit.collider != null)
            {
                GameObject hitObject = hit.collider.gameObject;
                if (hitObject == null) return;
                
                bool isEnemy = false;
                
                // 1. Tag kontrolü
                if (!string.IsNullOrEmpty(enemyTag) && hit.collider != null)
                {
                    if (hit.collider.CompareTag(enemyTag) || hitObject.CompareTag(enemyTag))
                    {
                        isEnemy = true;
                    }
                }
                
                // 2. Component kontrolü
                Enemy enemy = hitObject.GetComponent<Enemy>();
                EnemyAIController enemyAI = hitObject.GetComponent<EnemyAIController>();
                
                if (enemy == null)
                {
                    enemy = hitObject.GetComponentInParent<Enemy>();
                }
                if (enemyAI == null)
                {
                    enemyAI = hitObject.GetComponentInParent<EnemyAIController>();
                }
                
                if (enemy != null || enemyAI != null)
                {
                    isEnemy = true;
                }
                
                // Enemy'ye hasar ver
                if (isEnemy)
                {
                    Vector3 knockbackDir = rayDirection.normalized;
                    
                    if (enemyAI != null && !enemyAI.IsDead)
                    {
                        enemyAI.TakeDamage(damage, hit.point, knockbackDir);
                        NotifyEnemyHit();
                        hitEnemy = true;
                        Debug.Log($"<color=green>✅ [GunWeapon] HIT ENEMY AI! Dealt {damage} damage to {enemyAI.name}! Health: {enemyAI.GetCurrentHealth()}/{enemyAI.GetMaxHealth()}</color>");
                    }
                    else if (enemy != null && !enemy.IsDead)
                    {
                        enemy.TakeDamage(damage, hit.point, knockbackDir);
                        NotifyEnemyHit();
                        hitEnemy = true;
                        Debug.Log($"<color=green>✅ [GunWeapon] HIT ENEMY! Dealt {damage} damage to {enemy.name}! Health: {enemy.GetCurrentHealth()}/{enemy.GetMaxHealth()}</color>");
                    }
                }
            }
            
            // Impact efekti
            if (didHit)
            {
                if (impactEffect != null)
                {
                    GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impact, 2f);
                }
            }
        }
        
        Debug.Log($"[GunWeapon] Fired! Hit Enemy: {hitEnemy}");
    }
    
    /// <summary>
    /// Mermi izini göster
    /// </summary>
    private IEnumerator ShowBulletTrail(Vector3 startPoint, Vector3 endPoint)
    {
        if (bulletTrail == null) yield break;
        
        bulletTrail.SetPosition(0, startPoint);
        bulletTrail.SetPosition(1, endPoint);
        bulletTrail.enabled = true;
        
        yield return new WaitForSeconds(0.05f);
        
        bulletTrail.enabled = false;
    }
    
    /// <summary>
    /// Shooting animasyonunu resetle
    /// </summary>
    private IEnumerator ResetShootingAnimation(float delay)
    {
        yield return new WaitForSeconds(delay * 0.8f); // Fire rate'in %80'i kadar bekle
        
        if (playerAnimController != null)
        {
            playerAnimController.SetShooting(false);
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
