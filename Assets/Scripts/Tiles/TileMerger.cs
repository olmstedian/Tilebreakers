using UnityEngine;
using System.Collections.Generic;

public class TileMerger : MonoBehaviour
{
    /// <summary>
    /// Merges movingTile into staticTile if they have matching colors and meet the distance-based merging criteria.
    /// </summary>
    /// <param name="staticTile">The tile that remains in place</param>
    /// <param name="movingTile">The tile being merged (will be destroyed)</param>
    /// <param name="splitThreshold">Value at which tiles split (defaults to 12)</param>
    /// <returns>True if merge was successful, false otherwise</returns>
    public static bool MergeTiles(Tile staticTile, Tile movingTile, int splitThreshold = 12)
    {
        Debug.Log($"TileMerger: MergeTiles called with staticTile:{staticTile?.number}, movingTile:{movingTile?.number}");
        
        if (staticTile == null) {
            Debug.LogError("TileMerger: staticTile is null!");
            return false;
        }
        
        if (movingTile == null) {
            Debug.LogError("TileMerger: movingTile is null!");
            return false;
        }
        
        if (staticTile == movingTile) {
            Debug.LogError("TileMerger: Cannot merge a tile with itself!");
            return false;
        }

        staticTile.ClearSelectionState();
        movingTile.ClearSelectionState();

        if (!ColorMatch(staticTile.tileColor, movingTile.tileColor)) {
            Debug.LogError("TileMerger: Color mismatch between tiles!");
            return false;
        }

        // Ensure the tiles are within the allowed distance
        Vector2Int staticPos = BoardManager.Instance.GetGridPositionFromWorldPosition(staticTile.transform.position);
        Vector2Int movingPos = BoardManager.Instance.GetGridPositionFromWorldPosition(movingTile.transform.position);
        Vector2Int direction = movingPos - staticPos;
        
        Debug.Log($"TileMerger: staticPos:{staticPos}, movingPos:{movingPos}, direction:{direction}");
        Debug.Log($"TileMerger: Manhattan distance:{Mathf.Abs(direction.x) + Mathf.Abs(direction.y)}, movingTile.number:{movingTile.number}");

        // Validate the move is orthogonal and within range
        if ((Mathf.Abs(direction.x) + Mathf.Abs(direction.y)) > movingTile.number || 
            (direction.x != 0 && direction.y != 0))
        {
            Debug.LogError("TileMerger: Merge failed. Tiles are not within the allowed distance or direction.");
            return false;
        }

        // Perform the merge
        int originalNumber = staticTile.number;
        staticTile.number += movingTile.number;
        Debug.Log($"TileMerger: Merging numbers {originalNumber} + {movingTile.number} = {staticTile.number}");
        
        staticTile.UpdateVisuals();
        
        // Destroy the moving tile
        Debug.Log("TileMerger: Destroying moving tile");
        Object.Destroy(movingTile.gameObject);

        // Update the board state
        Debug.Log($"TileMerger: Updating board state - clearing cell {movingPos} and setting {staticPos}");
        BoardManager.Instance.ClearCell(movingPos);
        BoardManager.Instance.SetTileAtPosition(staticPos, staticTile);

        // Always update the lastMergedCellPosition
        Debug.Log($"TileMerger: Setting lastMergedCellPosition to {staticPos}");
        BoardManager.Instance.lastMergedCellPosition = staticPos;

        // Add score for the merge
        ScoreManager.Instance.AddMergeScore(staticTile.number);
        Debug.Log($"TileMerger: Added merge score for value {staticTile.number}");

        // Trigger special tile spawning after the merge via GameManager instead of directly
        if (Random.value < Constants.SPECIAL_TILE_CHANCE)
        {
            Debug.Log("TileMerger: Triggering special tile spawn via GameManager");
            GameManager.Instance.SpawnSpecialTile(staticPos, "Blaster"); // Choose a default special tile
        }

        // Check if we need to split the tile
        if (staticTile.number > splitThreshold)
        {
            Debug.Log($"TileMerger: Tile value {staticTile.number} exceeds split threshold {splitThreshold}, splitting");
            TileSplitter.SplitTile(staticTile, staticPos);
        }

        Debug.Log("TileMerger: Merge completed successfully");
        return true;
    }

    private static bool ColorMatch(Color a, Color b)
    {
        const float tolerance = 0.01f;
        return Mathf.Abs(a.r - b.r) < tolerance && 
               Mathf.Abs(a.g - b.g) < tolerance && 
               Mathf.Abs(a.b - b.b) < tolerance;
    }
}
