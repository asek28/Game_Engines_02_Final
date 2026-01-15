using TMPro;
using UnityEngine;

public class ItemSlot : MonoBehaviour
{
    [Header("Item Slot Settings")]
    [Tooltip("The item ID this slot represents (e.g., 'scrap_value5', 'scrap_value10')")]
    public string itemId;

    [Header("UI References")]
    [Tooltip("The Count text component that displays the quantity. If null, will search for a child named 'Count'.")]
    public TextMeshProUGUI countText;

    private int currentCount = 0;

    private void Awake()
    {
        if (countText == null)
        {
            // Önce "Count" isimli child ara
            Transform countTransform = transform.Find("Count");
            if (countTransform != null)
            {
                countText = countTransform.GetComponent<TextMeshProUGUI>();
                Debug.Log($"[ItemSlot] Found Count text by name for {gameObject.name}");
            }
            
            // Bulunamadıysa tüm TMP componentlerini ara
            if (countText == null)
            {
                TextMeshProUGUI[] tmpComponents = GetComponentsInChildren<TextMeshProUGUI>(true);
                if (tmpComponents != null && tmpComponents.Length > 0)
                {
                    countText = tmpComponents[0];
                    Debug.Log($"[ItemSlot] Found TMP component for {gameObject.name}: {countText.name}");
                }
            }
            
            // Hala bulunamadıysa uyar
            if (countText == null)
            {
                Debug.LogWarning($"[ItemSlot] ⚠️ Count text NOT found for {gameObject.name}! Item: {itemId}");
            }
        }

        UpdateCountDisplay();
    }

    public void SetItemId(string id)
    {
        itemId = id;
    }

    public void AddCount(int amount = 1)
    {
        currentCount += amount;
        UpdateCountDisplay();
    }

    public void SetCount(int count)
    {
        currentCount = Mathf.Max(0, count);
        UpdateCountDisplay();
    }

    public int GetCount()
    {
        return currentCount;
    }

    private void UpdateCountDisplay()
    {
        if (countText != null)
        {
            // Eğer count 0 ise boş göster, değilse "x" ile göster
            if (currentCount > 0)
            {
                countText.text = $"x{currentCount}";
            }
            else
            {
                countText.text = ""; // 0 ise boş
            }
            
            Debug.Log($"[ItemSlot] {gameObject.name} count updated: x{currentCount}");
        }
        else
        {
            Debug.LogWarning($"[ItemSlot] ⚠️ Cannot update count for {gameObject.name} - countText is NULL!");
        }
    }
}

