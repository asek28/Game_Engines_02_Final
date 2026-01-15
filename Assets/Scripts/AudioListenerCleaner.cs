using UnityEngine;

/// <summary>
/// Sahnedeki gereksiz AudioListener'ları temizler ve Unity'nin uyarısını önler
/// </summary>
public class AudioListenerCleaner : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Sadece Main Camera'da AudioListener bırak (diğerlerini kapat)")]
    [SerializeField] private bool keepOnlyMainCameraListener = true;
    
    [Tooltip("Console'a log yazdır")]
    [SerializeField] private bool showDebugLogs = false;
    
    private void Awake()
    {
        CleanupAudioListeners();
    }
    
    private void CleanupAudioListeners()
    {
        AudioListener[] allListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        
        if (allListeners.Length <= 1)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[AudioListenerCleaner] Scene has {allListeners.Length} AudioListener(s). No cleanup needed.");
            }
            return;
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[AudioListenerCleaner] Found {allListeners.Length} AudioListeners. Cleaning up...");
        }
        
        if (keepOnlyMainCameraListener)
        {
            // Main Camera'daki AudioListener'ı bul
            Camera mainCamera = Camera.main;
            AudioListener mainCameraListener = null;
            
            if (mainCamera != null)
            {
                mainCameraListener = mainCamera.GetComponent<AudioListener>();
            }
            
            // Tüm AudioListener'ları kontrol et
            foreach (AudioListener listener in allListeners)
            {
                if (listener == null)
                {
                    continue;
                }
                
                // Main Camera'daki listener'ı koru
                if (listener == mainCameraListener)
                {
                    if (showDebugLogs)
                    {
                        Debug.Log($"[AudioListenerCleaner] Keeping AudioListener on Main Camera: {mainCamera.name}");
                    }
                    continue;
                }
                
                // Diğer AudioListener'ları kapat
                listener.enabled = false;
                if (showDebugLogs)
                {
                    Debug.Log($"[AudioListenerCleaner] Disabled AudioListener on: {listener.gameObject.name}");
                }
            }
        }
        else
        {
            // İlk AudioListener'ı koru, diğerlerini kapat
            for (int i = 1; i < allListeners.Length; i++)
            {
                if (allListeners[i] != null)
                {
                    allListeners[i].enabled = false;
                    if (showDebugLogs)
                    {
                        Debug.Log($"[AudioListenerCleaner] Disabled AudioListener on: {allListeners[i].gameObject.name}");
                    }
                }
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[AudioListenerCleaner] Cleanup complete. Active AudioListeners: {GetActiveListenerCount()}");
        }
    }
    
    private int GetActiveListenerCount()
    {
        AudioListener[] allListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        int activeCount = 0;
        foreach (AudioListener listener in allListeners)
        {
            if (listener != null && listener.enabled)
            {
                activeCount++;
            }
        }
        return activeCount;
    }
}
