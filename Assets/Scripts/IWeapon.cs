using UnityEngine;

/// <summary>
/// Tüm silahlar için ortak interface
/// </summary>
public interface IWeapon
{
    /// <summary>
    /// Silahın adı
    /// </summary>
    string WeaponName { get; }
    
    /// <summary>
    /// Silahı ekiple (görünür yap, aktif et)
    /// </summary>
    void Equip();
    
    /// <summary>
    /// Silahı çıkar (gizle, pasif et)
    /// </summary>
    void Unequip();
    
    /// <summary>
    /// Silah şu an ekipli mi?
    /// </summary>
    bool IsEquipped { get; }
    
    /// <summary>
    /// Silahı kullan (saldır, ateş et, vb.)
    /// </summary>
    void Use();
    
    /// <summary>
    /// Silah GameObject'i
    /// </summary>
    GameObject WeaponObject { get; }
}
