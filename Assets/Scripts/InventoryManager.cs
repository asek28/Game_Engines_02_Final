using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance { get; private set; }
    
    // Para değiştiğinde tetiklenen event (tüm UI'ları güncellemek için)
    public static event Action<int> OnMoneyChanged;

    [Header("UI References")]
    public Transform itemListParent;
    public GameObject itemPrefab;
    public TextMeshProUGUI moneyText;
    public Canvas inventoryCanvas;
    [Tooltip("Sell All Items button (optional - will be created automatically if null)")]
    public Button sellAllButton;
    [Tooltip("Parent transform containing ItemSlot GameObjects (e.g., InventorySlots).")]
    public Transform inventorySlotsParent;
    [Tooltip("Optional root panel RectTransform that should stretch to the canvas size.")]
    public RectTransform inventoryRootPanel;
    [Tooltip("Behaviours (e.g. camera controllers) to disable while inventory is open.")]
    public MonoBehaviour[] disableWhileOpen;
    [Header("Camera Control")]
    [Tooltip("Automatically disable the main camera orbit / look controller while inventory is open.")]
    [SerializeField] private bool autoDisableCameraOrbit = true;
    [Tooltip("Optional explicit camera orbit component to disable. If empty, the manager attempts to locate RightMouseOrbit on the main camera.")]
    [SerializeField] private RightMouseOrbit cameraOrbitOverride;

    [Header("Economy")]
    public int money = 0;
    
    [Header("PlayerPrefs Keys")]
    [Tooltip("PlayerPrefs key for current money")]
    [SerializeField] private string moneyKey = "CurrentMoney";
    
    [Tooltip("PlayerPrefs key for total earned money (lifetime)")]
    [SerializeField] private string totalEarnedKey = "TotalEarnedMoney";

    // Persistent data - scene değişimlerinde korunur
    private List<ScrapData> collectedScraps = new List<ScrapData>();
    private Dictionary<string, ItemSlot> itemSlots = new Dictionary<string, ItemSlot>();
    private bool isInventoryVisible = true;
    private float cachedTimeScale = 1f;
    [SerializeField] private bool pauseWhenOpen = true;
    [SerializeField] private bool showCursorWhenOpen = true;
    public bool IsInventoryVisible => isInventoryVisible;

    private RightMouseOrbit cachedCameraOrbit;
    private bool cameraOrbitWasEnabled;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"InventoryManager: Duplicate instance detected on {name}. Destroying this component.");
            Destroy(this);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        ConfigureCanvas();
        CacheCameraOrbit();
        CacheItemSlots();

        // PlayerPrefs'ten para'yı yükle
        LoadMoneyFromPlayerPrefs();

        // Para UI'larını güncelle (tüm UI'ları bildir)
        NotifyMoneyChanged();
        RefreshInventoryUI();

        // Sell All Items button'unu oluştur veya bağla
        SetupSellAllButton();

        SetInventoryVisibility(false, true);

        // Gün döngüsü eventini dinle
        DayNightCycle.OnDayComplete += OnDayComplete;
    }
    
    /// <summary>
    /// Sell All Items button'una event bağlar (Unity Editor'da manuel eklenen button için)
    /// </summary>
    private void SetupSellAllButton()
    {
        // Eğer button atanmışsa, event'i bağla
        if (sellAllButton != null)
        {
            sellAllButton.onClick.RemoveAllListeners();
            sellAllButton.onClick.AddListener(OnSellAllButtonClicked);
            Debug.Log("[InventoryManager] ✅ Sell All Items button event connected.");
        }
        else
        {
            Debug.LogWarning("[InventoryManager] ⚠️ Sell All Items button is not assigned in the Inspector. Please add a button and assign it to 'Sell All Button' field.");
        }
    }
    
    /// <summary>
    /// Sell All Items button'una tıklandığında çağrılır
    /// </summary>
    private void OnSellAllButtonClicked()
    {
        SellAllScraps();
    }
    
    private void OnApplicationQuit()
    {
        // Oyun kapanırken para'yı kaydet
        SaveMoneyToPlayerPrefs();
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // Oyun duraklatıldığında para'yı kaydet
            SaveMoneyToPlayerPrefs();
        }
    }

    private void OnDestroy()
    {
        // Event dinleyicisini kaldır
        DayNightCycle.OnDayComplete -= OnDayComplete;
    }

    private void OnDayComplete()
    {
        SellAllScraps();
        ClearInventory();
    }

    public void AddScrap(Scrap scrap)
    {
        if (scrap == null)
        {
            Debug.LogWarning("InventoryManager: Attempted to add a null scrap.");
            return;
        }

        if (string.IsNullOrWhiteSpace(scrap.Name))
        {
            Debug.LogWarning("InventoryManager: Scrap name is empty. The item will still be added but consider providing a valid name.");
        }

        ScrapData data = new ScrapData(scrap.ItemId, scrap.Name, scrap.Value);
        collectedScraps.Add(data);

        // Update ItemSlot count if it exists (tam eşleşme veya genel eşleşme)
        if (!string.IsNullOrWhiteSpace(scrap.ItemId))
        {
            // Önce tam eşleşmeyi dene
            if (itemSlots.TryGetValue(scrap.ItemId, out ItemSlot slot))
            {
                slot.AddCount(1);
            }
            else
            {
                // Eğer tam eşleşme yoksa, value'ya göre genel slot bulmayı dene
                // Örneğin: scrap_tier1_value1 -> scrap_value1 slot'una ekle
                string baseItemId = ExtractBaseItemId(scrap.ItemId);
                if (!string.IsNullOrEmpty(baseItemId) && itemSlots.TryGetValue(baseItemId, out ItemSlot baseSlot))
                {
                    baseSlot.AddCount(1);
                }
            }
        }

        RefreshInventoryUI();
    }
    
    /// <summary>
    /// ItemId'den base itemId'yi çıkarır (tier bilgisini kaldırır)
    /// Örnek: scrap_tier1_value5 -> scrap_value5
    /// </summary>
    private string ExtractBaseItemId(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }
        
        // Tier sistemindeki itemId'leri base format'a çevir
        if (itemId.Contains("scrap_tier") && itemId.Contains("value"))
        {
            int valueIndex = itemId.IndexOf("value");
            if (valueIndex >= 0)
            {
                return "scrap_" + itemId.Substring(valueIndex);
            }
        }
        
        return null;
    }

    public void RefreshInventoryUI()
    {
        if (itemListParent == null || itemPrefab == null)
        {
            Debug.LogWarning("InventoryManager: UI references are missing. Cannot refresh inventory UI.");
            return;
        }

        // Mevcut UI entry'lerini temizle
        for (int i = itemListParent.childCount - 1; i >= 0; i--)
        {
            Destroy(itemListParent.GetChild(i).gameObject);
        }

        // ItemId'ye göre grupla (aynı itemId'ye sahip itemları birleştir)
        Dictionary<string, ScrapGroup> groupedScraps = new Dictionary<string, ScrapGroup>();
        
        for (int i = 0; i < collectedScraps.Count; i++)
        {
            ScrapData scrapData = collectedScraps[i];
            string key = scrapData.itemId;
            
            if (groupedScraps.TryGetValue(key, out ScrapGroup group))
            {
                group.count++;
                group.indices.Add(i);
            }
            else
            {
                groupedScraps[key] = new ScrapGroup
                {
                    data = scrapData,
                    count = 1,
                    indices = new List<int> { i }
                };
            }
        }

        // Gruplanmış itemları UI'da göster
        int displayIndex = 0;
        foreach (KeyValuePair<string, ScrapGroup> kvp in groupedScraps)
        {
            ScrapGroup group = kvp.Value;
            GameObject entry = Instantiate(itemPrefab, itemListParent);
            SetupItemEntry(entry, group.data, group.count, group.indices[0], displayIndex);
            displayIndex++;
        }
    }
    
    private class ScrapGroup
    {
        public ScrapData data;
        public int count;
        public List<int> indices;
    }

    /// <summary>
    /// Para miktarını ayarla (tüm UI'ları otomatik günceller)
    /// </summary>
    public void SetMoney(int newMoney)
    {
        money = newMoney;
        NotifyMoneyChanged();
        SaveMoneyToPlayerPrefs(); // Anında kaydet
    }
    
    /// <summary>
    /// Para ekle (tüm UI'ları otomatik günceller)
    /// </summary>
    public void AddMoney(int amount)
    {
        money += amount;
        
        // Toplam kazanılan parayı da güncelle
        int totalEarned = PlayerPrefs.GetInt(totalEarnedKey, 0);
        totalEarned += amount;
        PlayerPrefs.SetInt(totalEarnedKey, totalEarned);
        PlayerPrefs.Save();
        
        NotifyMoneyChanged();
        SaveMoneyToPlayerPrefs(); // Anında kaydet
    }
    
    /// <summary>
    /// Para çıkar (tüm UI'ları otomatik günceller)
    /// </summary>
    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            NotifyMoneyChanged();
            SaveMoneyToPlayerPrefs(); // Anında kaydet
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Para'yı PlayerPrefs'e kaydet
    /// </summary>
    private void SaveMoneyToPlayerPrefs()
    {
        PlayerPrefs.SetInt(moneyKey, money);
        PlayerPrefs.Save();
        Debug.Log($"[InventoryManager] 💾 Money saved to PlayerPrefs: ${money}");
    }
    
    /// <summary>
    /// PlayerPrefs'ten para'yı yükle
    /// </summary>
    private void LoadMoneyFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey(moneyKey))
        {
            money = PlayerPrefs.GetInt(moneyKey, 0);
            Debug.Log($"[InventoryManager] 📂 Money loaded from PlayerPrefs: ${money}");
        }
        else
        {
            money = 0;
            Debug.Log("[InventoryManager] 📂 No saved money found, starting with $0");
        }
    }
    
    /// <summary>
    /// Toplam kazanılan parayı al (oyun başından beri)
    /// </summary>
    public int GetTotalEarnedMoney()
    {
        return PlayerPrefs.GetInt(totalEarnedKey, 0);
    }
    
    /// <summary>
    /// Para'yı sıfırla (yeni oyun için)
    /// </summary>
    public void ResetMoney()
    {
        money = 0;
        PlayerPrefs.SetInt(moneyKey, 0);
        PlayerPrefs.SetInt(totalEarnedKey, 0);
        PlayerPrefs.Save();
        NotifyMoneyChanged();
        Debug.Log("[InventoryManager] 🔄 Money reset to $0");
    }
    
    /// <summary>
    /// Para değiştiğinde tüm UI'ları güncelle
    /// </summary>
    private void NotifyMoneyChanged()
    {
        UpdateMoneyUI();
        
        // Event'i tetikle
        int subscriberCount = OnMoneyChanged != null ? OnMoneyChanged.GetInvocationList().Length : 0;
        Debug.Log($"[InventoryManager] 💰 Money changed to ${money}. Notifying {subscriberCount} subscribers...");
        
        OnMoneyChanged?.Invoke(money);
    }
    
    /// <summary>
    /// Inventory UI'daki para text'ini güncelle
    /// </summary>
    public void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = "$" + money.ToString();
            Debug.Log($"[InventoryManager] 💰 Inventory UI updated: ${money}");
        }
        else
        {
            Debug.LogWarning("[InventoryManager] ⚠️ moneyText is NULL! Cannot update inventory money display.");
        }
    }

    /// <summary>
    /// Mevcut para miktarını döndürür
    /// </summary>
    public int GetCurrentMoney()
    {
        return money;
    }

    public void SellScrap(int index)
    {
        if (index < 0 || index >= collectedScraps.Count)
        {
            Debug.LogWarning($"InventoryManager: Invalid scrap index {index} when attempting to sell.");
            return;
        }

        ScrapData data = collectedScraps[index];
        AddMoney(data.value);
        collectedScraps.RemoveAt(index);
        RefreshInventoryUI();
    }
    
    /// <summary>
    /// Belirli bir itemId'ye sahip tüm scrapleri sat
    /// </summary>
    public void SellScrapByItemId(string itemId, int count = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return;
        }
        
        int soldCount = 0;
        int totalEarnings = 0;
        
        // Ters sırada döngü (RemoveAt için)
        for (int i = collectedScraps.Count - 1; i >= 0 && soldCount < count; i--)
        {
            if (collectedScraps[i].itemId == itemId)
            {
                totalEarnings += collectedScraps[i].value;
                collectedScraps.RemoveAt(i);
                soldCount++;
            }
        }
        
        AddMoney(totalEarnings);
        RefreshInventoryUI();
    }

    /// <summary>
    /// Tüm scrapleri sat ve money'ye ekle
    /// collectedScraps listesindeki tüm itemların value değerine göre para ekler
    /// </summary>
    public void SellAllScraps()
    {
        if (collectedScraps == null || collectedScraps.Count == 0)
        {
            Debug.Log("[InventoryManager] No items to sell.");
            return;
        }

        int totalEarnings = 0;
        int itemCount = collectedScraps.Count;

        // Tüm scrapleri value değerine göre topla
        foreach (ScrapData scrap in collectedScraps)
        {
            totalEarnings += scrap.value;
        }

        // Para ekle
        AddMoney(totalEarnings);

        // Inventory'yi temizle
        ClearInventory();

        Debug.Log($"[InventoryManager] ✅ Sold {itemCount} items for ${totalEarnings} total.");
    }

    /// <summary>
    /// ItemId'ye göre scrap fiyatını döndürür
    /// Yeni tier sistemi ve eski sistem desteklenir
    /// </summary>
    private int GetScrapPrice(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return 0;
        }

        // Yeni tier sistemi: scrap_tier1_value1, scrap_tier2_value5, scrap_tier3_value10
        if (itemId.Contains("scrap_tier"))
        {
            // Value'yu itemId'den çıkar (value1, value5, value10 gibi)
            if (itemId.Contains("value"))
            {
                int valueIndex = itemId.IndexOf("value");
                if (valueIndex >= 0 && valueIndex + 5 < itemId.Length)
                {
                    string valueStr = itemId.Substring(valueIndex + 5);
                    // Sayısal kısmı al
                    string numberStr = "";
                    for (int i = 0; i < valueStr.Length; i++)
                    {
                        if (char.IsDigit(valueStr[i]))
                        {
                            numberStr += valueStr[i];
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (int.TryParse(numberStr, out int value))
                    {
                        return value;
                    }
                }
            }
        }
        
        // Eski sistem: scrap_value5, scrap_value10
        if (itemId.Contains("scrap_value5") || itemId == "scrap_value5")
        {
            return 5;
        }
        else if (itemId.Contains("scrap_value10") || itemId == "scrap_value10")
        {
            return 10;
        }
        
        // ItemId'den sayısal değer çıkarmayı dene (value1, value5, value10 gibi)
        if (itemId.Contains("value"))
        {
            int valueIndex = itemId.IndexOf("value");
            if (valueIndex >= 0 && valueIndex + 5 < itemId.Length)
            {
                string valueStr = itemId.Substring(valueIndex + 5);
                string numberStr = "";
                for (int i = 0; i < valueStr.Length; i++)
                {
                    if (char.IsDigit(valueStr[i]))
                    {
                        numberStr += valueStr[i];
                    }
                    else
                    {
                        break;
                    }
                }
                if (int.TryParse(numberStr, out int value))
                {
                    return value;
                }
            }
        }

        // Varsayılan olarak 0 (bilinmeyen item)
        return 0;
    }

    /// <summary>
    /// Inventory'yi tamamen temizle (scrapleri ve ItemSlot count'larını sıfırla)
    /// </summary>
    public void ClearInventory()
    {
        collectedScraps.Clear();

        // Tüm ItemSlot count'larını sıfırla
        foreach (ItemSlot slot in itemSlots.Values)
        {
            if (slot != null && slot.gameObject != null)
            {
                slot.SetCount(0);
            }
        }

        RefreshInventoryUI();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.tabKey.wasPressedThisFrame)
        {
            ToggleInventoryVisibility();
        }
    }

    private void SetupItemEntry(GameObject entry, ScrapData data, int count, int firstIndex, int displayIndex)
    {
        if (entry == null)
        {
            return;
        }

        TextMeshProUGUI label = entry.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            if (count > 1)
            {
                label.text = $"{data.name} x{count} (Value: {data.value} each)";
            }
            else
            {
                label.text = $"{data.name} (Value: {data.value})";
            }
        }

        Button button = entry.GetComponentInChildren<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            // Tek bir item satmak için ilk index'i kullan
            button.onClick.AddListener(() => SellScrapByItemId(data.itemId, 1));
        }
    }

    [System.Serializable]
    public struct ScrapData
    {
        public string itemId;
        public string name;
        public int value;

        public ScrapData(string itemId, string name, int value)
        {
            this.itemId = string.IsNullOrWhiteSpace(itemId) ? "unknown" : itemId;
            this.name = string.IsNullOrWhiteSpace(name) ? "Unknown Scrap" : name;
            this.value = Mathf.Max(0, value);
        }
    }

    private void CacheItemSlots()
    {
        itemSlots.Clear();

        if (inventorySlotsParent == null)
        {
            return;
        }

        ItemSlot[] slots = inventorySlotsParent.GetComponentsInChildren<ItemSlot>(true);
        foreach (ItemSlot slot in slots)
        {
            if (slot == null || string.IsNullOrWhiteSpace(slot.itemId))
            {
                continue;
            }

            if (itemSlots.ContainsKey(slot.itemId))
            {
                continue;
            }

            itemSlots[slot.itemId] = slot;
            slot.SetCount(0); // Initialize to x0
        }
    }

    private void ToggleInventoryVisibility() => SetInventoryVisibility(!isInventoryVisible, true);

    private void SetInventoryVisibility(bool visible, bool force = false)
    {
        if (!force && isInventoryVisible == visible)
        {
            return;
        }

        isInventoryVisible = visible;

        if (inventoryCanvas != null)
        {
            inventoryCanvas.enabled = visible;
        }

        // Sell All button'unu da görünürlüğe göre ayarla
        if (sellAllButton != null)
        {
            sellAllButton.gameObject.SetActive(visible);
        }

        ApplyPauseState(visible);
    }

    private void ApplyPauseState(bool inventoryOpen)
    {
        if (pauseWhenOpen)
        {
            if (inventoryOpen)
            {
                cachedTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = Mathf.Approximately(cachedTimeScale, 0f) ? 1f : cachedTimeScale;
            }
        }

        if (showCursorWhenOpen)
        {
            if (inventoryOpen)
            {
                // Inventory açılınca cursor'ı göster
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                // Inventory kapanınca cursor'ı kilitle (RightMouseOrbit üzerinden)
                RightMouseOrbit cameraOrbit = FindFirstObjectByType<RightMouseOrbit>();
                if (cameraOrbit != null)
                {
                    cameraOrbit.LockCursorPublic();
                }
                else
                {
                    // Fallback: Manuel kilitle
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }

        if (disableWhileOpen != null)
        {
            foreach (MonoBehaviour behaviour in disableWhileOpen)
            {
                if (behaviour == null) continue;
                behaviour.enabled = !inventoryOpen;
            }
        }

        if (cachedCameraOrbit != null)
        {
            if (inventoryOpen)
            {
                cameraOrbitWasEnabled = cachedCameraOrbit.enabled;
                cachedCameraOrbit.enabled = false;
            }
            else if (cameraOrbitWasEnabled)
            {
                cachedCameraOrbit.enabled = true;
            }
        }
    }

    private void ConfigureCanvas()
    {
        if (inventoryCanvas == null)
        {
            return;
        }

        if (inventoryCanvas.renderMode == RenderMode.ScreenSpaceCamera && inventoryCanvas.worldCamera == null)
        {
            inventoryCanvas.worldCamera = Camera.main;
        }

        RectTransform canvasRect = inventoryCanvas.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.anchoredPosition = Vector2.zero;
            canvasRect.sizeDelta = Vector2.zero;
        }

        if (inventoryRootPanel != null)
        {
            inventoryRootPanel.anchorMin = Vector2.zero;
            inventoryRootPanel.anchorMax = Vector2.one;
            inventoryRootPanel.anchoredPosition = Vector2.zero;
            inventoryRootPanel.sizeDelta = Vector2.zero;
        }
    }

    private void CacheCameraOrbit()
    {
        if (!autoDisableCameraOrbit)
        {
            cachedCameraOrbit = null;
            return;
        }

        if (cameraOrbitOverride != null)
        {
            cachedCameraOrbit = cameraOrbitOverride;
            return;
        }

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            cachedCameraOrbit = mainCam.GetComponent<RightMouseOrbit>();
        }

        if (cachedCameraOrbit == null)
        {
            cachedCameraOrbit = FindFirstObjectByType<RightMouseOrbit>();
        }
    }

    private void OnDisable()
    {
        ApplyPauseState(false);
    }
}
