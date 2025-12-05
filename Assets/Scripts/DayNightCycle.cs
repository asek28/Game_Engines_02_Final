using System;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public static event Action OnDayComplete;
    public static event Action<int> OnDayChanged; // Gün değiştiğinde mevcut gün sayısını gönderir
    
    [Header("Time Settings")]
    [Tooltip("Bir günün süresi (saniye cinsinden). Varsayılan: 120 saniye (2 dakika)")]
    [SerializeField, Min(1f)] private float dayDuration = 120f;
    
    [Header("Day Counter")]
    [Tooltip("Mevcut gün sayısı")]
    [SerializeField] private int currentDay = 1;
    
    [Header("Rain Effect Settings")]
    [Tooltip("RainEffect GameObject (Player'ın child'ı). Eğer boşsa otomatik bulunur.")]
    [SerializeField] private GameObject rainEffect;
    [Tooltip("Kaç günde bir yağmur yağacak")]
    [SerializeField, Min(1)] private int rainInterval = 4;

    [Header("Sun Rotation")]
    [Tooltip("Güneşin başlangıç açısı (X ekseni). 0 = ufukta, 90 = tepede")]
    [SerializeField, Range(-90f, 90f)] private float startAngle = -10f;
    [Tooltip("Güneşin maksimum yüksekliği (X ekseni). 90 = tam tepede")]
    [SerializeField, Range(0f, 90f)] private float maxSunHeight = 60f;

    [Header("Light Settings")]
    [Tooltip("Gündüz ışık yoğunluğu")]
    [SerializeField, Min(0f)] private float dayIntensity = 1f;
    [Tooltip("Gece ışık yoğunluğu")]
    [SerializeField, Min(0f)] private float nightIntensity = 0.1f;
    [Tooltip("Gündüz ışık rengi")]
    [SerializeField] private Color dayColor = new Color(1f, 0.95f, 0.8f);
    [Tooltip("Gece ışık rengi (ay ışığı)")]
    [SerializeField] private Color nightColor = new Color(0.5f, 0.6f, 1f);

    [Header("References")]
    [Tooltip("Directional Light (Güneş). Eğer boşsa otomatik bulunur.")]
    [SerializeField] private Light sunLight;

    private float currentTime = 0f;
    private bool isInitialized = false;

    private void Awake()
    {
        if (sunLight == null)
        {
            sunLight = FindObjectOfType<Light>();
            if (sunLight == null || sunLight.type != LightType.Directional)
            {
                Debug.LogWarning("DayNightCycle: No Directional Light found in scene. Please assign one manually.");
                return;
            }
        }

        if (sunLight.type != LightType.Directional)
        {
            Debug.LogWarning("DayNightCycle: Assigned light is not a Directional Light. Please assign a Directional Light.");
            return;
        }

        // RainEffect'i bul (Player'ın child'ı olarak)
        if (rainEffect == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                // Tag yoksa isimle bul
                player = GameObject.Find("Player");
            }
            
            if (player != null)
            {
                // Player'ın child'ları arasında RainEffect'i ara
                Transform rainTransform = player.transform.Find("RainEffect");
                if (rainTransform != null)
                {
                    rainEffect = rainTransform.gameObject;
                }
                else
                {
                    // İsim farklı olabilir, tüm child'ları kontrol et
                    foreach (Transform child in player.transform)
                    {
                        if (child.name.ToLower().Contains("rain"))
                        {
                            rainEffect = child.gameObject;
                            break;
                        }
                    }
                }
            }
            
            if (rainEffect == null)
            {
                Debug.LogWarning("DayNightCycle: RainEffect GameObject not found. Please assign it manually in the Inspector.");
            }
        }

        isInitialized = true;
    }

    private void Start()
    {
        if (isInitialized)
        {
            UpdateSunRotation(0f);
            UpdateRainEffect();
        }
    }

    private void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        // Zamanı güncelle
        float previousTime = currentTime;
        currentTime += Time.deltaTime;
        
        if (currentTime >= dayDuration)
        {
            currentTime = 0f; // Günü sıfırla
            
            // Gün sayısını artır
            currentDay++;
            
            // Gün değişti eventini tetikle
            if (OnDayChanged != null)
            {
                OnDayChanged.Invoke(currentDay);
            }
            
            // Gün tamamlandı eventini tetikle
            if (OnDayComplete != null)
            {
                OnDayComplete.Invoke();
            }
            
            // Yağmur efektini güncelle
            UpdateRainEffect();
        }

        // Güneş rotasyonunu güncelle
        float normalizedTime = currentTime / dayDuration; // 0.0 - 1.0 arası
        UpdateSunRotation(normalizedTime);
    }

    private void UpdateSunRotation(float normalizedTime)
    {
        // Güneşin rotasyonunu hesapla
        // 0.0 = gece (güneş ufukta), 0.5 = öğle (güneş tepede), 1.0 = gece (güneş ufukta)
        
        float sunAngle;
        if (normalizedTime < 0.5f)
        {
            // Sabah: -10° -> 60° (0.0 -> 0.5)
            float morningProgress = normalizedTime * 2f; // 0.0 -> 1.0
            sunAngle = Mathf.Lerp(startAngle, maxSunHeight, morningProgress);
        }
        else
        {
            // Akşam: 60° -> -10° (0.5 -> 1.0)
            float eveningProgress = (normalizedTime - 0.5f) * 2f; // 0.0 -> 1.0
            sunAngle = Mathf.Lerp(maxSunHeight, startAngle, eveningProgress);
        }

        // Y ekseni etrafında döndür (güneş doğudan batıya)
        float yRotation = normalizedTime * 360f; // 0° -> 360°

        // Rotasyonu uygula
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, yRotation, 0f);

        // Işık yoğunluğunu ve rengini güncelle
        UpdateLightProperties(normalizedTime, sunAngle);
    }

    private void UpdateLightProperties(float normalizedTime, float sunAngle)
    {
        // Güneş ufukta ise (gece) düşük yoğunluk, tepede ise (gündüz) yüksek yoğunluk
        float intensityFactor = Mathf.Clamp01((sunAngle - startAngle) / (maxSunHeight - startAngle));
        sunLight.intensity = Mathf.Lerp(nightIntensity, dayIntensity, intensityFactor);

        // Renk geçişi
        sunLight.color = Color.Lerp(nightColor, dayColor, intensityFactor);
    }

    /// <summary>
    /// Mevcut zamanı 0-1 arası normalize edilmiş değer olarak döndürür (0 = gece başlangıcı, 0.5 = öğle, 1.0 = gece başlangıcı)
    /// </summary>
    public float GetNormalizedTime()
    {
        return currentTime / dayDuration;
    }

    /// <summary>
    /// Günün hangi saatinde olduğumuzu 0-24 arası değer olarak döndürür
    /// </summary>
    public float GetTimeOfDay()
    {
        return GetNormalizedTime() * 24f;
    }

    /// <summary>
    /// Gündüz mü gece mi olduğunu döndürür
    /// </summary>
    public bool IsDayTime()
    {
        float normalizedTime = GetNormalizedTime();
        return normalizedTime > 0.25f && normalizedTime < 0.75f; // Sabah 6 - Akşam 6 arası gündüz
    }

    /// <summary>
    /// Zamanı manuel olarak ayarlar (0-1 arası normalize edilmiş değer)
    /// </summary>
    public void SetTime(float normalizedTime)
    {
        currentTime = Mathf.Clamp01(normalizedTime) * dayDuration;
    }

    /// <summary>
    /// Yağmur efektini gün sayısına göre açıp kapatır
    /// </summary>
    private void UpdateRainEffect()
    {
        if (rainEffect == null)
        {
            return;
        }

        // 4 günde bir yağmur yağacak (gün 4, 8, 12, 16, ...)
        bool shouldRain = (currentDay % rainInterval == 0);
        
        rainEffect.SetActive(shouldRain);
        
        if (shouldRain)
        {
            Debug.Log($"[DayNightCycle] Day {currentDay}: Rain effect activated!");
        }
        else
        {
            Debug.Log($"[DayNightCycle] Day {currentDay}: Rain effect deactivated.");
        }
    }

    /// <summary>
    /// Mevcut gün sayısını döndürür
    /// </summary>
    public int GetCurrentDay()
    {
        return currentDay;
    }

    /// <summary>
    /// Gün sayısını manuel olarak ayarlar
    /// </summary>
    public void SetDay(int day)
    {
        currentDay = Mathf.Max(1, day);
        UpdateRainEffect();
    }
}

