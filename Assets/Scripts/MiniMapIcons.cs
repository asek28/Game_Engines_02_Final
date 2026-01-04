using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// MiniMap üzerinde Player ve Enemy iconlarını gösterir
/// Player: Ok/üçgen ikonu (baktığı yöne göre)
/// Enemy: Kırmızı yuvarlak ikon
/// </summary>
public class MiniMapIcons : MonoBehaviour
{
    [Header("Canvas Settings")]
    [Tooltip("MiniMap Canvas (RectTransform) - Iconlar buraya eklenecek")]
    [SerializeField] private RectTransform miniMapCanvas;
    
    [Header("Camera Settings")]
    [Tooltip("MiniMap kamerası")]
    [SerializeField] private Camera miniMapCamera;
    [Tooltip("Orthographic size (kamera ayarlarından alınır)")]
    [SerializeField] private float orthographicSize = 15f;
    
    [Header("Player Icon Settings")]
    [Tooltip("Player ikonu için prefab (ok/üçgen) - Boşsa otomatik oluşturulur")]
    [SerializeField] private GameObject playerIconPrefab;
    [Tooltip("Player ikonunun boyutu")]
    [SerializeField] private float playerIconSize = 20f;
    [Tooltip("Player ikonunun rengi")]
    [SerializeField] private Color playerIconColor = Color.blue;
    
    [Header("Enemy Icon Settings")]
    [Tooltip("Enemy ikonu için prefab (yuvarlak) - Boşsa otomatik oluşturulur")]
    [SerializeField] private GameObject enemyIconPrefab;
    [Tooltip("Enemy ikonunun boyutu")]
    [SerializeField] private float enemyIconSize = 15f;
    [Tooltip("Enemy ikonunun rengi")]
    [SerializeField] private Color enemyIconColor = Color.red;
    
    [Header("Update Settings")]
    [Tooltip("Icon güncelleme sıklığı (saniye)")]
    [SerializeField] private float updateInterval = 0.1f;
    
    private Transform playerTarget;
    private GameObject playerIconInstance;
    private RectTransform playerIconRect;
    
    private Dictionary<Enemy, GameObject> enemyIcons = new Dictionary<Enemy, GameObject>();
    private Dictionary<Enemy, RectTransform> enemyIconRects = new Dictionary<Enemy, RectTransform>();
    
    private float updateTimer = 0f;
    
    private void Awake()
    {
        // Canvas'ı bul
        if (miniMapCanvas == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                miniMapCanvas = canvas.GetComponent<RectTransform>();
            }
        }
        
        // Kamerayı bul
        if (miniMapCamera == null)
        {
            miniMapCamera = GetComponentInChildren<Camera>();
            if (miniMapCamera == null)
            {
                miniMapCamera = FindObjectOfType<Camera>();
            }
        }
        
        if (miniMapCamera != null && miniMapCamera.orthographic)
        {
            orthographicSize = miniMapCamera.orthographicSize;
        }
    }
    
    private void Start()
    {
        // Player'ı bul
        FindPlayer();
        
        // Player ikonunu oluştur
        CreatePlayerIcon();
        
        // Enemy iconlarını oluştur
        CreateEnemyIcons();
    }
    
    private void Update()
    {
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            
            // Player ikonunu güncelle
            UpdatePlayerIcon();
            
            // Enemy iconlarını güncelle
            UpdateEnemyIcons();
        }
    }
    
    /// <summary>
    /// Player'ı bulur
    /// </summary>
    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
        }
        else
        {
            CharacterController controller = FindObjectOfType<CharacterController>();
            if (controller != null)
            {
                playerTarget = controller.transform;
            }
        }
        
        if (playerTarget == null)
        {
            Debug.LogWarning("[MiniMapIcons] Player bulunamadı!");
        }
    }
    
    /// <summary>
    /// Player ikonunu oluşturur (ok/üçgen)
    /// </summary>
    private void CreatePlayerIcon()
    {
        if (miniMapCanvas == null || playerTarget == null)
        {
            return;
        }
        
        if (playerIconPrefab != null)
        {
            playerIconInstance = Instantiate(playerIconPrefab, miniMapCanvas);
        }
        else
        {
            // Yeni bir GameObject oluştur
            playerIconInstance = new GameObject("PlayerIcon");
            playerIconInstance.transform.SetParent(miniMapCanvas, false);
            
            // RectTransform ekle
            playerIconRect = playerIconInstance.AddComponent<RectTransform>();
            playerIconRect.sizeDelta = new Vector2(playerIconSize, playerIconSize);
            playerIconRect.anchorMin = new Vector2(0.5f, 0.5f);
            playerIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            playerIconRect.pivot = new Vector2(0.5f, 0.5f);
            
            // Image ekle
            Image iconImage = playerIconInstance.AddComponent<Image>();
            iconImage.color = playerIconColor;
            
            // Basit bir üçgen/ok şekli oluştur (Sprite olarak)
            // Unity Editor'da bir ok Sprite'ı oluşturup atayabilirsiniz
            // Şimdilik basit bir kare göster, sonra Sprite ile değiştirilebilir
        }
        
        if (playerIconRect == null)
        {
            playerIconRect = playerIconInstance.GetComponent<RectTransform>();
        }
    }
    
    /// <summary>
    /// Enemy iconlarını oluşturur
    /// </summary>
    private void CreateEnemyIcons()
    {
        if (miniMapCanvas == null)
        {
            return;
        }
        
        // Tüm enemy'leri bul
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || enemy.IsDead())
            {
                continue;
            }
            
            CreateEnemyIcon(enemy);
        }
    }
    
    /// <summary>
    /// Tek bir enemy için ikon oluşturur
    /// </summary>
    private void CreateEnemyIcon(Enemy enemy)
    {
        if (enemy == null || enemyIcons.ContainsKey(enemy))
        {
            return;
        }
        
        GameObject iconObject;
        
        if (enemyIconPrefab != null)
        {
            iconObject = Instantiate(enemyIconPrefab, miniMapCanvas);
        }
        else
        {
            // Yeni bir GameObject oluştur
            iconObject = new GameObject($"EnemyIcon_{enemy.name}");
            iconObject.transform.SetParent(miniMapCanvas, false);
            
            // RectTransform ekle
            RectTransform rectTransform = iconObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(enemyIconSize, enemyIconSize);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            
            // Image ekle
            Image iconImage = iconObject.AddComponent<Image>();
            iconImage.color = enemyIconColor;
            
            // Yuvarlak şekil için (Sprite olarak)
            // Unity Editor'da bir yuvarlak Sprite oluşturup atayabilirsiniz
        }
        
        enemyIcons[enemy] = iconObject;
        enemyIconRects[enemy] = iconObject.GetComponent<RectTransform>();
    }
    
    /// <summary>
    /// Player ikonunu günceller
    /// </summary>
    private void UpdatePlayerIcon()
    {
        if (playerIconInstance == null || playerTarget == null || miniMapCamera == null || miniMapCanvas == null)
        {
            return;
        }
        
        // Player'ın dünya pozisyonunu al
        Vector3 worldPos = playerTarget.position;
        
        // Kameranın görüş alanını hesapla
        float orthoSize = miniMapCamera.orthographicSize;
        Vector3 cameraPos = miniMapCamera.transform.position;
        
        // Player'ın kameraya göre pozisyonunu hesapla
        Vector3 relativePos = worldPos - cameraPos;
        
        // Orthographic kamera için normalize et
        float normalizedX = (relativePos.x / (orthoSize * 2f)) + 0.5f;
        float normalizedY = (relativePos.z / (orthoSize * 2f)) + 0.5f;
        
        // Clamp değerleri
        normalizedX = Mathf.Clamp01(normalizedX);
        normalizedY = Mathf.Clamp01(normalizedY);
        
        // Canvas boyutuna göre pozisyonu hesapla
        Vector2 canvasPos = new Vector2(
            (normalizedX - 0.5f) * miniMapCanvas.sizeDelta.x,
            (normalizedY - 0.5f) * miniMapCanvas.sizeDelta.y
        );
        
        // İkonu güncelle
        if (playerIconRect != null)
        {
            playerIconRect.anchoredPosition = canvasPos;
            
            // Player'ın rotasyonuna göre ikonu döndür (ok yönü)
            float playerYRotation = playerTarget.eulerAngles.y;
            playerIconRect.localEulerAngles = new Vector3(0f, 0f, -playerYRotation);
        }
    }
    
    /// <summary>
    /// Enemy iconlarını günceller
    /// </summary>
    private void UpdateEnemyIcons()
    {
        if (miniMapCamera == null || miniMapCanvas == null)
        {
            return;
        }
        
        // Yeni enemy'leri kontrol et
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy == null)
            {
                continue;
            }
            
            // Ölü enemy'lerin iconunu kaldır
            if (enemy.IsDead())
            {
                if (enemyIcons.ContainsKey(enemy))
                {
                    Destroy(enemyIcons[enemy]);
                    enemyIcons.Remove(enemy);
                    enemyIconRects.Remove(enemy);
                }
                continue;
            }
            
            // Yeni enemy için ikon oluştur
            if (!enemyIcons.ContainsKey(enemy))
            {
                CreateEnemyIcon(enemy);
            }
        }
        
        // Eski enemy iconlarını temizle (artık sahnede yoksa)
        List<Enemy> enemiesToRemove = new List<Enemy>();
        foreach (Enemy enemy in enemyIcons.Keys)
        {
            if (enemy == null || enemy.IsDead())
            {
                enemiesToRemove.Add(enemy);
            }
        }
        
        foreach (Enemy enemy in enemiesToRemove)
        {
            if (enemyIcons.ContainsKey(enemy))
            {
                Destroy(enemyIcons[enemy]);
                enemyIcons.Remove(enemy);
                enemyIconRects.Remove(enemy);
            }
        }
        
        // Enemy iconlarını güncelle
        float orthoSize = miniMapCamera.orthographicSize;
        Vector3 cameraPos = miniMapCamera.transform.position;
        
        foreach (KeyValuePair<Enemy, RectTransform> kvp in enemyIconRects)
        {
            Enemy enemy = kvp.Key;
            RectTransform iconRect = kvp.Value;
            
            if (enemy == null || iconRect == null || enemy.IsDead())
            {
                continue;
            }
            
            // Enemy'nin dünya pozisyonunu al
            Vector3 worldPos = enemy.transform.position;
            Vector3 relativePos = worldPos - cameraPos;
            
            // Normalize et
            float normalizedX = (relativePos.x / (orthoSize * 2f)) + 0.5f;
            float normalizedY = (relativePos.z / (orthoSize * 2f)) + 0.5f;
            
            // Clamp değerleri
            normalizedX = Mathf.Clamp01(normalizedX);
            normalizedY = Mathf.Clamp01(normalizedY);
            
            // Canvas boyutuna göre pozisyonu hesapla
            Vector2 canvasPos = new Vector2(
                (normalizedX - 0.5f) * miniMapCanvas.sizeDelta.x,
                (normalizedY - 0.5f) * miniMapCanvas.sizeDelta.y
            );
            
            // İkonu güncelle
            iconRect.anchoredPosition = canvasPos;
            // Enemy iconları dönmez, sadece yuvarlak
        }
    }
    
    private void OnDestroy()
    {
        // Tüm iconları temizle
        if (playerIconInstance != null)
        {
            Destroy(playerIconInstance);
        }
        
        foreach (GameObject icon in enemyIcons.Values)
        {
            if (icon != null)
            {
                Destroy(icon);
            }
        }
        
        enemyIcons.Clear();
        enemyIconRects.Clear();
    }
}

