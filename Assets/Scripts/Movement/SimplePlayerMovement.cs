using UnityEngine;
using UnityEngine.InputSystem;

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
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        if (controller == null)
        {
            Debug.LogError("CharacterController component not found! Please add CharacterController component to this GameObject.");
        }
    }
    
    void Update()
    {
        if (controller == null) return;
        
        // Get input using Input System
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
        else
        {
            Debug.LogWarning("No keyboard found!");
        }
        
        // Camera-relative basis
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

        // Rotate player
        if (Mathf.Abs(rotationInput) > 0.01f)
        {
            transform.Rotate(Vector3.up, rotationInput * rotationSpeed * Time.deltaTime);
        }
        else if (hasCameraDirection && Mathf.Abs(forwardInput) > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Create movement vector (strafe + forward)
        Vector3 move = (moveDirection * forwardInput) + (strafeDirection * horizontalInput);
        if (move.sqrMagnitude > 1f) move.Normalize();
        
        currentMoveInput = new Vector2(horizontalInput, forwardInput);

        // Determine speed
        float currentSpeed = sprintPressed ? runSpeed : walkSpeed;
        
        // Apply movement
        controller.Move(move * currentSpeed * Time.deltaTime);
        
        
        velocity.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        
        isRunning = sprintPressed && move.sqrMagnitude > 0.1f;
    }
    
    // AnimationController
    public bool IsMoving()
    {
        return currentMoveInput.sqrMagnitude > 0.1f;
    }
    
    public bool IsRunning()
    {
        return isRunning;
    }
}
