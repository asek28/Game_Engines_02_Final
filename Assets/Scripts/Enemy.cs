using System.Collections.Generic;
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
    [Tooltip("Küçük yükseklikleri geçebilme mesafesi (step offset). Yüksek değer = daha yüksek engelleri geçebilir")]
    [SerializeField, Min(0f)] private float stepOffset = 0.5f;
    [Tooltip("Eğimli yüzeylerde tırmanabileceği maksimum açı (derece)")]
    [SerializeField, Range(0f, 90f)] private float slopeLimit = 45f;

    [Header("Behavior Settings")]
    [Tooltip("Yürüme süresi (saniye)")]
    [SerializeField, Min(0.5f)] private float walkDuration = 3f;
    [Tooltip("Idle kalma süresi (saniye)")]
    [SerializeField, Min(0.5f)] private float idleDuration = 2f;
    [Tooltip("Yürüme süresinde rastgelelik (örn: 3 ± 1 saniye)")]
    [SerializeField, Min(0f)] private float walkDurationVariation = 1f;
    [Tooltip("Idle süresinde rastgelelik (örn: 2 ± 0.5 saniye)")]
    [SerializeField, Min(0f)] private float idleDurationVariation = 0.5f;
    
    [Header("Wall Detection Settings")]
    [Tooltip("Duvara çarptığını algılamak için gereken süre (saniye)")]
    [SerializeField, Min(0.1f)] private float wallHitDetectionTime = 0.5f;
    [Tooltip("Hareket edilmediğinde duvar algılama için minimum mesafe (birim)")]
    [SerializeField, Min(0.01f)] private float minMovementThreshold = 0.1f;
    
    [Header("Loot Collection Settings")]
    [Tooltip("Loot itemlarını algılama mesafesi")]
    [SerializeField, Min(1f)] private float lootDetectionRange = 5f;
    [Tooltip("Loot toplama mesafesi")]
    [SerializeField, Min(0.5f)] private float lootCollectionRange = 1.5f;
    [Tooltip("Loot arama sıklığı (saniye)")]
    [SerializeField, Min(0.1f)] private float lootSearchInterval = 0.5f;
    
    [Header("Movement Behavior")]
    [Tooltip("Nadiren yön değiştirme olasılığı (0-1 arası, yüksek = daha sık yön değiştirir)")]
    [SerializeField, Range(0f, 1f)] private float randomDirectionChangeChance = 0.1f;
    [Tooltip("Yön değiştirme kontrol sıklığı (saniye)")]
    [SerializeField, Min(0.5f)] private float directionCheckInterval = 2f;

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
    
    // Duvara çarpma algılama için
    private Vector3 lastPosition;
    private float wallHitTimer = 0f;
    private bool isStuck = false;
    
    // Loot toplama için
    private Loot currentTargetLoot = null;
    private float lootSearchTimer = 0f;
    private float directionCheckTimer = 0f;
    private Vector3 currentForwardDirection;
    
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

        startPosition = transform.position;
        currentDestination = startPosition;
        currentHealth = maxHealth;
        currentForwardDirection = transform.forward;
    }

    private void Start()
    {
        // Başlangıçta idle durumunda başla
        SetIdleState();
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (controller == null || isDead)
        {
            return;
        }

        // State timer'ı güncelle
        stateTimer += Time.deltaTime;
        
        // Loot arama
        lootSearchTimer += Time.deltaTime;
        if (lootSearchTimer >= lootSearchInterval)
        {
            lootSearchTimer = 0f;
            SearchForLoot();
        }
        
        // Yön değiştirme kontrolü
        directionCheckTimer += Time.deltaTime;
        if (directionCheckTimer >= directionCheckInterval && isWalking)
        {
            directionCheckTimer = 0f;
            CheckForRandomDirectionChange();
        }

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
        Vector3 direction;
        
        // Eğer loot hedefi varsa ona doğru git
        if (currentTargetLoot != null && currentTargetLoot.gameObject != null)
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
            // Normal hedefe doğru hareket et
            direction = (currentDestination - transform.position);
            direction.y = 0f;
        }

        float distanceToDestination = direction.magnitude;

        // Hedefe ulaşıldı mı kontrol et (loot değilse)
        if (currentTargetLoot == null && distanceToDestination < arrivalDistance)
        {
            // Hedefe ulaşıldı, yeni ileri yön seç
            ResetWallDetection();
            SetForwardDestination();
            return;
        }

        // Süre doldu mu kontrol et
        if (stateTimer >= currentStateDuration)
        {
            // Yürüme süresi doldu, yeni ileri yön seç
            ResetWallDetection();
            SetForwardDestination();
            return;
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
            
            // Belirli süre boyunca takılı kaldıysa, yeni hedef seç
            if (wallHitTimer >= wallHitDetectionTime)
            {
                Debug.Log($"[Enemy] {name}: Detected wall hit! Changing direction...");
                ResetWallDetection();
                currentTargetLoot = null;
                // Yeni yön seç ve kısa bir idle süresi ver
                SetIdleState();
                // Idle süresini kısalt (hızlıca yeni yöne geçsin)
                currentStateDuration = 0.3f;
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
        
        // İleri yönü güncelle
        if (movementAmount > 0.01f)
        {
            currentForwardDirection = horizontalMovement.normalized;
        }

        // Karakteri hedefe doğru döndür
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
    
    private void ResetWallDetection()
    {
        isStuck = false;
        wallHitTimer = 0f;
        lastPosition = transform.position;
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
    
    private void CheckForRandomDirectionChange()
    {
        // Nadiren yön değiştir
        if (Random.value < randomDirectionChangeChance)
        {
            // Rastgele yeni bir ileri yön seç
            float randomAngle = Random.Range(0f, 360f);
            currentForwardDirection = new Vector3(
                Mathf.Sin(randomAngle * Mathf.Deg2Rad),
                0f,
                Mathf.Cos(randomAngle * Mathf.Deg2Rad)
            );
            SetForwardDestination();
        }
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
                currentTargetLoot = null; // Hedef çok uzakta
            }
        }
        
        // Yakındaki loot itemlarını bul
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, lootDetectionRange);
        Loot closestLoot = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider col in nearbyColliders)
        {
            Loot loot = col.GetComponent<Loot>();
            if (loot != null && loot.gameObject != null)
            {
                float distance = Vector3.Distance(transform.position, loot.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestLoot = loot;
                }
            }
        }
        
        if (closestLoot != null)
        {
            currentTargetLoot = closestLoot;
            // Loot'a doğru yürümeye başla
            if (!isWalking)
            {
                SetWalkingState();
            }
        }
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

