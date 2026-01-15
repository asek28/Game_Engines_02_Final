using UnityEngine;

/// <summary>
/// Basit muzzle flash efekti
/// Silahın namlusundan çıkan alev efekti
/// </summary>
public class SimpleMuzzleFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private float flashDuration = 0.05f;
    [SerializeField] private Color flashColor = new Color(1f, 0.8f, 0.3f); // Yellow-Orange
    [SerializeField] private float lightIntensity = 10f;
    [SerializeField] private float lightRange = 3f;
    
    [Header("Visual Settings")]
    [SerializeField] private GameObject[] flashObjects;
    [SerializeField] private Light flashLight;
    
    private void Awake()
    {
        // Eğer light yoksa oluştur
        if (flashLight == null)
        {
            CreateFlashLight();
        }
        
        // Eğer flash object'leri yoksa oluştur
        if (flashObjects == null || flashObjects.Length == 0)
        {
            CreateDefaultFlashObjects();
        }
        
        // Başlangıçta hepsini kapat
        HideFlash();
    }
    
    /// <summary>
    /// Flash ışığı oluştur
    /// </summary>
    private void CreateFlashLight()
    {
        GameObject lightObj = new GameObject("MuzzleFlashLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;
        
        flashLight = lightObj.AddComponent<Light>();
        flashLight.type = LightType.Point;
        flashLight.color = flashColor;
        flashLight.intensity = lightIntensity;
        flashLight.range = lightRange;
        flashLight.renderMode = LightRenderMode.ForcePixel;
        flashLight.enabled = false;
    }
    
    /// <summary>
    /// Varsayılan flash object'leri oluştur (basit quad'lar)
    /// </summary>
    private void CreateDefaultFlashObjects()
    {
        int flashCount = 2; // 2 flash sprite (random rotation için)
        flashObjects = new GameObject[flashCount];
        
        for (int i = 0; i < flashCount; i++)
        {
            GameObject flashObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            flashObj.name = $"Flash_{i}";
            flashObj.transform.SetParent(transform);
            flashObj.transform.localPosition = Vector3.forward * 0.3f;
            flashObj.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            flashObj.transform.localScale = Vector3.one * 0.5f;
            
            // Material
            Renderer renderer = flashObj.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = flashColor;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", flashColor * 3f);
            renderer.material = mat;
            
            // Collider'ı kaldır
            Collider collider = flashObj.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
            
            flashObjects[i] = flashObj;
            flashObj.SetActive(false);
        }
    }
    
    /// <summary>
    /// Flash göster
    /// </summary>
    public void ShowFlash()
    {
        // Flash object'lerini göster (random rotation)
        if (flashObjects != null)
        {
            foreach (GameObject flashObj in flashObjects)
            {
                if (flashObj != null)
                {
                    flashObj.SetActive(true);
                    // Random rotation
                    flashObj.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
                }
            }
        }
        
        // Işığı aç
        if (flashLight != null)
        {
            flashLight.enabled = true;
        }
        
        // Kısa süre sonra kapat
        Invoke(nameof(HideFlash), flashDuration);
    }
    
    /// <summary>
    /// Flash gizle
    /// </summary>
    private void HideFlash()
    {
        if (flashObjects != null)
        {
            foreach (GameObject flashObj in flashObjects)
            {
                if (flashObj != null)
                {
                    flashObj.SetActive(false);
                }
            }
        }
        
        if (flashLight != null)
        {
            flashLight.enabled = false;
        }
    }
}
