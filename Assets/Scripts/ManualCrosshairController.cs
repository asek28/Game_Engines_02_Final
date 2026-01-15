using UnityEngine;

/// <summary>
/// Manuel crosshair kontrolü - garanti çalışır
/// Crosshair GameObject'e ekle
/// </summary>
public class ManualCrosshairController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool showOnStart = true;

    private void Start()
    {
        // Başlangıçta göster/gizle
        gameObject.SetActive(showOnStart);
        
        Debug.Log($"[ManualCrosshairController] Crosshair initialized. Visible: {showOnStart}");
    }

    private void Update()
    {
        // Settings veya Inventory açıksa gizle
        bool shouldHide = IsUIOpen();
        gameObject.SetActive(!shouldHide);
    }

    /// <summary>
    /// UI menülerinin açık olup olmadığını kontrol eder
    /// </summary>
    private bool IsUIOpen()
    {
        // Settings menüsü açık mı?
        SettingsMenuController settingsMenu = FindFirstObjectByType<SettingsMenuController>();
        if (settingsMenu != null && settingsMenu.IsSettingsOpen())
        {
            return true;
        }

        // Inventory açık mı?
        if (InventoryManager.instance != null && InventoryManager.instance.IsInventoryVisible)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Crosshair'i göster/gizle
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
