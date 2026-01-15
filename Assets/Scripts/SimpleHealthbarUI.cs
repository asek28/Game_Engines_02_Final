using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Basit 8 barlı healthbar sistemi
/// 100 can = 8 bar (her bar 12.5 can)
/// Can azaldıkça barlar teker teker kaybolur
/// </summary>
public class SimpleHealthbarUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Arka plan - boş bar çerçevesi (hep görünür, hiç değişmez)")]
    [SerializeField] private Image healthbarBackground;

    [Tooltip("Ön plan - can barları gösterilecek (sprite değişir)")]
    [SerializeField] private Image healthbarFill;

    [Tooltip("Can yazısı (opsiyonel) - örn: '75/100'")]
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Healthbar Sprites")]
    [Tooltip("Can barı sprite'ları (8 adet - Bar1, Bar2, ..., Bar8)")]
    [SerializeField] private Sprite[] healthBarSprites = new Sprite[8];

    [Header("Player Reference")]
    [SerializeField] private PlayerHealth playerHealth;

    // Sabitler
    private const int BARS_COUNT = 8;
    private float healthPerBar = 12.5f; // Her bar kaç can (dinamik olarak hesaplanacak)

    private void Awake()
    {
        // PlayerHealth'i bul
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (playerHealth == null)
        {
            Debug.LogError("[SimpleHealthbarUI] PlayerHealth not found!");
            return;
        }
    }

    private void OnEnable()
    {
        // Event'e subscribe ol
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(UpdateHealthbar);
            Debug.Log("[SimpleHealthbarUI] Subscribed to PlayerHealth events.");
        }
    }

    private void OnDisable()
    {
        // Event subscription'ı temizle
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.RemoveListener(UpdateHealthbar);
        }
    }

    private void Start()
    {
        // Background'u göster (hep görünür)
        if (healthbarBackground != null)
        {
            healthbarBackground.enabled = true;
        }

        // PlayerHealth'ten max health'i al ve hesapla
        if (playerHealth != null)
        {
            int maxHealth = playerHealth.MaxHealth;
            healthPerBar = maxHealth / (float)BARS_COUNT;
            
            Debug.Log($"[SimpleHealthbarUI] Max Health: {maxHealth}, Health Per Bar: {healthPerBar:F1}");
            
            // Başlangıç healthbar'ını göster
            UpdateHealthbar(playerHealth.CurrentHealth, maxHealth);
        }
    }

    /// <summary>
    /// Healthbar'ı güncelle
    /// </summary>
    private void UpdateHealthbar(int currentHealth, int maxHealth)
    {
        // Kaç bar göstereceğimizi hesapla
        int barsToShow = CalculateBarsCount(currentHealth);

        // Sprite'ı ayarla
        UpdateHealthbarSprite(barsToShow);

        // Text'i güncelle (varsa)
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }

        Debug.Log($"[SimpleHealthbarUI] Health: {currentHealth}/{maxHealth} → Showing {barsToShow} bars");
    }

    /// <summary>
    /// Can durumuna göre kaç bar gösterileceğini hesapla
    /// </summary>
    private int CalculateBarsCount(int currentHealth)
    {
        if (currentHealth <= 0)
        {
            return 0; // Boş bar
        }

        // Her healthPerBar can = 1 bar
        // Örnek: MaxHealth=100 → 100/8=12.5 can per bar
        //        MaxHealth=50  → 50/8=6.25 can per bar
        int bars = Mathf.CeilToInt(currentHealth / healthPerBar);

        // 0-8 arası sınırla
        return Mathf.Clamp(bars, 0, BARS_COUNT);
    }

    /// <summary>
    /// Healthbar sprite'ını güncelle
    /// </summary>
    private void UpdateHealthbarSprite(int barsCount)
    {
        if (healthbarFill == null)
        {
            Debug.LogWarning("[SimpleHealthbarUI] Healthbar Fill is null! Assign it in Inspector.");
            return;
        }

        // 0 bar = fill'i gizle (sadece background görünür)
        if (barsCount == 0)
        {
            healthbarFill.enabled = false;
            Debug.Log("[SimpleHealthbarUI] DEAD - Fill hidden, only background visible");
            return;
        }

        // Fill'i göster
        healthbarFill.enabled = true;

        // 1-8 bar = ilgili sprite
        // barsCount = 1 → healthBarSprites[0] (Bar1)
        // barsCount = 8 → healthBarSprites[7] (Bar8)
        int spriteIndex = barsCount - 1;

        if (healthBarSprites == null || healthBarSprites.Length < BARS_COUNT)
        {
            Debug.LogError($"[SimpleHealthbarUI] ⚠️ Health bar sprites array must have {BARS_COUNT} sprites! Current: {healthBarSprites?.Length ?? 0}");
            return;
        }

        if (spriteIndex < 0 || spriteIndex >= healthBarSprites.Length)
        {
            Debug.LogError($"[SimpleHealthbarUI] Invalid sprite index: {spriteIndex}");
            return;
        }

        if (healthBarSprites[spriteIndex] == null)
        {
            Debug.LogError($"[SimpleHealthbarUI] ⚠️ Health bar sprite at index {spriteIndex} (Bar {barsCount}) is NULL! Assign it in Inspector.");
            return;
        }

        healthbarFill.sprite = healthBarSprites[spriteIndex];
        Debug.Log($"[SimpleHealthbarUI] Showing Bar {barsCount} (sprite index {spriteIndex})");
    }

    /// <summary>
    /// Inspector'da doğru ayarlanıp ayarlanmadığını kontrol et
    /// </summary>
    private void OnValidate()
    {
        // UI References kontrol
        if (healthbarBackground == null)
        {
            Debug.LogWarning("[SimpleHealthbarUI] ⚠️ Healthbar Background is not assigned!");
        }

        if (healthbarFill == null)
        {
            Debug.LogWarning("[SimpleHealthbarUI] ⚠️ Healthbar Fill is not assigned!");
        }

        // Health bar sprites array'ini kontrol et
        if (healthBarSprites != null && healthBarSprites.Length != BARS_COUNT)
        {
            Debug.LogWarning($"[SimpleHealthbarUI] Health bar sprites array size should be {BARS_COUNT}, current: {healthBarSprites.Length}");
        }
    }
}
