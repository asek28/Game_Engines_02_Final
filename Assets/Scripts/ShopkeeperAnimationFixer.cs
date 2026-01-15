using UnityEngine;

/// <summary>
/// Shopkeeper animasyon sorunlarını düzeltir ve debug eder
/// </summary>
public class ShopkeeperAnimationFixer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Shopkeeper'ın Animator component'i")]
    [SerializeField] private Animator animator;
    
    [Header("Test Animation")]
    [Tooltip("Test için oynatılacak animasyon state ismi")]
    [SerializeField] private string testAnimationState = "Sitting Angry";
    
    [Tooltip("T tuşuna basınca test animasyonu oynat")]
    [SerializeField] private bool enableTestKey = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDetailedDebug = true;
    
    private void Awake()
    {
        // Animator'ı otomatik bul
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            
            if (animator == null)
            {
                // Child objelerde ara
                animator = GetComponentInChildren<Animator>();
            }
        }
    }
    
    private void Start()
    {
        if (animator == null)
        {
            Debug.LogError("[ShopkeeperAnimationFixer] Animator not found!");
            return;
        }
        
        Debug.Log("[ShopkeeperAnimationFixer] Starting animation diagnostics...");
        
        // Animasyon sistemini diagnose et
        DiagnoseAnimationSystem();
        
        // Animator'ı sıfırla ve başlat
        FixAnimator();
    }
    
    private void Update()
    {
        if (enableTestKey && Input.GetKeyDown(KeyCode.T))
        {
            PlayTestAnimation();
        }
    }
    
    /// <summary>
    /// Animasyon sistemini diagnose et
    /// </summary>
    private void DiagnoseAnimationSystem()
    {
        Debug.Log("=== SHOPKEEPER ANIMATION DIAGNOSTICS ===");
        
        // 1. Animator kontrolü
        Debug.Log($"Animator: {(animator != null ? "✓" : "✗")}");
        if (animator == null) return;
        
        // 2. Animator enabled kontrolü
        Debug.Log($"Animator Enabled: {animator.enabled}");
        
        // 3. Controller kontrolü
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        Debug.Log($"Animator Controller: {(controller != null ? controller.name : "NULL")}");
        
        // 4. Avatar kontrolü
        Avatar avatar = animator.avatar;
        Debug.Log($"Avatar: {(avatar != null ? avatar.name : "NULL")}");
        
        if (avatar != null)
        {
            Debug.Log($"Avatar Valid: {avatar.isValid}");
            Debug.Log($"Avatar Human: {avatar.isHuman}");
        }
        
        // 5. Current state kontrolü
        if (animator.isActiveAndEnabled && controller != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"Current State Hash: {stateInfo.fullPathHash}");
            Debug.Log($"Current State Length: {stateInfo.length}");
            Debug.Log($"Current State Speed: {stateInfo.speed}");
        }
        
        // 6. Animator parameters
        if (controller != null)
        {
            Debug.Log($"Parameter Count: {animator.parameterCount}");
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                Debug.Log($"  - {param.name} ({param.type})");
            }
        }
        
        Debug.Log("========================================");
    }
    
    /// <summary>
    /// Animator'ı düzelt ve başlat
    /// </summary>
    private void FixAnimator()
    {
        if (animator == null) return;
        
        Debug.Log("[ShopkeeperAnimationFixer] Fixing animator...");
        
        // 1. Animator'ı enable et
        animator.enabled = true;
        
        // 2. Avatar yoksa veya geçersizse uyar
        if (animator.avatar == null || !animator.avatar.isValid)
        {
            Debug.LogError("[ShopkeeperAnimationFixer] Avatar is NULL or INVALID!");
            Debug.LogError("FIX: Select Shopkeeper.fbx → Inspector → Rig → Animation Type: Humanoid → Apply");
            return;
        }
        
        // 3. Animator'ı rebind et (T-pose'dan çıkar)
        animator.Rebind();
        animator.Update(0f);
        
        // 4. İlk animasyonu başlat
        if (!string.IsNullOrEmpty(testAnimationState))
        {
            animator.Play(testAnimationState, 0, 0f);
            Debug.Log($"[ShopkeeperAnimationFixer] Playing: {testAnimationState}");
        }
        
        Debug.Log("[ShopkeeperAnimationFixer] Animator fixed!");
    }
    
    /// <summary>
    /// Test animasyonunu oynat (T tuşu)
    /// </summary>
    private void PlayTestAnimation()
    {
        if (animator == null || string.IsNullOrEmpty(testAnimationState))
        {
            Debug.LogWarning("[ShopkeeperAnimationFixer] Cannot play test animation!");
            return;
        }
        
        Debug.Log($"[ShopkeeperAnimationFixer] Playing test animation: {testAnimationState}");
        
        // Rebind ve play
        animator.Rebind();
        animator.Update(0f);
        animator.Play(testAnimationState, 0, 0f);
    }
    
    /// <summary>
    /// Gizmos ile Shopkeeper'ı işaretle
    /// </summary>
    private void OnDrawGizmos()
    {
        // Shopkeeper'ın başı üzerinde bir sphere çiz
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);
    }
}
