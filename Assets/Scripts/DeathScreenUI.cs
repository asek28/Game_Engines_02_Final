using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Death Screen UI Controller
/// Oyuncu öldüğünde görünür, para ve hayatta kalma süresi gösterir
/// </summary>
public class DeathScreenUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Ana death screen panel")]
    [SerializeField] private GameObject deathScreenPanel;

    [Tooltip("Arka plan görsel (ölüm ekranı arkaplanı)")]
    [SerializeField] private Image backgroundImage;

    [Tooltip("Kaç para ile öldüğünü gösteren text")]
    [SerializeField] private TextMeshProUGUI moneyText;

    [Tooltip("Kaç gün hayatta kaldığını gösteren text")]
    [SerializeField] private TextMeshProUGUI daysText;

    [Tooltip("MainMenu'ye dönüş butonu")]
    [SerializeField] private Button mainMenuButton;

    [Header("Text Formats")]
    [Tooltip("Para text formatı (örn: 'Para: {0}$')")]
    [SerializeField] private string moneyTextFormat = "Toplam Para: {0}$";

    [Tooltip("Gün text formatı (örn: '{0} Gün Hayatta Kaldın')")]
    [SerializeField] private string daysTextFormat = "{0} Gün Hayatta Kaldın";

    [Header("Settings")]
    [Tooltip("Death screen açıldığında oyun duracak mı?")]
    [SerializeField] private bool pauseGameOnDeath = true;

    [Tooltip("Death screen açıldığında cursor gösterilecek mi?")]
    [SerializeField] private bool showCursorOnDeath = true;

    [Tooltip("MainMenu scene adı")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Player References")]
    [SerializeField] private PlayerHealth playerHealth;

    private bool isDeathScreenActive = false;

    private void Awake()
    {
        // PlayerHealth'i bul
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (playerHealth == null)
        {
            Debug.LogError("[DeathScreenUI] PlayerHealth component not found!");
        }

        // Başlangıçta death screen'i gizle
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(false);
        }

        // Button listener ekle
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        }
    }

    private void OnEnable()
    {
        // Static event'e subscribe ol
        PlayerHealth.OnPlayerDied += ShowDeathScreen;
        Debug.Log("[DeathScreenUI] Subscribed to OnPlayerDied event.");
    }

    private void OnDisable()
    {
        // Event subscription'ı temizle
        PlayerHealth.OnPlayerDied -= ShowDeathScreen;
        Debug.Log("[DeathScreenUI] Unsubscribed from OnPlayerDied event.");
    }

    /// <summary>
    /// Death screen'i göster
    /// </summary>
    private void ShowDeathScreen()
    {
        if (isDeathScreenActive || deathScreenPanel == null)
        {
            return;
        }

        Debug.Log("[DeathScreenUI] Player died! Showing death screen...");

        // Panel'i aktif et
        deathScreenPanel.SetActive(true);
        isDeathScreenActive = true;

        // Oyunu durdur
        if (pauseGameOnDeath)
        {
            Time.timeScale = 0f;
        }

        // Cursor'u göster
        if (showCursorOnDeath)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // İstatistikleri göster
        UpdateDeathStats();
    }

    /// <summary>
    /// Death screen istatistiklerini güncelle
    /// </summary>
    private void UpdateDeathStats()
    {
        // Para bilgisi (InventoryManager'dan veya PlayerPrefs'den al)
        int totalMoney = GetTotalMoney();
        if (moneyText != null)
        {
            moneyText.text = string.Format(moneyTextFormat, totalMoney);
        }

        // Hayatta kalma süresi (PlayerPrefs veya bir GameManager'dan)
        int daysSurvived = GetDaysSurvived();
        if (daysText != null)
        {
            daysText.text = string.Format(daysTextFormat, daysSurvived);
        }

        Debug.Log($"[DeathScreenUI] Stats - Money: {totalMoney}$, Days: {daysSurvived}");
    }

    /// <summary>
    /// Toplam parayı al (InventoryManager'dan veya PlayerPrefs)
    /// </summary>
    private int GetTotalMoney()
    {
        // InventoryManager varsa ondan al
        InventoryManager inventoryManager = FindFirstObjectByType<InventoryManager>();
        if (inventoryManager != null)
        {
            return inventoryManager.GetCurrentMoney();
        }

        // PlayerPrefs'den al (varsa)
        return PlayerPrefs.GetInt("TotalMoney", 0);
    }

    /// <summary>
    /// Kaç gün hayatta kaldığını al
    /// </summary>
    private int GetDaysSurvived()
    {
        // GameManager veya başka bir sistemden alınabilir
        // Şimdilik PlayerPrefs kullanıyoruz
        return PlayerPrefs.GetInt("DaysSurvived", 0);
    }

    /// <summary>
    /// MainMenu butonuna basıldığında
    /// </summary>
    private void OnMainMenuButtonClicked()
    {
        Debug.Log("[DeathScreenUI] Returning to MainMenu...");

        // Time.timeScale'i sıfırla
        Time.timeScale = 1f;

        // İstatistikleri kaydet (opsiyonel)
        SaveDeathStats();

        // MainMenu'ye dön
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Ölüm istatistiklerini kaydet (opsiyonel)
    /// </summary>
    private void SaveDeathStats()
    {
        // En iyi skor, toplam ölüm sayısı vb. kaydedilebilir
        int totalDeaths = PlayerPrefs.GetInt("TotalDeaths", 0);
        PlayerPrefs.SetInt("TotalDeaths", totalDeaths + 1);
        PlayerPrefs.Save();

        Debug.Log($"[DeathScreenUI] Total deaths: {totalDeaths + 1}");
    }

    /// <summary>
    /// Death screen'i manuel olarak gizle
    /// </summary>
    public void HideDeathScreen()
    {
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(false);
        }

        isDeathScreenActive = false;
        Time.timeScale = 1f;

        Debug.Log("[DeathScreenUI] Death screen hidden.");
    }

    private void OnDestroy()
    {
        // Button listener'ı temizle
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClicked);
        }

        // Time.timeScale'i sıfırla (scene değişirken)
        Time.timeScale = 1f;
    }
}
