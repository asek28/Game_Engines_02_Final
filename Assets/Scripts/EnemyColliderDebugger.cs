using UnityEngine;

/// <summary>
/// Enemy collider yapısını debug eden script
/// Enemy GameObject'lerine ekle ve console'da detaylı bilgi gör
/// </summary>
public class EnemyColliderDebugger : MonoBehaviour
{
    private void Start()
    {
        DebugEnemyStructure();
    }
    
    private void DebugEnemyStructure()
    {
        Debug.Log($"<color=cyan>═══════════════════════════════════════</color>");
        Debug.Log($"<color=cyan>🔍 ENEMY DEBUG: {name}</color>");
        Debug.Log($"<color=cyan>═══════════════════════════════════════</color>");
        
        // GameObject bilgileri
        Debug.Log($"Tag: {tag}");
        Debug.Log($"Layer: {LayerMask.LayerToName(gameObject.layer)}");
        Debug.Log($"Position: {transform.position}");
        
        // EnemyAIController var mı?
        EnemyAIController enemyAI = GetComponent<EnemyAIController>();
        if (enemyAI != null)
        {
            Debug.Log($"✅ EnemyAIController FOUND on main GameObject");
            Debug.Log($"   - Health: {enemyAI.GetCurrentHealth()}/{enemyAI.GetMaxHealth()}");
        }
        else
        {
            Debug.LogWarning($"❌ EnemyAIController NOT FOUND on main GameObject!");
        }
        
        // Collider'lar
        Collider[] colliders = GetComponents<Collider>();
        Debug.Log($"Colliders on main GameObject: {colliders.Length}");
        foreach (Collider col in colliders)
        {
            Debug.Log($"   - {col.GetType().Name}: isTrigger={col.isTrigger}, enabled={col.enabled}");
        }
        
        // CharacterController var mı?
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            Debug.Log($"✅ CharacterController FOUND: radius={cc.radius}, height={cc.height}, enabled={cc.enabled}");
        }
        
        // Child collider'lar
        Collider[] childColliders = GetComponentsInChildren<Collider>(true);
        Debug.Log($"Total colliders (including children): {childColliders.Length}");
        foreach (Collider col in childColliders)
        {
            if (col.gameObject != gameObject)
            {
                Debug.Log($"   - CHILD: {col.gameObject.name} > {col.GetType().Name}: isTrigger={col.isTrigger}");
                
                // Child'da EnemyAIController var mı?
                EnemyAIController childEnemy = col.GetComponent<EnemyAIController>();
                if (childEnemy != null)
                {
                    Debug.LogWarning($"      ⚠️ EnemyAIController found on CHILD! This might cause issues.");
                }
            }
        }
        
        Debug.Log($"<color=cyan>═══════════════════════════════════════</color>");
    }
    
    // Gizmos ile collider'ları görselleştir
    private void OnDrawGizmos()
    {
        // CharacterController
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + cc.center, cc.radius);
        }
        
        // Collider'lar
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            if (col is CapsuleCollider capsule)
            {
                Gizmos.color = capsule.isTrigger ? Color.yellow : Color.red;
                Gizmos.DrawWireSphere(transform.position + capsule.center, capsule.radius);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.color = sphere.isTrigger ? Color.yellow : Color.red;
                Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
            }
        }
    }
}
