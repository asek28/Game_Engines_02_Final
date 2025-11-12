using System.Collections.Generic;
using UnityEngine;

public class Thresh_Spawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField, Min(0f)] private float spawnRadius = 5f;
    [SerializeField] private float spawnHeightOffset = 0f;
    [SerializeField] private bool randomizeYRotation = true;
    [SerializeField, Min(1)] private int duplicatesPerTemplate = 1;

    private readonly List<Transform> lootTemplates = new List<Transform>();

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

        foreach (Transform template in lootTemplates)
        {
            if (template == null)
            {
                continue;
            }

            for (int i = 0; i < duplicatesPerTemplate; i++)
            {
                Vector3 offset = Random.insideUnitSphere;
                offset.y = 0f;
                if (offset.sqrMagnitude > 0.001f)
                {
                    offset.Normalize();
                }
                offset *= Random.Range(0f, spawnRadius);

                Vector3 spawnPosition = transform.position + new Vector3(offset.x, spawnHeightOffset, offset.z);
                Quaternion spawnRotation = randomizeYRotation
                    ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                    : template.rotation;

                GameObject instance = Instantiate(template.gameObject, spawnPosition, spawnRotation, transform.parent);
                instance.SetActive(true);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * spawnHeightOffset, spawnRadius);
    }
}
