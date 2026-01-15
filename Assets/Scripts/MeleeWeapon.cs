using UnityEngine;

/// <summary>
/// Melee silah (Stick gibi) - IWeapon interface implementasyonu
/// WeaponHitDetector ile çalışır
/// </summary>
[RequireComponent(typeof(WeaponHitDetector))]
public class MeleeWeapon : MonoBehaviour, IWeapon
{
    [Header("Weapon Info")]
    [SerializeField] private string weaponName = "Stick";
    
    private WeaponHitDetector hitDetector;
    private bool isEquipped = false;
    
    public string WeaponName => weaponName;
    public bool IsEquipped => isEquipped;
    public GameObject WeaponObject => gameObject;
    
    private void Awake()
    {
        hitDetector = GetComponent<WeaponHitDetector>();
    }
    
    public void Equip()
    {
        isEquipped = true;
        gameObject.SetActive(true);
        
        if (hitDetector != null)
        {
            hitDetector.enabled = true;
        }
        
        Debug.Log($"[MeleeWeapon] Equipped: {weaponName}");
    }
    
    public void Unequip()
    {
        isEquipped = false;
        
        if (hitDetector != null)
        {
            hitDetector.enabled = false;
        }
        
        Debug.Log($"[MeleeWeapon] Unequipped: {weaponName}");
    }
    
    public void Use()
    {
        // Melee silah için Use() fonksiyonu ComboSystem tarafından yönetiliyor
        // Bu yüzden burada özel bir şey yapma
    }
}
