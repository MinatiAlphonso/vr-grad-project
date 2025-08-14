using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private Dictionary<CollectibleType, int> itemCounts = new();
    [SerializeField] private TextMeshProUGUI gemText;
    [SerializeField] private TextMeshProUGUI keyText;
    [SerializeField] private TextMeshProUGUI trophyText;

    public void AddItem(CollectibleType type)
    {
        if (!itemCounts.ContainsKey(type))
        {
            itemCounts[type] = 0;
        }

        itemCounts[type]++;
        Debug.Log($"{type} Found: {itemCounts[type]}");

        switch (type)
        {
            case CollectibleType.Gem:
                gemText.text = itemCounts[type].ToString();
                break;
            case CollectibleType.Key:
                keyText.text = itemCounts[type].ToString();
                break;
            case CollectibleType.Trophy:
                trophyText.text = itemCounts[type].ToString();
                break;
        }
    }    
}