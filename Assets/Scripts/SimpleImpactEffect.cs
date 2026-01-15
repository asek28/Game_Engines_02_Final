using UnityEngine;

/// <summary>
/// Basit impact efekti - Partiküller, ışık, ses
/// Bullet impact için kullanılır
/// </summary>
public class SimpleImpactEffect : MonoBehaviour
{
    [Header("Particle Settings")]
    [SerializeField] private bool autoPlay = true;
    [SerializeField] private float lifetime = 2f;
    
    [Header("Visual Settings")]
    [SerializeField] private Color impactColor = new Color(1f, 0.6f, 0f); // Orange
    [SerializeField] private float lightIntensity = 5f;
    [SerializeField] private float lightRange = 5f;
    
    private ParticleSystem[] particles;
    private Light impactLight;
    
    private void Awake()
    {
        // Partikülleri al
        particles = GetComponentsInChildren<ParticleSystem>();
        
        // Eğer particle yoksa, basit bir tane oluştur
        if (particles == null || particles.Length == 0)
        {
            CreateDefaultParticleSystem();
        }
        
        // Light ekle
        CreateImpactLight();
    }
    
    private void Start()
    {
        if (autoPlay)
        {
            PlayEffect();
        }
        
        // Lifetime sonunda yok et
        Destroy(gameObject, lifetime);
    }
    
    /// <summary>
    /// Varsayılan particle system oluştur
    /// </summary>
    private void CreateDefaultParticleSystem()
    {
        GameObject particleObj = new GameObject("ImpactParticles");
        particleObj.transform.SetParent(transform);
        particleObj.transform.localPosition = Vector3.zero;
        
        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
        
        // Main module
        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startColor = impactColor;
        main.maxParticles = 20;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // Emission
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 10) });
        
        // Shape (sphere)
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;
        
        // Color over lifetime (fade out)
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(impactColor, 0f), new GradientColorKey(impactColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;
        
        // Size over lifetime (shrink)
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);
        
        // Renderer
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.SetColor("_EmissionColor", impactColor * 2f);
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        particles = new ParticleSystem[] { ps };
    }
    
    /// <summary>
    /// Impact ışığı oluştur
    /// </summary>
    private void CreateImpactLight()
    {
        GameObject lightObj = new GameObject("ImpactLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;
        
        impactLight = lightObj.AddComponent<Light>();
        impactLight.type = LightType.Point;
        impactLight.color = impactColor;
        impactLight.intensity = lightIntensity;
        impactLight.range = lightRange;
        impactLight.renderMode = LightRenderMode.ForcePixel;
        
        // Işığı fade out yap
        StartCoroutine(FadeOutLight());
    }
    
    /// <summary>
    /// Efekti oynat
    /// </summary>
    public void PlayEffect()
    {
        if (particles != null)
        {
            foreach (ParticleSystem ps in particles)
            {
                if (ps != null)
                {
                    ps.Play();
                }
            }
        }
    }
    
    /// <summary>
    /// Işığı fade out yap
    /// </summary>
    private System.Collections.IEnumerator FadeOutLight()
    {
        if (impactLight == null) yield break;
        
        float startIntensity = impactLight.intensity;
        float timer = 0f;
        float fadeDuration = 0.2f;
        
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            impactLight.intensity = Mathf.Lerp(startIntensity, 0f, timer / fadeDuration);
            yield return null;
        }
        
        impactLight.enabled = false;
    }
}
