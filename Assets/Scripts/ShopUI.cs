using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shop UI Controller
/// Ürünleri gösterir, satın alma işlemlerini yönetir
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Close (X) butonu")]
    [SerializeField] private Button closeButton;
    
    [Tooltip("Para text'i (örn: '$250')")]
    [SerializeField] private TextMeshProUGUI moneyText;
    
    [Tooltip("Welcome mesajı (opsiyonel)")]
    [SerializeField] private TextMeshProUGUI welcomeText;
    
    [Header("Shop Items")]
    [Tooltip("Shop item prefab (satın alınabilir ürünler için)")]
    [SerializeField] private GameObject shopItemPrefab;
    
    [Tooltip("Item'ların yerleştirileceği parent (Content)")]
    [SerializeField] private Transform itemsContainer;
    
    [Header("Settings")]
    [Tooltip("Welcome mesajı")]
    [SerializeField] private string welcomeMessage = "Welcome to the Shop!";
    
    private ShopTrigger shopTrigger;
    private InventoryManager inventoryManager;
    
    private void Awake()
    {
        // ShopTrigger'ı bul
        shopTrigger = FindFirstObjectByType<ShopTrigger>();
        
        // InventoryManager'ı bul (para için)
        inventoryManager = InventoryManager.instance;
        
        // Close button listener
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
        
        // Event'e subscribe ol (Awake'de de subscribe ol ki GameObject aktif olduğu sürece çalışsın)
        InventoryManager.OnMoneyChanged += OnMoneyChanged;
        Debug.Log("[ShopUI] ✅ Subscribed to OnMoneyChanged event in Awake().");
    }
    
    private void OnDestroy()
    {
        // Event subscription'ı temizle
        InventoryManager.OnMoneyChanged -= OnMoneyChanged;
        Debug.Log("[ShopUI] Unsubscribed from OnMoneyChanged event.");
    }
    
    private void OnEnable()
    {
        // Shop açıldığında para gösterimini güncelle
        UpdateMoneyDisplay();
        
        // Welcome mesajını göster
        if (welcomeText != null)
        {
            welcomeText.text = welcomeMessage;
        }
        
        // Shop item'ları oluştur (ilk açılışta)
        // Bu kısmı daha sonra genişletebilirsin
        Debug.Log("[ShopUI] Shop opened!");
    }
    
    /// <summary>
    /// Para değiştiğinde event handler
    /// </summary>
    private void OnMoneyChanged(int newMoney)
    {
        if (moneyText != null)
        {
            moneyText.text = $"${newMoney}";
            Debug.Log($"[ShopUI] 💰 Money updated to ${newMoney}");
        }
        else
        {
            Debug.LogWarning("[ShopUI] ⚠️ moneyText is NULL! Cannot update money display.");
        }
    }
    
    /// <summary>
    /// Para gösterimini günceller (manuel çağrılar için)
    /// </summary>
    private void UpdateMoneyDisplay()
    {
        if (moneyText != null && inventoryManager != null)
        {
            int currentMoney = inventoryManager.GetCurrentMoney();
            moneyText.text = $"${currentMoney}";
        }
    }
    
    /// <summary>
    /// Close button'a tıklandığında
    /// </summary>
    private void OnCloseButtonClicked()
    {
        if (shopTrigger != null)
        {
            shopTrigger.CloseShop();
        }
        else
        {
            // ShopTrigger yoksa direkt kapat
            gameObject.SetActive(false);
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        Debug.Log("[ShopUI] Close button clicked.");
    }
    
    /// <summary>
    /// Ürün satın alma (örnek - daha sonra genişletilebilir)
    /// </summary>
    public void BuyItem(string itemName, int price)
    {
        if (inventoryManager == null)
        {
            Debug.LogWarning("[ShopUI] InventoryManager not found!");
            return;
        }
        
        // Para çıkar (otomatik olarak tüm UI'lar güncellenir)
        if (inventoryManager.SpendMoney(price))
        {
            Debug.Log($"[ShopUI] Purchased {itemName} for ${price}!");
        }
        else
        {
            int currentMoney = inventoryManager.GetCurrentMoney();
            Debug.Log($"[ShopUI] Not enough money! Need ${price}, have ${currentMoney}");
        }
    }
}
