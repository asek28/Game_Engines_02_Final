using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance { get; private set; }

    [Header("UI References")]
    public Transform itemListParent;
    public GameObject itemPrefab;
    public TextMeshProUGUI moneyText;
    public Canvas inventoryCanvas;
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
    public int money;

    private readonly List<ScrapData> collectedScraps = new List<ScrapData>();
    private readonly Dictionary<string, ItemSlot> itemSlots = new Dictionary<string, ItemSlot>();
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

        UpdateMoneyUI();
        RefreshInventoryUI();

        SetInventoryVisibility(false, true);
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

        // Update ItemSlot count if it exists
        if (!string.IsNullOrWhiteSpace(scrap.ItemId) && itemSlots.TryGetValue(scrap.ItemId, out ItemSlot slot))
        {
            slot.AddCount(1);
        }

        RefreshInventoryUI();
    }

    public void RefreshInventoryUI()
    {
        if (itemListParent == null || itemPrefab == null)
        {
            Debug.LogWarning("InventoryManager: UI references are missing. Cannot refresh inventory UI.");
            return;
        }

        for (int i = itemListParent.childCount - 1; i >= 0; i--)
        {
            Destroy(itemListParent.GetChild(i).gameObject);
        }

        for (int i = 0; i < collectedScraps.Count; i++)
        {
            ScrapData scrapData = collectedScraps[i];
            GameObject entry = Instantiate(itemPrefab, itemListParent);
            SetupItemEntry(entry, scrapData, i);
        }
    }

    public void UpdateMoneyUI()
    {
        if (moneyText == null)
        {
            return;
        }

        moneyText.text = money.ToString();
    }

    public void SellScrap(int index)
    {
        if (index < 0 || index >= collectedScraps.Count)
        {
            Debug.LogWarning($"InventoryManager: Invalid scrap index {index} when attempting to sell.");
            return;
        }

        ScrapData data = collectedScraps[index];
        money += data.value;
        collectedScraps.RemoveAt(index);
        UpdateMoneyUI();
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

    private void SetupItemEntry(GameObject entry, ScrapData data, int index)
    {
        if (entry == null)
        {
            return;
        }

        TextMeshProUGUI label = entry.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            label.text = $"{data.name} (Value: {data.value})";
        }

        Button button = entry.GetComponentInChildren<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SellScrap(index));
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
            Cursor.visible = inventoryOpen;
            Cursor.lockState = inventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
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
            cachedCameraOrbit = FindObjectOfType<RightMouseOrbit>();
        }
    }

    private void OnDisable()
    {
        ApplyPauseState(false);
    }
}
