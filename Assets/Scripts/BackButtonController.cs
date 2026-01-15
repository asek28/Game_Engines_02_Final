using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Görsel Back butonu - Hover efekti ve animasyon
/// </summary>
public class BackButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual Settings")]
    [Tooltip("Normal renk")]
    [SerializeField] private Color normalColor = Color.white;
    
    [Tooltip("Hover renk (üzerine gelindiğinde)")]
    [SerializeField] private Color hoverColor = new Color(1f, 0.9f, 0.5f); // Sarımsı
    
    [Tooltip("Scale animasyonu uygula")]
    [SerializeField] private bool useScaleAnimation = true;
    
    [Tooltip("Hover olduğunda büyütme oranı")]
    [SerializeField] private float hoverScale = 1.1f;
    
    private Image buttonImage;
    private Vector3 originalScale;
    private bool isHovering = false;
    
    private void Awake()
    {
        buttonImage = GetComponent<Image>();
        originalScale = transform.localScale;
        
        if (buttonImage != null)
        {
            buttonImage.color = normalColor;
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        
        // Renk değiştir
        if (buttonImage != null)
        {
            buttonImage.color = hoverColor;
        }
        
        // Scale animasyonu
        if (useScaleAnimation)
        {
            transform.localScale = originalScale * hoverScale;
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        
        // Normal renge dön
        if (buttonImage != null)
        {
            buttonImage.color = normalColor;
        }
        
        // Normal scale'e dön
        if (useScaleAnimation)
        {
            transform.localScale = originalScale;
        }
    }
}
