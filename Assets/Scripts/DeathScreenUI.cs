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
    
    /// <summary>
    /// Death screen aktif mi kontrol et (public getter)
    /// </summary>
    public bool IsDeathScreenActive()
    {
        return isDeathScreenActive;
    }

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
        Debug.Log($"[DeathScreenUI] ShowDeathScreen() called! isDeathScreenActive: {isDeathScreenActive}, deathScreenPanel: {(deathScreenPanel != null ? deathScreenPanel.name : "NULL")}");
        
        if (isDeathScreenActive)
        {
            Debug.LogWarning("[DeathScreenUI] Death screen already active!");
            return;
        }
        
        if (deathScreenPanel == null)
        {
            Debug.LogError("[DeathScreenUI] ❌ deathScreenPanel is NULL! Assign it in Inspector!");
            return;
        }

        Debug.Log("[DeathScreenUI] ✅ Player died! Showing death screen...");

        // Player movement'ı devre dışı bırak (Time.timeScale = 0 yeterli olmayabilir)
        DisablePlayerMovement();
        
        // Kamera hareketini durdur
        DisableCameraMovement();

        // Panel'i aktif et
        deathScreenPanel.SetActive(true);
        isDeathScreenActive = true;

        // Oyunu durdur
        if (pauseGameOnDeath)
        {
            Time.timeScale = 0f;
            Debug.Log("[DeathScreenUI] ⏸️ Time.timeScale set to 0f - Game paused!");
        }

        // Cursor'u göster
        if (showCursorOnDeath)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Debug.Log("[DeathScreenUI] 🖱️ Cursor unlocked and visible.");
        }

        // İstatistikleri göster
        UpdateDeathStats();
    }
    
    /// <summary>
    /// Player movement sistemlerini devre dışı bırak
    /// </summary>
    private void DisablePlayerMovement()
    {
        // Player GameObject'ini bul
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[DeathScreenUI] Player GameObject not found by tag! Trying alternative methods...");
            // Alternatif: PlayerHealth'den al
            if (playerHealth != null)
            {
                player = playerHealth.gameObject;
            }
        }
        
        if (player == null)
        {
            Debug.LogError("[DeathScreenUI] ❌ Cannot find Player GameObject! Movement may not be disabled.");
            return;
        }
        
        Debug.Log($"[DeathScreenUI] 🛑 Disabling movement on player: {player.name}");
        
        // SimplePlayerMovement'ı devre dışı bırak
        SimplePlayerMovement simpleMovement = player.GetComponent<SimplePlayerMovement>();
        if (simpleMovement != null)
        {
            simpleMovement.enabled = false;
            Debug.Log("[DeathScreenUI] ✓ SimplePlayerMovement disabled.");
        }
        
        // ShopPlayerMovement'ı devre dışı bırak
        ShopPlayerMovement shopMovement = player.GetComponent<ShopPlayerMovement>();
        if (shopMovement != null)
        {
            shopMovement.enabled = false;
            Debug.Log("[DeathScreenUI] ✓ ShopPlayerMovement disabled.");
        }
        
        // CharacterController'ı devre dışı bırak (hareketi tamamen engelle)
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            Debug.Log("[DeathScreenUI] ✓ CharacterController disabled.");
        }
        
        // NavMeshAgent'ı devre dışı bırak (eğer varsa)
        UnityEngine.AI.NavMeshAgent navAgent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.enabled = false;
            Debug.Log("[DeathScreenUI] ✓ NavMeshAgent disabled.");
        }
    }
    
    /// <summary>
    /// Kamera hareketini devre dışı bırak
    /// </summary>
    private void DisableCameraMovement()
    {
        RightMouseOrbit cameraOrbit = FindFirstObjectByType<RightMouseOrbit>();
        if (cameraOrbit != null)
        {
            cameraOrbit.enabled = false;
            Debug.Log("[DeathScreenUI] ✓ RightMouseOrbit disabled.");
        }
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
    /// Toplam kazanılan parayı al (oyun başından beri kazanılan)
    /// </summary>
    private int GetTotalMoney()
    {
        // InventoryManager varsa ondan al (toplam kazanılan para)
        InventoryManager inventoryManager = FindFirstObjectByType<InventoryManager>();
        if (inventoryManager != null)
        {
            return inventoryManager.GetTotalEarnedMoney();
        }

        // PlayerPrefs'den al (varsa)
        return PlayerPrefs.GetInt("TotalEarnedMoney", 0);
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
