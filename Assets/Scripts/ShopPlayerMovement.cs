using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Shop sahnesinde Player hareketi - Input System kullanır
/// </summary>
public class ShopPlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 180f;
    
    [Header("Camera Settings")]
    [SerializeField] private bool useCameraDirection = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    private CharacterController controller;
    private Vector3 velocity;
    private bool isRunning;
    
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        
        if (controller == null)
        {
            Debug.LogError("[ShopPlayerMovement] CharacterController not found!");
            enabled = false;
            return;
        }
        
        // SimplePlayerMovement'i devre dışı bırak
        SimplePlayerMovement oldMovement = GetComponent<SimplePlayerMovement>();
        if (oldMovement != null)
        {
            oldMovement.enabled = false;
            Debug.Log("[ShopPlayerMovement] SimplePlayerMovement disabled, ShopPlayerMovement active.");
        }
        
        Debug.Log("[ShopPlayerMovement] Initialized - Use WASD to move, Shift to run.");
    }
    
    private void Update()
    {
        // Time.timeScale = 0 ise hareket etme (oyun duraklatılmış)
        if (Time.timeScale <= 0f)
        {
            return;
        }
        
        if (controller == null || !controller.enabled) return;
        
        // Settings/Inventory açıksa hareket etme
        if (IsUIOpen()) return;
        
        // Input al (Input System)
        float horizontal = 0f;
        float vertical = 0f;
        bool sprint = false;
        
        var keyboard = Keyboard.current;
        var gamepad = Gamepad.current;
        
        // Keyboard input
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed) horizontal -= 1f;
            if (keyboard.dKey.isPressed) horizontal += 1f;
            if (keyboard.wKey.isPressed) vertical += 1f;
            if (keyboard.sKey.isPressed) vertical -= 1f;
            
            sprint = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
        }
        
        // Gamepad input (varsa)
        if (gamepad != null)
        {
            Vector2 stickInput = gamepad.leftStick.ReadValue();
            horizontal += stickInput.x;
            vertical += stickInput.y;
            
            if (gamepad.leftTrigger.isPressed) sprint = true;
        }
        
        // Input'u clamp et
        horizontal = Mathf.Clamp(horizontal, -1f, 1f);
        vertical = Mathf.Clamp(vertical, -1f, 1f);
        
        // Debug
        if (showDebugLogs && (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f))
        {
            Debug.Log($"[ShopPlayerMovement] Input: H={horizontal:F2}, V={vertical:F2}, Sprint={sprint}");
        }
        
        // Hareket yönünü hesapla
        Vector3 moveDirection = CalculateMoveDirection(horizontal, vertical);
        
        // Hız hesapla
        float currentSpeed = sprint ? runSpeed : walkSpeed;
        isRunning = sprint && moveDirection.sqrMagnitude > 0.1f;
        
        // Player'ı döndür (hareket varsa)
        if (moveDirection.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );
        }
        
        // Horizontal movement
        Vector3 horizontalMovement = moveDirection * currentSpeed * Time.deltaTime;
        controller.Move(horizontalMovement);
        
        // Gravity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        else
        {
            velocity.y += Physics.gravity.y * Time.deltaTime;
        }
        
        // Vertical movement (gravity)
        controller.Move(velocity * Time.deltaTime);
        
        // Debug pozisyon
        if (showDebugLogs && moveDirection.sqrMagnitude > 0.1f)
        {
            Debug.Log($"[ShopPlayerMovement] Pos: {transform.position}, Grounded: {controller.isGrounded}");
        }
    }
    
    /// <summary>
    /// Hareket yönünü hesapla (kamera bazlı veya world space)
    /// </summary>
    private Vector3 CalculateMoveDirection(float horizontal, float vertical)
    {
        Vector3 moveDirection = Vector3.zero;
        
        if (useCameraDirection && Camera.main != null)
        {
            // Kamera yönüne göre hareket
            Transform cam = Camera.main.transform;
            Vector3 camForward = cam.forward;
            Vector3 camRight = cam.right;
            
            // Y eksenini kaldır (yatay düzlemde hareket)
            camForward.y = 0;
            camRight.y = 0;
            
            camForward.Normalize();
            camRight.Normalize();
            
            moveDirection = (camForward * vertical) + (camRight * horizontal);
        }
        else
        {
            // World space hareket
            moveDirection = new Vector3(horizontal, 0, vertical);
        }
        
        // Normalize (diagonal hareket hızlı olmasın)
        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }
        
        return moveDirection;
    }
    
    /// <summary>
    /// UI açık mı kontrol et
    /// </summary>
    private bool IsUIOpen()
    {
        // Settings menu kontrolü
        SettingsMenuController settingsMenu = FindFirstObjectByType<SettingsMenuController>();
        if (settingsMenu != null && settingsMenu.IsSettingsOpen())
        {
            return true;
        }
        
        // Inventory kontrolü
        if (InventoryManager.instance != null && InventoryManager.instance.IsInventoryVisible)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Hareket ediyor mu? (AnimationController için)
    /// </summary>
    public bool IsMoving()
    {
        return controller.velocity.sqrMagnitude > 0.1f;
    }
    
    /// <summary>
    /// Koşuyor mu? (AnimationController için)
    /// </summary>
    public bool IsRunning()
    {
        return isRunning;
    }
}
