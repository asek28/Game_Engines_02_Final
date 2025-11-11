using UnityEngine;

public class Thresh_Spawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField, Min(0)] private int scrapValue5Count = 3;
    [SerializeField, Min(0)] private int scrapValue10Count = 1;
    [SerializeField, Min(0f)] private float spawnRadius = 5f;
    [SerializeField] private float spawnHeightOffset = 0f;
    [SerializeField] private bool randomizeYRotation = true;

    private Transform scrapValue5Template;
    private Transform scrapValue10Template;

    private void Awake()
    {
        scrapValue5Template = transform.Find("Scrap_value5");
        scrapValue10Template = transform.Find("Scrap_value10");

        DeactivateTemplate(scrapValue5Template);
        DeactivateTemplate(scrapValue10Template);
    }

    private void Start()
    {
        HideSpawner();
        SpawnScrap(scrapValue5Template, scrapValue5Count);
        SpawnScrap(scrapValue10Template, scrapValue10Count);
    }

    private void HideSpawner()
    {
        var renderers = GetComponents<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }
    }

    private void DeactivateTemplate(Transform template)
    {
        if (template == null)
        {
            Debug.LogWarning($"{name}: Missing template child.");
            return;
        }

        if (template.gameObject.activeSelf)
        {
            template.gameObject.SetActive(false);
        }
    }

    private void SpawnScrap(Transform template, int count)
    {
        if (template == null || count <= 0)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Vector3 offset = Random.insideUnitSphere;
            offset.y = 0f;
            offset.Normalize();
            offset *= Random.Range(0f, spawnRadius);

            Vector3 spawnPosition = transform.position + new Vector3(offset.x, spawnHeightOffset, offset.z);
            Quaternion spawnRotation = randomizeYRotation
                ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                : template.rotation;

            GameObject instance = Instantiate(template.gameObject, spawnPosition, spawnRotation, transform.parent);
            instance.SetActive(true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * spawnHeightOffset, spawnRadius);
    }
}
