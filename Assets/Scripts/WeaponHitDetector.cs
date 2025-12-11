using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WeaponHitDetector : MonoBehaviour
{
    [Header("Weapon Settings")]
    [Tooltip("Hasar miktarı")]
    [SerializeField, Min(1)] private int damage = 1;
    [Tooltip("Saldırı aktifken hasar ver (ComboSystem isAttacking kontrolü)")]
    [SerializeField] private bool requireActiveAttack = true;

    [Header("Juice Settings - Hit Feedback")]
    [Tooltip("Hit Stop süresi (saniye) - Vuruş anında oyunu dondurur")]
    [SerializeField, Min(0.01f)] private float hitStopDuration = 0.1f;
    [Tooltip("Screen shake kullanılsın mı?")]
    [SerializeField] private bool useScreenShake = true;
    [Tooltip("Screen shake gücü")]
    [SerializeField, Min(0.01f)] private float screenShakePower = 0.15f;
    [Tooltip("Screen shake süresi")]
    [SerializeField, Min(0.01f)] private float screenShakeDuration = 0.2f;

    private Collider weaponCollider;
    private readonly System.Collections.Generic.List<Enemy> enemiesInRange = new System.Collections.Generic.List<Enemy>();
    private CameraShake cameraShake;
    private Transform playerTransform;

    private void Awake()
    {
        weaponCollider = GetComponent<Collider>();
        if (weaponCollider == null)
        {
            Debug.LogError($"WeaponHitDetector on {name}: Collider component not found!");
            return;
        }

        // Collider'ı trigger yap
        weaponCollider.isTrigger = true;
        
        // Player transform'unu bul
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            SimplePlayerMovement playerMovement = FindObjectOfType<SimplePlayerMovement>();
            if (playerMovement != null)
            {
                playerTransform = playerMovement.transform;
            }
        }
        else
        {
            playerTransform = player.transform;
        }
    }

    private void Start()
    {
        // ComboSystem eventini dinle
        ComboSystem.OnAttackPerformed += OnAttackPerformed;
        
        // Camera shake'i bul
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraShake = mainCamera.GetComponent<CameraShake>();
            if (cameraShake == null)
            {
                cameraShake = mainCamera.gameObject.AddComponent<CameraShake>();
            }
        }
        else
        {
            Debug.LogWarning("[WeaponHitDetector] Main camera not found! Screen shake will not work.");
        }
    }

    private void OnDestroy()
    {
        // Event dinleyicisini kaldır
        ComboSystem.OnAttackPerformed -= OnAttackPerformed;
    }

    private void OnAttackPerformed()
    {
        bool hitEnemy = false;
        Vector3 hitPoint = Vector3.zero;
        
        // Saldırı yapıldığında, menzildeki tüm Enemy'lere hasar ver
        foreach (Enemy enemy in enemiesInRange)
        {
            if (enemy != null && !enemy.IsDead())
            {
                // Hit point'i hesapla (enemy pozisyonu)
                hitPoint = enemy.transform.position;
                
                // Enemy'ye hasar ver (knockback ve visual feedback dahil)
                Vector3 knockbackDirection = playerTransform != null 
                    ? (enemy.transform.position - playerTransform.position).normalized 
                    : Vector3.forward;
                
                enemy.TakeDamage(damage, hitPoint, knockbackDirection);
                hitEnemy = true;
                
                Debug.Log($"[WeaponHitDetector] {name}: Hit enemy {enemy.name} for {damage} damage!");
            }
        }

        // Temizle (null referansları kaldır)
        enemiesInRange.RemoveAll(e => e == null);
        
        // Eğer enemy'ye vurulduysa, juice efektlerini uygula
        if (hitEnemy)
        {
            ApplyHitJuice(hitPoint);
        }
    }
    
    /// <summary>
    /// Hit Stop ve Screen Shake gibi "juice" efektlerini uygular
    /// </summary>
    private void ApplyHitJuice(Vector3 hitPoint)
    {
        // Hit Stop: Vuruş anında oyunu kısa süreliğine dondur (impact hissi)
        StartCoroutine(HitStopCoroutine());
        
        // Screen Shake: Kamerayı sars (impact hissi)
        if (useScreenShake && cameraShake != null)
        {
            cameraShake.Shake(screenShakeDuration, screenShakePower);
        }
    }
    
    /// <summary>
    /// Hit Stop Coroutine - Vuruş anında Time.timeScale'i 0 yaparak freeze frame efekti verir
    /// </summary>
    private IEnumerator HitStopCoroutine()
    {
        // Oyunu dondur
        Time.timeScale = 0f;
        
        // Gerçek zamanı bekle (Time.timeScale = 0 olduğu için Time.deltaTime çalışmaz)
        float realTime = 0f;
        while (realTime < hitStopDuration)
        {
            realTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // Oyunu normale döndür
        Time.timeScale = 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Add(enemy);
            Debug.Log($"[WeaponHitDetector] {name}: Enemy {enemy.name} entered weapon range.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Remove(enemy);
            Debug.Log($"[WeaponHitDetector] {name}: Enemy {enemy.name} left weapon range.");
        }
    }
}

