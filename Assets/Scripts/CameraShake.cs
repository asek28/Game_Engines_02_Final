using System.Collections;
using UnityEngine;

/// <summary>
/// Camera shake sistemi - hit impact için ekran sarsıntısı sağlar
/// </summary>
public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [Tooltip("Shake süresi (saniye)")]
    [SerializeField, Min(0.01f)] private float shakeDuration = 0.2f;
    [Tooltip("Shake gücü (ne kadar sarsılacak)")]
    [SerializeField, Min(0.01f)] private float shakeMagnitude = 0.1f;
    [Tooltip("Shake sıklığı (ne kadar hızlı sarsılacak)")]
    [SerializeField, Min(1f)] private float shakeFrequency = 10f;
    
    private Vector3 originalPosition;
    private bool isShaking = false;
    
    private void Awake()
    {
        originalPosition = transform.localPosition;
    }
    
    /// <summary>
    /// Kamera shake'i başlatır
    /// </summary>
    public void Shake()
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine());
        }
    }
    
    /// <summary>
    /// Özelleştirilmiş shake (güç ve süre ayarlanabilir)
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine(duration, magnitude));
        }
    }
    
    private IEnumerator ShakeCoroutine()
    {
        yield return StartCoroutine(ShakeCoroutine(shakeDuration, shakeMagnitude));
    }
    
    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            // Rastgele offset hesapla (Perlin noise kullanarak daha smooth shake)
            float x = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f) * 2f * magnitude;
            float y = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f) * 2f * magnitude;
            
            transform.localPosition = originalPosition + new Vector3(x, y, 0f);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Orijinal pozisyona dön
        transform.localPosition = originalPosition;
        isShaking = false;
    }
}

