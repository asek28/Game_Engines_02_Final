using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Silah slot sistemi - 1,2,3 tuşları ile silah değiştirme
/// </summary>
public class WeaponSlotSystem : MonoBehaviour
{
    [Header("Weapon Slots")]
    [Tooltip("Slot 1 - 1 tuşu ile aktif olur")]
    [SerializeField] private GameObject weaponSlot1; // Stick
    
    [Tooltip("Slot 2 - 2 tuşu ile aktif olur")]
    [SerializeField] private GameObject weaponSlot2; // Gun
    
    [Tooltip("Slot 3 - 3 tuşu ile aktif olur")]
    [SerializeField] private GameObject weaponSlot3; // Future weapon
    
    [Header("Settings")]
    [Tooltip("Başlangıçta aktif slot (1-3)")]
    [SerializeField] private int defaultSlot = 1;
    
    private int currentSlot = 1;
    private IWeapon currentWeapon;
    
    private void Start()
    {
        // Başlangıçta tüm silahları pasif yap (satın alınmadan kullanılamaz)
        DeactivateAllWeapons();
        
        // Başlangıçta varsayılan slot'u aktif et (eğer silah varsa)
        if (GetWeaponObjectForSlot(defaultSlot) != null)
        {
            SwitchToSlot(defaultSlot);
        }
        else
        {
            // Hiç silah yoksa, currentWeapon null olarak başla
            currentWeapon = null;
            Debug.Log("[WeaponSlotSystem] No weapons available at start. Buy weapons from shop!");
        }
    }
    
    /// <summary>
    /// Tüm silahları pasif yap (oyun başında)
    /// </summary>
    private void DeactivateAllWeapons()
    {
        if (weaponSlot1 != null)
        {
            weaponSlot1.SetActive(false);
        }
        
        if (weaponSlot2 != null)
        {
            weaponSlot2.SetActive(false);
        }
        
        if (weaponSlot3 != null)
        {
            weaponSlot3.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Settings veya Inventory açıksa input alma
        if (IsUIOpen())
        {
            return;
        }
        
        // Keyboard input
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
            {
                SwitchToSlot(1);
            }
            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
            {
                SwitchToSlot(2);
            }
            else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
            {
                SwitchToSlot(3);
            }
        }
    }
    
    /// <summary>
    /// Belirtilen slot'a geç
    /// </summary>
    public void SwitchToSlot(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > 3)
        {
            Debug.LogWarning($"[WeaponSlotSystem] Invalid slot number: {slotNumber}");
            return;
        }
        
        if (slotNumber == currentSlot)
        {
            // Zaten bu slot aktif
            return;
        }
        
        // Mevcut silahı çıkar
        UnequipCurrentWeapon();
        
        // Yeni slot'u aktif et
        currentSlot = slotNumber;
        GameObject weaponObject = GetWeaponObjectForSlot(slotNumber);
        
        if (weaponObject != null)
        {
            // Silahı aktif et
            weaponObject.SetActive(true);
            
            // IWeapon interface'ini al
            currentWeapon = weaponObject.GetComponent<IWeapon>();
            if (currentWeapon != null)
            {
                currentWeapon.Equip();
                Debug.Log($"[WeaponSlotSystem] Switched to slot {slotNumber}: {currentWeapon.WeaponName}");
            }
            else
            {
                Debug.LogWarning($"[WeaponSlotSystem] Weapon in slot {slotNumber} doesn't have IWeapon component!");
            }
        }
        else
        {
            Debug.LogWarning($"[WeaponSlotSystem] No weapon assigned to slot {slotNumber}");
            currentWeapon = null;
        }
    }
    
    /// <summary>
    /// Mevcut silahı çıkar
    /// </summary>
    private void UnequipCurrentWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon.Unequip();
            
            if (currentWeapon.WeaponObject != null)
            {
                currentWeapon.WeaponObject.SetActive(false);
            }
        }
        
        currentWeapon = null;
    }
    
    /// <summary>
    /// Slot numarasına göre silah GameObject'ini döndür
    /// </summary>
    private GameObject GetWeaponObjectForSlot(int slotNumber)
    {
        return slotNumber switch
        {
            1 => weaponSlot1,
            2 => weaponSlot2,
            3 => weaponSlot3,
            _ => null
        };
    }
    
    /// <summary>
    /// UI menülerinin açık olup olmadığını kontrol et
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
    /// Slot'a silah ata (Inventory UI'dan veya Shop'tan çağrılacak)
    /// </summary>
    public void AssignWeaponToSlot(int slotNumber, GameObject weaponObject)
    {
        if (weaponObject == null)
        {
            Debug.LogWarning($"[WeaponSlotSystem] Cannot assign null weapon to slot {slotNumber}");
            return;
        }
        
        switch (slotNumber)
        {
            case 1:
                weaponSlot1 = weaponObject;
                break;
            case 2:
                weaponSlot2 = weaponObject;
                break;
            case 3:
                weaponSlot3 = weaponObject;
                break;
        }
        
        // Eğer şu an bu slot aktifse, silahı direkt aktif et ve equip et
        if (slotNumber == currentSlot)
        {
            UnequipCurrentWeapon();
            SwitchToSlot(slotNumber);
        }
        else
        {
            // Aktif değilse, silahı pasif yap (sadece satın alındı ama henüz kullanılmıyor)
            weaponObject.SetActive(false);
        }
        
        Debug.Log($"[WeaponSlotSystem] ✅ Weapon assigned to slot {slotNumber}: {weaponObject.name}");
    }
    
    /// <summary>
    /// Şu anki slot numarasını döndür
    /// </summary>
    public int GetCurrentSlot()
    {
        return currentSlot;
    }
    
    /// <summary>
    /// Şu anki silahı döndür
    /// </summary>
    public IWeapon GetCurrentWeapon()
    {
        return currentWeapon;
    }
}
