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
    
    [Header("Shop Buttons")]
    [Tooltip("Buy Water butonu (susuzluk barını doldurur)")]
    [SerializeField] private Button buyWaterButton;
    
    [Tooltip("Buy Food butonu (açlık barını doldurur)")]
    [SerializeField] private Button buyFoodButton;
    
    [Tooltip("Buy Gun butonu (Slot 2'ye Gun ekler)")]
    [SerializeField] private Button buyGunButton;
    
    [Tooltip("Buy Stick butonu (Slot 1'e Stick ekler)")]
    [SerializeField] private Button buyStickButton;
    
    [Header("Shop Items")]
    [Tooltip("Shop item prefab (satın alınabilir ürünler için)")]
    [SerializeField] private GameObject shopItemPrefab;
    
    [Tooltip("Item'ların yerleştirileceği parent (Content)")]
    [SerializeField] private Transform itemsContainer;
    
    [Header("Settings")]
    [Tooltip("Welcome mesajı")]
    [SerializeField] private string welcomeMessage = "Welcome to the Shop!";
    
    [Header("Shop Prices")]
    [Tooltip("Su fiyatı")]
    [SerializeField] private int waterPrice = 10;
    
    [Tooltip("Yemek fiyatı")]
    [SerializeField] private int foodPrice = 15;
    
    [Tooltip("Gun fiyatı")]
    [SerializeField] private int gunPrice = 50;
    
    [Tooltip("Stick fiyatı")]
    [SerializeField] private int stickPrice = 30;
    
    [Header("Restore Amounts")]
    [Tooltip("Su alındığında susuzluk barına eklenecek miktar (0-100)")]
    [SerializeField] private float waterRestoreAmount = 100f;
    
    [Tooltip("Yemek alındığında açlık barına eklenecek miktar (0-100)")]
    [SerializeField] private float foodRestoreAmount = 100f;
    
    [Header("Weapon Prefabs")]
    [Tooltip("Gun prefab (satın alındığında Slot 2'ye eklenecek)")]
    [SerializeField] private GameObject gunPrefab;
    
    [Tooltip("Stick prefab (satın alındığında Slot 1'e eklenecek)")]
    [SerializeField] private GameObject stickPrefab;
    
    private ShopTrigger shopTrigger;
    private InventoryManager inventoryManager;
    private HungerThirstManager hungerThirstManager;
    private WeaponSlotSystem weaponSlotSystem;
    
    private void Awake()
    {
        // ShopTrigger'ı bul
        shopTrigger = FindFirstObjectByType<ShopTrigger>();
        
        // InventoryManager'ı bul (para için)
        inventoryManager = InventoryManager.instance;
        
        // HungerThirstManager'ı bul
        hungerThirstManager = FindFirstObjectByType<HungerThirstManager>();
        
        // WeaponSlotSystem'ı bul
        weaponSlotSystem = FindFirstObjectByType<WeaponSlotSystem>();
        
        // Close button listener
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
        
        // Buy Water button listener
        if (buyWaterButton != null)
        {
            buyWaterButton.onClick.AddListener(OnBuyWaterClicked);
        }
        
        // Buy Food button listener
        if (buyFoodButton != null)
        {
            buyFoodButton.onClick.AddListener(OnBuyFoodClicked);
        }
        
        // Buy Gun button listener
        if (buyGunButton != null)
        {
            buyGunButton.onClick.AddListener(OnBuyGunClicked);
        }
        
        // Buy Stick button listener
        if (buyStickButton != null)
        {
            buyStickButton.onClick.AddListener(OnBuyStickClicked);
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
    
    /// <summary>
    /// Buy Water button'a tıklandığında
    /// </summary>
    private void OnBuyWaterClicked()
    {
        if (inventoryManager == null)
        {
            Debug.LogWarning("[ShopUI] InventoryManager not found!");
            return;
        }
        
        if (hungerThirstManager == null)
        {
            Debug.LogWarning("[ShopUI] HungerThirstManager not found!");
            return;
        }
        
        // Para kontrolü
        if (!inventoryManager.SpendMoney(waterPrice))
        {
            int currentMoney = inventoryManager.GetCurrentMoney();
            Debug.Log($"[ShopUI] 💰 Not enough money to buy water! Need ${waterPrice}, have ${currentMoney}");
            return;
        }
        
        // Susuzluk barını doldur
        hungerThirstManager.AddThirst(waterRestoreAmount);
        
        Debug.Log($"[ShopUI] 💧 Bought water for ${waterPrice}! Thirst restored by {waterRestoreAmount}.");
    }
    
    /// <summary>
    /// Buy Food button'a tıklandığında
    /// </summary>
    private void OnBuyFoodClicked()
    {
        if (inventoryManager == null)
        {
            Debug.LogWarning("[ShopUI] InventoryManager not found!");
            return;
        }
        
        if (hungerThirstManager == null)
        {
            Debug.LogWarning("[ShopUI] HungerThirstManager not found!");
            return;
        }
        
        // Para kontrolü
        if (!inventoryManager.SpendMoney(foodPrice))
        {
            int currentMoney = inventoryManager.GetCurrentMoney();
            Debug.Log($"[ShopUI] 💰 Not enough money to buy food! Need ${foodPrice}, have ${currentMoney}");
            return;
        }
        
        // Açlık barını doldur
        hungerThirstManager.AddHunger(foodRestoreAmount);
        
        Debug.Log($"[ShopUI] 🍖 Bought food for ${foodPrice}! Hunger restored by {foodRestoreAmount}.");
    }
    
    /// <summary>
    /// Buy Gun button'a tıklandığında
    /// </summary>
    private void OnBuyGunClicked()
    {
        if (inventoryManager == null)
        {
            Debug.LogWarning("[ShopUI] InventoryManager not found!");
            return;
        }
        
        if (weaponSlotSystem == null)
        {
            Debug.LogWarning("[ShopUI] WeaponSlotSystem not found!");
            return;
        }
        
        if (gunPrefab == null)
        {
            Debug.LogWarning("[ShopUI] Gun prefab is not assigned!");
            return;
        }
        
        // Para kontrolü
        if (!inventoryManager.SpendMoney(gunPrice))
        {
            int currentMoney = inventoryManager.GetCurrentMoney();
            Debug.Log($"[ShopUI] 💰 Not enough money to buy gun! Need ${gunPrice}, have ${currentMoney}");
            return;
        }
        
        // Gun'u Slot 2'ye ekle
        GameObject gunInstance = Instantiate(gunPrefab);
        
        // Silahı Player'ın altına ekle (WeaponSlotSystem'in parent'ına)
        if (weaponSlotSystem != null && weaponSlotSystem.transform.parent != null)
        {
            gunInstance.transform.SetParent(weaponSlotSystem.transform.parent, false);
        }
        
        weaponSlotSystem.AssignWeaponToSlot(2, gunInstance);
        
        Debug.Log($"[ShopUI] 🔫 Bought gun for ${gunPrice}! Gun added to Slot 2.");
    }
    
    /// <summary>
    /// Buy Stick button'a tıklandığında
    /// </summary>
    private void OnBuyStickClicked()
    {
        if (inventoryManager == null)
        {
            Debug.LogWarning("[ShopUI] InventoryManager not found!");
            return;
        }
        
        if (weaponSlotSystem == null)
        {
            Debug.LogWarning("[ShopUI] WeaponSlotSystem not found!");
            return;
        }
        
        if (stickPrefab == null)
        {
            Debug.LogWarning("[ShopUI] Stick prefab is not assigned!");
            return;
        }
        
        // Para kontrolü
        if (!inventoryManager.SpendMoney(stickPrice))
        {
            int currentMoney = inventoryManager.GetCurrentMoney();
            Debug.Log($"[ShopUI] 💰 Not enough money to buy stick! Need ${stickPrice}, have ${currentMoney}");
            return;
        }
        
        // Stick'i Slot 1'e ekle
        GameObject stickInstance = Instantiate(stickPrefab);
        
        // Silahı Player'ın altına ekle (WeaponSlotSystem'in parent'ına)
        if (weaponSlotSystem != null && weaponSlotSystem.transform.parent != null)
        {
            stickInstance.transform.SetParent(weaponSlotSystem.transform.parent, false);
        }
        
        weaponSlotSystem.AssignWeaponToSlot(1, stickInstance);
        
        Debug.Log($"[ShopUI] 🪵 Bought stick for ${stickPrice}! Stick added to Slot 1.");
    }
}
