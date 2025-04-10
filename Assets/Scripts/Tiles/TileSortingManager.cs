using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages tile sorting order to ensure proper layering during animations and recovery after errors
/// </summary>
public class TileSortingManager : MonoBehaviour
{
    private static Dictionary<int, int> originalSortingOrders = new Dictionary<int, int>();
    
    /// <summary>
    /// Stores the original sorting order for a tile
    /// </summary>
    public static void StoreOriginalSortingOrder(GameObject tileObject)
    {
        if (tileObject == null) return;
        
        SpriteRenderer sr = tileObject.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            int instanceID = tileObject.GetInstanceID();
            if (!originalSortingOrders.ContainsKey(instanceID))
            {
                originalSortingOrders.Add(instanceID, sr.sortingOrder);
            }
        }
    }
    
    /// <summary>
    /// Temporarily lowers the sorting order for animations
    /// </summary>
    public static void SetAnimationSortingOrder(GameObject tileObject)
    {
        if (tileObject == null) return;
        
        StoreOriginalSortingOrder(tileObject);
        
        SpriteRenderer sr = tileObject.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = -1;
        }
    }
    
    /// <summary>
    /// Restores the original sorting order
    /// </summary>
    public static void RestoreSortingOrder(GameObject tileObject)
    {
        if (tileObject == null) return;
        
        int instanceID = tileObject.GetInstanceID();
        SpriteRenderer sr = tileObject.GetComponent<SpriteRenderer>();
        
        if (sr != null && originalSortingOrders.ContainsKey(instanceID))
        {
            sr.sortingOrder = originalSortingOrders[instanceID];
        }
        else if (sr != null && sr.sortingOrder < 0)
        {
            // Default restoration if original order not found
            sr.sortingOrder = 0;
            Debug.LogWarning($"TileSortingManager: Restored default sorting order for tile without stored value");
        }
    }
    
    /// <summary>
    /// Removes the stored sorting order for a destroyed tile
    /// </summary>
    public static void RemoveTile(GameObject tileObject)
    {
        if (tileObject == null) return;
        
        int instanceID = tileObject.GetInstanceID();
        if (originalSortingOrders.ContainsKey(instanceID))
        {
            originalSortingOrders.Remove(instanceID);
        }
    }
    
    /// <summary>
    /// Restore all tile sorting orders to their original values
    /// </summary>
    public static void RestoreAllSortingOrders()
    {
        List<int> invalidKeys = new List<int>();
        
        foreach (var entry in originalSortingOrders)
        {
            GameObject tileObject = FindObjectFromInstanceID(entry.Key);
            if (tileObject != null)
            {
                SpriteRenderer sr = tileObject.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = entry.Value;
                }
            }
            else
            {
                invalidKeys.Add(entry.Key);
            }
        }
        
        // Clean up invalid entries
        foreach (int key in invalidKeys)
        {
            originalSortingOrders.Remove(key);
        }
    }
    
    private static GameObject FindObjectFromInstanceID(int instanceID)
    {
        // Unity doesn't provide a direct method to get GameObject from instanceID
        // A workaround is to check all GameObjects in the scene
        foreach (Tile tile in Object.FindObjectsOfType<Tile>())
        {
            if (tile.gameObject.GetInstanceID() == instanceID)
            {
                return tile.gameObject;
            }
        }
        return null;
    }
}
