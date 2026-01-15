using UnityEngine;

/// <summary>
/// MainMenu scene'inde cursor'ın her zaman görünür ve serbest olmasını garanti eder
/// </summary>
public class MainMenuCursorController : MonoBehaviour
{
    private void Start()
    {
        // MainMenu'de cursor her zaman görünür ve serbest olmalı
        SetupCursor();
    }
    
    private void OnEnable()
    {
        // Scene aktif olduğunda cursor'ı ayarla
        SetupCursor();
    }
    
    private void Update()
    {
        // Her frame cursor durumunu kontrol et (güvenlik için)
        // Eğer başka bir script cursor'ı kilitlemeye çalışırsa, tekrar aç
        if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
        {
            SetupCursor();
        }
    }
    
    /// <summary>
    /// Cursor'ı görünür ve serbest yapar
    /// </summary>
    private void SetupCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Time.timeScale'i de kontrol et (pause durumundan çık)
        if (Time.timeScale != 1f)
        {
            Time.timeScale = 1f;
            Debug.Log("[MainMenuCursorController] Reset Time.timeScale to 1.");
        }
        
        Debug.Log("[MainMenuCursorController] Cursor unlocked and visible.");
    }
}
