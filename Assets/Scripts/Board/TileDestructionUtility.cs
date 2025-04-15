using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Static utility class for tile destruction operations.
/// Acts as a bridge between special tiles and the TileDestructionHandler.
/// </summary>
public static class TileDestructionUtility
{
    /// <summary>
    /// Destroys a tile at the specified position
    /// </summary>
    public static void DestroyTileAtPosition(Vector2Int position)
    {
        if (!BoardManager.Instance.IsWithinBounds(position))
        {
            Debug.LogWarning($"TileDestructionUtility: Attempted to destroy tile at out-of-bounds position {position}");
            return;
        }

        Tile tile = BoardManager.Instance.GetTileAtPosition(position);
        if (tile != null)
        {
            DestroyTile(tile, position);
        }
        else
        {
            Debug.LogWarning($"TileDestructionUtility: No tile found at position {position}");
        }
    }

    /// <summary>
    /// Destroys a GameObject with Tile component, supporting direct GameObject parameter
    /// </summary>
    public static void DestroyTile(GameObject tileObject, Vector2Int position)
    {
        if (tileObject == null)
        {
            Debug.LogWarning($"TileDestructionUtility: Attempted to destroy null tile object at position {position}");
            return;
        }
        
        // Get the Tile component from the GameObject
        Tile tile = tileObject.GetComponent<Tile>();
        if (tile != null)
        {
            DestroyTile(tile, position);
        }
        else
        {
            // If there's no Tile component, just destroy the GameObject directly
            Debug.LogWarning($"TileDestructionUtility: GameObject at {position} doesn't have a Tile component. Destroying directly.");
            TileDestructionHandler.Instance.DestroyTile(tileObject, position);
        }
    }

    /// <summary>
    /// Destroys a tile with its position
    /// </summary>
    public static void DestroyTile(Tile tile, Vector2Int position)
    {
        if (tile == null)
        {
            Debug.LogWarning($"TileDestructionUtility: Attempted to destroy null tile at position {position}");
            return;
        }

        // Delegate to the TileDestructionHandler for proper destruction handling
        TileDestructionHandler.Instance.DestroyTile(tile.gameObject, position);
    }

    /// <summary>
    /// Destroys multiple tiles at specified positions
    /// </summary>
    public static void DestroyTilesAtPositions(List<Vector2Int> positions)
    {
        if (positions == null || positions.Count == 0)
        {
            Debug.LogWarning("TileDestructionUtility: Attempted to destroy tiles with empty position list");
            return;
        }

        // Queue the positions for destruction through TileDestructionHandler
        TileDestructionHandler.Instance.QueueTilesForDestruction(positions);
    }

    /// <summary>
    /// Destroys a GameObject with animation, supporting direct GameObject parameter
    /// </summary>
    public static void DestroyTileWithAnimation(GameObject tileObject, Vector2Int position, System.Action onComplete = null)
    {
        if (tileObject == null)
        {
            Debug.LogWarning($"TileDestructionUtility: Attempted to animate destruction of null GameObject at {position}");
            
            // Still execute callback if provided
            onComplete?.Invoke();
            return;
        }
        
        // Get the Tile component from the GameObject
        Tile tile = tileObject.GetComponent<Tile>();
        if (tile != null)
        {
            DestroyTileWithAnimation(tile, position, onComplete);
        }
        else
        {
            // If there's no Tile component, just destroy the GameObject directly with a simple animation
            Debug.LogWarning($"TileDestructionUtility: GameObject at {position} doesn't have a Tile component. Using simple animation.");
            
            // First disable collisions
            Collider2D collider = tileObject.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            
            // Simple fade and destroy animation
            TileDestructionHandler.Instance.StartCoroutine(SimpleDestroyAnimation(tileObject, position, onComplete));
        }
    }

    /// <summary>
    /// Destroys a tile with animation
    /// </summary>
    public static void DestroyTileWithAnimation(Tile tile, Vector2Int position, System.Action onComplete = null)
    {
        if (tile == null)
        {
            Debug.LogWarning($"TileDestructionUtility: Attempted to animate destruction of null tile at {position}");
            
            // Still execute callback if provided
            onComplete?.Invoke();
            return;
        }

        // Delegate to TileDestructionHandler
        TileDestructionHandler.Instance.StartCoroutine(
            TileDestructionHandler.Instance.DestroyTileWithAnimation(tile, position, onComplete)
        );
    }
    
    /// <summary>
    /// Simple destroy animation for GameObjects without Tile component
    /// </summary>
    private static IEnumerator SimpleDestroyAnimation(GameObject obj, Vector2Int position, System.Action onComplete = null)
    {
        if (obj == null)
        {
            onComplete?.Invoke();
            yield break;
        }
        
        // Scale down animation
        float duration = 0.3f;
        Vector3 startScale = obj.transform.localScale;
        float elapsed = 0;
        
        // Fade out renderer if available
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        Color startColor = renderer != null ? renderer.color : Color.white;
        
        while (elapsed < duration && obj != null)
        {
            float t = elapsed / duration;
            obj.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            
            if (renderer != null)
            {
                Color fadeColor = startColor;
                fadeColor.a = UnityEngine.Mathf.Lerp(1, 0, t);
                renderer.color = fadeColor;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Mark position as empty in board data if in bounds
        if (BoardManager.Instance.IsWithinBounds(position))
        {
            BoardManager.Instance.ClearCell(position);
            BoardManager.Instance.AddToEmptyCells(position);
        }
        
        // Destroy the GameObject
        Object.Destroy(obj);
        
        // Wait a small amount of time for visual clarity
        yield return new WaitForSeconds(0.05f);
        
        // Execute completion callback
        onComplete?.Invoke();
    }
}
