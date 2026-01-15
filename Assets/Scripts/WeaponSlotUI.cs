using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Inventory'de weapon slot dropdown UI'ı
/// Hangi silahın hangi slota atanacağını seçer
/// </summary>
public class WeaponSlotUI : MonoBehaviour
{
    [Header("Dropdown References")]
    [Tooltip("Slot 1 dropdown")]
    [SerializeField] private TMP_Dropdown slot1Dropdown;
    
    [Tooltip("Slot 2 dropdown")]
    [SerializeField] private TMP_Dropdown slot2Dropdown;
    
    [Tooltip("Slot 3 dropdown")]
    [SerializeField] private TMP_Dropdown slot3Dropdown;
    
    [Header("Weapon References")]
    [Tooltip("Tüm kullanılabilir silahlar (Player'ın child'ları olmalı)")]
    [SerializeField] private GameObject[] availableWeapons;
    
    [Header("System Reference")]
    [Tooltip("WeaponSlotSystem referansı")]
    [SerializeField] private WeaponSlotSystem weaponSlotSystem;
    
    private List<string> weaponNames = new List<string>();
    
    private void Start()
    {
        // WeaponSlotSystem'i bul (eğer atanmamışsa)
        if (weaponSlotSystem == null)
        {
            weaponSlotSystem = FindFirstObjectByType<WeaponSlotSystem>();
        }
        
        // Silah isimlerini topla
        weaponNames.Add("None"); // Boş slot seçeneği
        
        foreach (GameObject weapon in availableWeapons)
        {
            if (weapon != null)
            {
                IWeapon iWeapon = weapon.GetComponent<IWeapon>();
                if (iWeapon != null)
                {
                    weaponNames.Add(iWeapon.WeaponName);
                }
                else
                {
                    weaponNames.Add(weapon.name);
                }
            }
        }
        
        // Dropdown'ları ayarla
        SetupDropdown(slot1Dropdown, 1);
        SetupDropdown(slot2Dropdown, 2);
        SetupDropdown(slot3Dropdown, 3);
        
        Debug.Log($"[WeaponSlotUI] Initialized with {availableWeapons.Length} weapons.");
    }
    
    /// <summary>
    /// Dropdown'ı ayarla
    /// </summary>
    private void SetupDropdown(TMP_Dropdown dropdown, int slotNumber)
    {
        if (dropdown == null)
        {
            Debug.LogWarning($"[WeaponSlotUI] Dropdown for slot {slotNumber} is null!");
            return;
        }
        
        // Dropdown option'larını temizle ve yenilerini ekle
        dropdown.ClearOptions();
        dropdown.AddOptions(weaponNames);
        
        // Varsayılan değer (örnek: Slot 1 = Stick, Slot 2 = Gun)
        int defaultValue = slotNumber <= availableWeapons.Length ? slotNumber : 0;
        dropdown.value = defaultValue;
        
        // Event listener ekle
        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.onValueChanged.AddListener((int value) => OnSlotDropdownChanged(slotNumber, value));
        
        // İlk atamayı yap
        OnSlotDropdownChanged(slotNumber, defaultValue);
    }
    
    /// <summary>
    /// Dropdown değeri değiştiğinde çağrılır
    /// </summary>
    private void OnSlotDropdownChanged(int slotNumber, int dropdownValue)
    {
        if (weaponSlotSystem == null)
        {
            Debug.LogWarning("[WeaponSlotUI] WeaponSlotSystem is null! Cannot assign weapon.");
            return;
        }
        
        // Dropdown value: 0 = None, 1 = İlk silah, 2 = İkinci silah, vb.
        if (dropdownValue == 0)
        {
            // "None" seçildi, slot'u temizle
            weaponSlotSystem.AssignWeaponToSlot(slotNumber, null);
            Debug.Log($"[WeaponSlotUI] Slot {slotNumber} cleared.");
        }
        else
        {
            // Silah index'i (dropdown value - 1, çünkü 0 = None)
            int weaponIndex = dropdownValue - 1;
            
            if (weaponIndex >= 0 && weaponIndex < availableWeapons.Length)
            {
                GameObject selectedWeapon = availableWeapons[weaponIndex];
                weaponSlotSystem.AssignWeaponToSlot(slotNumber, selectedWeapon);
                
                IWeapon iWeapon = selectedWeapon != null ? selectedWeapon.GetComponent<IWeapon>() : null;
                string weaponName = iWeapon != null ? iWeapon.WeaponName : selectedWeapon?.name ?? "Unknown";
                
                Debug.Log($"[WeaponSlotUI] Slot {slotNumber} assigned to: {weaponName}");
            }
        }
    }
    
    /// <summary>
    /// Dropdown değerlerini güncelle (PlayerPrefs'ten yüklemek için kullanılabilir)
    /// </summary>
    public void LoadSlotSettings(int slot1Value, int slot2Value, int slot3Value)
    {
        if (slot1Dropdown != null) slot1Dropdown.value = slot1Value;
        if (slot2Dropdown != null) slot2Dropdown.value = slot2Value;
        if (slot3Dropdown != null) slot3Dropdown.value = slot3Value;
    }
    
    /// <summary>
    /// Dropdown değerlerini kaydet (PlayerPrefs'e kaydetmek için kullanılabilir)
    /// </summary>
    public void SaveSlotSettings()
    {
        if (slot1Dropdown != null) PlayerPrefs.SetInt("WeaponSlot1", slot1Dropdown.value);
        if (slot2Dropdown != null) PlayerPrefs.SetInt("WeaponSlot2", slot2Dropdown.value);
        if (slot3Dropdown != null) PlayerPrefs.SetInt("WeaponSlot3", slot3Dropdown.value);
        PlayerPrefs.Save();
        
        Debug.Log("[WeaponSlotUI] Weapon slot settings saved.");
    }
}
