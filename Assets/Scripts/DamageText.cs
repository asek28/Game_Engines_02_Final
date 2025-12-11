using UnityEngine;
using TMPro;

/// <summary>
/// Floating damage text - hasar sayısını gösterir ve yukarı doğru kaybolur
/// </summary>
public class DamageText : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Yukarı hareket hızı")]
    [SerializeField] private float floatSpeed = 2f;
    [Tooltip("Fade out süresi (saniye)")]
    [SerializeField] private float fadeDuration = 1f;
    [Tooltip("Yaşam süresi (saniye)")]
    [SerializeField] private float lifetime = 2f;
    
    private TextMeshPro textMesh;
    private Vector3 startPosition;
    private float elapsedTime = 0f;
    
    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null)
        {
            textMesh = GetComponentInChildren<TextMeshPro>();
        }
        
        startPosition = transform.position;
    }
    
    private void Start()
    {
        // Belirli süre sonra yok et
        Destroy(gameObject, lifetime);
    }
    
    private void Update()
    {
        elapsedTime += Time.deltaTime;
        
        // Yukarı hareket
        transform.position = startPosition + Vector3.up * (floatSpeed * elapsedTime);
        
        // Fade out
        if (textMesh != null && elapsedTime > fadeDuration * 0.5f)
        {
            float alpha = 1f - ((elapsedTime - fadeDuration * 0.5f) / (fadeDuration * 0.5f));
            alpha = Mathf.Clamp01(alpha);
            Color color = textMesh.color;
            color.a = alpha;
            textMesh.color = color;
        }
    }
    
    /// <summary>
    /// Damage değerini set eder
    /// </summary>
    public void SetDamage(int damage)
    {
        if (textMesh != null)
        {
            textMesh.text = damage.ToString();
        }
    }
    
    /// <summary>
    /// Damage değerini ve rengini set eder
    /// </summary>
    public void SetDamage(int damage, Color color)
    {
        if (textMesh != null)
        {
            textMesh.text = damage.ToString();
            textMesh.color = color;
        }
    }
}

