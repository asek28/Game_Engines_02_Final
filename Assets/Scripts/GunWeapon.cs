using UnityEngine;
using UnityEngine.InputSystem;
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
        
        // Raycast ile ateş et
        Vector3 rayOrigin = firePoint != null ? firePoint.position : transform.position;
        Vector3 rayDirection = firePoint != null ? firePoint.forward : transform.forward;
        
        // Ekran merkezinden raycast (crosshair'den ateş etme)
        if (mainCamera != null)
        {
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            rayOrigin = ray.origin;
            rayDirection = ray.direction;
        }
        
        RaycastHit hit;
        // ÖNEMLI: QueryTriggerInteraction.Collide kullanarak trigger collider'ları da algıla
        bool didHit = Physics.Raycast(rayOrigin, rayDirection, out hit, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
        
        Vector3 hitPoint = didHit ? hit.point : rayOrigin + rayDirection * range;
        
        // Debug: Raycast'i görselleştir
        if (showRaycastDebug)
        {
            Color debugColor = didHit ? Color.green : Color.red;
            Debug.DrawLine(rayOrigin, hitPoint, debugColor, 2f); // 2 saniye görünür
        }
        
        // Mermi izi göster
        if (bulletTrail != null)
        {
            StartCoroutine(ShowBulletTrail(rayOrigin, hitPoint));
        }
        
        // Hasar ver (eğer enemy'ye isabet ettiyse)
        if (didHit)
        {
            Debug.Log($"[GunWeapon] Hit: {hit.collider.name} (Tag: {hit.collider.tag}, GameObject: {hit.collider.gameObject.name})");
            
            // Enemy'ye hasar ver - hem collider'da hem de parent'ta ara
            EnemyAIController enemy = hit.collider.GetComponent<EnemyAIController>();
            
            // Eğer collider'da yoksa parent'a bak (çünkü collider child object olabilir)
            if (enemy == null)
            {
                enemy = hit.collider.GetComponentInParent<EnemyAIController>();
            }
            
            if (enemy != null)
            {
                Vector3 knockbackDir = rayDirection.normalized;
                enemy.TakeDamage(damage, hit.point, knockbackDir);
                
                // Büyük hasar logu (daha görünür)
                Debug.Log($"<color=green>✅ [GunWeapon] HIT ENEMY! Dealt {damage} damage to {enemy.name}! Health: {enemy.GetCurrentHealth()}/{enemy.GetMaxHealth()}</color>");
            }
            else
            {
                // Enemy component'i bulunamadı
                Debug.Log($"<color=yellow>⚠️ [GunWeapon] Hit {hit.collider.name} but no EnemyAIController found! (Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})</color>");
            }
            
            // Impact efekti (her vuruşta - duvar, zemin, enemy hepsi için)
            if (impactEffect != null)
            {
                GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 2f);
            }
            else
            {
                // Varsayılan impact efekti (basit sarı sphere)
                GameObject defaultImpact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                defaultImpact.transform.position = hit.point;
                defaultImpact.transform.localScale = Vector3.one * 0.2f;
                Renderer impactRenderer = defaultImpact.GetComponent<Renderer>();
                if (impactRenderer != null)
                {
                    impactRenderer.material.color = Color.yellow;
                    impactRenderer.material.EnableKeyword("_EMISSION");
                    impactRenderer.material.SetColor("_EmissionColor", Color.yellow * 2f);
                }
                Destroy(defaultImpact, 0.1f);
            }
        }
        
        Debug.Log($"[GunWeapon] Fired! Hit: {didHit}");
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
}
