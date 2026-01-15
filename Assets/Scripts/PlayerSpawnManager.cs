using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Player spawn pozisyonunu yönetir - Shop'tan dönerken kaydedilen pozisyona spawn eder
/// </summary>
public class PlayerSpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Default spawn pozisyonu (Shop'tan dönmemişse)")]
    [SerializeField] private Transform defaultSpawnPoint;
    
    [Tooltip("Default spawn pozisyonu (Transform yoksa)")]
    [SerializeField] private Vector3 defaultSpawnPosition = new Vector3(0, 1, 0);
    
    [Header("Debug")]
    [Tooltip("Debug log'ları göster")]
    [SerializeField] private bool showDebugLogs = true;
    
    private void Start()
    {
        // Player'ı bul
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            if (showDebugLogs)
                Debug.LogWarning("[PlayerSpawnManager] Player not found!");
            return;
        }
        
        // Shop'tan mı dönülüyor kontrol et
        bool shouldRestore = PlayerPrefs.GetInt("ShouldRestorePosition", 0) == 1;
        
        if (shouldRestore && PlayerPrefs.HasKey("LastPosX"))
        {
            // Kaydedilen pozisyona spawn et
            RestorePlayerPosition(player);
        }
        else
        {
            // Default pozisyona spawn et
            SpawnAtDefaultPosition(player);
        }
        
        // Flag'i temizle
        PlayerPrefs.SetInt("ShouldRestorePosition", 0);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Player'ı kaydedilen pozisyona spawn eder
    /// </summary>
    private void RestorePlayerPosition(GameObject player)
    {
        float x = PlayerPrefs.GetFloat("LastPosX", 0);
        float y = PlayerPrefs.GetFloat("LastPosY", 1);
        float z = PlayerPrefs.GetFloat("LastPosZ", 0);
        Vector3 savedPosition = new Vector3(x, y, z);
        
        // CharacterController'ı geçici olarak devre dışı bırak
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.transform.position = savedPosition;
            cc.enabled = true;
        }
        else
        {
            player.transform.position = savedPosition;
        }
        
        if (showDebugLogs)
            Debug.Log($"[PlayerSpawnManager] Restored player to saved position: {savedPosition}");
    }
    
    /// <summary>
    /// Player'ı default pozisyona spawn eder
    /// </summary>
    private void SpawnAtDefaultPosition(GameObject player)
    {
        Vector3 spawnPos = defaultSpawnPoint != null 
            ? defaultSpawnPoint.position 
            : defaultSpawnPosition;
        
        // CharacterController'ı geçici olarak devre dışı bırak
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.transform.position = spawnPos;
            cc.enabled = true;
        }
        else
        {
            player.transform.position = spawnPos;
        }
        
        if (showDebugLogs)
            Debug.Log($"[PlayerSpawnManager] Spawned player at default position: {spawnPos}");
    }
}
