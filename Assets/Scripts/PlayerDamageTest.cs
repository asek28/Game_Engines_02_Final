using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player health sistemini test etmek için debug script
/// Geliştirme tamamlandığında silinebilir
/// </summary>
public class PlayerDamageTest : MonoBehaviour
{
    [Header("Test Settings")]
    [Tooltip("Test hasar miktarı")]
    [SerializeField] private int testDamage = 10;
    
    [Tooltip("Test heal miktarı")]
    [SerializeField] private int testHeal = 20;
    
    private PlayerHealth playerHealth;
    
    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        
        if (playerHealth == null)
        {
            Debug.LogError("[PlayerDamageTest] PlayerHealth component not found!");
        }
    }
    
    private void Update()
    {
        if (playerHealth == null) return;
        
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // F1: Hasar al
        if (keyboard.f1Key.wasPressedThisFrame)
        {
            playerHealth.TakeDamage(testDamage);
            Debug.Log($"<color=red>[Test] F1 pressed - Took {testDamage} damage</color>");
        }
        
        // F2: Can kazan
        if (keyboard.f2Key.wasPressedThisFrame)
        {
            playerHealth.Heal(testHeal);
            Debug.Log($"<color=green>[Test] F2 pressed - Healed {testHeal} HP</color>");
        }
        
        // F3: Canı tam doldur
        if (keyboard.f3Key.wasPressedThisFrame)
        {
            playerHealth.HealFull();
            Debug.Log($"<color=green>[Test] F3 pressed - Full heal</color>");
        }
        
        // F4: Öldür (test)
        if (keyboard.f4Key.wasPressedThisFrame)
        {
            playerHealth.TakeDamage(playerHealth.CurrentHealth);
            Debug.Log($"<color=red>[Test] F4 pressed - Instant death</color>");
        }
    }
}
