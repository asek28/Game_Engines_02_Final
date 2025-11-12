using UnityEngine;
using UnityEngine.InputSystem;

public class testHit : MonoBehaviour
{
    [Header("Hit Settings")]
    [Tooltip("Hasar miktarı (her vuruşta)")]
    [SerializeField, Min(1)] private int damagePerHit = 1;
    [Tooltip("Run animasyonu için gerekli can (3 vuruş = 5-3=2 can)")]
    [SerializeField, Min(1)] private int healthForRun = 2;
    [Tooltip("Ölüm için gerekli can (5 vuruş = 5-5=0 can)")]
    [SerializeField, Min(1)] private int healthForDeath = 0;

    [Header("Raycast Settings")]
    [Tooltip("Vuruş menzili")]
    [SerializeField, Min(0.1f)] private float hitRange = 5f;
    [Tooltip("Hangi layer'ları hedef alacak")]
    [SerializeField] private LayerMask enemyLayer = -1;

    private Camera playerCamera;

    private void Start()
    {
        // Ana kamerayı bul
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }

        if (playerCamera == null)
        {
            Debug.LogError("testHit: Camera not found!");
        }
    }

    private void Update()
    {
        // Sol tık kontrolü
        bool attackPressed = false;

        var mouse = Mouse.current;
        var keyboard = Keyboard.current;

        if (mouse != null)
        {
            attackPressed = mouse.leftButton.wasPressedThisFrame;
        }
        else if (keyboard != null)
        {
            attackPressed = keyboard.enterKey.wasPressedThisFrame;
        }

        if (attackPressed)
        {
            PerformHit();
        }
    }

    private void PerformHit()
    {
        if (playerCamera == null)
        {
            return;
        }

        // Kameradan ileri doğru raycast at
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, hitRange, enemyLayer))
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Enemy'ye hasar ver
                enemy.TakeDamage(damagePerHit);
                Debug.Log($"[testHit] Hit enemy {enemy.name}! Current health: {enemy.GetCurrentHealth()}/{enemy.GetMaxHealth()}");

                // Can kontrolü ve animasyon tetikleme
                int currentHealth = enemy.GetCurrentHealth();
                int maxHealth = enemy.GetMaxHealth();

                // Run animasyonu kontrolü (3 vuruş = maxHealth - 3)
                if (currentHealth <= healthForRun && currentHealth > healthForDeath)
                {
                    // Run animasyonu zaten TakeDamage içinde tetikleniyor, ama emin olmak için
                    Debug.Log($"[testHit] Enemy {enemy.name} should be running now!");
                }

                // Death kontrolü (5 vuruş = maxHealth - 5 = 0)
                if (currentHealth <= healthForDeath)
                {
                    // Death animasyonu zaten TakeDamage içinde tetikleniyor
                    Debug.Log($"[testHit] Enemy {enemy.name} should be dead now!");
                }
            }
            else
            {
                Debug.Log($"[testHit] Hit object {hit.collider.name} but it's not an Enemy.");
            }
        }
        else
        {
            Debug.Log($"[testHit] No enemy hit in range.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Raycast menzilini görselleştir
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * hitRange);
        }
    }
}

