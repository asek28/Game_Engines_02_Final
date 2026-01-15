using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RaritySpawnSettings
{
    [Header("Tier Settings")]
    public LootRarity rarity = LootRarity.Tier1;
    
    [Tooltip("Spawn şansı (weighted random için - yüksek değer = daha sık çıkar)")]
    [SerializeField, Min(0.1f)] public float spawnWeight = 1f;
    
    [Tooltip("Bu tier için scrap değeri")]
    [SerializeField, Min(1)] public int scrapValue = 1;
    
    [Tooltip("Bu tier için spawn edilecek minimum sayı")]
    [SerializeField, Min(0)] public int minSpawnCount = 0;
    
    [Tooltip("Bu tier için spawn edilecek maksimum sayı")]
    [SerializeField, Min(1)] public int maxSpawnCount = 1;
}

public class Thresh_Spawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField, Min(0f)] private float spawnRadius = 5f;
    [SerializeField] private float spawnHeightOffset = 0f;
    [SerializeField] private bool randomizeYRotation = true;
    [SerializeField, Min(1)] private int duplicatesPerTemplate = 1;
    
    [Header("Rarity System")]
    [Tooltip("Nadirlik sistemini kullan (aktifse, duplicatesPerTemplate yerine rarity ayarları kullanılır)")]
    [SerializeField] private bool useRaritySystem = false;
    
    [Tooltip("Template'lerdeki Loot component'lerinin ayarlarını kullan (aktifse, template'deki rarity ve scrapValue kullanılır)")]
    [SerializeField] private bool useTemplateLootSettings = false;
    
    [Tooltip("Her tier için spawn ayarları")]
    [SerializeField] private RaritySpawnSettings[] raritySettings = new RaritySpawnSettings[]
    {
        new RaritySpawnSettings { rarity = LootRarity.Tier1, spawnWeight = 70f, scrapValue = 1, minSpawnCount = 2, maxSpawnCount = 5 },
        new RaritySpawnSettings { rarity = LootRarity.Tier2, spawnWeight = 25f, scrapValue = 5, minSpawnCount = 1, maxSpawnCount = 3 },
        new RaritySpawnSettings { rarity = LootRarity.Tier3, spawnWeight = 5f, scrapValue = 10, minSpawnCount = 0, maxSpawnCount = 1 }
    };

    private readonly List<Transform> lootTemplates = new List<Transform>();

    /// <summary>
    /// LootBox'tan template almak için public metod
    /// </summary>
    public GameObject GetRandomLootTemplate()
    {
        if (lootTemplates == null || lootTemplates.Count == 0)
        {
            return null;
        }
        
        List<Transform> validTemplates = new List<Transform>();
        foreach (Transform template in lootTemplates)
        {
            if (template != null)
            {
                validTemplates.Add(template);
            }
        }
        
        if (validTemplates.Count == 0)
        {
            return null;
        }
        
        Transform selectedTemplate = validTemplates[Random.Range(0, validTemplates.Count)];
        return selectedTemplate != null ? selectedTemplate.gameObject : null;
    }

    private void Awake()
    {
        lootTemplates.Clear();

        Loot[] lootComponents = GetComponentsInChildren<Loot>(true);
        foreach (Loot loot in lootComponents)
        {
            if (loot == null)
            {
                continue;
            }

            Transform templateTransform = loot.transform;
            if (templateTransform == transform)
            {
                continue;
            }

            lootTemplates.Add(templateTransform);

            if (templateTransform.gameObject.activeSelf)
            {
                templateTransform.gameObject.SetActive(false);
            }
        }
    }

    private void Start()
    {
        HideSpawner();
        SpawnLoots();

        // Gün döngüsü eventini dinle
        DayNightCycle.OnDayComplete += OnDayComplete;
    }

    private void OnDestroy()
    {
        // Event dinleyicisini kaldır
        DayNightCycle.OnDayComplete -= OnDayComplete;
    }

    private void OnDayComplete()
    {
        // Her gün döngüsünde scrapleri tekrar spawn et
        SpawnLoots();
    }

    private void HideSpawner()
    {
        Renderer[] renderers = GetComponents<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }
    }

    private void SpawnLoots()
    {
        if (lootTemplates.Count == 0)
        {
            Debug.LogWarning($"{name}: No child templates with a Loot component found under Thresh_Spawner.");
            return;
        }

        if (useRaritySystem)
        {
            SpawnLootsWithRarity();
        }
        else
        {
            SpawnLootsLegacy();
        }
    }
    
    private void SpawnLootsLegacy()
    {
        // Eski sistem - duplicatesPerTemplate kullanır
        foreach (Transform template in lootTemplates)
        {
            if (template == null)
            {
                continue;
            }

            for (int i = 0; i < duplicatesPerTemplate; i++)
            {
                SpawnLootInstance(template);
            }
        }
    }
    
    private void SpawnLootsWithRarity()
    {
        if (useTemplateLootSettings)
        {
            // Template'lerdeki Loot component'lerinin ayarlarını kullan
            SpawnLootsWithTemplateSettings();
        }
        else
        {
            // Spawner'daki rarity ayarlarını kullan
            SpawnLootsWithSpawnerSettings();
        }
    }
    
    private void SpawnLootsWithTemplateSettings()
    {
        // Her template için, template'deki Loot component'inin ayarlarını kullan
        foreach (Transform template in lootTemplates)
        {
            if (template == null)
            {
                continue;
            }

            Loot templateLoot = template.GetComponent<Loot>();
            if (templateLoot == null)
            {
                Debug.LogWarning($"{name}: Template {template.name} has no Loot component. Skipping.");
                continue;
            }

            // Template'deki nadirlik ve scrap value'yu al
            LootRarity templateRarity = templateLoot.GetRarity();
            int templateScrapValue = templateLoot.GetScrapValue();
            
            // Bu template için spawn sayısını belirle (nadirliğe göre)
            RaritySpawnSettings settings = GetRaritySettings(templateRarity);
            int spawnCount = duplicatesPerTemplate; // Varsayılan olarak duplicatesPerTemplate kullan
            
            if (settings != null)
            {
                spawnCount = Random.Range(settings.minSpawnCount, settings.maxSpawnCount + 1);
            }
            
            for (int i = 0; i < spawnCount; i++)
            {
                GameObject instance = SpawnLootInstance(template);
                
                if (instance != null)
                {
                    Loot lootComponent = instance.GetComponent<Loot>();
                    if (lootComponent != null)
                    {
                        // Template'deki ayarları kullan (zaten doğru değerler var, sadece itemId ve displayName'i güncelle)
                        string newItemId = $"scrap_tier{(int)templateRarity + 1}_value{templateScrapValue}";
                        lootComponent.SetItemId(newItemId);
                        
                        string newDisplayName = $"Tier {(int)templateRarity + 1} Scrap ({templateScrapValue})";
                        lootComponent.SetItemDisplayName(newDisplayName);
                    }
                }
            }
        }
    }
    
    private void SpawnLootsWithSpawnerSettings()
    {
        // Yeni sistem - nadirlik bazlı spawn
        // Her template için toplam spawn sayısını belirle (tüm tier'ların toplamı)
        int totalSpawns = CalculateTotalSpawnCount();
        
        for (int spawnIndex = 0; spawnIndex < totalSpawns; spawnIndex++)
        {
            // Her spawn için bir template seç (rastgele)
            if (lootTemplates.Count == 0)
            {
                break;
            }
            
            Transform template = lootTemplates[Random.Range(0, lootTemplates.Count)];
            if (template == null)
            {
                continue;
            }

            // Her spawn edilen item için ayrı nadirlik seç
            LootRarity selectedRarity = SelectRarityByWeight();
            RaritySpawnSettings settings = GetRaritySettings(selectedRarity);
            
            if (settings == null)
            {
                Debug.LogWarning($"{name}: No settings found for rarity {selectedRarity}. Using template's default values.");
                SpawnLootInstance(template);
                continue;
            }
            
            GameObject instance = SpawnLootInstance(template);
            
            if (instance != null)
            {
                // Loot component'ini bul ve nadirlik/scrap value'yu ayarla
                Loot lootComponent = instance.GetComponent<Loot>();
                if (lootComponent != null)
                {
                    lootComponent.SetRarity(selectedRarity);
                    lootComponent.SetScrapValue(settings.scrapValue);
                    
                    // ItemId ve DisplayName'i güncelle
                    string newItemId = $"scrap_tier{(int)selectedRarity + 1}_value{settings.scrapValue}";
                    lootComponent.SetItemId(newItemId);
                    
                    string newDisplayName = $"Tier {(int)selectedRarity + 1} Scrap ({settings.scrapValue})";
                    lootComponent.SetItemDisplayName(newDisplayName);
                }
            }
        }
    }
    
    /// <summary>
    /// Tüm tier'ların min/max spawn sayılarına göre toplam spawn sayısını hesapla
    /// </summary>
    private int CalculateTotalSpawnCount()
    {
        int totalMin = 0;
        int totalMax = 0;
        
        foreach (RaritySpawnSettings settings in raritySettings)
        {
            totalMin += settings.minSpawnCount;
            totalMax += settings.maxSpawnCount;
        }
        
        // Ortalama bir değer döndür (veya min-max arası rastgele)
        return Random.Range(totalMin, totalMax + 1);
    }
    
    private LootRarity SelectRarityByWeight()
    {
        // Weighted random seçim
        float totalWeight = 0f;
        foreach (RaritySpawnSettings settings in raritySettings)
        {
            totalWeight += settings.spawnWeight;
        }
        
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (RaritySpawnSettings settings in raritySettings)
        {
            currentWeight += settings.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return settings.rarity;
            }
        }
        
        // Fallback - en yaygın tier
        return LootRarity.Tier1;
    }
    
    private RaritySpawnSettings GetRaritySettings(LootRarity rarity)
    {
        foreach (RaritySpawnSettings settings in raritySettings)
        {
            if (settings.rarity == rarity)
            {
                return settings;
            }
        }
        return null;
    }
    
    private GameObject SpawnLootInstance(Transform template)
    {
        Vector3 offset = Random.insideUnitSphere;
        offset.y = 0f;
        if (offset.sqrMagnitude > 0.001f)
        {
            offset.Normalize();
        }
        offset *= Random.Range(0f, spawnRadius);

        Vector3 spawnPosition = transform.position + new Vector3(offset.x, spawnHeightOffset, offset.z);
        
        // Özel rotation kontrolü: makas ve testere için X: -90
        Quaternion spawnRotation;
        string templateName = template.name.ToLower();
        bool isMakasOrTestere = templateName.Contains("makas") || templateName.Contains("testere");
        
        if (isMakasOrTestere)
        {
            // Makas ve testere için X: -90, Y rotation randomize edilmiş veya template'in Y rotation'ı
            float yRotation = randomizeYRotation 
                ? Random.Range(0f, 360f) 
                : template.rotation.eulerAngles.y;
            spawnRotation = Quaternion.Euler(-90f, yRotation, template.rotation.eulerAngles.z);
        }
        else
        {
            // Diğer loot'lar için normal rotation
            spawnRotation = randomizeYRotation
                ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                : template.rotation;
        }

        GameObject instance = Instantiate(template.gameObject, spawnPosition, spawnRotation, transform.parent);
        instance.SetActive(true);
        
        return instance;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * spawnHeightOffset, spawnRadius);
    }
}
