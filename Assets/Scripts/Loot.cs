using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class Loot : MonoBehaviour
{
    [Header("Loot Info")]
    [SerializeField] private string itemId = "scrap_value5";
    [SerializeField] private string itemDisplayName = "Scrap Value 5";
    [SerializeField, Min(1)] private int amount = 1;

    private bool playerInRange;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"[Loot] {name}: No Collider component found! Loot requires a Collider (set as Trigger) to detect player proximity.");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"[Loot] {name}: Collider is not set as Trigger! Setting it to Trigger automatically.");
            col.isTrigger = true;
        }

        // Item ID validasyonu
        if (string.IsNullOrWhiteSpace(itemId))
        {
            Debug.LogError($"Loot on {name}: Item ID is empty or null! Please set a valid Item ID in the inspector. This loot cannot be collected.");
        }

        // Display Name boşsa, Item ID'den otomatik isim oluştur
        if (string.IsNullOrWhiteSpace(itemDisplayName))
        {
            if (!string.IsNullOrWhiteSpace(itemId))
            {
                // Item ID'yi daha okunabilir hale getir (örn: "scrap_value5" -> "Scrap Value 5")
                itemDisplayName = FormatItemIdAsDisplayName(itemId);
                Debug.LogWarning($"Loot on {name}: Display Name was empty. Auto-generated from Item ID: '{itemDisplayName}'");
            }
            else
            {
                itemDisplayName = "Unknown Item";
                Debug.LogWarning($"Loot on {name}: Both Item ID and Display Name are empty! Using 'Unknown Item' as fallback.");
            }
        }
    }

    private string FormatItemIdAsDisplayName(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return "Unknown Item";

        // Alt çizgileri ve tireleri boşlukla değiştir, kelimelerin ilk harfini büyük yap
        string formatted = id.Replace('_', ' ').Replace('-', ' ');
        string[] words = formatted.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpper(words[i][0]) + (words[i].Length > 1 ? words[i].Substring(1).ToLower() : "");
            }
        }
        
        return string.Join(" ", words);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Player tag kontrolü veya CharacterController kontrolü
        bool isPlayer = other.CompareTag("Player") || other.GetComponent<CharacterController>() != null;
        
        if (isPlayer)
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Player tag kontrolü veya CharacterController kontrolü
        bool isPlayer = other.CompareTag("Player") || other.GetComponent<CharacterController>() != null;
        
        if (isPlayer)
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        if (!playerInRange)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
        {
            TryCollect();
        }
    }

    private void TryCollect()
    {
        // Item ID kontrolü - eğer boşsa toplama işlemini durdur
        if (string.IsNullOrWhiteSpace(itemId))
        {
            Debug.LogError($"[Loot] {name}: Cannot collect item - Item ID is empty! Please set a valid Item ID in the inspector.");
            return;
        }

        if (InventorySystem.Instance == null)
        {
            Debug.LogError($"[Loot] {name}: No InventorySystem instance found in the scene! Please add InventorySystem to a GameObject in the scene.");
            return;
        }

        // Inventory'e ekle
        InventorySystem.Instance.AddItem(itemId, itemDisplayName, amount);
        
        // Nesneyi tamamen yok et
        Destroy(gameObject);
    }
}
