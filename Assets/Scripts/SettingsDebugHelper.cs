using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Settings panel ve butonları için debug yardımcısı
/// </summary>
public class SettingsDebugHelper : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    
    [Header("References to Check")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button backButton;
    [SerializeField] private SettingsMenuController settingsController;
    
    private void Start()
    {
        if (!enableDebugLogs) return;
        
        DebugReferences();
    }
    
    private void DebugReferences()
    {
        Debug.Log("=== SETTINGS DEBUG HELPER ===");
        
        // Settings Panel kontrolü
        if (settingsPanel != null)
        {
            Debug.Log($"✅ Settings Panel found: {settingsPanel.name} (Active: {settingsPanel.activeSelf})");
        }
        else
        {
            Debug.LogError("❌ Settings Panel is NULL!");
        }
        
        // Back Button kontrolü
        if (backButton != null)
        {
            Debug.Log($"✅ Back Button found: {backButton.name}");
            Debug.Log($"   - Interactable: {backButton.interactable}");
            Debug.Log($"   - OnClick event count: {backButton.onClick.GetPersistentEventCount()}");
            
            // Event detayları
            for (int i = 0; i < backButton.onClick.GetPersistentEventCount(); i++)
            {
                var target = backButton.onClick.GetPersistentTarget(i);
                var methodName = backButton.onClick.GetPersistentMethodName(i);
                Debug.Log($"   - Event {i}: {target?.name}.{methodName}");
            }
        }
        else
        {
            Debug.LogError("❌ Back Button is NULL!");
        }
        
        // SettingsMenuController kontrolü
        if (settingsController != null)
        {
            Debug.Log($"✅ SettingsMenuController found on: {settingsController.gameObject.name}");
        }
        else
        {
            Debug.LogError("❌ SettingsMenuController is NULL!");
            
            // Scene'de var mı bul
            SettingsMenuController found = FindFirstObjectByType<SettingsMenuController>();
            if (found != null)
            {
                Debug.LogWarning($"⚠️ SettingsMenuController found in scene on: {found.gameObject.name} (but not assigned in inspector)");
            }
        }
        
        Debug.Log("=== END DEBUG ===");
    }
    
    /// <summary>
    /// Test butonu - Inspector'dan veya kod ile çağrılabilir
    /// </summary>
    public void TestBackButton()
    {
        Debug.Log("[SettingsDebugHelper] TestBackButton() called!");
        
        if (settingsController != null)
        {
            settingsController.OnBackButtonClicked();
        }
        else
        {
            Debug.LogError("[SettingsDebugHelper] Cannot test - SettingsMenuController is null!");
        }
    }
}
