using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Debug script - M tuşuna basınca 100 dolar ekler
/// </summary>
public class MoneyDebug : MonoBehaviour
{
    [Header("Debug Settings")]
    [Tooltip("M tuşuna basınca eklenecek para miktarı")]
    [SerializeField] private int moneyToAdd = 100;
    
    [Tooltip("Debug log'ları göster")]
    [SerializeField] private bool showDebugLogs = true;
    
    private void Start()
    {
        if (showDebugLogs)
        {
            Debug.Log("[MoneyDebug] ✅ Script started! Press M to add money.");
        }
    }
    
    private void Update()
    {
        // Time.timeScale = 0 ise çalışma
        if (Time.timeScale <= 0f)
        {
            return;
        }
        
        // M tuşuna basıldı mı?
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            if (showDebugLogs && Time.frameCount % 300 == 0) // Her 5 saniyede bir (60fps * 5)
            {
                Debug.LogWarning("[MoneyDebug] ⚠️ Keyboard.current is NULL! Input System may not be initialized.");
            }
            return;
        }
        
        if (keyboard.mKey.wasPressedThisFrame)
        {
            if (showDebugLogs)
            {
                Debug.Log("[MoneyDebug] 🔑 M key pressed!");
            }
            AddMoney();
        }
    }
    
    /// <summary>
    /// Para ekle
    /// </summary>
    private void AddMoney()
    {
        Debug.Log("[MoneyDebug] 🔄 AddMoney() called!");
        
        if (InventoryManager.instance == null)
        {
            Debug.LogError("[MoneyDebug] ❌ InventoryManager.instance is NULL! Cannot add money.");
            Debug.LogError("[MoneyDebug] Make sure InventoryManager exists in the scene and has been initialized!");
            return;
        }
        
        Debug.Log($"[MoneyDebug] 📦 InventoryManager found! Current money: {InventoryManager.instance.GetCurrentMoney()}$");
        
        // Para ekle (otomatik olarak tüm UI'lar güncellenir)
        InventoryManager.instance.AddMoney(moneyToAdd);
        
        int newMoney = InventoryManager.instance.GetCurrentMoney();
        Debug.Log($"[MoneyDebug] ✅ Added {moneyToAdd}$! Total money: {newMoney}$");
    }
}
