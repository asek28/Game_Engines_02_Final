using UnityEngine;

public class Scrap
{
    public string ItemId { get; }
    public string Name { get; }
    public int Value { get; }

    public Scrap(string itemId, string name, int value)
    {
        ItemId = string.IsNullOrWhiteSpace(itemId) ? "unknown" : itemId;
        Name = string.IsNullOrWhiteSpace(name) ? "Unknown Scrap" : name;
        Value = Mathf.Max(0, value);
    }
}
