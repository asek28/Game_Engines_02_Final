using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WeaponHitDetector : MonoBehaviour
{
    [Header("Weapon Settings")]
    [Tooltip("Hasar miktarı")]
    [SerializeField, Min(1)] private int damage = 1;
    [Tooltip("Saldırı aktifken hasar ver (ComboSystem isAttacking kontrolü)")]
    [SerializeField] private bool requireActiveAttack = true;

    private Collider weaponCollider;
    private readonly System.Collections.Generic.List<Enemy> enemiesInRange = new System.Collections.Generic.List<Enemy>();

    private void Awake()
    {
        weaponCollider = GetComponent<Collider>();
        if (weaponCollider == null)
        {
            Debug.LogError($"WeaponHitDetector on {name}: Collider component not found!");
            return;
        }

        // Collider'ı trigger yap
        weaponCollider.isTrigger = true;
    }

    private void Start()
    {
        // ComboSystem eventini dinle
        ComboSystem.OnAttackPerformed += OnAttackPerformed;
    }

    private void OnDestroy()
    {
        // Event dinleyicisini kaldır
        ComboSystem.OnAttackPerformed -= OnAttackPerformed;
    }

    private void OnAttackPerformed()
    {
        // Saldırı yapıldığında, menzildeki tüm Enemy'lere hasar ver
        foreach (Enemy enemy in enemiesInRange)
        {
            if (enemy != null && !enemy.IsDead())
            {
                enemy.TakeDamage(damage);
                Debug.Log($"[WeaponHitDetector] {name}: Hit enemy {enemy.name} for {damage} damage!");
            }
        }

        // Temizle (null referansları kaldır)
        enemiesInRange.RemoveAll(e => e == null);
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Add(enemy);
            Debug.Log($"[WeaponHitDetector] {name}: Enemy {enemy.name} entered weapon range.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Remove(enemy);
            Debug.Log($"[WeaponHitDetector] {name}: Enemy {enemy.name} left weapon range.");
        }
    }
}

