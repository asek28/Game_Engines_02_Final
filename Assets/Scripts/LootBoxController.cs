using UnityEngine;
using System.Collections;

/// <summary>
/// Basit LootBox Controller - Solid mesh'i kapatır, animasyonu oynatır, loot spawn eder ve yok olur
/// </summary>
public class LootBoxController : MonoBehaviour
{
    [Header("Mesh References")]
    [Tooltip("Solid (sağlam) kutu mesh renderer'ı - açılınca disable edilecek")]
    [SerializeField] private Renderer solidMesh;
    
    [Tooltip("kutu_anim child GameObject'i - animasyon/particle'lar için (başlangıçta inactive olmalı)")]
    [SerializeField] private GameObject kutuAnim;
    
    [Header("Loot Settings")]
    [Tooltip("Spawn edilecek Loot Prefab (World Space'de spawn edilecek, parent = null)")]
    [SerializeField] private GameObject lootPrefab;
    
    [Header("Animation Settings")]
    [Tooltip("Animasyon süresi (saniye) - bu süre sonunda loot spawn edilip kutu yok olur")]
    [SerializeField, Min(0.1f)] private float animationDuration = 1f;
    
    private bool isOpened = false;
    
    private void Awake()
    {
        // Solid mesh renderer'ı otomatik bul (eğer manuel atanmamışsa)
        if (solidMesh == null)
        {
            solidMesh = GetComponent<Renderer>();
            if (solidMesh == null)
            {
                // Child'larda ara
                solidMesh = GetComponentInChildren<Renderer>();
            }
        }
        
        // kutu_anim'i otomatik bul (eğer manuel atanmamışsa)
        if (kutuAnim == null)
        {
            Transform[] children = GetComponentsInChildren<Transform>(true); // inactive child'ları da dahil et
            foreach (Transform child in children)
            {
                if (child.name.ToLower().Contains("kutu_anim") || child.name.ToLower().Contains("kutuanim"))
                {
                    kutuAnim = child.gameObject;
                    Debug.Log($"[LootBoxController] {name}: Found kutu_anim automatically: {child.name}");
                    break;
                }
            }
        }
        
        // kutu_anim'in başlangıçta inactive olduğundan emin ol
        if (kutuAnim != null && kutuAnim.activeSelf)
        {
            kutuAnim.SetActive(false);
            Debug.Log($"[LootBoxController] {name}: kutu_anim was active, disabled it.");
        }
        
        // Validasyon
        if (solidMesh == null)
        {
            Debug.LogWarning($"[LootBoxController] {name}: ⚠️ Solid Mesh Renderer not found! Please assign it in Inspector.");
        }
        
        if (kutuAnim == null)
        {
            Debug.LogWarning($"[LootBoxController] {name}: ⚠️ kutu_anim GameObject not found! Please assign it in Inspector or ensure a child GameObject named 'kutu_anim' exists.");
        }
        
        if (lootPrefab == null)
        {
            Debug.LogWarning($"[LootBoxController] {name}: ⚠️ Loot Prefab not assigned! Please assign it in Inspector.");
        }
    }
    
    /// <summary>
    /// Kutu açılır: Solid mesh kapanır, animasyon oynar, loot spawn edilir, kutu yok olur
    /// </summary>
    public void OpenBox()
    {
        if (isOpened)
        {
            Debug.LogWarning($"[LootBoxController] {name}: Box already opened!");
            return;
        }
        
        isOpened = true;
        Debug.Log($"[LootBoxController] {name}: Opening box...");
        
        // 1. Solid mesh'i HEMEN kapat
        if (solidMesh != null)
        {
            solidMesh.enabled = false;
            Debug.Log($"[LootBoxController] {name}: ✅ Solid mesh disabled.");
        }
        else
        {
            Debug.LogWarning($"[LootBoxController] {name}: ⚠️ Solid mesh is null, cannot disable.");
        }
        
        // 2. kutu_anim'i HEMEN aktif et ve parent'tan ayır (böylece parent yok olunca animasyon devam eder)
        if (kutuAnim != null)
        {
            kutuAnim.SetActive(true);
            
            // ÖNEMLİ: kutu_anim'i parent'tan ayır (World Space'de bağımsız olarak devam etsin)
            kutuAnim.transform.SetParent(null);
            Debug.Log($"[LootBoxController] {name}: ✅ kutu_anim activated and detached from parent (now independent).");
        }
        else
        {
            Debug.LogWarning($"[LootBoxController] {name}: ⚠️ kutu_anim is null, cannot activate.");
        }
        
        // 3. Animasyon süresini bekle, sonra loot spawn et ve yok ol
        StartCoroutine(WaitForAnimationAndSpawnLoot());
    }
    
    /// <summary>
    /// Hasar alınca kutu açılır (WeaponHitDetector ile entegrasyon için)
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (!isOpened)
        {
            OpenBox();
        }
    }
    
    /// <summary>
    /// Animasyon süresini bekler, sonra loot spawn eder ve kutu yok olur
    /// </summary>
    private IEnumerator WaitForAnimationAndSpawnLoot()
    {
        Debug.Log($"[LootBoxController] {name}: ⏳ Waiting {animationDuration}s for animation...");
        
        // Animasyon süresini bekle
        yield return new WaitForSeconds(animationDuration);
        
        Debug.Log($"[LootBoxController] {name}: ⏰ Animation finished, spawning loot...");
        
        // 4. Loot spawn et (World Space, parent = null)
        if (lootPrefab != null)
        {
            Vector3 spawnPosition = transform.position;
            
            // Loot'u World Space'de spawn et (parent = null)
            GameObject lootInstance = Instantiate(lootPrefab, spawnPosition, Quaternion.identity, null);
            
            if (lootInstance != null)
            {
                lootInstance.SetActive(true);
                Debug.Log($"[LootBoxController] {name}: ✅ Loot spawned at position {spawnPosition} (World Space, parent = null).");
            }
            else
            {
                Debug.LogError($"[LootBoxController] {name}: ❌ Failed to instantiate loot prefab!");
            }
        }
        else
        {
            Debug.LogWarning($"[LootBoxController] {name}: ⚠️ Loot Prefab is null, cannot spawn loot.");
        }
        
        // 5. Kutu GameObject'ini yok et (kutu_anim zaten parent'tan ayrıldı, o devam edecek)
        Debug.Log($"[LootBoxController] {name}: 🗑️ Destroying LootBox GameObject (kutu_anim will continue independently)...");
        Destroy(gameObject);
        
        // 6. kutu_anim'i de animasyon bitince yok et (eğer hala varsa)
        if (kutuAnim != null)
        {
            StartCoroutine(DestroyKutuAnimAfterDelay());
        }
    }
    
    /// <summary>
    /// kutu_anim'i animasyon bitince yok eder
    /// </summary>
    private IEnumerator DestroyKutuAnimAfterDelay()
    {
        // Kısa bir süre bekle (animasyonun tamamen bitmesi için)
        yield return new WaitForSeconds(0.5f);
        
        if (kutuAnim != null)
        {
            Debug.Log($"[LootBoxController] {name}: 🗑️ Destroying kutu_anim GameObject...");
            Destroy(kutuAnim);
        }
    }
    
    /// <summary>
    /// DEBUG: K tuşuna basınca kutu aç (test için)
    /// </summary>
    private void Update()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (UnityEngine.InputSystem.Keyboard.current != null && 
            UnityEngine.InputSystem.Keyboard.current.kKey.wasPressedThisFrame)
        {
            if (!isOpened)
            {
                Debug.Log($"[LootBoxController] {name}: K key pressed - opening box for debug.");
                OpenBox();
            }
        }
        #endif
    }
}
