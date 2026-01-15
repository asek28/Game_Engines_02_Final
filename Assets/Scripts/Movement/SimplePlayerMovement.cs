using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(100)]
public class SimplePlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    
    [Header("Options")]
    public bool useCameraDirection = true;
    
    [Header("Rotation Settings")]
    public float rotationSpeed = 180f;
    
    private CharacterController controller;
    private Vector3 velocity;
    private bool isRunning;
    private Vector2 currentMoveInput;
    private PlayerHealth playerHealth;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerHealth = GetComponent<PlayerHealth>();
        
        if (controller == null)
        {
            Debug.LogError("[SimplePlayerMovement] CharacterController NULL!");
            enabled = false;
            return;
        }
        
        if (!controller.enabled)
        {
            Debug.LogWarning("[SimplePlayerMovement] CharacterController DISABLED!");
            enabled = false;
            return;
        }
        
        Debug.Log("[SimplePlayerMovement] Started successfully!");
    }
    
    void Update()
    {
        // Time.timeScale = 0 ise hareket etme (oyun duraklatılmış)
        if (Time.timeScale <= 0f)
        {
            return;
        }
        
        // Controller'ı YENİDEN al (her frame)
        controller = GetComponent<CharacterController>();
        
        // Controller kontrolü
        if (controller == null || !controller.enabled)
        {
            enabled = false;
            return;
        }
        
        // Player ölü mü?
        if (playerHealth != null && playerHealth.IsDead) 
        {
            enabled = false;
            return;
        }
        
        // Settings paneli açıksa hareket etme
        SettingsMenuController settingsMenu = FindFirstObjectByType<SettingsMenuController>();
        if (settingsMenu != null && settingsMenu.IsSettingsOpen())
        {
            return;
        }
        
        // Inventory açıksa hareket etme
        if (InventoryManager.instance != null && InventoryManager.instance.IsInventoryVisible)
        {
            return;
        }
        
        // Input
        float rotationInput = 0f;
        float forwardInput = 0f;
        float horizontalInput = 0f;
        bool sprintPressed = false;
        
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            rotationInput = (keyboard.rightArrowKey.isPressed ? 1 : 0) - (keyboard.leftArrowKey.isPressed ? 1 : 0);
            forwardInput  = (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0);
            horizontalInput = (keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0);
            sprintPressed = keyboard.leftShiftKey.isPressed;
        }
        
        // Camera direction
        Vector3 moveDirection = transform.forward;
        Vector3 strafeDirection = transform.right;
        bool hasCameraDirection = false;
        
        if (useCameraDirection && Camera.main != null && Mathf.Abs(rotationInput) <= 0.01f)
        {
            Transform cam = Camera.main.transform;
            Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
            
            if (camForward.sqrMagnitude > 0.001f)
            {
                moveDirection = camForward;
                hasCameraDirection = true;
            }
            if (camRight.sqrMagnitude > 0.001f)
            {
                strafeDirection = camRight;
            }
        }

        // Rotate
        if (Mathf.Abs(rotationInput) > 0.01f)
        {
            transform.Rotate(Vector3.up, rotationInput * rotationSpeed * Time.deltaTime);
        }
        else if (hasCameraDirection && Mathf.Abs(forwardInput) > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Movement
        Vector3 move = (moveDirection * forwardInput) + (strafeDirection * horizontalInput);
        if (move.sqrMagnitude > 1f) move.Normalize();
        
        currentMoveInput = new Vector2(horizontalInput, forwardInput);
        float currentSpeed = sprintPressed ? runSpeed : walkSpeed;
        
        // Apply movement - YENİDEN AL VE KONTROL ET
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null && cc.enabled && cc.gameObject.activeInHierarchy)
        {
            cc.Move(move * currentSpeed * Time.deltaTime);
        }
        else
        {
            enabled = false;
            return;
        }
        
        // Gravity
        velocity.y += Physics.gravity.y * Time.deltaTime;
        
        // Apply gravity - YENİDEN AL VE KONTROL ET
        cc = GetComponent<CharacterController>();
        if (cc != null && cc.enabled && cc.gameObject.activeInHierarchy)
        {
            cc.Move(velocity * Time.deltaTime);
        }
        else
        {
            enabled = false;
            return;
        }
        
        // Ground check
        if (controller != null && controller.enabled && controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        // Running state
        isRunning = sprintPressed && move.sqrMagnitude > 0.1f;
    }
    
    public bool IsMoving()
    {
        return currentMoveInput.sqrMagnitude > 0.1f;
    }
    
    public bool IsRunning()
    {
        return isRunning;
    }
}
