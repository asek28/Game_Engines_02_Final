using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Shop sahnesinde NavMeshAgent'ı devre dışı bırakır
/// Player CharacterController ile hareket edebilir
/// </summary>
[DefaultExecutionOrder(-100)] // SimplePlayerMovement'ten önce çalış
public class DisableNavMeshInShop : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Shop sahnesinde NavMesh'i devre dışı bırak")]
    [SerializeField] private bool disableNavMeshInShop = true;
    
    [Tooltip("CharacterController'ı etkinleştir")]
    [SerializeField] private bool enableCharacterController = true;
    
    private void Start()
    {
        if (!disableNavMeshInShop) return;
        
        // Player'ı bul
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[DisableNavMeshInShop] Player not found!");
            return;
        }
        
        Debug.Log("<color=green>[DisableNavMeshInShop] Setting up Player for Shop Scene...</color>");
        
        // NavMeshAgent'ı devre dışı bırak
        NavMeshAgent navAgent = player.GetComponent<NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.enabled = false;
            Debug.Log("[DisableNavMeshInShop] ✓ NavMeshAgent disabled.");
        }
        
        // CharacterController'ı etkinleştir
        if (enableCharacterController)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = true;
                Debug.Log("[DisableNavMeshInShop] ✓ CharacterController enabled.");
            }
        }
        
        // SimplePlayerMovement'i etkinleştir
        SimplePlayerMovement simpleMovement = player.GetComponent<SimplePlayerMovement>();
        if (simpleMovement != null)
        {
            simpleMovement.enabled = true;
            Debug.Log("[DisableNavMeshInShop] ✓ SimplePlayerMovement enabled.");
        }
        
        Debug.Log("<color=green>[DisableNavMeshInShop] ✓✓✓ Shop Scene setup complete!</color>");
    }
}
