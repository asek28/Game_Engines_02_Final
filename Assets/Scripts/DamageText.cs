using UnityEngine;
using TMPro;

/// <summary>
/// Floating damage text - Hasar sayısını gösterir ve yukarı doğru yüzer
/// </summary>
[RequireComponent(typeof(TextMeshPro))]
public class DamageText : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private Vector3 randomOffset = new Vector3(0.5f, 0.5f, 0.5f);
    
    [Header("Text Settings")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float fontSize = 4f;
    
    private TextMeshPro textMesh;
    private float timer = 0f;
    private Vector3 floatDirection;
    
    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        
        // Text ayarları
        textMesh.fontSize = fontSize;
        textMesh.color = damageColor;
        textMesh.alignment = TextAlignmentOptions.Center;
        
        // Random offset
        Vector3 randomPos = new Vector3(
            Random.Range(-randomOffset.x, randomOffset.x),
            Random.Range(0, randomOffset.y),
            Random.Range(-randomOffset.z, randomOffset.z)
        );
        transform.position += randomPos;
        
        // Yukarı doğru float
        floatDirection = Vector3.up;
    }
    
    private void Start()
    {
        // Camera'ya bak
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
        }
    }
    
    private void Update()
    {
        // Yukarı doğru hareket et
        transform.position += floatDirection * floatSpeed * Time.deltaTime;
        
        // Fade out
        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);
        
        Color currentColor = textMesh.color;
        currentColor.a = alpha;
        textMesh.color = currentColor;
        
        // Lifetime bitti mi?
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
        
        // Camera'ya bak (her frame)
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
        }
    }
    
    /// <summary>
    /// Hasar miktarını set et
    /// </summary>
    public void SetDamage(int damage)
    {
        if (textMesh != null)
        {
            textMesh.text = damage.ToString();
        }
    }
    
    /// <summary>
    /// Hasar miktarını ve rengini set et
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
