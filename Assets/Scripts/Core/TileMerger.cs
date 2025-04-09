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
        
        // Save original collider state and ensure it's enabled for the merged tile
        Collider2D staticTileCollider = staticTile.GetComponent<Collider2D>();
        bool wasColliderEnabled = staticTileCollider != null ? staticTileCollider.enabled : true;
        
        // Update visuals and make sure the merged tile remains interactive
        staticTile.UpdateVisuals();
        
        // Re-enable collider if it was disabled during animation
        if (staticTileCollider != null && !staticTileCollider.enabled && wasColliderEnabled)
        {
            Debug.Log("TileMerger: Re-enabling collider on merged tile");
            staticTileCollider.enabled = true;
        }
        
        // Reset the tile's state to ensure it can be selected again
        staticTile.SetState(Tile.TileState.Idle);
        
        // Store the reference to the GameObject before destruction
        GameObject movingTileObject = movingTile.gameObject;

        // Use the utility to safely destroy the moving tile
        TileDestructionUtility.DestroyTile(movingTileObject, movingPos);

        // Update the board state for the static tile
        Debug.Log($"TileMerger: Setting tile at {staticPos}");
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
            string specialTileType = "Random"; // Use weighted random selection
            
            // During testing, force Doubler tile sometimes
            if (Constants.TESTING_MODE && Random.value < 0.6f)
            {
                specialTileType = "Doubler";
                Debug.Log("TileMerger: Testing mode forced a Doubler tile spawn.");
            }
            
            GameManager.Instance.SpawnSpecialTile(staticPos, specialTileType);
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