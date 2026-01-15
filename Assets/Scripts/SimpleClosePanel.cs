using UnityEngine;

/// <summary>
/// Çok basit - Butona basınca panel'i kapat
/// </summary>
public class SimpleClosePanel : MonoBehaviour
{
    [Tooltip("Kapatılacak panel")]
    public GameObject panelToClose;
    
    /// <summary>
    /// Panel'i kapat
    /// </summary>
    public void ClosePanel()
    {
        if (panelToClose != null)
        {
            panelToClose.SetActive(false);
            Debug.Log($"[SimpleClosePanel] Closed: {panelToClose.name}");
        }
    }
}
