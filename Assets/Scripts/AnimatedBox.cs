using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Animasyonlu kutu script'i - Vurunca animasyon oynar, bitince yok olur (LootBox gibi)
/// </summary>
[RequireComponent(typeof(Collider))]
public class AnimatedBox : MonoBehaviour
{
    [Header("Box Settings")]
    [Tooltip("Kutunun sağlığı (genellikle 1 - tek vuruşta kırılır)")]
    [SerializeField, Min(1)] private int maxHealth = 1;
    
    [Header("Animation Settings")]
    [Tooltip("Animator component (kırılma animasyonu için)")]
    [SerializeField] private Animator animator;
    
    [Tooltip("Kırılma animasyonu trigger adı (Animator Controller'da tanımlı olmalı)")]
    [SerializeField] private string breakTriggerName = "Hit";
    
    [Tooltip("Animasyonun hedef süresi (saniye) - animasyon bu süreye göre otomatik hızlandırılır")]
    [SerializeField, Min(0.1f)] private float targetAnimationDuration = 1.5f;
    
    [Tooltip("Animasyon süresini kısaltmak için speed multiplier (örn: 2.0 = 2x hızlı)")]
    [SerializeField, Min(0.1f)] private float animationSpeedMultiplier = 1f;
    
    [Tooltip("Animasyon bitince ne kadar süre sonra yok olsun (saniye) - 0 = hemen yok ol")]
    [SerializeField, Min(0f)] private float destroyDelayAfterAnimation = 0f;
    
    [Header("Mesh Settings")]
    [Tooltip("Normal (sağlam) kutu mesh'i - animasyon başlamadan önce kapatılacak (opsiyonel)")]
    [SerializeField] private GameObject normalMesh;
    
    [Tooltip("kutu_anim mesh'i - vuruştan 1 saniye sonra disable olacak (otomatik bulunur veya manuel atanabilir)")]
    [SerializeField] private GameObject kutuAnimMesh;
    
    [Tooltip("kutu_anim mesh'ini vuruştan kaç saniye sonra disable et")]
    [SerializeField, Min(0f)] private float disableKutuAnimDelay = 1f;
    
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
            Debug.LogError($"[AnimatedBox] {name}: No Collider component found!");
        }
        else
        {
            // Ana collider trigger OLMAMALI (karakter geçmesin)
            boxCollider.isTrigger = false;
        }

        // Trigger ile çarpışmaların çalışması için en az bir Rigidbody gerekli
        boxRigidbody = GetComponent<Rigidbody>();
        if (boxRigidbody == null)
        {
            boxRigidbody = gameObject.AddComponent<Rigidbody>();
        }
        boxRigidbody.isKinematic = true;
        boxRigidbody.useGravity = false;
        
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
            Debug.Log($"[AnimatedBox] {name}: Animator disabled at start. Will be enabled when broken.");
        }
        
        // kutu_anim mesh'ini otomatik bul (eğer manuel atanmamışsa)
        if (kutuAnimMesh == null)
        {
            // Önce child'larda ara
            Transform[] children = GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name.ToLower().Contains("kutu_anim"))
                {
                    kutuAnimMesh = child.gameObject;
                    Debug.Log($"[AnimatedBox] {name}: Found kutu_anim mesh automatically: {child.name}");
                    break;
                }
            }
            
            // Bulunamazsa, tüm child'larda "anim" içeren mesh'leri ara
            if (kutuAnimMesh == null)
            {
                foreach (Transform child in children)
                {
                    if (child.name.ToLower().Contains("anim"))
                    {
                        kutuAnimMesh = child.gameObject;
                        Debug.Log($"[AnimatedBox] {name}: Found animation mesh automatically: {child.name}");
                        break;
                    }
                }
            }
        }
    }

    private void Update()
    {
        // DEBUG: K tuşuna basınca kutuyu hemen kır (test için)
        Keyboard keyboard = Keyboard.current;
        if (!isBroken && keyboard != null && keyboard.kKey.wasPressedThisFrame)
        {
            Debug.Log($"[AnimatedBox] {name}: K key pressed - force break for debug.");
            TakeDamage(maxHealth);
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
        Debug.Log($"[AnimatedBox] {name}: Box broken!");
        
        // Normal mesh'i HEMEN kapat (animasyon başlamadan önce)
        if (normalMesh != null)
        {
            normalMesh.SetActive(false);
            Debug.Log($"[AnimatedBox] {name}: Normal mesh disabled before animation.");
        }
        
        // kutu_anim mesh'ini vuruştan 1 saniye sonra disable et
        if (kutuAnimMesh != null)
        {
            StartCoroutine(DisableKutuAnimAfterDelay());
        }
        else
        {
            Debug.LogWarning($"[AnimatedBox] {name}: kutu_anim mesh not found! Please assign it in Inspector or ensure a child GameObject named 'kutu_anim' exists.");
        }
        
        // Animasyon varsa oynat
        if (animator != null && !string.IsNullOrEmpty(breakTriggerName))
        {
            Debug.Log($"[AnimatedBox] {name}: 🎬 Preparing to play break animation...");
            Debug.Log($"[AnimatedBox] {name}: Animator GameObject Active: {animator.gameObject.activeSelf}, Animator Enabled: {animator.enabled}");
            
            // ÖNEMLİ: Animator'ın GameObject'i aktif mi kontrol et
            if (!animator.gameObject.activeSelf)
            {
                animator.gameObject.SetActive(true);
                Debug.Log($"[AnimatedBox] {name}: ✅ Activated Animator GameObject.");
            }
            
            // ÖNEMLİ: Animator'ı aktif hale getir (başlangıçta devre dışıydı)
            if (!animator.enabled)
            {
                animator.enabled = true;
                Debug.Log($"[AnimatedBox] {name}: ✅ Animator enabled for break animation.");
            }
            else
            {
                Debug.Log($"[AnimatedBox] {name}: ✅ Animator already enabled.");
            }
            
            // Animator'ın gerçekten aktif olduğundan emin ol
            if (!animator.enabled)
            {
                Debug.LogError($"[AnimatedBox] {name}: ❌ CRITICAL - Animator is still disabled! Forcing enable...");
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
                Debug.Log($"[AnimatedBox] {name}: Animation speed set to: {animationSpeedMultiplier}");
            }
            
            // Bir frame bekle (animator'ın tam aktif olması için) ve animasyonu başlat
            StartCoroutine(StartAnimationAfterFrame());
        }
        else
        {
            if (animator == null)
            {
                Debug.LogError($"[AnimatedBox] {name}: ❌ CRITICAL - Animator is NULL!");
            }
            if (string.IsNullOrEmpty(breakTriggerName))
            {
                Debug.LogError($"[AnimatedBox] {name}: ❌ CRITICAL - breakTriggerName is empty!");
            }
            
            // Animasyon yoksa hemen yok et
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
            Debug.LogError($"[AnimatedBox] {name}: ❌ CRITICAL - Animator is NULL in StartAnimationAfterFrame!");
            yield break;
        }
        
        // Animator'ın hala aktif olduğunu kontrol et
        if (!animator.enabled)
        {
            Debug.LogError($"[AnimatedBox] {name}: ❌ CRITICAL - Animator was disabled! Re-enabling...");
            animator.enabled = true;
            yield return null; // Tekrar bir frame bekle
        }
        
        // Animator GameObject'inin aktif olduğunu kontrol et
        if (!animator.gameObject.activeSelf)
        {
            Debug.LogError($"[AnimatedBox] {name}: ❌ CRITICAL - Animator GameObject is inactive! Activating...");
            animator.gameObject.SetActive(true);
            yield return null; // Tekrar bir frame bekle
        }
        
        // Trigger'ı set et
        if (animator != null && animator.enabled && !string.IsNullOrEmpty(breakTriggerName))
        {
            Debug.Log($"[AnimatedBox] {name}: 🎬 Setting trigger '{breakTriggerName}' on Animator...");
            animator.SetTrigger(breakTriggerName);
            Debug.Log($"[AnimatedBox] {name}: ✅ Break animation trigger '{breakTriggerName}' set successfully!");
            
            // Animasyonun gerçekten başladığını kontrol et (bir frame sonra)
            yield return null;
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"[AnimatedBox] {name}: Animation state - NameHash: {stateInfo.fullPathHash}, NormalizedTime: {stateInfo.normalizedTime}, IsPlaying: {stateInfo.normalizedTime > 0}");
            
            // Animasyon bitince FinishBreak() çağrılacak
            StartCoroutine(WaitForAnimationAndFinish());
        }
        else
        {
            Debug.LogError($"[AnimatedBox] {name}: ❌ CRITICAL - Cannot set trigger! Animator: {(animator != null ? "exists" : "null")}, Enabled: {(animator != null ? animator.enabled.ToString() : "N/A")}, TriggerName: {breakTriggerName}");
        }
    }
    
    /// <summary>
    /// Animasyon bitince çağrılır - GameObject'i yok eder
    /// </summary>
    private void FinishBreak()
    {
        // Animasyondaki parçaları HEMEN yok et
        DestroyBrokenPieces();
        
        Debug.Log($"[AnimatedBox] {name}: Break animation finished, box destroyed.");
    }
    
    /// <summary>
    /// Animasyondaki parçaları ve tüm görsel elementleri yok eder
    /// </summary>
    private void DestroyBrokenPieces()
    {
        // Tüm renderer'ları kapat (görünmez yap)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
        
        // Animator'ı kapat
        if (animator != null)
        {
            animator.enabled = false;
        }
        
        // Collider'ları kapat
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }
        
        // Rigidbody'yi kapat
        if (boxRigidbody != null)
        {
            boxRigidbody.isKinematic = true;
            boxRigidbody.useGravity = false;
        }
        
        // Ana GameObject'i yok et
        StartCoroutine(DestroyGameObjectAfterFrame());
    }
    
    /// <summary>
    /// Kısa bir süre sonra GameObject'i tamamen yok eder
    /// </summary>
    private System.Collections.IEnumerator DestroyGameObjectAfterFrame()
    {
        // destroyDelayAfterAnimation kadar bekle (varsayılan 0 = hemen yok ol)
        if (destroyDelayAfterAnimation > 0f)
        {
            yield return new WaitForSeconds(destroyDelayAfterAnimation);
        }
        else
        {
            // Hemen yok olmak için sadece bir frame bekle
            yield return null;
        }
        
        // GameObject hala varsa yok et
        if (gameObject != null)
        {
            Debug.Log($"[AnimatedBox] {name}: 🗑️ Destroying GameObject completely.");
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning($"[AnimatedBox] {name}: GameObject already destroyed!");
        }
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
                Debug.Log($"[AnimatedBox] {name}: Animation speed set to {animator.speed}x (original length: {stateInfo.length}s, target: {targetAnimationDuration}s)");
            }
        }
    }
    
    /// <summary>
    /// Animasyonun gerçekten bitmesini bekleyip FinishBreak() çağırır.
    /// Animasyon state'i "Scene" bitince hemen yok olur.
    /// </summary>
    private System.Collections.IEnumerator WaitForAnimationAndFinish()
    {
        if (animator == null)
        {
            Debug.LogWarning($"[AnimatedBox] {name}: ⚠️ Animator is null, calling FinishBreak() immediately.");
            FinishBreak();
            yield break;
        }
        
        Debug.Log($"[AnimatedBox] {name}: ⏳ Waiting for animation to finish...");
        
        // Animasyonun başlamasını bekle
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // Animasyon state'ini bul ve bitmesini bekle
        float maxWaitTime = targetAnimationDuration > 0f ? targetAnimationDuration * 2f : 3f; // Maksimum bekleme süresi (güvenlik için)
        float elapsedTime = 0f;
        
        while (elapsedTime < maxWaitTime)
        {
            if (animator == null || !animator.enabled)
            {
                Debug.Log($"[AnimatedBox] {name}: ⚠️ Animator disabled or null, finishing break.");
                break;
            }
            
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // Animasyon bitmiş mi kontrol et (normalizedTime >= 1.0)
            if (stateInfo.normalizedTime >= 1.0f)
            {
                Debug.Log($"[AnimatedBox] {name}: ✅ Animation finished! (normalizedTime: {stateInfo.normalizedTime:F2}, state: {stateInfo.fullPathHash})");
                break;
            }
            
            // Her frame kontrol et
            yield return null;
            elapsedTime += Time.deltaTime;
        }
        
        // Animasyon bitti veya maksimum süre doldu
        if (elapsedTime >= maxWaitTime)
        {
            Debug.LogWarning($"[AnimatedBox] {name}: ⚠️ Max wait time ({maxWaitTime}s) reached, finishing break anyway.");
        }
        
        Debug.Log($"[AnimatedBox] {name}: ⏰ Animation finished, calling FinishBreak()...");
        
        if (isBroken && gameObject != null)
        {
            FinishBreak();
        }
        else
        {
            Debug.LogWarning($"[AnimatedBox] {name}: ⚠️ Cannot call FinishBreak() - isBroken: {isBroken}, gameObject: {(gameObject != null ? "exists" : "null")}");
        }
    }
    
    /// <summary>
    /// Animation Event'ten çağrılabilir (animasyon clip'inin son frame'ine ekle)
    /// </summary>
    public void OnBreakAnimationEnd()
    {
        if (isBroken)
        {
            FinishBreak();
        }
    }
    
    /// <summary>
    /// kutu_anim mesh'ini vuruştan belirli bir süre sonra disable eder
    /// </summary>
    private System.Collections.IEnumerator DisableKutuAnimAfterDelay()
    {
        if (kutuAnimMesh == null)
        {
            yield break;
        }
        
        Debug.Log($"[AnimatedBox] {name}: ⏳ Waiting {disableKutuAnimDelay}s before disabling kutu_anim mesh...");
        
        yield return new WaitForSeconds(disableKutuAnimDelay);
        
        if (kutuAnimMesh != null)
        {
            kutuAnimMesh.SetActive(false);
            Debug.Log($"[AnimatedBox] {name}: ✅ kutu_anim mesh disabled after {disableKutuAnimDelay}s.");
        }
        else
        {
            Debug.LogWarning($"[AnimatedBox] {name}: ⚠️ kutu_anim mesh is null when trying to disable!");
        }
    }
}
