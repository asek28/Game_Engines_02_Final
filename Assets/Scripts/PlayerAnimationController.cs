using UnityEngine;

/// <summary>
/// Player animator parametrelerini yöneten controller
/// Yeni Player karakter için: isWalking, isRunning, isHitting, isStanding, isShooting
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player Animator component")]
    [SerializeField] private Animator animator;
    
    [Header("Components")]
    private SimplePlayerMovement movement;
    private WeaponSlotSystem weaponSystem;
    
    // Animator parameter names (Animator Controller'daki isimler)
    private const string PARAM_IS_WALKING = "isWalking";
    private const string PARAM_IS_RUNNING = "isRunning";
    private const string PARAM_IS_HITTING = "isHitting";
    private const string PARAM_IS_STANDING = "isStanding";
    private const string PARAM_IS_SHOOTING = "isShooting";
    
    // Animation state tracking
    private bool isWalking = false;
    private bool isRunning = false;
    private bool isHitting = false;
    private bool isStanding = false;
    private bool isShooting = false;
    
    private void Awake()
    {
        // Animator'ı bul
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (animator == null)
        {
            Debug.LogError("[PlayerAnimationController] Animator component not found!");
        }
        else
        {
            Debug.Log($"[PlayerAnimationController] Animator found: {animator.name}");
        }
        
        // Component'leri bul
        movement = GetComponent<SimplePlayerMovement>();
        weaponSystem = GetComponent<WeaponSlotSystem>();
        
        if (movement == null)
        {
            Debug.LogWarning("[PlayerAnimationController] SimplePlayerMovement not found!");
        }
        else
        {
            Debug.Log("[PlayerAnimationController] SimplePlayerMovement found!");
        }
        
        if (weaponSystem == null)
        {
            Debug.LogWarning("[PlayerAnimationController] WeaponSlotSystem not found!");
        }
        else
        {
            Debug.Log("[PlayerAnimationController] WeaponSlotSystem found!");
        }
    }
    
    private void Start()
    {
        // Başlangıçta tüm parametreleri false yap
        ResetAllParameters();
    }
    
    private void LateUpdate()
    {
        if (animator == null) return;
        
        // Her frame animator parametrelerini güncelle
        UpdateAnimatorParameters();
    }
    
    /// <summary>
    /// Animator parametrelerini güncelle
    /// </summary>
    private void UpdateAnimatorParameters()
    {
        // Movement (isWalking, isRunning)
        UpdateMovementAnimation();
        
        // Attack animations (isHitting, isStanding, isShooting) manuel olarak set edilir
        // Bu parametreler ComboSystem, GunWeapon, vb. tarafından tetiklenir
    }
    
    /// <summary>
    /// Movement animasyonlarını güncelle (isWalking, isRunning)
    /// </summary>
    private void UpdateMovementAnimation()
    {
        if (movement == null)
        {
            Debug.LogWarning("[PlayerAnimationController] SimplePlayerMovement is null!");
            return;
        }
        
        bool shouldWalk = movement.IsMoving();
        bool shouldRun = movement.IsRunning();
        
        // Debug: Hareket durumu
        if (shouldWalk || shouldRun)
        {
            Debug.Log($"[PlayerAnimationController] Movement - shouldWalk: {shouldWalk}, shouldRun: {shouldRun}");
        }
        
        // Eğer saldırı animasyonu aktifse movement animasyonunu durdur
        if (isHitting || isStanding || isShooting)
        {
            shouldWalk = false;
            shouldRun = false;
            Debug.Log($"[PlayerAnimationController] Attack active, blocking movement animations");
        }
        
        // isWalking
        if (isWalking != shouldWalk)
        {
            isWalking = shouldWalk;
            SetBool(PARAM_IS_WALKING, isWalking);
            Debug.Log($"[PlayerAnimationController] isWalking = {isWalking}");
        }
        
        // isRunning
        if (isRunning != shouldRun)
        {
            isRunning = shouldRun;
            SetBool(PARAM_IS_RUNNING, isRunning);
            Debug.Log($"<color=magenta>[PlayerAnimationController] ✅ isRunning SET TO = {isRunning}</color>");
            
            // Animator'a set edildi mi kontrol et
            if (animator != null && HasParameter(PARAM_IS_RUNNING))
            {
                bool animatorValue = animator.GetBool(PARAM_IS_RUNNING);
                Debug.Log($"   - Animator.GetBool('isRunning') = {animatorValue}");
            }
        }
    }
    
    /// <summary>
    /// Boş elle vuruş animasyonu (isHitting)
    /// </summary>
    public void SetHitting(bool value)
    {
        if (isHitting == value) return;
        
        isHitting = value;
        SetBool(PARAM_IS_HITTING, isHitting);
        
        Debug.Log($"[PlayerAnimationController] isHitting = {isHitting}");
    }
    
    /// <summary>
    /// Stick ile vuruş animasyonu (isStanding)
    /// </summary>
    public void SetStanding(bool value)
    {
        if (isStanding == value) return;
        
        isStanding = value;
        SetBool(PARAM_IS_STANDING, isStanding);
        
        Debug.Log($"[PlayerAnimationController] isStanding = {isStanding}");
    }
    
    /// <summary>
    /// Gun ile ateş animasyonu (isShooting)
    /// </summary>
    public void SetShooting(bool value)
    {
        if (isShooting == value) return;
        
        isShooting = value;
        SetBool(PARAM_IS_SHOOTING, isShooting);
        
        Debug.Log($"[PlayerAnimationController] isShooting = {isShooting}");
    }
    
    /// <summary>
    /// Tüm saldırı animasyonlarını false yap
    /// </summary>
    public void ResetAttackAnimations()
    {
        SetHitting(false);
        SetStanding(false);
        SetShooting(false);
    }
    
    /// <summary>
    /// Tüm animator parametrelerini false yap
    /// </summary>
    private void ResetAllParameters()
    {
        SetBool(PARAM_IS_WALKING, false);
        SetBool(PARAM_IS_RUNNING, false);
        SetBool(PARAM_IS_HITTING, false);
        SetBool(PARAM_IS_STANDING, false);
        SetBool(PARAM_IS_SHOOTING, false);
    }
    
    /// <summary>
    /// Animator Bool parametresini güvenli şekilde set et
    /// </summary>
    private void SetBool(string parameterName, bool value)
    {
        if (animator == null) return;
        
        // Parametre var mı kontrol et
        if (!HasParameter(parameterName))
        {
            Debug.LogWarning($"[PlayerAnimationController] Animator parameter '{parameterName}' not found!");
            return;
        }
        
        animator.SetBool(parameterName, value);
    }
    
    /// <summary>
    /// Animator parametresinin varlığını kontrol et
    /// </summary>
    private bool HasParameter(string parameterName)
    {
        if (animator == null) return false;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == parameterName)
            {
                return true;
            }
        }
        return false;
    }
    
    // Public getters
    public bool IsWalking => isWalking;
    public bool IsRunning => isRunning;
    public bool IsHitting => isHitting;
    public bool IsStanding => isStanding;
    public bool IsShooting => isShooting;
}
