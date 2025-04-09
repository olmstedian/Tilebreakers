using UnityEngine;
using System.Collections;

/// <summary>
/// Utility class for reliable tile destruction across the game.
/// </summary>
public static class TileDestructionUtility
{
    /// <summary>
    /// Safely destroy a tile and clean up all references in the game board.
    /// </summary>
    public static void DestroyTile(GameObject tileObject, Vector2Int position)
    {
        if (tileObject == null)
        {
            Debug.LogWarning($"TileDestructionUtility: Attempted to destroy null tile object at position {position}");
            
            // Still clean up the board position
            BoardManager.Instance.ClearCell(position);
            BoardManager.Instance.AddToEmptyCells(position);
            
            return;
        }
        
        // Check for SpecialTile component
        SpecialTile specialTile = tileObject.GetComponent<SpecialTile>();
        if (specialTile != null)
        {
            SpecialTileManager.Instance?.UnregisterSpecialTile(specialTile);
            Debug.Log($"TileDestructionUtility: Unregistered special tile '{specialTile.specialAbilityName}' at {position}");
        }
        
        // Update board state
        BoardManager.Instance.ClearCell(position);
        BoardManager.Instance.AddToEmptyCells(position);
        
        // Destroy the GameObject
        Object.Destroy(tileObject);
        
        Debug.Log($"TileDestructionUtility: Destroyed tile at {position}");
    }
    
    /// <summary>
    /// Safely destroy a tile while ensuring all references are cleaned up.
    /// </summary>
    public static void DestroyTile(Tile tile)
    {
        if (tile == null)
        {
            Debug.LogWarning("TileDestructionUtility: Attempted to destroy null tile");
            return;
        }
        
        Vector2Int position = BoardManager.Instance.GetGridPositionFromWorldPosition(tile.transform.position);
        DestroyTile(tile.gameObject, position);
    }
    
    /// <summary>
    /// Verifies that a tile has been properly destroyed and performs cleanup if necessary.
    /// </summary>
    public static void VerifyTileDestruction(Vector2Int position)
    {
        // Check if the position is within board bounds
        if (!BoardManager.Instance.IsWithinBounds(position))
        {
            Debug.LogWarning($"TileDestructionUtility: Position {position} is out of bounds. Cannot verify destruction.");
            return;
        }
        
        // Check if there's still a tile at this position in the board array
        Tile tileAtPosition = BoardManager.Instance.GetTileAtPosition(position);
        if (tileAtPosition != null)
        {
            Debug.LogError($"TileDestructionUtility: Found tile at {position} after destruction! Cleaning up.");
            BoardManager.Instance.ClearCell(position);
            
            // If the GameObject still exists, destroy it
            if (tileAtPosition.gameObject != null)
            {
                Object.Destroy(tileAtPosition.gameObject);
            }
        }
        
        // Ensure the position is marked as empty
        bool empty = global::BoardManager.Instance.IsCellEmpty(position); // resolved ambiguity
        if (!empty)
        {
            Debug.LogWarning($"TileDestructionUtility: Cell at {position} is not marked as empty after destruction. Fixing...");
            BoardManager.Instance.AddToEmptyCells(position);
        }
        
        // Check for physical objects at this position
        Vector2 worldPos = BoardManager.Instance.GetWorldPosition(position);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, 0.4f);
        
        foreach (var collider in colliders)
        {
            // Skip triggers and UI elements
            if (collider.isTrigger || collider.CompareTag("Highlight")) continue;
            
            Tile remainingTile = collider.GetComponent<Tile>();
            SpecialTile remainingSpecialTile = collider.GetComponent<SpecialTile>();
            
            if (remainingTile != null || remainingSpecialTile != null)
            {
                Debug.LogError($"TileDestructionUtility: Found physical {(remainingTile != null ? "Tile" : "SpecialTile")} at {position} after destruction! Cleaning up.");
                Object.Destroy(collider.gameObject);
            }
        }
    }
}
