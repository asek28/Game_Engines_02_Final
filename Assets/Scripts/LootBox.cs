using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class LootBox : MonoBehaviour
{
    [Header("LootBox Settings")]
    [Tooltip("Kutunun sağlığı (genellikle 1 - tek vuruşta kırılır)")]
    [SerializeField, Min(1)] private int maxHealth = 1;
    
    [Header("Mesh References")]
    [Tooltip("Normal (sağlam) kutu mesh'i - animasyon sonrası kapatılacak")]
    [SerializeField] private GameObject normalMesh;
    [Tooltip("Kırık mesh (animasyonsuz kullanım için, opsiyonel)")]
    [SerializeField] private GameObject brokenMesh;
    
    [Header("Animation Settings")]
    [Tooltip("Animator component (kırılma animasyonu için)")]
    [SerializeField] private Animator animator;
    
    [Tooltip("Kırılma animasyonu trigger adı (Animator Controller'da tanımlı olmalı)")]
    [SerializeField] private string breakTriggerName = "Hit";
    
    [Tooltip("Animasyon süresini kısaltmak için speed multiplier (örn: 2.0 = 2x hızlı)")]
    [SerializeField, Min(0.1f)] private float animationSpeedMultiplier = 1f;
    
    [Tooltip("Animasyonun hedef süresi (saniye) - animasyon bu süreye göre otomatik hızlandırılır. Bu süre sonunda kutunun parçaları yok olur (0.5 saniye önerilir)")]
    [SerializeField, Min(0.1f)] private float targetAnimationDuration = 0.5f;
    
    [Tooltip("Animasyon bitince ne kadar süre sonra yok olsun (saniye) - 0 = hemen yok ol")]
    [SerializeField, Min(0f)] private float destroyDelayAfterAnimation = 0f;
    
    [Header("Loot Spawn Settings")]
    [Tooltip("Loot çıkma şansı (0-100) - 100 = her zaman çıkar")]
    [SerializeField, Range(0f, 100f)] private float lootSpawnChance = 100f;
    
    [Tooltip("Loot spawn şansını yok say ve her zaman loot çıkar (lootSpawnChance'ı görmezden gelir)")]
    [SerializeField] private bool alwaysSpawnLoot = true;
    
    [Tooltip("Tier bazlı loot spawn ayarları")]
    [SerializeField] private RaritySpawnSettings[] tierLootSettings = new RaritySpawnSettings[]
    {
        new RaritySpawnSettings { rarity = LootRarity.Tier1, spawnWeight = 70f, scrapValue = 1, minSpawnCount = 1, maxSpawnCount = 2 },
        new RaritySpawnSettings { rarity = LootRarity.Tier2, spawnWeight = 25f, scrapValue = 5, minSpawnCount = 1, maxSpawnCount = 1 },
        new RaritySpawnSettings { rarity = LootRarity.Tier3, spawnWeight = 5f, scrapValue = 10, minSpawnCount = 1, maxSpawnCount = 1 }
    };
    
    [Header("Special Loot")]
    [Tooltip("tabeFinal mesh'inin spawn şansı (%)")]
    [SerializeField, Range(0f, 100f)] private float tabeFinalSpawnChance = 5f;
    
    [Tooltip("tabeFinal prefab referansı (Resources'tan yüklenecek veya manuel atanacak)")]
    [SerializeField] private GameObject tabeFinalPrefab;
    
    [Tooltip("tabeFinal'i Resources klasöründen yükle (aktifse, tabeFinalPrefab görmezden gelinir)")]
    [SerializeField] private bool loadTabeFinalFromResources = true;
    
    [Tooltip("Resources klasöründeki tabeFinal prefab yolu")]
    [SerializeField] private string tabeFinalResourcePath = "tabeFinal";
    
    [Header("Loot Templates")]
    [Tooltip("Spawn edilecek loot template'leri (boşsa, Thresh_Spawner'daki template'ler kullanılır)")]
    [SerializeField] private List<GameObject> lootTemplates = new List<GameObject>();
    
    [Header("Spawn Settings")]
    [Tooltip("Manuel spawn point'ler kullan (aktifse, spawnRadius ve spawnHeightOffset görmezden gelinir)")]
    [SerializeField] private bool useManualSpawnPoints = false;
    
    [Tooltip("Manuel spawn point'ler (boşsa veya useManualSpawnPoints false ise, otomatik spawn kullanılır)")]
    [SerializeField] private Transform[] manualSpawnPoints = new Transform[0];
    
    [Tooltip("Loot spawn mesafesi (kutunun etrafında - sadece useManualSpawnPoints false ise kullanılır)")]
    [SerializeField, Min(0f)] private float spawnRadius = 1f;
    
    [Tooltip("Loot spawn yüksekliği offset (sadece useManualSpawnPoints false ise kullanılır) - 0 = yerde")]
    [SerializeField] private float spawnHeightOffset = 0f;
    
    [Tooltip("Loot'u yere raycast ile yerleştir (aktifse, spawnHeightOffset görmezden gelinir)")]
    [SerializeField] private bool spawnOnGround = true;
    
    [Tooltip("Trigger collider genişletme çarpanı (1.0 = normal, 1.3 = %30 daha büyük - daha kolay vurma için)")]
    [SerializeField, Min(1.0f)] private float triggerColliderSizeMultiplier = 1.3f;
    
    private int currentHealth;
    private bool isBroken = false;
    private Collider boxCollider;
    private Rigidbody boxRigidbody;
    
    private void Awake()
    {
        // Ana collider'ı bul (karakter geçmesin diye normal collider)
        boxCollider = GetComponent<Collider>();
        if (boxCollider == null)
        {
            Debug.LogError($"[LootBox] {name}: No Collider component found!");
        }
        else
        {
            // Ana collider trigger OLMAMALI (karakter geçmesin)
            boxCollider.isTrigger = false;
        }

        // Trigger ile çarpışmaların çalışması için en az bir Rigidbody gerekli
        // Enemy'lerde olduğu gibi, LootBox'a kinematik bir Rigidbody ekleyelim
        boxRigidbody = GetComponent<Rigidbody>();
        if (boxRigidbody == null)
        {
            boxRigidbody = gameObject.AddComponent<Rigidbody>();
        }
        boxRigidbody.isKinematic = true;
        boxRigidbody.useGravity = false;
        
        // WeaponHitDetector için ayrı bir trigger collider oluştur
        // Unity'de OnTriggerEnter çalışması için: trigger-trigger veya trigger-rigidbody gerekir
        // Ana collider normal kalır (karakter geçmesin), trigger collider weapon hit için
        CreateWeaponHitTriggerCollider();
        
        currentHealth = maxHealth;
        
        // Animator'ı otomatik bul (kendisinde veya child'ında)
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }
        
        // Animator'ı başlangıçta devre dışı bırak (sadece vurulunca aktif olsun)
        if (animator != null)
        {
            animator.enabled = false;
            Debug.Log($"[LootBox] {name}: Animator disabled at start. Will be enabled when broken.");
        }
        
        // Mesh'leri başlangıç durumuna getir
        UpdateMeshState();
    }
    
    /// <summary>
    /// WeaponHitDetector için ayrı bir trigger collider oluşturur
    /// </summary>
    private void CreateWeaponHitTriggerCollider()
    {
        // Zaten bir trigger collider var mı kontrol et
        Collider[] allColliders = GetComponents<Collider>();
        foreach (Collider col in allColliders)
        {
            if (col != boxCollider && col.isTrigger)
            {
                // Zaten trigger collider var, gerek yok
                return;
            }
        }
        
        // Ana collider'ın tipine göre uygun bir trigger collider oluştur
        if (boxCollider != null)
        {
            Collider triggerCollider = null;
            
            if (boxCollider is BoxCollider)
            {
                BoxCollider boxTrigger = gameObject.AddComponent<BoxCollider>();
                BoxCollider originalBox = boxCollider as BoxCollider;
                boxTrigger.center = originalBox.center;
                // Trigger collider'ı biraz daha büyük yap (daha kolay vurma için)
                boxTrigger.size = originalBox.size * triggerColliderSizeMultiplier;
                triggerCollider = boxTrigger;
            }
            else if (boxCollider is SphereCollider)
            {
                SphereCollider sphereTrigger = gameObject.AddComponent<SphereCollider>();
                SphereCollider originalSphere = boxCollider as SphereCollider;
                sphereTrigger.center = originalSphere.center;
                // Trigger collider'ı biraz daha büyük yap
                sphereTrigger.radius = originalSphere.radius * triggerColliderSizeMultiplier;
                triggerCollider = sphereTrigger;
            }
            else if (boxCollider is CapsuleCollider)
            {
                CapsuleCollider capsuleTrigger = gameObject.AddComponent<CapsuleCollider>();
                CapsuleCollider originalCapsule = boxCollider as CapsuleCollider;
                capsuleTrigger.center = originalCapsule.center;
                // Trigger collider'ı biraz daha büyük yap
                capsuleTrigger.radius = originalCapsule.radius * triggerColliderSizeMultiplier;
                capsuleTrigger.height = originalCapsule.height * triggerColliderSizeMultiplier;
                capsuleTrigger.direction = originalCapsule.direction;
                triggerCollider = capsuleTrigger;
            }
            else
            {
                // Diğer collider tipleri için BoxCollider kullan
                BoxCollider boxTrigger = gameObject.AddComponent<BoxCollider>();
                boxTrigger.center = boxCollider.bounds.center - transform.position;
                // Trigger collider'ı biraz daha büyük yap
                boxTrigger.size = boxCollider.bounds.size * triggerColliderSizeMultiplier;
                triggerCollider = boxTrigger;
            }
            
            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
                Debug.Log($"[LootBox] {name}: Created weapon hit trigger collider ({triggerCollider.GetType().Name}).");
            }
        }
    }

    private void Start()
    {
        // Eğer tabeFinal prefab manuel atanmamışsa ve Resources'tan yüklenecekse
        if (tabeFinalPrefab == null && loadTabeFinalFromResources)
        {
            LoadTabeFinalFromResources();
        }
    }

    private void Update()
    {
        // DEBUG: K tuşuna basınca kutuyu hemen kır (test için)
        Keyboard keyboard = Keyboard.current;
        if (!isBroken && keyboard != null && keyboard.kKey.wasPressedThisFrame)
        {
            Debug.Log($"[LootBox] {name}: K key pressed - force break for debug.");
            TakeDamage(maxHealth);
        }
        
        // ALTERNATIF HIT DETECTION: Eğer WeaponHitDetector çalışmıyorsa, kendi hit detection'ımızı kullan
        // Bu, trigger collider sorunlarını çözer
        if (!isBroken && boxCollider != null)
        {
            // Yakındaki WeaponHitDetector'ları bul
            WeaponHitDetector[] weapons = FindObjectsByType<WeaponHitDetector>(FindObjectsSortMode.None);
            foreach (WeaponHitDetector weapon in weapons)
            {
                if (weapon != null)
                {
                    Collider weaponCol = weapon.GetComponent<Collider>();
                    if (weaponCol != null && weaponCol.bounds.Intersects(boxCollider.bounds))
                    {
                        // WeaponHitDetector'ın OnAttackPerformed event'ini dinle
                        // Ama bu zaten WeaponHitDetector'da yapılıyor, burada sadece kontrol ediyoruz
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// WeaponHitDetector'dan bağımsız olarak direkt hasar alabilir (fallback)
    /// </summary>
    public void ForceTakeDamage(int damageAmount)
    {
        if (!isBroken)
        {
            Debug.Log($"[LootBox] {name}: ForceTakeDamage called with {damageAmount} damage.");
            TakeDamage(damageAmount);
        }
    }
    
    private void LoadTabeFinalFromResources()
    {
        if (string.IsNullOrEmpty(tabeFinalResourcePath))
        {
            Debug.LogWarning($"[LootBox] {name}: tabeFinalResourcePath is empty. Cannot load from Resources.");
            return;
        }
        
        tabeFinalPrefab = Resources.Load<GameObject>(tabeFinalResourcePath);
        if (tabeFinalPrefab == null)
        {
            Debug.LogWarning($"[LootBox] {name}: Could not load tabeFinal prefab from Resources path: {tabeFinalResourcePath}");
        }
        else
        {
            Debug.Log($"[LootBox] {name}: Successfully loaded tabeFinal prefab from Resources.");
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (isBroken)
        {
            return;
        }
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        if (currentHealth <= 0)
        {
            Break();
        }
    }
    
    private void Break()
    {
        if (isBroken)
        {
            return;
        }
        
        isBroken = true;
        Debug.Log($"[LootBox] {name}: Box broken!");
        
        // ÖNEMLİ: Normal mesh'i kapatma - animasyon oynarken görünür kalmalı
        // Animasyon bitince DestroyBrokenPieces() tüm renderer'ları kapatacak
        // Normal mesh animasyonun bir parçası olabilir, bu yüzden animasyon bitene kadar açık kalmalı
        
        // Animasyon varsa oynat, bitince loot spawn et ve yok ol
        if (animator != null && !string.IsNullOrEmpty(breakTriggerName))
        {
            Debug.Log($"[LootBox] {name}: 🎬 Preparing to play break animation...");
            Debug.Log($"[LootBox] {name}: Animator GameObject Active: {animator.gameObject.activeSelf}, Animator Enabled: {animator.enabled}");
            
            // ÖNEMLİ: Animator'ın GameObject'i aktif mi kontrol et
            if (!animator.gameObject.activeSelf)
            {
                animator.gameObject.SetActive(true);
                Debug.Log($"[LootBox] {name}: ✅ Activated Animator GameObject.");
            }
            
            // ÖNEMLİ: Animator'ı aktif hale getir (başlangıçta devre dışıydı)
            if (!animator.enabled)
            {
                animator.enabled = true;
                Debug.Log($"[LootBox] {name}: ✅ Animator enabled for break animation.");
            }
            else
            {
                Debug.Log($"[LootBox] {name}: ✅ Animator already enabled.");
            }
            
            // Animator'ın gerçekten aktif olduğundan emin ol
            if (!animator.enabled)
            {
                Debug.LogError($"[LootBox] {name}: ❌ CRITICAL - Animator is still disabled! Forcing enable...");
                animator.enabled = true;
            }
            
            // Animasyon hızını ayarla (targetAnimationDuration'a göre)
            if (targetAnimationDuration > 0f)
            {
                // Animator yeni aktif olduğu için bir frame bekle, sonra speed'i ayarla
                StartCoroutine(SetAnimationSpeedAfterStart());
            }
            else if (animationSpeedMultiplier != 1f)
            {
                animator.speed = animationSpeedMultiplier;
                Debug.Log($"[LootBox] {name}: Animation speed set to: {animationSpeedMultiplier}");
            }
            
            // Bir frame bekle (animator'ın tam aktif olması için) ve animasyonu başlat
            StartCoroutine(StartAnimationAfterFrame());
        }
        else
        {
            if (animator == null)
            {
                Debug.LogWarning($"[LootBox] {name}: ⚠️ No Animator found! Spawning loot immediately without animation.");
            }
            if (string.IsNullOrEmpty(breakTriggerName))
            {
                Debug.LogWarning($"[LootBox] {name}: ⚠️ breakTriggerName is empty! Spawning loot immediately without animation.");
            }
            
            // Animasyon yoksa hemen loot spawn et ve yok ol
            FinishBreak();
        }
    }
    
    /// <summary>
    /// Bir frame bekleyip animasyonu başlatır (animator'ın tam aktif olması için)
    /// </summary>
    private System.Collections.IEnumerator StartAnimationAfterFrame()
    {
        // Bir frame bekle (animator'ın tam aktif olması için)
        yield return null;
        
        if (animator == null)
        {
            Debug.LogError($"[LootBox] {name}: ❌ CRITICAL - Animator is NULL in StartAnimationAfterFrame!");
            FinishBreak(); // Animasyon yoksa hemen bitir
            yield break;
        }
        
        // Animator'ın hala aktif olduğunu kontrol et
        if (!animator.enabled)
        {
            Debug.LogError($"[LootBox] {name}: ❌ CRITICAL - Animator was disabled! Re-enabling...");
            animator.enabled = true;
            yield return null; // Tekrar bir frame bekle
        }
        
        // Animator GameObject'inin aktif olduğunu kontrol et
        if (!animator.gameObject.activeSelf)
        {
            Debug.LogError($"[LootBox] {name}: ❌ CRITICAL - Animator GameObject is inactive! Activating...");
            animator.gameObject.SetActive(true);
            yield return null; // Tekrar bir frame bekle
        }
        
        // Trigger'ı set et
        if (animator != null && animator.enabled && !string.IsNullOrEmpty(breakTriggerName))
        {
            Debug.Log($"[LootBox] {name}: 🎬 Setting trigger '{breakTriggerName}' on Animator...");
            animator.SetTrigger(breakTriggerName);
            Debug.Log($"[LootBox] {name}: ✅ Break animation trigger '{breakTriggerName}' set successfully!");
            
            // Animasyonun gerçekten başladığını kontrol et (bir frame sonra)
            yield return null;
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"[LootBox] {name}: Animation state - NameHash: {stateInfo.fullPathHash}, NormalizedTime: {stateInfo.normalizedTime}, IsPlaying: {stateInfo.normalizedTime > 0}");
            
            // Animasyon bitince FinishBreak() çağrılacak
            StartCoroutine(WaitForAnimationAndFinish());
        }
        else
        {
            Debug.LogError($"[LootBox] {name}: ❌ CRITICAL - Cannot set trigger! Animator: {(animator != null ? "exists" : "null")}, Enabled: {(animator != null ? animator.enabled.ToString() : "N/A")}, TriggerName: {breakTriggerName}");
            FinishBreak(); // Animasyon başlatılamadıysa hemen bitir
        }
    }
    
    /// <summary>
    /// Animasyon bitince çağrılır (Animation Event ile veya coroutine ile) - Loot spawn eder ve parçaları yok eder
    /// </summary>
    private void FinishBreak()
    {
        if (isBroken)
        {
            Debug.Log($"[LootBox] {name}: ✅ FinishBreak() called - animation finished.");
            
            // Normal mesh zaten kapatıldı (Break() metodunda)
            
            // Loot spawn et (ÖNCE loot spawn et, sonra yok et)
            SpawnLoot();
            
            // Animasyondaki parçaları HEMEN yok et (loot hariç her şey)
            DestroyBrokenPieces();
            
            Debug.Log($"[LootBox] {name}: ✅ Break animation finished, loot spawned, broken pieces destroyed.");
        }
        else
        {
            Debug.LogWarning($"[LootBox] {name}: ⚠️ FinishBreak() called but isBroken is false!");
        }
    }
    
    /// <summary>
    /// Animasyondaki parçaları ve tüm görsel elementleri yok eder (sadece loot kalır)
    /// </summary>
    private void DestroyBrokenPieces()
    {
        Debug.Log($"[LootBox] {name}: 🗑️ Destroying broken pieces...");
        
        // ÖNEMLİ: Loot'ları korumak için, sadece LootBox'ın kendi renderer'larını kapat
        // Loot'lar zaten ayrı GameObject olarak spawn edildi, onlar etkilenmemeli
        
        // Sadece bu GameObject'in renderer'ını kapat (child'lardaki loot'ları etkilemez)
        Renderer[] renderers = GetComponents<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
        
        // Tüm child renderer'ları kapat (loot'lar hariç)
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null)
            {
                // Loot component'i olan GameObject'leri koru
                if (renderer.GetComponent<Loot>() == null && renderer.GetComponentInParent<Loot>() == null)
                {
                    renderer.enabled = false;
                }
            }
        }
        
        // Animator'ı kapat
        if (animator != null)
        {
            animator.enabled = false;
        }
        
        // Tüm collider'ları kapat (loot'lar hariç)
        Collider[] allColliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider col in allColliders)
        {
            if (col != null)
            {
                // Loot component'i olan GameObject'leri koru
                if (col.GetComponent<Loot>() == null && col.GetComponentInParent<Loot>() == null)
                {
                    col.enabled = false;
                }
            }
        }
        
        // Tüm rigidbody'leri kapat (loot'lar hariç)
        Rigidbody[] allRigidbodies = GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody rb in allRigidbodies)
        {
            if (rb != null)
            {
                // Loot component'i olan GameObject'leri koru
                if (rb.GetComponent<Loot>() == null && rb.GetComponentInParent<Loot>() == null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
            }
        }
        
        // Ana GameObject'i HEMEN yok et (loot'lar zaten ayrı GameObject olarak spawn edildi)
        Destroy(gameObject);
    }
    
    
    /// <summary>
    /// Animasyon başladıktan sonra speed'ini ayarlar
    /// </summary>
    private System.Collections.IEnumerator SetAnimationSpeedAfterStart()
    {
        // Animasyon başlayana kadar bekle
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.length > 0f && targetAnimationDuration > 0f)
            {
                float targetSpeed = stateInfo.length / targetAnimationDuration;
                animator.speed = targetSpeed * animationSpeedMultiplier;
                Debug.Log($"[LootBox] {name}: Animation speed set to {animator.speed}x (original length: {stateInfo.length}s, target: {targetAnimationDuration}s)");
            }
        }
    }
    
    /// <summary>
    /// Sabit bir süre (targetAnimationDuration) bekleyip FinishBreak() çağırır.
    /// Animasyon devam ediyor olsa bile kutu bu sürenin sonunda yok edilir.
    /// </summary>
    /// <summary>
    /// Animasyonun gerçekten bitmesini bekler (normalizedTime >= 1.0), sonra FinishBreak() çağırır
    /// BASIT VERSİYON: Animasyon bitince direkt yok et
    /// </summary>
    private System.Collections.IEnumerator WaitForAnimationAndFinish()
    {
        if (animator == null)
        {
            Debug.LogWarning($"[LootBox] {name}: ⚠️ Animator is null, calling FinishBreak() immediately.");
            FinishBreak();
            yield break;
        }
        
        Debug.Log($"[LootBox] {name}: ⏳ Waiting for animation to finish...");
        
        // Animasyonun başlamasını bekle (2 frame)
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // Animasyon state'ini bul ve bitmesini bekle
        float maxWaitTime = targetAnimationDuration > 0f ? targetAnimationDuration * 3f : 3f; // Maksimum bekleme süresi
        float elapsedTime = 0f;
        bool animationFinished = false;
        
        while (elapsedTime < maxWaitTime && !animationFinished)
        {
            if (animator == null || !animator.enabled || !animator.gameObject.activeSelf)
            {
                Debug.Log($"[LootBox] {name}: ⚠️ Animator disabled or null, finishing break.");
                break;
            }
            
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // Animasyon bitmiş mi kontrol et (normalizedTime >= 1.0)
            if (stateInfo.normalizedTime >= 1.0f && stateInfo.normalizedTime > 0f)
            {
                Debug.Log($"[LootBox] {name}: ✅ Animation finished! (normalizedTime: {stateInfo.normalizedTime:F2})");
                animationFinished = true;
                break;
            }
            
            // Her frame kontrol et
            yield return null;
            elapsedTime += Time.deltaTime;
        }
        
        // Animasyon bitti veya maksimum süre doldu
        if (!animationFinished && elapsedTime >= maxWaitTime)
        {
            Debug.LogWarning($"[LootBox] {name}: ⚠️ Max wait time ({maxWaitTime}s) reached, finishing break anyway.");
        }
        
        Debug.Log($"[LootBox] {name}: ⏰ Calling FinishBreak() - destroying all pieces except loot...");
        
        if (isBroken && gameObject != null)
        {
            FinishBreak();
        }
        else
        {
            Debug.LogWarning($"[LootBox] {name}: ⚠️ Cannot call FinishBreak() - isBroken: {isBroken}, gameObject: {(gameObject != null ? "exists" : "null")}");
        }
    }
    
    /// <summary>
    /// Animation Event'ten çağrılabilir (animasyon clip'inin son frame'ine ekle)
    /// </summary>
    public void OnBreakAnimationEnd()
    {
        Debug.Log($"[LootBox] {name}: 🎬 OnBreakAnimationEnd() called from Animation Event.");
        if (isBroken && gameObject != null)
        {
            FinishBreak();
        }
        else
        {
            Debug.LogWarning($"[LootBox] {name}: ⚠️ OnBreakAnimationEnd() called but isBroken: {isBroken}, gameObject: {(gameObject != null ? "exists" : "null")}");
        }
    }
    
    private void UpdateMeshState()
    {
        // Sadece başlangıç durumu için (animasyon kullanılıyorsa bu metod kullanılmaz)
        if (normalMesh != null)
        {
            normalMesh.SetActive(!isBroken);
        }
        
        if (brokenMesh != null)
        {
            brokenMesh.SetActive(isBroken);
        }
    }
    
    private void SpawnLoot()
    {
        Debug.Log($"[LootBox] {name}: 🎯 Starting loot spawn process...");
        
        // Önce %5 şansla tabeFinal spawn et
        bool tabeFinalAttempted = Random.Range(0f, 100f) <= tabeFinalSpawnChance;
        if (tabeFinalAttempted)
        {
            bool tabeFinalSpawned = SpawnTabeFinal();
            if (tabeFinalSpawned)
            {
                Debug.Log($"[LootBox] {name}: ✅ tabeFinal spawned successfully. Skipping normal loot.");
                return; // tabeFinal çıktıysa normal loot çıkmasın
            }
            else
            {
                Debug.LogWarning($"[LootBox] {name}: ⚠️ tabeFinal spawn failed (prefab null or instantiate failed). Falling back to normal loot.");
                // tabeFinal spawn başarısız oldu, normal loot'a geç
            }
        }
        
        // Normal loot spawn şansı kontrolü
        if (!alwaysSpawnLoot)
        {
            float lootRoll = Random.Range(0f, 100f);
            if (lootRoll > lootSpawnChance)
            {
                Debug.LogWarning($"[LootBox] {name}: ❌ No loot spawned (chance failed: rolled {lootRoll:F2}%, required {lootSpawnChance}%).");
                return;
            }
        }
        else
        {
            Debug.Log($"[LootBox] {name}: ✅ Always spawn loot enabled - skipping chance check.");
        }
        
        // Template kontrolü - eğer template yoksa, loot spawn edemeyiz
        GameObject testTemplate = GetRandomLootTemplate();
        if (testTemplate == null)
        {
            Debug.LogError($"[LootBox] {name}: ❌ CRITICAL - Cannot spawn loot - no loot templates available! Please assign loot templates in Inspector or ensure Thresh_Spawner has templates.");
            // Template yoksa bile en az bir şey spawn etmeye çalış (fallback)
            TrySpawnFallbackLoot();
            return;
        }
        
        // Tier bazlı loot seç
        LootRarity selectedRarity = SelectRarityByWeight();
        RaritySpawnSettings settings = GetRaritySettings(selectedRarity);
        
        if (settings == null)
        {
            Debug.LogError($"[LootBox] {name}: ❌ No settings found for rarity {selectedRarity}. Cannot spawn loot!");
            TrySpawnFallbackLoot();
            return;
        }
        
        // Spawn sayısını belirle (en az 1 olmalı)
        int spawnCount = Random.Range(settings.minSpawnCount, settings.maxSpawnCount + 1);
        spawnCount = Mathf.Max(1, spawnCount); // En az 1 loot garantile
        
        Debug.Log($"[LootBox] {name}: 🎲 Attempting to spawn {spawnCount} loot item(s) of rarity {selectedRarity} (Value: {settings.scrapValue} each).");
        
        int successfulSpawns = 0;
        int maxRetries = 3; // Her item için maksimum 3 deneme
        
        for (int i = 0; i < spawnCount; i++)
        {
            bool spawned = false;
            for (int retry = 0; retry < maxRetries && !spawned; retry++)
            {
                if (SpawnLootItem(selectedRarity, settings))
                {
                    successfulSpawns++;
                    spawned = true;
                }
                else
                {
                    Debug.LogWarning($"[LootBox] {name}: ⚠️ Failed to spawn loot item {i + 1}/{spawnCount}, retry {retry + 1}/{maxRetries}...");
                }
            }
            
            if (!spawned)
            {
                Debug.LogError($"[LootBox] {name}: ❌ Failed to spawn loot item {i + 1}/{spawnCount} after {maxRetries} retries!");
            }
        }
        
        if (successfulSpawns == 0)
        {
            Debug.LogError($"[LootBox] {name}: ❌ CRITICAL - Failed to spawn any loot items! Trying fallback...");
            TrySpawnFallbackLoot();
        }
        else
        {
            Debug.Log($"[LootBox] {name}: ✅ Successfully spawned {successfulSpawns}/{spawnCount} loot item(s).");
        }
    }
    
    /// <summary>
    /// Fallback loot spawn - template bulunamazsa veya spawn başarısız olursa çağrılır
    /// </summary>
    private void TrySpawnFallbackLoot()
    {
        Debug.Log($"[LootBox] {name}: 🔄 Attempting fallback loot spawn...");
        
        // Thresh_Spawner'dan template almayı dene
        Thresh_Spawner spawner = FindObjectOfType<Thresh_Spawner>();
        if (spawner != null)
        {
            GameObject fallbackTemplate = spawner.GetRandomLootTemplate();
            if (fallbackTemplate != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
                GameObject instance = Instantiate(fallbackTemplate, spawnPos, Quaternion.identity);
                if (instance != null)
                {
                    instance.transform.SetParent(null);
                    instance.SetActive(true);
                    Debug.Log($"[LootBox] {name}: ✅ Fallback loot spawned: {instance.name}");
                    return;
                }
            }
        }
        
        Debug.LogError($"[LootBox] {name}: ❌ Fallback loot spawn also failed! No loot will spawn.");
    }
    
    private bool SpawnTabeFinal()
    {
        if (tabeFinalPrefab == null)
        {
            Debug.LogWarning($"[LootBox] {name}: tabeFinalPrefab is null! Cannot spawn tabeFinal.");
            return false;
        }
        
        Vector3 spawnPosition = GetRandomSpawnPosition();
        Quaternion spawnRotation = Quaternion.identity;
        
        GameObject instance = Instantiate(tabeFinalPrefab, spawnPosition, spawnRotation);
        if (instance == null)
        {
            Debug.LogError($"[LootBox] {name}: Failed to instantiate tabeFinal prefab!");
            return false;
        }
        
        // ÖNEMLİ: tabeFinal'i LootBox'ın child'ı yapma, ayrı bir GameObject olarak bırak
        instance.transform.SetParent(null);
        instance.SetActive(true);
        Debug.Log($"[LootBox] {name}: Spawned tabeFinal at position {spawnPosition}!");
        return true;
    }
    
    private bool SpawnLootItem(LootRarity rarity, RaritySpawnSettings settings)
    {
        // Loot template seç
        GameObject template = GetRandomLootTemplate();
        if (template == null)
        {
            Debug.LogError($"[LootBox] {name}: No loot template available! Cannot spawn loot item.");
            return false;
        }
        
        Vector3 spawnPosition = GetRandomSpawnPosition();
        Quaternion spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        
        GameObject instance = Instantiate(template, spawnPosition, spawnRotation);
        if (instance == null)
        {
            Debug.LogError($"[LootBox] {name}: Failed to instantiate loot template '{template.name}'!");
            return false;
        }
        
        // ÖNEMLİ: Loot'u LootBox'ın child'ı yapma, ayrı bir GameObject olarak bırak
        // Böylece DestroyBrokenPieces() loot'u etkilemez
        instance.transform.SetParent(null);
        
        // Loot component'ini bul ve ayarla (önce component'i bul, sonra aktif et)
        Loot lootComponent = instance.GetComponent<Loot>();
        if (lootComponent == null)
        {
            // GetComponentInChildren ile child'larda da ara
            lootComponent = instance.GetComponentInChildren<Loot>();
        }
        
        if (lootComponent != null)
        {
            lootComponent.SetRarity(rarity);
            lootComponent.SetScrapValue(settings.scrapValue);
            
            string newItemId = $"scrap_tier{(int)rarity + 1}_value{settings.scrapValue}";
            lootComponent.SetItemId(newItemId);
            
            string newDisplayName = $"Tier {(int)rarity + 1} Scrap ({settings.scrapValue})";
            lootComponent.SetItemDisplayName(newDisplayName);
        }
        else
        {
            Debug.LogWarning($"[LootBox] {name}: Spawned loot item '{instance.name}' but it has no Loot component! Adding one...");
            // Loot component yoksa ekle
            lootComponent = instance.AddComponent<Loot>();
            if (lootComponent != null)
            {
                lootComponent.SetRarity(rarity);
                lootComponent.SetScrapValue(settings.scrapValue);
                string newItemId = $"scrap_tier{(int)rarity + 1}_value{settings.scrapValue}";
                lootComponent.SetItemId(newItemId);
                string newDisplayName = $"Tier {(int)rarity + 1} Scrap ({settings.scrapValue})";
                lootComponent.SetItemDisplayName(newDisplayName);
            }
        }
        
        // Son olarak aktif et
        instance.SetActive(true);
        
        Debug.Log($"[LootBox] {name}: ✅ Spawned {rarity} loot '{instance.name}' (Value: {settings.scrapValue}) at position {spawnPosition}!");
        
        return true;
    }
    
    private GameObject GetRandomLootTemplate()
    {
        // Önce manuel atanmış template'leri kontrol et
        if (lootTemplates != null && lootTemplates.Count > 0)
        {
            List<GameObject> validTemplates = new List<GameObject>();
            foreach (GameObject template in lootTemplates)
            {
                if (template != null)
                {
                    validTemplates.Add(template);
                }
            }
            
            if (validTemplates.Count > 0)
            {
                GameObject selected = validTemplates[Random.Range(0, validTemplates.Count)];
                Debug.Log($"[LootBox] {name}: Selected template from manual list: {selected.name} ({validTemplates.Count} total templates).");
                return selected;
            }
            else
            {
                Debug.LogWarning($"[LootBox] {name}: Manual loot templates list is empty or all templates are null.");
            }
        }
        else
        {
            Debug.LogWarning($"[LootBox] {name}: No manual loot templates assigned in Inspector.");
        }
        
        // Eğer manuel template yoksa, Thresh_Spawner'daki template'leri kullan
        Thresh_Spawner spawner = FindObjectOfType<Thresh_Spawner>();
        if (spawner != null)
        {
            GameObject template = spawner.GetRandomLootTemplate();
            if (template != null)
            {
                Debug.Log($"[LootBox] {name}: Using template from Thresh_Spawner: {template.name}");
                return template;
            }
            else
            {
                Debug.LogWarning($"[LootBox] {name}: Thresh_Spawner found but has no valid templates.");
            }
        }
        else
        {
            Debug.LogWarning($"[LootBox] {name}: No Thresh_Spawner found in scene.");
        }
        
        Debug.LogError($"[LootBox] {name}: ❌ No loot templates available! Please assign loot templates in the Inspector or ensure Thresh_Spawner has templates.");
        return null;
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 spawnPosition;
        
        // Manuel spawn point'ler kullanılıyorsa
        if (useManualSpawnPoints && manualSpawnPoints != null && manualSpawnPoints.Length > 0)
        {
            // Geçerli spawn point'leri filtrele
            List<Transform> validSpawnPoints = new List<Transform>();
            foreach (Transform spawnPoint in manualSpawnPoints)
            {
                if (spawnPoint != null)
                {
                    validSpawnPoints.Add(spawnPoint);
                }
            }
            
            if (validSpawnPoints.Count > 0)
            {
                // Rastgele bir spawn point seç
                Transform selectedPoint = validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
                spawnPosition = selectedPoint.position;
            }
            else
            {
                // Fallback: otomatik spawn
                Vector3 offset = Random.insideUnitSphere;
                offset.y = 0f;
                if (offset.sqrMagnitude > 0.001f)
                {
                    offset.Normalize();
                }
                offset *= Random.Range(0f, spawnRadius);
                spawnPosition = transform.position + new Vector3(offset.x, 0f, offset.z);
            }
        }
        else
        {
            // Otomatik spawn (kutunun etrafında rastgele)
            Vector3 offset = Random.insideUnitSphere;
            offset.y = 0f;
            if (offset.sqrMagnitude > 0.001f)
            {
                offset.Normalize();
            }
            offset *= Random.Range(0f, spawnRadius);
            spawnPosition = transform.position + new Vector3(offset.x, 0f, offset.z);
        }
        
        // Yere raycast ile yerleştir (spawnOnGround aktifse)
        if (spawnOnGround)
        {
            RaycastHit hit;
            // Daha geniş bir raycast yap (yukarıdan ve biraz daha yüksekten)
            Vector3 rayStart = spawnPosition + Vector3.up * 15f; // Yukarıdan raycast at
            
            // Layer mask: sadece ground layer'larına bak (varsayılan: tüm layer'lar)
            int layerMask = ~0; // Tüm layer'lar
            
            // Raycast'i daha uzun mesafeye çıkar ve birden fazla deneme yap
            if (Physics.Raycast(rayStart, Vector3.down, out hit, 30f, layerMask, QueryTriggerInteraction.Ignore))
            {
                spawnPosition = hit.point + Vector3.up * 0.1f; // Yere biraz yukarıdan yerleştir (çakışma önleme)
                Debug.Log($"[LootBox] {name}: Spawn position adjusted to ground: {spawnPosition} (hit: {hit.collider.name})");
            }
            else
            {
                // Raycast başarısız olursa, LootBox'ın pozisyonunu kullan
                Debug.LogWarning($"[LootBox] {name}: Raycast failed, using LootBox position as fallback. Original: {spawnPosition}");
                spawnPosition = transform.position + Vector3.up * 0.1f; // LootBox'ın pozisyonunu kullan
            }
        }
        else
        {
            spawnPosition.y += spawnHeightOffset;
        }
        
        return spawnPosition;
    }
    
    private LootRarity SelectRarityByWeight()
    {
        float totalWeight = 0f;
        foreach (RaritySpawnSettings settings in tierLootSettings)
        {
            totalWeight += settings.spawnWeight;
        }
        
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (RaritySpawnSettings settings in tierLootSettings)
        {
            currentWeight += settings.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return settings.rarity;
            }
        }
        
        return LootRarity.Tier1;
    }
    
    private RaritySpawnSettings GetRaritySettings(LootRarity rarity)
    {
        foreach (RaritySpawnSettings settings in tierLootSettings)
        {
            if (settings.rarity == rarity)
            {
                return settings;
            }
        }
        return null;
    }
    
}
