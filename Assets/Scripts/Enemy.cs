using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    PassiveWalker,      // Pasif yürüyen - loot çok aramayan, düz yürüyen
    PassiveLootCollector, // Pasif loot toplayıcı - sadece loot topluyor, saldırmıyor
    Aggressive          // Agresif - player'a yaklaşınca saldırıyor
}

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider))]
public class Enemy : MonoBehaviour
{
    [Header("Enemy Type")]
    [Tooltip("Enemy türü - Pasif yürüyen, Pasif loot toplayıcı, veya Agresif")]
    [SerializeField] private EnemyType enemyType = EnemyType.PassiveWalker;
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
    [Tooltip("Küçük yükseklikleri geçebilme mesafesi (step offset). Yüksek değer = daha yüksek engelleri geçebilir")]
    [SerializeField, Min(0f)] private float stepOffset = 0.5f;
    [Tooltip("Eğimli yüzeylerde tırmanabileceği maksimum açı (derece)")]
    [SerializeField, Range(0f, 90f)] private float slopeLimit = 45f;

    [Header("Behavior Settings")]
    [Tooltip("Yürüme süresi (saniye) - Artık kullanılmıyor, sadece çarpışmada yön değiştirir")]
    [SerializeField, Min(0.5f)] private float walkDuration = 3f;
    [Tooltip("Idle kalma süresi (saniye) - Artık kullanılmıyor")]
    [SerializeField, Min(0.5f)] private float idleDuration = 2f;
    [Tooltip("Yürüme süresinde rastgelelik - Artık kullanılmıyor")]
    [SerializeField, Min(0f)] private float walkDurationVariation = 1f;
    [Tooltip("Idle süresinde rastgelelik - Artık kullanılmıyor")]
    [SerializeField, Min(0f)] private float idleDurationVariation = 0.5f;
    
    [Header("Wall Detection Settings")]
    [Tooltip("Duvara çarptığını algılamak için gereken süre (saniye)")]
    [SerializeField, Min(0.1f)] private float wallHitDetectionTime = 0.3f;
    [Tooltip("Hareket edilmediğinde duvar algılama için minimum mesafe (birim)")]
    [SerializeField, Min(0.01f)] private float minMovementThreshold = 0.1f;
    [Tooltip("Çarpışma sonrası yön değiştirme açısı (derece) - Daha küçük değer = daha yumuşak dönüş")]
    [SerializeField, Range(30f, 90f)] private float collisionTurnAngle = 60f;
    [Tooltip("Dönüş hızı (daha yüksek = daha hızlı döner, daha düşük = daha yumuşak)")]
    [SerializeField, Min(0.5f)] private float rotationSpeed = 2f;
    
    [Header("Loot Collection Settings")]
    [Tooltip("Loot itemlarını algılama mesafesi")]
    [SerializeField, Min(1f)] private float lootDetectionRange = 5f;
    [Tooltip("Loot toplama mesafesi")]
    [SerializeField, Min(0.5f)] private float lootCollectionRange = 1.5f;
    [Tooltip("Loot arama sıklığı (saniye) - Daha yüksek değer = daha az arama, daha çok düz yürüme")]
    [SerializeField, Min(0.5f)] private float lootSearchInterval = 4f;
    
    [Header("Player Detection Settings (Aggressive Enemy)")]
    [Tooltip("Player'ı algılama mesafesi (sadece agresif enemy için)")]
    [SerializeField, Min(1f)] private float playerDetectionRange = 10f;
    [Tooltip("Player'a saldırma mesafesi")]
    [SerializeField, Min(0.5f)] private float attackRange = 2f;
    [Tooltip("Saldırı hasarı")]
    [SerializeField, Min(1)] private int attackDamage = 1;
    [Tooltip("Saldırı aralığı (saniye)")]
    [SerializeField, Min(0.5f)] private float attackCooldown = 1.5f;
    
    [Header("Movement Behavior")]
    [Tooltip("Nadiren yön değiştirme olasılığı - Artık kullanılmıyor (sadece çarpışmada yön değiştirir)")]
    [SerializeField, Range(0f, 1f)] private float randomDirectionChangeChance = 0.1f;
    [Tooltip("Yön değiştirme kontrol sıklığı - Artık kullanılmıyor")]
    [SerializeField, Min(0.5f)] private float directionCheckInterval = 2f;

    [Header("References")]
    [Tooltip("Animator component. Eğer boşsa otomatik bulunur.")]
    [SerializeField] private Animator animator;
    
    [Header("Juice Settings - Visual & Audio Feedback")]
    [Tooltip("Hit sound effect")]
    [SerializeField] private AudioClip hitSound;
    [Tooltip("Hit VFX particle system (hit point'te spawn olacak)")]
    [SerializeField] private GameObject hitVFXPrefab;
    [Tooltip("Floating damage text prefab (optional)")]
    [SerializeField] private GameObject damageTextPrefab;
    [Tooltip("Material flash süresi (saniye)")]
    [SerializeField, Min(0.01f)] private float flashDuration = 0.1f;
    [Tooltip("Flash rengi (beyaz flash için)")]
    [SerializeField] private Color flashColor = Color.white;
    [Tooltip("Knockback gücü")]
    [SerializeField, Min(0f)] private float knockbackForce = 5f;
    [Tooltip("Knockback yukarı kuvveti")]
    [SerializeField, Min(0f)] private float knockbackUpwardForce = 2f;

    private CharacterController controller;
    private Rigidbody enemyRigidbody;
    private Renderer[] enemyRenderers;
    private Material[] originalMaterials;
    private Material[] flashMaterials;
    private AudioSource audioSource;
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
    
    // Duvara çarpma algılama için
    private Vector3 lastPosition;
    private float wallHitTimer = 0f;
    private bool isStuck = false;
    
    // Loot toplama için
    private Loot currentTargetLoot = null;
    private float lootSearchTimer = 0f;
    private float directionCheckTimer = 0f;
    private Vector3 currentForwardDirection;
    
    // Player detection ve saldırı için
    private Transform playerTransform = null;
    private bool hasBeenAttacked = false; // Vuruldu mu kontrolü (pasif enemy için)
    private bool isChasingPlayer = false;
    private float attackTimer = 0f;
    private float playerDetectionTimer = 0f;
    private float playerDetectionInterval = 0.5f; // Player'ı ne sıklıkla kontrol et
    
    // Enemy envanteri
    private readonly Dictionary<string, int> enemyInventory = new Dictionary<string, int>();

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

        // CharacterController ayarlarını yapılandır
        controller.stepOffset = stepOffset;
        controller.slopeLimit = slopeLimit;

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
        
        // Rigidbody'yi bul veya ekle (knockback için)
        enemyRigidbody = GetComponent<Rigidbody>();
        if (enemyRigidbody == null)
        {
            enemyRigidbody = gameObject.AddComponent<Rigidbody>();
            enemyRigidbody.isKinematic = true; // CharacterController ile çalışması için
            enemyRigidbody.useGravity = false;
        }

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

        startPosition = transform.position;
        currentDestination = startPosition;
        currentHealth = maxHealth;
        currentForwardDirection = transform.forward;
        
        // Player'ı bul (tag ile)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            // Tag yoksa SimplePlayerMovement component'ini ara
            SimplePlayerMovement playerMovement = FindObjectOfType<SimplePlayerMovement>();
            if (playerMovement != null)
            {
                playerTransform = playerMovement.transform;
            }
        }
        
        // Enemy türüne göre ayarları uygula
        ApplyEnemyTypeSettings();
    }
    
    private void ApplyEnemyTypeSettings()
    {
        switch (enemyType)
        {
            case EnemyType.PassiveWalker:
                // Pasif yürüyen - loot aramayı minimize et
                lootSearchInterval = 10f; // Çok seyrek loot ara
                lootDetectionRange = 3f; // Daha küçük algılama mesafesi
                break;
                
            case EnemyType.PassiveLootCollector:
                // Pasif loot toplayıcı - loot aramayı artır
                lootSearchInterval = 2f; // Daha sık loot ara
                lootDetectionRange = 8f; // Daha büyük algılama mesafesi
                break;
                
            case EnemyType.Aggressive:
                // Agresif - player'ı takip et, loot aramayı azalt
                lootSearchInterval = 6f; // Orta seviye loot arama
                lootDetectionRange = 5f;
                break;
        }
    }

    private void Start()
    {
        // Başlangıçta direkt yürümeye başla
        SetWalkingState();
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (controller == null || isDead)
        {
            return;
        }

        // Player detection (agresif enemy için)
        if (enemyType == EnemyType.Aggressive && playerTransform != null)
        {
            playerDetectionTimer += Time.deltaTime;
            if (playerDetectionTimer >= playerDetectionInterval)
            {
                playerDetectionTimer = 0f;
                CheckForPlayer();
            }
        }
        
        // Pasif loot toplayıcı enemy için - vurulmadıysa loot ara
        // Agresif enemy için - player yoksa loot ara
        bool shouldSearchLoot = false;
        if (enemyType == EnemyType.PassiveLootCollector && !hasBeenAttacked)
        {
            shouldSearchLoot = true;
        }
        else if (enemyType == EnemyType.Aggressive && !isChasingPlayer)
        {
            shouldSearchLoot = true;
        }
        else if (enemyType == EnemyType.PassiveWalker)
        {
            shouldSearchLoot = true; // Pasif yürüyen de ara ama çok seyrek
        }
        
        // Loot arama
        if (shouldSearchLoot)
        {
        lootSearchTimer += Time.deltaTime;
        if (lootSearchTimer >= lootSearchInterval)
        {
            lootSearchTimer = 0f;
                // Sadece şu anda loot hedefi yoksa ara (sürekli aramayı önlemek için)
                if (currentTargetLoot == null)
                {
            SearchForLoot();
        }
            }
        }
        
        // Saldırı cooldown
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }

        // Sürekli yürüme durumunda (idle yok)
        // Enemy her zaman yürüyor olmalı (ölmediği sürece)
        if (!isWalking)
        {
            isWalking = true;
        }
        UpdateWalkingState();

        // Yer çekimi uygula
        ApplyGravity();

        // Animator'ı güncelle
        UpdateAnimator();
    }

    private void UpdateWalkingState()
    {
        Vector3 direction;
        
        // Agresif enemy - player'ı takip et
        if (enemyType == EnemyType.Aggressive && isChasingPlayer && playerTransform != null)
        {
            direction = (playerTransform.position - transform.position);
            direction.y = 0f;
            
            float distanceToPlayer = direction.magnitude;
            
            // Player'a yakınsa saldır
            if (distanceToPlayer <= attackRange)
            {
                AttackPlayer();
                return;
            }
            
            // Player çok uzaktaysa takibi bırak
            if (distanceToPlayer > playerDetectionRange * 1.5f)
            {
                isChasingPlayer = false;
                currentTargetLoot = null;
                SetForwardDestination();
            }
        }
        // Pasif loot toplayıcı - vurulmadıysa loot topla
        else if (enemyType == EnemyType.PassiveLootCollector && !hasBeenAttacked && currentTargetLoot != null && currentTargetLoot.gameObject != null)
        {
            direction = (currentTargetLoot.transform.position - transform.position);
            direction.y = 0f;
            
            float distanceToLoot = direction.magnitude;
            
            // Loot'a ulaşıldı mı kontrol et
            if (distanceToLoot <= lootCollectionRange)
            {
                CollectLoot(currentTargetLoot);
                currentTargetLoot = null;
                SetForwardDestination();
                return;
            }
            
            // Loot çok uzaktaysa hedefi iptal et
            if (distanceToLoot > lootDetectionRange * 2f)
            {
                currentTargetLoot = null;
                SetForwardDestination();
            }
        }
        // Eğer loot hedefi varsa ona doğru git (agresif değilse veya player takip etmiyorsa)
        else if (currentTargetLoot != null && currentTargetLoot.gameObject != null && !isChasingPlayer)
        {
            direction = (currentTargetLoot.transform.position - transform.position);
            direction.y = 0f;
            
            float distanceToLoot = direction.magnitude;
            
            // Loot'a ulaşıldı mı kontrol et
            if (distanceToLoot <= lootCollectionRange)
            {
                CollectLoot(currentTargetLoot);
                currentTargetLoot = null;
                // Düz ileri yürümeye devam et
                SetForwardDestination();
                return;
            }
            
            // Loot çok uzaktaysa hedefi iptal et
            if (distanceToLoot > lootDetectionRange * 2f)
            {
                currentTargetLoot = null;
                SetForwardDestination();
            }
        }
        else
        {
            // Loot yoksa, sürekli ileri yönde yürü (mevcut forward direction'ı kullan)
            direction = currentForwardDirection;
            
            // Hedef çok yakınsa veya çok uzaktaysa yeni hedef belirle (sürekli ileri gitmek için)
            float distanceToDestination = Vector3.Distance(transform.position, currentDestination);
            if (distanceToDestination < arrivalDistance || distanceToDestination > wanderRadius * 1.5f)
            {
            SetForwardDestination();
            }
        }

        // Hedefe doğru hareket et
        direction.Normalize();
        float currentSpeed = isRunning ? walkSpeed * runSpeedMultiplier : walkSpeed;
        Vector3 move = direction * currentSpeed * Time.deltaTime;
        
        // Hareketi uygula
        Vector3 positionBeforeMove = transform.position;
        controller.Move(move);
        Vector3 positionAfterMove = transform.position;
        
        // Yatay hareket miktarını kontrol et (Y eksenini hariç tut)
        Vector3 horizontalMovement = positionAfterMove - positionBeforeMove;
        horizontalMovement.y = 0f;
        float movementAmount = horizontalMovement.magnitude;
        
        // Duvara çarpma kontrolü
        if (movementAmount < minMovementThreshold)
        {
            // Hareket çok az, muhtemelen duvara çarptı
            if (!isStuck)
            {
                wallHitTimer = 0f;
                isStuck = true;
            }
            
            wallHitTimer += Time.deltaTime;
            
            // Belirli süre boyunca takılı kaldıysa, yön değiştir
            if (wallHitTimer >= wallHitDetectionTime)
            {
                Debug.Log($"[Enemy] {name}: Detected wall hit! Changing direction...");
                ResetWallDetection();
                currentTargetLoot = null;
                // Yön değiştir (geri dön veya yan tarafa dön)
                ChangeDirectionOnCollision();
                return;
            }
        }
        else
        {
            // Normal hareket ediyor, duvar algılamasını sıfırla
            ResetWallDetection();
        }
        
        // Son pozisyonu güncelle
        lastPosition = transform.position;
        
        // İleri yönü güncelle (sadece önemli hareket olduğunda ve büyük fark varsa)
        // Bu, sürekli yön değişikliğini önler ve rotation'ı sabit tutar
        if (movementAmount > 0.1f) // Daha yüksek threshold (daha az güncelleme)
        {
            // Sadece hareket yönü mevcut yönden çok farklıysa güncelle
            float angleDifference = Vector3.Angle(currentForwardDirection, horizontalMovement.normalized);
            
            // Normal düz yürüme durumunda daha büyük açı farkı gerektir (30 derece)
            float requiredAngle = (currentTargetLoot == null && !isChasingPlayer) ? 30f : 15f;
            
            if (angleDifference > requiredAngle) // Daha büyük fark gerektir
            {
                // Çok yavaş bir şekilde yönü güncelle (ani değişiklikleri önlemek için)
                float updateSpeed = (currentTargetLoot == null && !isChasingPlayer) ? 0.5f : 2f;
                currentForwardDirection = Vector3.Slerp(currentForwardDirection, horizontalMovement.normalized, Time.deltaTime * updateSpeed);
                currentForwardDirection.Normalize();
            }
        }

        // Karakteri hedefe doğru döndür (sadece gerektiğinde)
        if (direction.sqrMagnitude > 0.01f)
        {
            Vector3 targetDirection;
            bool shouldRotate = false;
            float rotationMultiplier = 1f;
            
            // Agresif enemy player'ı takip ediyorsa player'a doğru dön
            if (enemyType == EnemyType.Aggressive && isChasingPlayer && playerTransform != null)
            {
                targetDirection = direction; // Player yönü
                shouldRotate = true;
                rotationMultiplier = 1f; // Normal rotation
            }
            // Loot varsa loot'a doğru dön
            else if (currentTargetLoot != null && !isChasingPlayer)
            {
                targetDirection = direction; // Loot yönü
                shouldRotate = true;
                rotationMultiplier = 0.5f; // Yavaş rotation (loot için)
            }
            // Normal düz yürüme - rotation'ı minimize et veya tamamen devre dışı bırak
            else
            {
                targetDirection = currentForwardDirection;
                
                // Sadece mevcut yön ile hedef yön arasında çok büyük bir fark varsa dön
                float angleDifference = Vector3.Angle(transform.forward, currentForwardDirection);
                
                // 30 dereceden fazla fark varsa çok yavaşça düzelt (sadece büyük sapmalar için)
                if (angleDifference > 30f)
                {
                    shouldRotate = true;
                    rotationMultiplier = 0.1f; // Çok yavaş rotation (düz yürüme için)
                }
                // Küçük farklar için rotation yapma, rotation sabit kalır
                else
                {
                    shouldRotate = false;
                }
            }
            
            // Rotation yapılacaksa yap
            if (shouldRotate)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed * rotationMultiplier);
            }
            // Rotation yapılmayacaksa, sadece forward direction'ı koru (rotation sabit kalır)
        }
    }
    
    private void ResetWallDetection()
    {
        isStuck = false;
        wallHitTimer = 0f;
        lastPosition = transform.position;
    }

    private void SetWalkingState()
    {
        isWalking = true;
        // Düz ileri yön seç
        SetForwardDestination();
    }
    
    private void SetForwardDestination()
    {
        // Mevcut ileri yöne göre hedef belirle
        currentDestination = transform.position + currentForwardDirection * wanderRadius;
        
        // Y eksenini mevcut pozisyondan al
        currentDestination.y = transform.position.y;
    }
    
    private void ChangeDirectionOnCollision()
    {
        // Çarpışma sonrası yön değiştir
        // Daha yumuşak bir açıyla dön (sağa veya sola)
        float turnAngle = Random.Range(collisionTurnAngle * 0.7f, collisionTurnAngle * 1.3f);
        float randomSign = Random.value < 0.5f ? -1f : 1f;
        turnAngle *= randomSign;
        
        // Mevcut yönü döndür (daha yumuşak dönüş için)
        Quaternion rotation = Quaternion.Euler(0f, turnAngle, 0f);
        currentForwardDirection = rotation * currentForwardDirection;
        currentForwardDirection.Normalize();
        
        // Karakteri hemen yeni yöne döndür (takılmayı önlemek için)
        transform.rotation = Quaternion.LookRotation(currentForwardDirection);
        
        // Yeni hedef belirle
            SetForwardDestination();
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

    private void SearchForLoot()
    {
        // Eğer zaten bir loot hedefi varsa kontrol et
        if (currentTargetLoot != null && currentTargetLoot.gameObject != null)
        {
            float distance = Vector3.Distance(transform.position, currentTargetLoot.transform.position);
            if (distance <= lootDetectionRange * 2f)
            {
                return; // Mevcut hedef hala geçerli
            }
            else
            {
                currentTargetLoot = null; // Hedef çok uzakta, normal yürümeye devam et
            }
        }
        
        // Yakındaki loot itemlarını bul (sadece yakındakileri kontrol et)
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, lootDetectionRange);
        Loot closestLoot = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider col in nearbyColliders)
        {
            Loot loot = col.GetComponent<Loot>();
            if (loot != null && loot.gameObject != null)
            {
                float distance = Vector3.Distance(transform.position, loot.transform.position);
                // Sadece yakındaki loot'ları hedefle (çok uzaktakileri görmezden gel)
                if (distance <= lootDetectionRange && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestLoot = loot;
                }
            }
        }
        
        // Loot bulunduysa hedefle, bulunamadıysa normal düz yürümeye devam et
        if (closestLoot != null)
        {
            currentTargetLoot = closestLoot;
            // Loot'a doğru yürümeye başla
            if (!isWalking)
            {
                SetWalkingState();
            }
        }
        // Loot bulunamadıysa hiçbir şey yapma, enemy zaten düz yürüyor
    }
    
    private void CheckForPlayer()
    {
        if (playerTransform == null || enemyType != EnemyType.Aggressive)
        {
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Player algılama mesafesinde mi?
        if (distanceToPlayer <= playerDetectionRange)
        {
            if (!isChasingPlayer)
            {
                Debug.Log($"[Enemy] {name}: Player detected! Starting chase...");
                isChasingPlayer = true;
                currentTargetLoot = null; // Loot'u bırak, player'ı takip et
            }
        }
        else if (distanceToPlayer > playerDetectionRange * 1.5f)
        {
            // Player çok uzakta, takibi bırak
            if (isChasingPlayer)
            {
                Debug.Log($"[Enemy] {name}: Player too far, stopping chase...");
                isChasingPlayer = false;
            }
        }
    }
    
    private void AttackPlayer()
    {
        if (playerTransform == null || attackTimer > 0f)
        {
            return;
        }
        
        // Saldırı animasyonu
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // Player'a hasar ver
        PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
        if (playerHealth != null && !playerHealth.IsDead())
        {
            playerHealth.TakeDamage(attackDamage);
            Debug.Log($"[Enemy] {name}: Attacking player! Damage: {attackDamage}");
        }
        else
        {
            Debug.LogWarning($"[Enemy] {name}: Player has no PlayerHealth component or is already dead!");
        }
        
        // Saldırı cooldown'u başlat
        attackTimer = attackCooldown;
    }
    
    private void CollectLoot(Loot loot)
    {
        if (loot == null || loot.gameObject == null)
        {
            return;
        }
        
        // Loot'tan bilgileri al
        string itemId = loot.GetItemId();
        string itemDisplayName = loot.GetItemDisplayName();
        int scrapValue = loot.GetScrapValue();
        
        // Enemy envanterine ekle
        if (enemyInventory.ContainsKey(itemId))
        {
            enemyInventory[itemId]++;
        }
        else
        {
            enemyInventory[itemId] = 1;
        }
        
        Debug.Log($"[Enemy] {name}: Collected {itemDisplayName} (ItemId: {itemId}). Inventory count: {enemyInventory[itemId]}");
        
        // Loot nesnesini yok et
        Destroy(loot.gameObject);
        currentTargetLoot = null;
    }

    private void UpdateAnimator()
    {
        if (animator == null)
        {
            return;
        }

        // Walk parametresini güncelle
        animator.SetBool("Walk", isWalking);

        // Run parametresini güncelle (düşük can veya player takibi)
        bool shouldRun = isRunning || (enemyType == EnemyType.Aggressive && isChasingPlayer);
        animator.SetBool("Run", shouldRun);
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
            Debug.Log($"[Enemy] {name}: Already dead, cannot take damage.");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Pasif loot toplayıcı enemy vurulduğunda, loot toplamayı bırak
        if (enemyType == EnemyType.PassiveLootCollector && !hasBeenAttacked)
        {
            hasBeenAttacked = true;
            currentTargetLoot = null; // Loot hedefini iptal et
            Debug.Log($"[Enemy] {name}: Has been attacked! Stopping loot collection.");
        }

        Debug.Log($"[Enemy] {name}: Took {damage} damage! Current health: {currentHealth}/{maxHealth}");

        // Visual & Audio Feedback
        ApplyHitFeedback(hitPoint, knockbackDirection, damage);

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
        
        // Audio: Hit sound çal
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
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
    private System.Collections.IEnumerator MaterialFlashCoroutine()
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
        
        // CharacterController kullanıyorsak velocity ile knockback uygula
        if (controller != null)
        {
            velocity += knockbackVector;
        }
        // Rigidbody varsa force ile uygula
        else if (enemyRigidbody != null && !enemyRigidbody.isKinematic)
        {
            enemyRigidbody.AddForce(knockbackVector, ForceMode.Impulse);
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
        
        // Enemy envanterini player'a aktar
        TransferInventoryToPlayer();

        // Enemy'yi birkaç saniye sonra yok et (animasyon bitince)
        Destroy(gameObject, 5f);
    }
    
    private void TransferInventoryToPlayer()
    {
        if (InventoryManager.instance == null)
        {
            Debug.LogWarning($"[Enemy] {name}: Cannot transfer inventory - InventoryManager not found!");
            return;
        }
        
        if (enemyInventory.Count == 0)
        {
            return; // Envanter boş
        }
        
        int totalItems = 0;
        foreach (KeyValuePair<string, int> kvp in enemyInventory)
        {
            string itemId = kvp.Key;
            int count = kvp.Value;
            
            // ItemId'den display name oluştur
            string displayName = FormatItemIdAsDisplayName(itemId);
            
            // Scrap value'yu itemId'den çıkar
            int scrapValue = 1;
            if (itemId.Contains("scrap_value5"))
            {
                scrapValue = 5;
            }
            else if (itemId.Contains("scrap_value10"))
            {
                scrapValue = 10;
            }
            
            // Her item için player envanterine ekle
            for (int i = 0; i < count; i++)
            {
                Scrap scrap = new Scrap(itemId, displayName, scrapValue);
                InventoryManager.instance.AddScrap(scrap);
                totalItems++;
            }
        }
        
        Debug.Log($"[Enemy] {name}: Transferred {totalItems} items to player inventory.");
        enemyInventory.Clear();
    }
    
    private string FormatItemIdAsDisplayName(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return "Unknown Item";

        // Alt çizgileri ve tireleri boşlukla değiştir, kelimelerin ilk harfini büyük yap
        string formatted = itemId.Replace('_', ' ').Replace('-', ' ');
        string[] words = formatted.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpper(words[i][0]) + (words[i].Length > 1 ? words[i].Substring(1).ToLower() : "");
            }
        }
        
        return string.Join(" ", words);
    }

    private void OnDrawGizmosSelected()
    {
        // Wander radius'u görselleştir
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(center, wanderRadius);
    }
}

