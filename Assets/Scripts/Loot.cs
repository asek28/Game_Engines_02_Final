using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class Loot : MonoBehaviour
{
    [Header("Loot Info")]
    [SerializeField] private string itemId = "scrap_value5";
    [SerializeField] private string itemDisplayName = "Scrap Value 5";
    [SerializeField, Min(1)] private int scrapValue = 1;

    [Header("Highlight Settings")]
    [SerializeField] private bool useHighlight = true;
    [SerializeField] private Color highlightColor = new Color(1f, 0.88f, 0f); // warm yellow default
    [SerializeField, Min(0f)] private float highlightIntensity = 2.5f;
    [SerializeField] private bool adjustOutline = true;
    [SerializeField, Min(0f)] private float outlineWidth = 4f;

    private bool playerInRange;
    private bool highlightActive;

    private readonly List<MaterialHighlightState> highlightStates = new List<MaterialHighlightState>();

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"[Loot] {name}: No Collider component found! Loot requires a Collider (set as Trigger) to detect player proximity.");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"[Loot] {name}: Collider is not set as Trigger! Setting it to Trigger automatically.");
            col.isTrigger = true;
        }

        CacheHighlightMaterials();

        ValidateNaming();
    }

    private string FormatItemIdAsDisplayName(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return "Unknown Item";

        // Alt çizgileri ve tireleri boşlukla değiştir, kelimelerin ilk harfini büyük yap
        string formatted = id.Replace('_', ' ').Replace('-', ' ');
        string[] words = formatted.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpper(words[i][0]) + (words[i].Length > 1 ? words[i].Substring(1).ToLower() : "");
            }
        }
        
        return string.Join(" ", words);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Player tag kontrolü veya CharacterController kontrolü
        bool isPlayer = other.CompareTag("Player") || other.GetComponent<CharacterController>() != null;
        
        if (isPlayer)
        {
            playerInRange = true;
            UpdateHighlight(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Player tag kontrolü veya CharacterController kontrolü
        bool isPlayer = other.CompareTag("Player") || other.GetComponent<CharacterController>() != null;
        
        if (isPlayer)
        {
            playerInRange = false;
            UpdateHighlight(false);
        }
    }
    
    /// <summary>
    /// PlayerLootCollector tarafından çağrılır - highlight'ı günceller
    /// </summary>
    public void SetPlayerInRange(bool inRange)
    {
        if (playerInRange != inRange)
        {
            playerInRange = inRange;
            UpdateHighlight(inRange);
        }
    }

    // Update metodu kaldırıldı - Artık PlayerLootCollector E tuşu ile topluyor
    // Bu sayede daha uzaktan loot toplanabilir
    
    /// <summary>
    /// Loot'u toplar (PlayerLootCollector tarafından çağrılır)
    /// </summary>
    public void TryCollect()
    {
        if (string.IsNullOrWhiteSpace(itemDisplayName))
        {
            ValidateNaming();
        }

        if (InventoryManager.instance == null)
        {
            Debug.LogError($"[Loot] {name}: No InventoryManager instance found in the scene! Please add InventoryManager to a GameObject in the scene.");
            return;
        }

        Scrap scrap = new Scrap(itemId, itemDisplayName, scrapValue);
        InventoryManager.instance.AddScrap(scrap);
        
        // Nesneyi tamamen yok et
        Destroy(gameObject);
    }
    
    // Enemy'lerin loot bilgilerine erişmesi için public metodlar
    public string GetItemId()
    {
        return itemId;
    }
    
    public string GetItemDisplayName()
    {
        if (string.IsNullOrWhiteSpace(itemDisplayName))
        {
            ValidateNaming();
        }
        return itemDisplayName;
    }
    
    public int GetScrapValue()
    {
        return scrapValue;
    }
    private void ValidateNaming()
    {
        if (string.IsNullOrWhiteSpace(itemId) && string.IsNullOrWhiteSpace(itemDisplayName))
        {
            itemDisplayName = "Unknown Item";
            Debug.LogWarning($"Loot on {name}: Both Item ID and Display Name are empty! Using 'Unknown Item' as fallback.");
            return;
        }

        if (string.IsNullOrWhiteSpace(itemDisplayName))
        {
            itemDisplayName = FormatItemIdAsDisplayName(itemId);
            Debug.LogWarning($"Loot on {name}: Display Name was empty. Auto-generated from Item ID: '{itemDisplayName}'");
        }
    }

    private void CacheHighlightMaterials()
    {
        highlightStates.Clear();

        if (!useHighlight)
        {
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            Material[] materials = renderer.materials;
            foreach (Material mat in materials)
            {
                if (mat == null)
                {
                    continue;
                }

                bool hasEmissionProperty = mat.HasProperty("_EmissionColor");
                Color originalEmission = hasEmissionProperty ? mat.GetColor("_EmissionColor") : Color.black;
                bool originallyHadEmission = hasEmissionProperty && mat.IsKeywordEnabled("_EMISSION");
                bool hasOutlineColor = adjustOutline && mat.HasProperty("_OutlineColor");
                Color originalOutlineColor = hasOutlineColor ? mat.GetColor("_OutlineColor") : Color.black;
                bool hasOutlineWidth = adjustOutline && mat.HasProperty("_OutlineWidth");
                float originalOutlineWidth = hasOutlineWidth ? mat.GetFloat("_OutlineWidth") : 0f;

                highlightStates.Add(new MaterialHighlightState
                {
                    Material = mat,
                    OriginalEmission = originalEmission,
                    OriginallyHadEmission = originallyHadEmission,
                    HasEmissionProperty = hasEmissionProperty,
                    HasOutlineColor = hasOutlineColor,
                    OriginalOutlineColor = originalOutlineColor,
                    HasOutlineWidth = hasOutlineWidth,
                    OriginalOutlineWidth = originalOutlineWidth
                });
            }
        }
    }

    private void UpdateHighlight(bool enable)
    {
        if (!useHighlight || highlightActive == enable)
        {
            return;
        }

        highlightActive = enable;

        foreach (MaterialHighlightState state in highlightStates)
        {
            Material mat = state.Material;
            if (mat == null)
            {
                continue;
            }

            if (state.HasEmissionProperty)
            {
                if (enable)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", highlightColor * Mathf.LinearToGammaSpace(highlightIntensity));
                }
                else
                {
                    mat.SetColor("_EmissionColor", state.OriginalEmission);
                    if (state.OriginallyHadEmission)
                    {
                        mat.EnableKeyword("_EMISSION");
                    }
                    else
                    {
                        mat.DisableKeyword("_EMISSION");
                    }
                }
            }

            if (state.HasOutlineColor)
            {
                mat.SetColor("_OutlineColor", enable ? highlightColor : state.OriginalOutlineColor);
            }

            if (state.HasOutlineWidth)
            {
                mat.SetFloat("_OutlineWidth", enable ? outlineWidth : state.OriginalOutlineWidth);
            }
        }
    }

    private void OnDisable()
    {
        UpdateHighlight(false);
    }

    private void OnDestroy()
    {
        UpdateHighlight(false);
    }

    private struct MaterialHighlightState
    {
        public Material Material;
        public Color OriginalEmission;
        public bool OriginallyHadEmission;
        public bool HasEmissionProperty;
        public bool HasOutlineColor;
        public Color OriginalOutlineColor;
        public bool HasOutlineWidth;
        public float OriginalOutlineWidth;
    }
}
