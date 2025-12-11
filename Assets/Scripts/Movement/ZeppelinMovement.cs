using UnityEngine;

public class ZeppelinMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Zeppelin'in hareket hızı")]
    [SerializeField, Min(0.1f)] private float moveSpeed = 5f;
    [Tooltip("Zeppelin'in kat edeceği maksimum mesafe (yok olmadan önce)")]
    [SerializeField, Min(10f)] private float maxTravelDistance = 100f;
    [Tooltip("Yukarı-aşağı salınım genliği (zeplin gibi)")]
    [SerializeField, Min(0f)] private float verticalOscillationAmplitude = 2f;
    [Tooltip("Yukarı-aşağı salınım hızı")]
    [SerializeField, Min(0.1f)] private float verticalOscillationSpeed = 1f;
    
    [Header("Spawn Settings")]
    [Tooltip("Kaç günde bir spawn olacak")]
    [SerializeField, Min(1)] private int spawnInterval = 4;
    [Tooltip("Spawn olacağı yükseklik")]
    [SerializeField, Min(0f)] private float spawnHeight = 20f;
    
    [Header("Rotation Settings")]
    [Tooltip("Hafif yatay salınım (zeplin gibi sallanma)")]
    [SerializeField, Min(0f)] private float horizontalOscillationAmplitude = 5f;
    [Tooltip("Yatay salınım hızı")]
    [SerializeField, Min(0.1f)] private float horizontalOscillationSpeed = 0.5f;
    
    private Vector3 startPosition;
    private Vector3 currentPosition;
    private float distanceTraveled = 0f;
    private bool isActive = false;
    private float verticalOscillationTime = 0f;
    private float horizontalOscillationTime = 0f;
    private Vector3 moveDirection;
    private DayNightCycle dayNightCycle;
    private int lastSpawnDay = 0;
    
    private void Awake()
    {
        // Başlangıç pozisyonunu kaydet
        startPosition = transform.position;
        currentPosition = startPosition;
        
        // DayNightCycle'i bul
        dayNightCycle = FindObjectOfType<DayNightCycle>();
        if (dayNightCycle == null)
        {
            Debug.LogWarning("[ZeppelinMovement] DayNightCycle not found! Zeppelin will not spawn automatically.");
        }
        
        // İlk spawn kontrolü
        if (dayNightCycle != null)
        {
            int currentDay = dayNightCycle.GetCurrentDay();
            CheckSpawnCondition(currentDay);
        }
    }
    
    private void Start()
    {
        // DayNightCycle event'ini dinle
        if (dayNightCycle != null)
        {
            DayNightCycle.OnDayChanged += OnDayChanged;
        }
        
        // Başlangıçta gizle (spawn olana kadar)
        if (!isActive)
        {
            SetActive(false);
        }
    }
    
    private void OnDestroy()
    {
        // Event dinleyicisini kaldır
        if (dayNightCycle != null)
        {
            DayNightCycle.OnDayChanged -= OnDayChanged;
        }
    }
    
    private void OnDayChanged(int newDay)
    {
        CheckSpawnCondition(newDay);
    }
    
    private void CheckSpawnCondition(int currentDay)
    {
        // 4 günde bir spawn ol (gün 4, 8, 12, 16, ...)
        if (currentDay % spawnInterval == 0 && currentDay != lastSpawnDay)
        {
            Spawn();
            lastSpawnDay = currentDay;
        }
    }
    
    private void Spawn()
    {
        // Başlangıç pozisyonuna dön
        transform.position = new Vector3(startPosition.x, startPosition.y + spawnHeight, startPosition.z);
        currentPosition = transform.position;
        distanceTraveled = 0f;
        isActive = true;
        
        // Rastgele bir yön seç (ileri doğru)
        float randomAngle = Random.Range(-30f, 30f); // -30 ile +30 derece arası
        moveDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;
        moveDirection.Normalize();
        
        // Zeppelin'i görünür yap
        SetActive(true);
        
        Debug.Log($"[ZeppelinMovement] Zeppelin spawned at day {dayNightCycle.GetCurrentDay()}!");
    }
    
    private void SetActive(bool active)
    {
        isActive = active;
        
        // Tüm child'ları ve kendini görünür/gizli yap
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = active;
        }
        
        // Collider'ları etkinleştir/devre dışı bırak
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = active;
        }
    }
    
    private void Update()
    {
        if (!isActive)
        {
            return;
        }
        
        // Hareket zamanlarını güncelle
        verticalOscillationTime += Time.deltaTime * verticalOscillationSpeed;
        horizontalOscillationTime += Time.deltaTime * horizontalOscillationSpeed;
        
        // İleri hareket
        Vector3 forwardMovement = moveDirection * moveSpeed * Time.deltaTime;
        distanceTraveled += forwardMovement.magnitude;
        
        // Yukarı-aşağı salınım (zeplin gibi)
        float verticalOffset = Mathf.Sin(verticalOscillationTime) * verticalOscillationAmplitude;
        
        // Yatay salınım (hafif sallanma)
        float horizontalOffset = Mathf.Sin(horizontalOscillationTime) * horizontalOscillationAmplitude;
        Vector3 horizontalOffsetVector = transform.right * horizontalOffset;
        
        // Pozisyonu güncelle
        currentPosition += forwardMovement;
        currentPosition.y = startPosition.y + spawnHeight + verticalOffset;
        currentPosition += horizontalOffsetVector;
        
        transform.position = currentPosition;
        
        // Hafif yatay salınım için rotation
        float rollAngle = Mathf.Sin(horizontalOscillationTime * 0.5f) * horizontalOscillationAmplitude * 0.5f;
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection) * Quaternion.Euler(0f, 0f, rollAngle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
        
        // Maksimum mesafeye ulaşıldı mı kontrol et
        if (distanceTraveled >= maxTravelDistance)
        {
            Despawn();
        }
    }
    
    private void Despawn()
    {
        isActive = false;
        SetActive(false);
        
        Debug.Log($"[ZeppelinMovement] Zeppelin despawned after traveling {distanceTraveled:F1} units.");
        
        // Pozisyonu başlangıç pozisyonuna sıfırla (görünmez)
        transform.position = startPosition;
        currentPosition = startPosition;
        distanceTraveled = 0f;
    }
    
    /// <summary>
    /// Zeppelin'i manuel olarak spawn eder
    /// </summary>
    public void ManualSpawn()
    {
        Spawn();
    }
    
    /// <summary>
    /// Zeppelin'i manuel olarak despawn eder
    /// </summary>
    public void ManualDespawn()
    {
        Despawn();
    }
    
    /// <summary>
    /// Zeppelin aktif mi kontrol eder
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Spawn pozisyonunu görselleştir
        Gizmos.color = Color.cyan;
        Vector3 spawnPos = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(new Vector3(spawnPos.x, spawnPos.y + spawnHeight, spawnPos.z), 2f);
        
        // Maksimum mesafeyi görselleştir
        if (Application.isPlaying && isActive)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + moveDirection * (maxTravelDistance - distanceTraveled));
        }
    }
}

