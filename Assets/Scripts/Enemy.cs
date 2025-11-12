using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider))]
public class Enemy : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maksimum can")]
    [SerializeField, Min(1)] private int maxHealth = 5;
    [Tooltip("Her vuruşta alınan hasar")]
    [SerializeField, Min(1)] private int damagePerHit = 1;
    [Tooltip("Can 2'ye düşünce koşma hızı çarpanı")]
    [SerializeField, Min(1f)] private float runSpeedMultiplier = 1.5f;

    [Header("Movement Settings")]
    [Tooltip("Yürüme hızı")]
    [SerializeField, Min(0.1f)] private float walkSpeed = 3f;
    [Tooltip("Rastgele yürüme mesafesi (spawner'dan maksimum uzaklık)")]
    [SerializeField, Min(1f)] private float wanderRadius = 10f;
    [Tooltip("Hedefe ulaşıldığında kabul edilebilir mesafe")]
    [SerializeField, Min(0.1f)] private float arrivalDistance = 0.5f;
    [Tooltip("Yer çekimi kuvveti")]
    [SerializeField] private float gravity = -9.81f;

    [Header("Behavior Settings")]
    [Tooltip("Yürüme süresi (saniye)")]
    [SerializeField, Min(0.5f)] private float walkDuration = 3f;
    [Tooltip("Idle kalma süresi (saniye)")]
    [SerializeField, Min(0.5f)] private float idleDuration = 2f;
    [Tooltip("Yürüme süresinde rastgelelik (örn: 3 ± 1 saniye)")]
    [SerializeField, Min(0f)] private float walkDurationVariation = 1f;
    [Tooltip("Idle süresinde rastgelelik (örn: 2 ± 0.5 saniye)")]
    [SerializeField, Min(0f)] private float idleDurationVariation = 0.5f;

    [Header("References")]
    [Tooltip("Animator component. Eğer boşsa otomatik bulunur.")]
    [SerializeField] private Animator animator;

    private CharacterController controller;
    private Collider enemyCollider;
    private Vector3 startPosition;
    private Vector3 currentDestination;
    private Vector3 velocity;
    private bool isWalking = false;
    private float stateTimer = 0f;
    private float currentStateDuration = 0f;
    
    private int currentHealth;
    private bool isDead = false;
    private bool isRunning = false;

    private void Awake()
    {
        // NavMeshAgent varsa devre dışı bırak (artık kullanmıyoruz)
        UnityEngine.AI.NavMeshAgent navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.enabled = false;
            Debug.Log($"Enemy on {name}: NavMeshAgent disabled (using CharacterController instead).");
        }

        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError($"Enemy on {name}: CharacterController component not found!");
            return;
        }

        enemyCollider = GetComponent<Collider>();
        if (enemyCollider == null)
        {
            Debug.LogWarning($"Enemy on {name}: Collider component not found. Hit detection may not work.");
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning($"Enemy on {name}: Animator component not found. Animation control will not work.");
            }
        }

        startPosition = transform.position;
        currentDestination = startPosition;
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // Başlangıçta idle durumunda başla
        SetIdleState();
    }

    private void Update()
    {
        if (controller == null || isDead)
        {
            return;
        }

        // State timer'ı güncelle
        stateTimer += Time.deltaTime;

        if (isWalking)
        {
            // Yürüme durumunda
            UpdateWalkingState();
        }
        else
        {
            // Idle durumunda
            UpdateIdleState();
        }

        // Yer çekimi uygula
        ApplyGravity();

        // Animator'ı güncelle
        UpdateAnimator();
    }

    private void UpdateWalkingState()
    {
        // Hedefe doğru hareket et
        Vector3 direction = (currentDestination - transform.position);
        direction.y = 0f; // Y eksenini sıfırla (sadece yatay hareket)

        float distanceToDestination = direction.magnitude;

        // Hedefe ulaşıldı mı kontrol et
        if (distanceToDestination < arrivalDistance)
        {
            // Hedefe ulaşıldı, idle'e geç
            SetIdleState();
            return;
        }

        // Süre doldu mu kontrol et
        if (stateTimer >= currentStateDuration)
        {
            // Yürüme süresi doldu, idle'e geç
            SetIdleState();
            return;
        }

        // Hedefe doğru hareket et
        direction.Normalize();
        float currentSpeed = isRunning ? walkSpeed * runSpeedMultiplier : walkSpeed;
        Vector3 move = direction * currentSpeed * Time.deltaTime;
        controller.Move(move);

        // Karakteri hedefe doğru döndür
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    private void UpdateIdleState()
    {
        // Idle süresi doldu mu kontrol et
        if (stateTimer >= currentStateDuration)
        {
            // Idle süresi doldu, yürümeye başla
            SetWalkingState();
        }
    }

    private void SetWalkingState()
    {
        isWalking = true;
        stateTimer = 0f;
        currentStateDuration = walkDuration + Random.Range(-walkDurationVariation, walkDurationVariation);
        currentStateDuration = Mathf.Max(0.5f, currentStateDuration); // Minimum 0.5 saniye

        // Rastgele bir hedef seç
        currentDestination = GetRandomDestination();
    }

    private void SetIdleState()
    {
        isWalking = false;
        stateTimer = 0f;
        currentStateDuration = idleDuration + Random.Range(-idleDurationVariation, idleDurationVariation);
        currentStateDuration = Mathf.Max(0.5f, currentStateDuration); // Minimum 0.5 saniye
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private Vector3 GetRandomDestination()
    {
        // Başlangıç pozisyonundan rastgele bir nokta seç
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        Vector3 randomPosition = startPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

        // Y eksenini mevcut pozisyondan al (zemin seviyesinde kal)
        randomPosition.y = transform.position.y;

        return randomPosition;
    }

    private void UpdateAnimator()
    {
        if (animator == null)
        {
            return;
        }

        // Walk parametresini güncelle
        animator.SetBool("Walk", isWalking);

        // Run parametresini güncelle
        animator.SetBool("Run", isRunning);
    }


    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            Debug.Log($"[Enemy] {name}: Already dead, cannot take damage.");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"[Enemy] {name}: Took {damage} damage! Current health: {currentHealth}/{maxHealth}");

        // Hit animasyonu trigger'la
        if (animator != null)
        {
            animator.SetTrigger("Hit");
            Debug.Log($"[Enemy] {name}: Hit trigger set!");
        }
        else
        {
            Debug.LogWarning($"[Enemy] {name}: Animator is null, cannot set Hit trigger!");
        }

        // Can 2'ye düştü mü kontrol et
        if (currentHealth <= 2 && currentHealth > 0)
        {
            isRunning = true;
            Debug.Log($"[Enemy] {name}: Health low! Starting to run...");
        }

        // Can 0 oldu mu kontrol et
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public bool IsDead()
    {
        return isDead;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        isWalking = false;
        isRunning = false;

        // Death animasyonu trigger'la
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Hareketi durdur
        if (controller != null)
        {
            controller.enabled = false;
        }

        // Enemy'yi birkaç saniye sonra yok et (animasyon bitince)
        Destroy(gameObject, 5f);
    }

    private void OnDrawGizmosSelected()
    {
        // Wander radius'u görselleştir
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(center, wanderRadius);
    }
}

