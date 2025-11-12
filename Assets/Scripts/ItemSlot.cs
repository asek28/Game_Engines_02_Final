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
            Transform countTransform = transform.Find("Count");
            if (countTransform != null)
            {
                countText = countTransform.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                countText = GetComponentInChildren<TextMeshProUGUI>();
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
            countText.text = $"x{currentCount}";
        }
    }
}

