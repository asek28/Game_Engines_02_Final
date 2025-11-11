using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [Header("Inventory UI")]
    [SerializeField] private Canvas inventoryCanvas;
    [SerializeField] private Transform itemListRoot;
    [SerializeField] private GameObject itemEntryPrefab;

    [Header("Settings")]
    [SerializeField] private bool closeOnStart = true;

    private readonly Dictionary<string, InventoryEntry> inventory = new Dictionary<string, InventoryEntry>();
    private readonly List<GameObject> activeEntries = new List<GameObject>();
    private bool isVisible;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Multiple InventorySystem instances detected. Destroying duplicate on {name}");
            Destroy(this);
            return;
        }

        Instance = this;

        if (inventoryCanvas == null)
        {
            inventoryCanvas = GetComponentInChildren<Canvas>(true);
        }

        if (inventoryCanvas != null)
        {
            if (inventoryCanvas.renderMode == RenderMode.ScreenSpaceCamera && inventoryCanvas.worldCamera == null)
            {
                inventoryCanvas.worldCamera = Camera.main;
            }

            if (closeOnStart)
            {
                inventoryCanvas.enabled = false;
                isVisible = false;
            }
            else
            {
                isVisible = inventoryCanvas.enabled;
            }
        }
        else
        {
            Debug.LogWarning("InventorySystem: Inventory canvas reference is missing.");
        }

        if (itemListRoot == null && inventoryCanvas != null)
        {
            itemListRoot = inventoryCanvas.transform;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.tabKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    public void AddItem(string itemId, string displayName, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            Debug.LogWarning("InventorySystem: Item ID is null or empty.");
            return;
        }

        if (!inventory.TryGetValue(itemId, out InventoryEntry entry))
        {
            entry = new InventoryEntry(displayName, 0);
        }

        entry.Count += Mathf.Max(1, amount);
        entry.DisplayName = string.IsNullOrWhiteSpace(displayName) ? itemId : displayName;
        inventory[itemId] = entry;

        if (isVisible)
        {
            RefreshUI();
        }
    }

    public int GetItemCount(string itemId)
    {
        return inventory.TryGetValue(itemId, out InventoryEntry entry) ? entry.Count : 0;
    }

    private void ToggleInventory()
    {
        isVisible = !isVisible;
        if (inventoryCanvas != null)
        {
            inventoryCanvas.enabled = isVisible;
        }

        if (isVisible)
        {
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        if (itemListRoot == null || itemEntryPrefab == null)
        {
            Debug.LogWarning("InventorySystem: UI references are not set correctly.");
            return;
        }

        foreach (GameObject entryObj in activeEntries)
        {
            if (entryObj != null)
            {
                Destroy(entryObj);
            }
        }
        activeEntries.Clear();

        foreach (KeyValuePair<string, InventoryEntry> kvp in inventory)
        {
            GameObject entryInstance = Instantiate(itemEntryPrefab, itemListRoot);
            activeEntries.Add(entryInstance);

            Text textComponent = entryInstance.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.text = $"{kvp.Value.DisplayName}: {kvp.Value.Count}";
            }
            else
            {
                Debug.LogWarning("InventorySystem: Item entry prefab is missing a Text component.");
            }
        }
    }

    private struct InventoryEntry
    {
        public string DisplayName;
        public int Count;

        public InventoryEntry(string displayName, int count)
        {
            DisplayName = displayName;
            Count = count;
        }
    }
}
