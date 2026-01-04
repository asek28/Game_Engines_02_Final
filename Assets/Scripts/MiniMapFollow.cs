using UnityEngine;

/// <summary>
/// MiniMap kamerasının Player'ı takip etmesini sağlar
/// Y ekseni sabit kalır, sadece X ve Z pozisyonu takip edilir
/// </summary>
public class MiniMapFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Takip edilecek Player Transform")]
    [SerializeField] private Transform playerTarget;
    
    [Header("Camera Settings")]
    [Tooltip("Kamera yüksekliği (Y pozisyonu)")]
    [SerializeField] private float height = 20f;
    [Tooltip("Orthographic Size (daha geniş görüş için artır)")]
    [SerializeField] private float orthographicSize = 15f;
    
    [Header("Offset Settings")]
    [Tooltip("Player pozisyonuna eklenen offset (X, Z)")]
    [SerializeField] private Vector2 offset = Vector2.zero;
    
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    
    private Camera cam;
    
    private void Awake()
    {
        // Kamera component'ini al
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = orthographicSize;
        }
        
        // Başlangıç pozisyon ve rotasyonunu kaydet
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        
        // Eğer height ayarlanmamışsa, mevcut Y pozisyonunu kullan
        if (height <= 0f)
        {
            height = initialPosition.y;
        }
        
        // Eğer playerTarget atanmamışsa, Player tag'ine sahip objeyi bul
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
            else
            {
                Debug.LogWarning("[MiniMapFollow] Player target not found! Please assign a player transform in the Inspector.");
            }
        }
    }
    
    private void LateUpdate()
    {
        if (playerTarget == null)
        {
            return;
        }
        
        // Player'ın X ve Z pozisyonunu al
        Vector3 playerPosition = playerTarget.position;
        
        // Kamera pozisyonunu ayarla (Y sabit kalır)
        Vector3 newPosition = new Vector3(
            playerPosition.x + offset.x,
            height,
            playerPosition.z + offset.y
        );
        
        transform.position = newPosition;
        
        // Rotasyonu sabit tut (başlangıç rotasyonu)
        transform.rotation = initialRotation;
    }
    
    /// <summary>
    /// Player target'ı manuel olarak ayarlar
    /// </summary>
    public void SetPlayerTarget(Transform target)
    {
        playerTarget = target;
    }
    
    /// <summary>
    /// Kamera yüksekliğini ayarlar
    /// </summary>
    public void SetHeight(float newHeight)
    {
        height = newHeight;
    }
    
    /// <summary>
    /// Orthographic size'ı ayarlar
    /// </summary>
    public void SetOrthographicSize(float size)
    {
        orthographicSize = size;
        if (cam != null)
        {
            cam.orthographicSize = orthographicSize;
        }
    }
}

