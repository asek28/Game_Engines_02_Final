using UnityEngine;
using System.Collections;

/// <summary>
/// CharacterController tabanlı Enemy AI Controller (NavMesh gerektirmez)
/// Street boyunca smooth hareket, scrap toplama, player takibi ve saldırı
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider))]
public class EnemyAIController : MonoBehaviour
{
    public enum EnemyType
    {
        Passive,    // Pasif - sadece yürür, scrap toplar, player yaklaşırsa kaçar
        Aggressive   // Agresif - player'ı takip eder ve saldırır
    }

    [Header("Enemy Type")]
    [Tooltip("Enemy türü - Passive veya Aggressive")]
    [SerializeField] private EnemyType enemyType = EnemyType.Passive;

    [Header("Movement Settings")]
    [Tooltip("CharacterController component (otomatik bulunur)")]
    [SerializeField] private CharacterController controller;
    
    [Tooltip("Yürüme hızı")]
    [SerializeField, Min(0.1f)] private float walkSpeed = 2f; // Daha yavaş hareket için
    
    [Tooltip("Rotation hızı (derece/saniye) - Daha yüksek = daha hızlı döner")]
    [SerializeField, Min(1f)] private float rotationSpeed = 120f;
    
    [Tooltip("Stopping distance - Hedefe bu mesafede durur (jitter önlemek için)")]
    [SerializeField, Min(0.1f)] private float stoppingDistance = 0.5f;
    
    [Tooltip("Yer çekimi kuvveti")]
    [SerializeField] private float gravity = -9.81f;
    
    [Tooltip("Step offset - Küçük engelleri geçebilme yüksekliği")]
    [SerializeField, Min(0f)] private float stepOffset = 0.5f;
    
    [Tooltip("Slope limit - Tırmanabileceği maksimum eğim (derece)")]
    [SerializeField, Range(0f, 90f)] private float slopeLimit = 45f;
    
    [Tooltip("Hızlanma/duraklama hızı (daha yüksek = daha ani hareket)")]
    [SerializeField, Min(1f)] private float acceleration = 10f;
    
    [Tooltip("Collision detection mesafesi (duvarları algılamak için)")]
    [SerializeField, Min(0.1f)] private float collisionDetectionDistance = 0.5f;

    [Header("Street Movement Settings")]
    [Tooltip("Wander radius - Street boyunca ne kadar ileri/geri gidebilir")]
    [SerializeField, Min(1f)] private float wanderRadius = 10f;
    
    [Tooltip("Street yönü (genelde Z-axis forward) - Enemy bu yönde yürür")]
    [SerializeField] private Vector3 streetDirection = Vector3.forward;
    
    [Tooltip("Wander interval - Ne sıklıkla yeni hedef seçer (saniye)")]
    [SerializeField, Min(0.5f)] private float wanderInterval = 3f;

    [Header("Scrap Collection (Passive Enemy)")]
    [Tooltip("Scrap algılama mesafesi")]
    [SerializeField, Min(1f)] private float scrapDetectionRange = 5f;
    
    [Tooltip("Scrap toplama mesafesi")]
    [SerializeField, Min(0.5f)] private float scrapCollectionRange = 1.5f;
    
    [Tooltip("Scrap arama sıklığı (saniye)")]
    [SerializeField, Min(0.5f)] private float scrapSearchInterval = 2f;

    [Header("Player Detection (Aggressive Enemy)")]
    [Tooltip("Player algılama mesafesi")]
    [SerializeField, Min(1f)] private float playerDetectionRange = 10f;
    
    [Tooltip("Saldırı mesafesi")]
    [SerializeField, Min(0.5f)] private float attackRange = 2f;
    
    [Tooltip("Saldırı cooldown (saniye)")]
    [SerializeField, Min(0.5f)] private float attackCooldown = 1.5f;

    [Header("Flee Settings (Passive Enemy)")]
    [Tooltip("Player'dan kaçma mesafesi")]
    [SerializeField, Min(1f)] private float fleeRange = 5f;
    
    [Tooltip("Kaçma hızı çarpanı")]
    [SerializeField, Min(1f)] private float fleeSpeedMultiplier = 1.5f;

    [Header("Health Settings")]
    [Tooltip("Maksimum can")]
    [SerializeField, Min(1)] private int maxHealth = 5;
    
    [Tooltip("Mevcut can")]
    [SerializeField] private int currentHealth;
    
    [Header("References")]
    [Tooltip("Player transform (otomatik bulunur)")]
    [SerializeField] private Transform playerTransform;
    
    [Tooltip("Animator component (otomatik bulunur)")]
    [SerializeField] private Animator animator;
    
    [Tooltip("EnemyAnimationController component (isteğe bağlı)")]
    [SerializeField] private EnemyAnimationController animationController;
    
    [Header("Juice Settings - Visual & Audio Feedback")]
    [Tooltip("Hasar alma sesi (enemy vurulduğunda çalar)")]
    [SerializeField] private AudioClip hitSound;
    
    [Tooltip("Ölüm sesi (enemy öldüğünde çalar)")]
    [SerializeField] private AudioClip deathSound;
    
    [Tooltip("Hit VFX particle system (hit point'te spawn olacak)")]
    [SerializeField] private GameObject hitVFXPrefab;
    
    [Tooltip("Floating damage text prefab (optional)")]
    [SerializeField] private GameObject damageTextPrefab;
    
    [Tooltip("Material flash süresi (saniye)")]
    [SerializeField, Min(0.01f)] private float flashDuration = 0.1f;
    
    [Tooltip("Flash rengi (beyaz flash için)")]
    [SerializeField] private Color flashColor = Color.white;
    
    [Tooltip("Knockback gücü")]
    [SerializeField, Min(0f)] private float knockbackForce = 3f;
    
    [Tooltip("Knockback yukarı kuvveti")]
    [SerializeField, Min(0f)] private float knockbackUpwardForce = 1f;

    // Private variables
    private Vector3 currentDestination;
    private Vector3 velocity = Vector3.zero; // Hareket velocity'si (CharacterController için)
    private float wanderTimer = 0f;
    private float scrapSearchTimer = 0f;
    private float attackTimer = 0f;
    private bool isChasingPlayer = false;
    private bool isFleeing = false;
    private bool isCollectingScrap = false;
    private GameObject currentTargetScrap = null;
    private Vector3 startPosition;
    private bool isDead = false;
    private bool hasBeenAttacked = false;
    
    // Combat feedback için
    private Renderer[] enemyRenderers;
    private Material[] originalMaterials;
    private Material[] flashMaterials;
    private AudioSource audioSource;
    private Vector3 knockbackVelocity = Vector3.zero;

    // AI State
    private enum AIState
    {
        Wandering,      // Street boyunca yürüyor
        Chasing,        // Player'ı takip ediyor (Aggressive)
        Attacking,      // Saldırıyor (Aggressive)
        Collecting,     // Scrap topluyor (Passive)
        Fleeing         // Player'dan kaçıyor (Passive)
    }
    
    private AIState currentState = AIState.Wandering;

    private void Awake()
    {
        // CharacterController'ı al veya ekle
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
            DebugLog($"[EnemyAIController] {name}: CharacterController component added automatically.");
        }

        // CharacterController ayarları
        controller.stepOffset = stepOffset;
        controller.slopeLimit = slopeLimit;
        
        // CharacterController boyutları (collider gibi çalışır)
        controller.radius = 0.5f; // Enemy genişliği
        controller.height = 2f; // Enemy yüksekliği
        controller.center = new Vector3(0f, 1f, 0f); // Y offset
        
        // CharacterController'ın collision detection'ını etkinleştir
        // NOT: CharacterController zaten otomatik olarak collision detect eder
        // Ama duvarların da collider'ı olmalı (isTrigger = false)
        
        // NavMeshAgent varsa devre dışı bırak (artık kullanmıyoruz)
        UnityEngine.AI.NavMeshAgent navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.enabled = false;
            DebugLog($"[EnemyAIController] {name}: NavMeshAgent disabled (using CharacterController instead).");
        }

        // Animator'ı al
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            DebugLogWarning($"[EnemyAIController] {name}: Animator component not found!");
        }
        else
        {
            // Root motion'u devre dışı bırak (CharacterController hareketi yönetiyor)
            // Animasyonların pozisyon değiştirmesini önler (ışınlanma sorununu çözer)
            animator.applyRootMotion = false;
            DebugLog($"[EnemyAIController] {name}: Root motion disabled (CharacterController handles movement).");
        }

        // EnemyAnimationController'ı bul
        animationController = GetComponent<EnemyAnimationController>();

        // Player'ı bul
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            SimplePlayerMovement playerMovement = FindFirstObjectByType<SimplePlayerMovement>();
            if (playerMovement != null)
            {
                playerTransform = playerMovement.transform;
            }
        }

        startPosition = transform.position;
        currentDestination = startPosition;
        
        // Health'i başlat
        currentHealth = maxHealth;
        
        // Renderer'ları bul (material flash için)
        enemyRenderers = GetComponentsInChildren<Renderer>();
        if (enemyRenderers != null && enemyRenderers.Length > 0)
        {
            originalMaterials = new Material[enemyRenderers.Length];
            flashMaterials = new Material[enemyRenderers.Length];
            
            for (int i = 0; i < enemyRenderers.Length; i++)
            {
                if (enemyRenderers[i] != null)
                {
                    originalMaterials[i] = enemyRenderers[i].material;
                    // Flash material oluştur (beyaz emission)
                    flashMaterials[i] = new Material(originalMaterials[i]);
                    flashMaterials[i].EnableKeyword("_EMISSION");
                    flashMaterials[i].SetColor("_EmissionColor", flashColor);
                }
            }
        }
        
        // AudioSource'u bul veya ekle
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }
    }

    private void Start()
    {
        // Collider kontrolü - WeaponHitDetector için gerekli
        // CharacterController normal Collider'larla etkileşime girmez
        // WeaponHitDetector için trigger collider gerekli
        // Duvarlar için CharacterController'ın kendisi + manuel raycast kullanıyoruz
        
        Collider[] allColliders = GetComponents<Collider>();
        Collider triggerCollider = null;
        
        // CharacterController dışında bir collider var mı kontrol et
        foreach (Collider col in allColliders)
        {
            if (!(col is CharacterController))
            {
                triggerCollider = col;
                break;
            }
        }
        
        if (triggerCollider == null)
        {
            // Collider yoksa ekle (WeaponHitDetector için trigger olmalı)
            triggerCollider = gameObject.AddComponent<CapsuleCollider>();
            CapsuleCollider capsule = triggerCollider as CapsuleCollider;
            if (capsule != null)
            {
                capsule.height = 2f;
                capsule.radius = 0.5f;
                capsule.center = new Vector3(0f, 1f, 0f);
                capsule.isTrigger = true; // WeaponHitDetector için trigger olmalı
            }
            DebugLog($"[EnemyAIController] {name}: Trigger Collider component added automatically for WeaponHitDetector.");
        }
        else
        {
            // Mevcut collider'ı trigger yap (WeaponHitDetector için)
            if (!triggerCollider.isTrigger)
            {
                triggerCollider.isTrigger = true;
                DebugLog($"[EnemyAIController] {name}: Collider set to trigger for WeaponHitDetector.");
            }
        }
        
        // NOT: Duvarlar için CharacterController'ın kendisi + manuel raycast kullanıyoruz
        // CharacterController normal Collider'larla etkileşime girmez, bu yüzden raycast ile duvar algılama yapıyoruz
        
        // Başlangıçta wander state'ine geç
        SetState(AIState.Wandering);
        
        // Kısa bir gecikme sonrası hedef belirle
        StartCoroutine(DelayedStart());
    }
    
    /// <summary>
    /// Başlangıç gecikmesi
    /// </summary>
    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.1f);
        SetRandomDestinationOnStreet();
    }
    
    // Conditional compilation - sadece development build'de log göster
    #if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void DebugLog(string message)
    {
        Debug.Log(message);
    }
    private void DebugLogWarning(string message)
    {
        Debug.LogWarning(message);
    }
    #else
    private void DebugLog(string message) { }
    private void DebugLogWarning(string message) { }
    #endif

    private void Update()
    {
        if (controller == null || isDead)
        {
            return;
        }

        // Yer çekimi uygula
        if (!controller.isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            velocity.y = -0.5f; // Yerdeyken küçük bir downward force
        }

        // Knockback uygula
        if (knockbackVelocity.sqrMagnitude > 0.01f)
        {
            velocity += knockbackVelocity * Time.deltaTime;
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, 5f * Time.deltaTime);
        }

        // Smooth rotation uygula (hedefe doğru dön)
        ApplySmoothRotation();

        // State'e göre davranış
        switch (currentState)
        {
            case AIState.Wandering:
                UpdateWandering();
                break;
            case AIState.Chasing:
                UpdateChasing();
                break;
            case AIState.Attacking:
                UpdateAttacking();
                break;
            case AIState.Collecting:
                UpdateCollecting();
                break;
            case AIState.Fleeing:
                UpdateFleeing();
                break;
        }

        // Manuel collision detection - CharacterController normal Collider'larla otomatik etkileşime girmez
        // Önce hareket yönünde raycast yap (duvarları algılamak için)
        Vector3 moveDirection = new Vector3(velocity.x, 0f, velocity.z);
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            moveDirection.Normalize();
            
            // Hareket yönünde raycast yap
            RaycastHit hit;
            float rayDistance = collisionDetectionDistance + (new Vector3(velocity.x, 0f, velocity.z).magnitude * Time.deltaTime);
            
            // Birden fazla raycast yap (merkez, sol, sağ) - daha güvenilir algılama için
            Vector3[] rayOrigins = new Vector3[]
            {
                transform.position + Vector3.up * 0.5f, // Merkez
                transform.position + Vector3.up * 0.5f + transform.right * -0.3f, // Sol
                transform.position + Vector3.up * 0.5f + transform.right * 0.3f // Sağ
            };
            
            bool hitWall = false;
            Vector3 wallNormal = Vector3.zero;
            
            foreach (Vector3 rayOrigin in rayOrigins)
            {
                if (Physics.Raycast(rayOrigin, moveDirection, out hit, rayDistance))
                {
                    // Enemy'nin kendi collider'ını ignore et
                    if (hit.collider != null && hit.collider.gameObject != gameObject && 
                        hit.collider.transform != transform && 
                        !hit.collider.isTrigger)
                    {
                        hitWall = true;
                        wallNormal = hit.normal;
                        DebugLog($"[EnemyAIController] {name}: Hit wall '{hit.collider.name}' at distance {hit.distance:F2}");
                        break;
                    }
                }
            }
            
            if (hitWall)
            {
                // Duvara çarptı, yön değiştir
                wallNormal.y = 0f;
                wallNormal.Normalize();
                
                // Duvardan uzaklaşacak şekilde yeni yön belirle (reflect)
                Vector3 newDirection = Vector3.Reflect(moveDirection, wallNormal);
                newDirection.y = 0f;
                newDirection.Normalize();
                
                // Eğer yeni yön çok küçükse, rastgele bir yön seç
                if (newDirection.sqrMagnitude < 0.1f)
                {
                    newDirection = new Vector3(
                        Random.Range(-1f, 1f),
                        0f,
                        Random.Range(-1f, 1f)
                    ).normalized;
                }
                
                // Yeni hedef belirle
                currentDestination = transform.position + newDirection * wanderRadius;
                
                // Velocity'yi sıfırla ve yeni yöne doğru hareket et
                velocity.x = 0f;
                velocity.z = 0f;
            }
        }
        
        // Hareketi uygula (CharacterController otomatik olarak collision detect eder)
        CollisionFlags collisionFlags = controller.Move(velocity * Time.deltaTime);
        
        // CharacterController collision kontrolü (yedek - Rigidbody'li objeler için)
        if ((collisionFlags & CollisionFlags.Sides) != 0)
        {
            // Duvara çarptı, yön değiştir
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;
            currentDestination = transform.position + randomDirection * wanderRadius;
            
            // Velocity'yi sıfırla
            velocity.x = 0f;
            velocity.z = 0f;
        }

        // Animator'ı güncelle
        UpdateAnimator();
    }

    /// <summary>
    /// Smooth rotation uygula - hedefe doğru dön
    /// </summary>
    private void ApplySmoothRotation()
    {
        Vector3 direction = Vector3.zero;
        
        // State'e göre yön belirle
        switch (currentState)
        {
            case AIState.Wandering:
            case AIState.Collecting:
            case AIState.Fleeing:
                direction = (currentDestination - transform.position);
                break;
            case AIState.Chasing:
            case AIState.Attacking:
                if (playerTransform != null)
                {
                    direction = (playerTransform.position - transform.position);
                }
                break;
        }
        
        direction.y = 0f; // Y eksenini sıfırla
        
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime / 90f
            );
        }
    }
    
    /// <summary>
    /// Hedefe doğru hareket et
    /// </summary>
    private void MoveTowardsDestination()
    {
        Vector3 direction = (currentDestination - transform.position);
        direction.y = 0f; // Y eksenini sıfırla
        
        if (direction.sqrMagnitude < 0.01f)
        {
            velocity.x = 0f;
            velocity.z = 0f;
            return;
        }
        
        direction.Normalize();
        
        float currentSpeed = walkSpeed;
        if (currentState == AIState.Fleeing)
        {
            currentSpeed *= fleeSpeedMultiplier;
        }
        
        // Smooth movement için velocity kullan
        Vector3 targetVelocity = direction * currentSpeed;
        Vector3 currentHorizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        
        currentHorizontalVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity,
            targetVelocity,
            acceleration * Time.deltaTime
        );
        
        velocity.x = currentHorizontalVelocity.x;
        velocity.z = currentHorizontalVelocity.z;
    }

    /// <summary>
    /// Wandering state güncellemesi
    /// </summary>
    private void UpdateWandering()
    {
        // Passive enemy için scrap ara
        if (enemyType == EnemyType.Passive)
        {
            scrapSearchTimer += Time.deltaTime;
            if (scrapSearchTimer >= scrapSearchInterval)
            {
                scrapSearchTimer = 0f;
                SearchForScrap();
            }
        }

        // Aggressive enemy için player kontrolü
        if (enemyType == EnemyType.Aggressive && playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= playerDetectionRange)
            {
                SetState(AIState.Chasing);
                return;
            }
        }

        // Passive enemy için player yakınlık kontrolü (flee için)
        if (enemyType == EnemyType.Passive && playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= fleeRange)
            {
                SetState(AIState.Fleeing);
                return;
            }
        }

        // Hedefe doğru hareket et
        MoveTowardsDestination();
        
        // Hedefe ulaşıldı mı kontrol et
        float distanceToDestination = Vector3.Distance(transform.position, currentDestination);
        if (distanceToDestination < stoppingDistance)
        {
            wanderTimer += Time.deltaTime;
            if (wanderTimer >= wanderInterval)
            {
                wanderTimer = 0f;
                SetRandomDestinationOnStreet();
            }
        }
    }

    /// <summary>
    /// Chasing state güncellemesi (Aggressive enemy)
    /// </summary>
    private void UpdateChasing()
    {
        if (playerTransform == null)
        {
            SetState(AIState.Wandering);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Saldırı mesafesinde mi?
        if (distanceToPlayer <= attackRange)
        {
            SetState(AIState.Attacking);
            return;
        }

        // Player çok uzakta mı?
        if (distanceToPlayer > playerDetectionRange * 1.5f)
        {
            SetState(AIState.Wandering);
            return;
        }

        // Player'a doğru git
        currentDestination = playerTransform.position;
        MoveTowardsDestination();
    }

    /// <summary>
    /// Attacking state güncellemesi (Aggressive enemy)
    /// </summary>
    private void UpdateAttacking()
    {
        if (playerTransform == null)
        {
            SetState(AIState.Wandering);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Player çok uzakta mı?
        if (distanceToPlayer > attackRange * 1.5f)
        {
            SetState(AIState.Chasing);
            return;
        }

        // Dur (saldırı sırasında hareket etme)
        velocity.x = 0f;
        velocity.z = 0f;

        // Saldırı cooldown kontrolü
        if (attackTimer <= 0f)
        {
            PerformAttack();
            attackTimer = attackCooldown;
        }
        else
        {
            attackTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Collecting state güncellemesi (Passive enemy)
    /// </summary>
    private void UpdateCollecting()
    {
        if (currentTargetScrap == null)
        {
            SetState(AIState.Wandering);
            return;
        }

        float distanceToScrap = Vector3.Distance(transform.position, currentTargetScrap.transform.position);

        // Scrap'a ulaşıldı mı?
        if (distanceToScrap <= scrapCollectionRange)
        {
            CollectScrap(currentTargetScrap);
            currentTargetScrap = null;
            SetState(AIState.Wandering);
            return;
        }

        // Scrap çok uzakta mı?
        if (distanceToScrap > scrapDetectionRange * 2f)
        {
            currentTargetScrap = null;
            SetState(AIState.Wandering);
            return;
        }

        // Scrap'a doğru git
        currentDestination = currentTargetScrap.transform.position;
        MoveTowardsDestination();
    }

    /// <summary>
    /// Fleeing state güncellemesi (Passive enemy)
    /// </summary>
    private void UpdateFleeing()
    {
        if (playerTransform == null)
        {
            SetState(AIState.Wandering);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Player yeterince uzakta mı?
        if (distanceToPlayer > fleeRange * 1.5f)
        {
            SetState(AIState.Wandering);
            return;
        }

        // Player'dan uzaklaş
        Vector3 fleeDirection = (transform.position - playerTransform.position);
        fleeDirection.y = 0f;
        fleeDirection.Normalize();

        currentDestination = transform.position + fleeDirection * wanderRadius;
        MoveTowardsDestination();
    }

    /// <summary>
    /// Street boyunca rastgele bir hedef seç (Z-axis forward)
    /// </summary>
    private void SetRandomDestinationOnStreet()
    {
        // Street yönünde (genelde Z-axis) ileri/geri rastgele bir nokta seç
        Vector3 randomOffset = new Vector3(
            Random.Range(-wanderRadius * 0.3f, wanderRadius * 0.3f), // X ekseninde küçük sapma
            0f,
            Random.Range(-wanderRadius * 0.5f, wanderRadius) // Z ekseninde daha fazla ileri/geri
        );

        // Street direction'ı kullan (genelde Vector3.forward)
        Vector3 targetPosition = startPosition + Quaternion.LookRotation(streetDirection) * randomOffset;
        
        // Raycast ile zemin bul
        RaycastHit hit;
        if (Physics.Raycast(targetPosition + Vector3.up * 10f, Vector3.down, out hit, 20f))
        {
            currentDestination = hit.point;
        }
        else
        {
            currentDestination = targetPosition;
        }
    }

    /// <summary>
    /// Scrap ara (Passive enemy)
    /// </summary>
    private void SearchForScrap()
    {
        if (currentTargetScrap != null)
        {
            return; // Zaten bir scrap hedefi var
        }

        // Yakındaki scrap'ları bul
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, scrapDetectionRange);
        GameObject closestScrap = null;
        float closestDistance = float.MaxValue;

        foreach (Collider col in nearbyColliders)
        {
            // Scrap component'i veya "Scrap" tag'i kontrol et
            Loot loot = col.GetComponent<Loot>();
            if (loot != null && col.gameObject != null)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestScrap = col.gameObject;
                }
            }
        }

        // Scrap bulundu mu?
        if (closestScrap != null)
        {
            currentTargetScrap = closestScrap;
            SetState(AIState.Collecting);
        }
    }

    /// <summary>
    /// Scrap topla (Passive enemy)
    /// </summary>
    private void CollectScrap(GameObject scrap)
    {
        if (scrap == null)
        {
            return;
        }

        // Loot component'i varsa bilgileri al
        Loot loot = scrap.GetComponent<Loot>();
        if (loot != null)
        {
            // EnemyAnimationController ile loot animasyonunu tetikle
            if (animationController != null)
            {
                animationController.FoundLoot();
            }

            DebugLog($"[EnemyAIController] {name}: Collected scrap: {loot.GetItemDisplayName()}");
        }

        // Scrap'ı yok et
        Destroy(scrap);
    }

    /// <summary>
    /// Saldırı yap (Aggressive enemy)
    /// </summary>
    private void PerformAttack()
    {
        if (playerTransform == null)
        {
            return;
        }

        // EnemyAnimationController ile saldırı animasyonunu tetikle
        if (animationController != null)
        {
            animationController.PerformAttack();
        }

        // Player'a hasar ver
        PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
        if (playerHealth != null && !playerHealth.IsDead)
        {
            playerHealth.TakeDamage(1); // Hasar miktarını ayarlayabilirsiniz
            DebugLog($"[EnemyAIController] {name}: Attacking player!");
        }
    }

    /// <summary>
    /// State değiştir
    /// </summary>
    private void SetState(AIState newState)
    {
        if (currentState == newState)
        {
            return;
        }

        currentState = newState;
        // DebugLog($"[EnemyAIController] {name}: State changed to {newState}"); // Gereksiz log - sadece debug için

        // State'e göre hız ayarları (CharacterController için gerekli değil, ama state değişikliği için)
        // Hız MoveTowardsDestination() metodunda kontrol ediliyor
    }

    /// <summary>
    /// Animator'ı güncelle
    /// </summary>
    private void UpdateAnimator()
    {
        if (isDead)
        {
            // Ölüyse animasyonu durdur
            if (animationController != null)
            {
                animationController.SetWalking(false);
            }
            if (animator != null && HasAnimatorParameter("Walking"))
            {
                animator.SetBool("Walking", false);
            }
            return;
        }
        
        // Walking animasyonunu kontrol et
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        bool isMoving = horizontalVelocity.sqrMagnitude > 0.01f; // Threshold'u düşürdük (0.1 -> 0.01) - daha hassas algılama
        
        // Animator mantığına göre:
        // Walking = true olmalı eğer:
        // 1. Hareket ediyorsa (velocity > 0.1)
        // 2. Scrap toplamıyorsa (!isCollectingScrap)
        // 3. Saldırı yapmıyorsa (currentState != Attacking)
        // 4. Diğer animasyonlar çalışmıyorsa (Hitting, Damage, Loot, Damaged false olmalı)
        bool isWalking = isMoving && !isCollectingScrap && currentState != AIState.Attacking;
        
        // Animator'da diğer parametreleri kontrol et
        // Eğer Hitting, Loot, Damage, veya Damaged true ise Walking false olmalı
        if (animator != null)
        {
            if (HasAnimatorParameter("Hitting") && animator.GetBool("Hitting"))
            {
                isWalking = false;
            }
            if (HasAnimatorParameter("Loot") && animator.GetBool("Loot"))
            {
                isWalking = false;
            }
            if (HasAnimatorParameter("Damage") && animator.GetBool("Damage"))
            {
                isWalking = false;
            }
            if (HasAnimatorParameter("Damaged") && animator.GetBool("Damaged"))
            {
                isWalking = false;
            }
            // Death trigger aktifse Walking false olmalı
            // NOT: Trigger parametreleri otomatik reset olur, bu yüzden kontrol etmeye gerek yok
            // Ama ölüyse zaten yürümemeli (isDead kontrolü zaten var)
        }
        
        // EnemyAnimationController varsa onu kullan (otomatik olarak diğer parametreleri kontrol eder)
        if (animationController != null)
        {
            animationController.SetWalking(isWalking);
        }
        
        // Direkt animator'ı da güncelle (EnemyAnimationController yoksa veya yedek olarak)
        // NOT: Animator'da "Walking" parametresi YOK! Walking bir state (default state).
        // Diğer parametreler (Hitting, Damage, Loot, Damaged) false olduğunda otomatik Walking state'ine döner.
        if (animator != null)
        {
            // Walking state'ine dönmek için diğer animasyonları false yap
            if (isWalking)
            {
                if (HasAnimatorParameter("Hitting"))
                {
                    animator.SetBool("Hitting", false);
                }
                if (HasAnimatorParameter("Damage"))
                {
                    animator.SetBool("Damage", false);
                }
                if (HasAnimatorParameter("Loot"))
                {
                    animator.SetBool("Loot", false);
                }
                // Damaged ve Death parametrelerini false yapma, çünkü bunlar duruma bağlı
            }
            
            // Debug için (sadece development build'de)
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Time.frameCount % 120 == 0) // Her 2 saniyede bir log (60 FPS varsayarak)
            {
                bool hitting = HasAnimatorParameter("Hitting") ? animator.GetBool("Hitting") : false;
                bool loot = HasAnimatorParameter("Loot") ? animator.GetBool("Loot") : false;
                bool damage = HasAnimatorParameter("Damage") ? animator.GetBool("Damage") : false;
                bool damaged = HasAnimatorParameter("Damaged") ? animator.GetBool("Damaged") : false;
                // Death artık Trigger parametresi, kontrol etmeye gerek yok (otomatik reset olur)
                // Trigger parametreleri bir frame sonra otomatik false olur, bu yüzden debug'da göstermeye gerek yok
                
                DebugLog($"[EnemyAIController] {name}: isWalking={isWalking}, isMoving={isMoving}, velocity={horizontalVelocity.magnitude:F2}, state={currentState}, " +
                    $"Hitting={hitting}, Loot={loot}, Damage={damage}, Damaged={damaged}, Death=Trigger (auto-reset)");
            }
            #endif
        }
    }
    
    /// <summary>
    /// Animator'da parametre var mı kontrol et
    /// </summary>
    private bool HasAnimatorParameter(string paramName)
    {
        if (animator == null) return false;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Enemy'ye hasar verir (juice efektleri ile)
    /// </summary>
    /// <param name="damage">Hasar miktarı</param>
    /// <param name="hitPoint">Vuruş noktası (VFX için)</param>
    /// <param name="knockbackDirection">Knockback yönü</param>
    public void TakeDamage(int damage, Vector3 hitPoint, Vector3 knockbackDirection)
    {
        if (isDead)
        {
            DebugLog($"[EnemyAIController] {name}: Already dead, cannot take damage.");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Pasif enemy vurulduğunda, scrap toplamayı bırak
        if (enemyType == EnemyType.Passive && !hasBeenAttacked)
        {
            hasBeenAttacked = true;
            currentTargetScrap = null; // Scrap hedefini iptal et
            DebugLog($"[EnemyAIController] {name}: Has been attacked! Stopping scrap collection.");
        }

        DebugLog($"[EnemyAIController] {name}: Took {damage} damage! Current health: {currentHealth}/{maxHealth}");

        // Visual & Audio Feedback
        ApplyHitFeedback(hitPoint, knockbackDirection, damage);

        // EnemyAnimationController ile hasar animasyonunu tetikle
        if (animationController != null)
        {
            animationController.SetHealth(currentHealth);
            animationController.TakeDamage(damage);
        }

        // Passive enemy vurulduğunda kaç
        if (enemyType == EnemyType.Passive && currentState != AIState.Fleeing && !isDead)
        {
            SetState(AIState.Fleeing);
        }

        // Can 0 oldu mu kontrol et
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Eski TakeDamage fonksiyonu (geriye dönük uyumluluk için)
    /// </summary>
    public void TakeDamage(int damage)
    {
        Vector3 hitPoint = transform.position;
        Vector3 knockbackDirection = playerTransform != null 
            ? (transform.position - playerTransform.position).normalized 
            : -transform.forward;
        
        TakeDamage(damage, hitPoint, knockbackDirection);
    }
    
    /// <summary>
    /// Hit feedback efektlerini uygular (material flash, knockback, audio, VFX, floating text)
    /// </summary>
    private void ApplyHitFeedback(Vector3 hitPoint, Vector3 knockbackDirection, int damage)
    {
        // Material Flash: Enemy'nin material'ını beyaz yap (split second)
        StartCoroutine(MaterialFlashCoroutine());
        
        // Knockback: Enemy'yi geriye it
        ApplyKnockback(knockbackDirection);
        
        // Audio: Hasar alma sesi çal
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
            DebugLog($"[EnemyAIController] {name}: Hit sound played!");
        }
        else if (hitSound == null)
        {
            DebugLogWarning($"[EnemyAIController] {name}: Hit sound not assigned! Enemy won't make sound when taking damage.");
        }
        else if (audioSource == null)
        {
            DebugLogWarning($"[EnemyAIController] {name}: AudioSource is null! Cannot play hit sound.");
        }
        
        // VFX: Hit particle effect spawn et
        if (hitVFXPrefab != null)
        {
            GameObject vfx = Instantiate(hitVFXPrefab, hitPoint, Quaternion.identity);
            Destroy(vfx, 5f); // 5 saniye sonra temizle
        }
        
        // Floating Text: Damage number göster (optional)
        if (damageTextPrefab != null)
        {
            GameObject damageText = Instantiate(damageTextPrefab, hitPoint + Vector3.up * 2f, Quaternion.identity);
            // Floating text script'i varsa damage'i set et
            DamageText dt = damageText.GetComponent<DamageText>();
            if (dt != null)
            {
                dt.SetDamage(damage);
            }
            else
            {
                // Basit text mesh varsa
                TMPro.TextMeshPro tmp = damageText.GetComponent<TMPro.TextMeshPro>();
                if (tmp != null)
                {
                    tmp.text = damage.ToString();
                }
            }
        }
    }
    
    /// <summary>
    /// Material Flash Coroutine - Enemy'nin material'ını beyaz yapar ve geri döndürür
    /// </summary>
    private IEnumerator MaterialFlashCoroutine()
    {
        if (enemyRenderers == null || flashMaterials == null)
        {
            yield break;
        }
        
        // Material'ları flash material'a değiştir
        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            if (enemyRenderers[i] != null && flashMaterials[i] != null)
            {
                enemyRenderers[i].material = flashMaterials[i];
            }
        }
        
        // Flash süresini bekle
        yield return new WaitForSeconds(flashDuration);
        
        // Material'ları orijinal haline döndür
        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            if (enemyRenderers[i] != null && originalMaterials[i] != null)
            {
                enemyRenderers[i].material = originalMaterials[i];
            }
        }
    }
    
    /// <summary>
    /// Knockback uygular - Enemy'yi geriye iter
    /// </summary>
    private void ApplyKnockback(Vector3 direction)
    {
        if (knockbackForce <= 0f)
        {
            return;
        }
        
        // Knockback direction'ı normalize et
        direction.y = 0f; // Y eksenini sıfırla (sadece yatay)
        direction.Normalize();
        
        // Yukarı kuvvet ekle
        Vector3 knockbackVector = direction * knockbackForce + Vector3.up * knockbackUpwardForce;
        
        // Knockback velocity'yi ayarla (Update'te uygulanacak)
        knockbackVelocity = knockbackVector;
    }
    
    /// <summary>
    /// Enemy ölür
    /// </summary>
    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        
        // Death sound çal
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
            DebugLog($"[EnemyAIController] {name}: Death sound played!");
        }
        else if (deathSound == null)
        {
            DebugLogWarning($"[EnemyAIController] {name}: Death sound not assigned!");
        }
        
        // Death animasyonu EnemyAnimationController üzerinden zaten ayarlandı (TakeDamage'da Death = 1 yapıldı)
        // Animasyonun bitmesini bekle
        if (animator != null)
        {
            // Death animasyonunun süresini al ve bekle
            StartCoroutine(WaitForDeathAnimationAndDestroy());
        }
        else
        {
            // Animator yoksa direkt yok et
            Destroy(gameObject, 2f);
        }
        
        // Controller'ı durdur (hareketi engelle)
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        // Velocity'yi sıfırla
        velocity = Vector3.zero;
    }
    
    /// <summary>
    /// Ölüm animasyonunun bitmesini bekler ve sonra enemy'yi yok eder
    /// </summary>
    private IEnumerator WaitForDeathAnimationAndDestroy()
    {
        if (animator == null)
        {
            DebugLogWarning($"[EnemyAIController] {name}: Animator is null, destroying enemy in 2 seconds.");
            Destroy(gameObject, 2f);
            yield break;
        }
        
        // Death trigger'ının tetiklendiğinden emin ol
        if (HasAnimatorParameter("Death"))
        {
            // Death artık Trigger parametresi, tekrar tetikle
            animator.SetTrigger("Death");
            DebugLog($"[EnemyAIController] {name}: Death trigger activated!");
        }
        else
        {
            DebugLogWarning($"[EnemyAIController] {name}: Death parameter not found in Animator!");
        }
        
        // Death animasyonunun state'ine geçmesini bekle (maksimum 2 saniye)
        float waitTime = 0f;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        string currentStateName = GetStateName(stateInfo);
        DebugLog($"[EnemyAIController] {name}: Current animator state: '{currentStateName}'");
        
        // Death state'ine geçmesini bekle
        while (waitTime < 2f)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            currentStateName = GetStateName(stateInfo);
            
            // Death state'ine geçti mi kontrol et
            if (currentStateName.Contains("Death") || currentStateName.Contains("death"))
            {
                DebugLog($"[EnemyAIController] {name}: Death animation state reached: '{currentStateName}'");
                break;
            }
            
            yield return null;
            waitTime += Time.deltaTime;
        }
        
        // Death state'ine geçti mi kontrol et
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        currentStateName = GetStateName(stateInfo);
        
        if (!currentStateName.Contains("Death") && !currentStateName.Contains("death"))
        {
            DebugLogWarning($"[EnemyAIController] {name}: Death animation state not reached after 2 seconds! Current state: '{currentStateName}'. Destroying anyway.");
            Destroy(gameObject, 0.5f);
            yield break;
        }
        
        // Death animasyonunun bitmesini bekle
        // Animasyon süresini al (normalizedTime 1.0 olana kadar bekle)
        float animationLength = stateInfo.length;
        DebugLog($"[EnemyAIController] {name}: Death animation length: {animationLength} seconds. Waiting for animation to finish...");
        
        float elapsedTime = 0f;
        float maxWaitTime = animationLength + 1f; // Animasyon süresi + 1 saniye buffer
        
        while (elapsedTime < maxWaitTime)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // Animasyon bitti mi kontrol et (normalizedTime >= 1.0 veya animasyon tekrar başladı)
            if (stateInfo.normalizedTime >= 1.0f)
            {
                DebugLog($"[EnemyAIController] {name}: Death animation finished (normalizedTime: {stateInfo.normalizedTime:F2}).");
                break;
            }
            
            yield return null;
            elapsedTime += Time.deltaTime;
        }
        
        // Animasyon bitti, enemy'yi yok et
        DebugLog($"[EnemyAIController] {name}: Death animation finished, destroying enemy.");
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Animator state ismini al (debug için)
    /// </summary>
    private string GetStateName(AnimatorStateInfo stateInfo)
    {
        // State ismini almak için Animator Controller'dan state'leri kontrol et
        // NOT: Runtime'da AnimatorController'a erişemeyiz, bu yüzden hash kullanıyoruz
        // State ismini bulmak için tüm olası isimleri kontrol ediyoruz
        
        // Bilinen death state isimleri
        string[] possibleDeathStates = { 
            "Standing Death Backward 01", 
            "Death", 
            "Death From Right",
            "Standing Death",
            "Death Backward"
        };
        
        // Hash'leri kontrol et
        foreach (string stateName in possibleDeathStates)
        {
            int hash = Animator.StringToHash(stateName);
            if (hash == stateInfo.fullPathHash || hash == stateInfo.shortNameHash)
            {
                return stateName;
            }
        }
        
        // Fallback: Hash'ten isim çıkaramazsak hash'i döndür
        return $"State_Hash_{stateInfo.fullPathHash}";
    }
    
    /// <summary>
    /// Enemy'nin ölü olup olmadığını döndürür
    /// </summary>
    public bool IsDead => isDead; // Property (PlayerHealth ile tutarlılık için)
    
    /// <summary>
    /// Mevcut canı döndürür
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    /// <summary>
    /// Maksimum canı döndürür
    /// </summary>
    public int GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// Gizmos çiz (Editor'da görselleştirme için)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Wander radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, wanderRadius);

        // Scrap detection range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, scrapDetectionRange);

        // Player detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRange);

        // Attack range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Current destination
        if (Application.isPlaying && currentDestination != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, currentDestination);
            Gizmos.DrawWireSphere(currentDestination, 0.5f);
        }
    }
}
