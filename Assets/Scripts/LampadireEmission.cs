using UnityEngine;

/// <summary>
/// Lambadirelerin akşam olunca Light component ile parlamasını sağlar
/// Mesh'in üst kısmına Light ekler, akşamları açılır sabahları kapanır
/// </summary>
public class LampadireEmission : MonoBehaviour
{
    [Header("Light Settings")]
    [Tooltip("Light rengi")]
    [SerializeField] private Color lightColor = new Color(1f, 0.9f, 0.7f, 1f); // Sıcak sarı-turuncu
    [Tooltip("Light yoğunluğu (gece)")]
    [SerializeField, Min(0f)] private float lightIntensity = 2f;
    [Tooltip("Light menzili")]
    [SerializeField, Min(0.1f)] private float lightRange = 10f;
    [Tooltip("Light tipi")]
    [SerializeField] private LightType lightType = LightType.Point;
    [Tooltip("Shadow tipi")]
    [SerializeField] private LightShadows shadowType = LightShadows.Soft;
    [Tooltip("Light'ın mesh üstünden yukarı offset (birim)")]
    [SerializeField, Min(0f)] private float lightHeightOffset = 0.5f;
    
    [Header("Day/Night Cycle Settings")]
    [Tooltip("Akşam başlangıç saati (0-24 arası, örn: 18 = akşam 6)")]
    [SerializeField, Range(0f, 24f)] private float eveningStartHour = 18f;
    [Tooltip("Sabah bitiş saati (0-24 arası, örn: 6 = sabah 6)")]
    [SerializeField, Range(0f, 24f)] private float morningEndHour = 6f;
    [Tooltip("Geçiş süresi (saniye) - Yumuşak açılma/kapanma")]
    [SerializeField, Min(0f)] private float transitionDuration = 5f;
    
    private Renderer[] renderers;
    private GameObject lightGameObject;
    private Light lightComponent;
    private DayNightCycle dayNightCycle;
    private bool isNightTime = false;
    private float transitionTimer = 0f;
    private bool lightCreated = false;
    
    private void Awake()
    {
        // DayNightCycle'i bul
        dayNightCycle = FindObjectOfType<DayNightCycle>();
        if (dayNightCycle == null)
        {
            Debug.LogWarning($"[LampadireEmission] {name}: DayNightCycle bulunamadı! Light çalışmayacak.");
            enabled = false;
            return;
        }
        
        // Renderer'ları bul
        renderers = GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0)
        {
            renderers = new Renderer[] { GetComponent<Renderer>() };
        }
    }
    
    private void Start()
    {
        // Light'ı oluştur
        CreateLight();
        
        // İlk durumu kontrol et
        UpdateLightState();
    }
    
    private void Update()
    {
        if (dayNightCycle == null || lightComponent == null)
        {
            return;
        }
        
        // Gün döngüsüne göre light durumunu güncelle
        UpdateLightState();
    }
    
    /// <summary>
    /// Mesh'in üst kısmına Light component ekler
    /// </summary>
    private void CreateLight()
    {
        if (lightCreated)
        {
            return;
        }
        
        // En büyük renderer'ı bul (ana mesh)
        Renderer mainRenderer = null;
        float maxBounds = 0f;
        
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }
            
            float boundsSize = renderer.bounds.size.magnitude;
            if (boundsSize > maxBounds)
            {
                maxBounds = boundsSize;
                mainRenderer = renderer;
            }
        }
        
        if (mainRenderer == null)
        {
            Debug.LogWarning($"[LampadireEmission] {name}: Renderer bulunamadı! Light eklenemedi.");
            return;
        }
        
        // Zaten Light var mı kontrol et
        Light existingLight = GetComponentInChildren<Light>();
        if (existingLight != null)
        {
            Debug.Log($"[LampadireEmission] {name}: Zaten Light component'i var, mevcut light kullanılıyor.");
            lightComponent = existingLight;
            lightGameObject = existingLight.gameObject;
            lightCreated = true;
            return;
        }
        
        // Mesh'in üst kısmını hesapla (bounds kullanarak)
        Bounds meshBounds = mainRenderer.bounds;
        Vector3 topPosition = new Vector3(
            meshBounds.center.x,
            meshBounds.max.y + lightHeightOffset, // Mesh'in en üst noktası + offset
            meshBounds.center.z
        );
        
        // Light GameObject'i oluştur
        lightGameObject = new GameObject($"{name}_Light");
        lightGameObject.transform.SetParent(transform);
        lightGameObject.transform.position = topPosition;
        lightGameObject.transform.rotation = Quaternion.identity;
        
        // Light component ekle
        lightComponent = lightGameObject.AddComponent<Light>();
        lightComponent.type = lightType;
        lightComponent.color = lightColor;
        lightComponent.intensity = 0f; // Başlangıçta kapalı
        lightComponent.range = lightRange;
        lightComponent.shadows = shadowType;
        lightComponent.enabled = false; // Başlangıçta kapalı
        
        lightCreated = true;
        
        Debug.Log($"[LampadireEmission] {name}: Light eklendi. Pozisyon: {topPosition}, Mesh üstü: {meshBounds.max.y}");
    }
    
    /// <summary>
    /// Gün döngüsüne göre light'ı açıp kapatır
    /// </summary>
    private void UpdateLightState()
    {
        if (dayNightCycle == null || lightComponent == null)
        {
            return;
        }
        
        // Günün saatini al
        float timeOfDay = dayNightCycle.GetTimeOfDay();
        
        // Gece mi kontrol et (akşam 18:00 - sabah 6:00 arası)
        bool shouldBeNight = false;
        
        if (eveningStartHour > morningEndHour)
        {
            // Normal durum: 18:00 - 24:00 ve 0:00 - 6:00
            shouldBeNight = timeOfDay >= eveningStartHour || timeOfDay < morningEndHour;
        }
        else
        {
            // Ters durum (örn: 6:00 - 18:00 arası gece)
            shouldBeNight = timeOfDay >= eveningStartHour && timeOfDay < morningEndHour;
        }
        
        // Geçiş animasyonu
        if (shouldBeNight != isNightTime)
        {
            transitionTimer += Time.deltaTime;
            float transitionProgress = Mathf.Clamp01(transitionTimer / transitionDuration);
            
            if (transitionProgress >= 1f)
            {
                isNightTime = shouldBeNight;
                transitionTimer = 0f;
            }
        }
        else
        {
            transitionTimer = 0f;
        }
        
        // Light intensity hesapla
        float targetIntensity = shouldBeNight ? lightIntensity : 0f;
        
        // Geçiş animasyonu varsa lerp yap
        if (transitionTimer > 0f && transitionDuration > 0f)
        {
            float currentIntensity = isNightTime ? lightIntensity : 0f;
            float lerpProgress = Mathf.Clamp01(transitionTimer / transitionDuration);
            targetIntensity = Mathf.Lerp(currentIntensity, shouldBeNight ? lightIntensity : 0f, lerpProgress);
        }
        
        // Light'ı güncelle
        if (lightComponent != null)
        {
            lightComponent.enabled = targetIntensity > 0.01f;
            lightComponent.intensity = targetIntensity;
            lightComponent.color = lightColor;
        }
    }
    
    private void OnDestroy()
    {
        // Light GameObject'ini temizle (opsiyonel - kalıcı olmasını istiyorsanız kaldırın)
        // if (lightGameObject != null)
        // {
        //     Destroy(lightGameObject);
        // }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Light pozisyonunu görselleştir
        if (lightGameObject != null && lightComponent != null)
        {
            Gizmos.color = lightColor;
            Gizmos.DrawWireSphere(lightGameObject.transform.position, 0.5f);
            Gizmos.DrawLine(lightGameObject.transform.position, lightGameObject.transform.position + Vector3.down * 2f);
        }
    }
}

