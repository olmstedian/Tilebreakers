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
        if (staticTile == null || movingTile == null || staticTile == movingTile) return false;

        staticTile.ClearSelectionState();
        movingTile.ClearSelectionState();

        if (!ColorMatch(staticTile.tileColor, movingTile.tileColor)) return false;

        // Ensure the tiles are within the allowed distance
        Vector2Int staticPos = BoardManager.Instance.GetGridPositionFromWorldPosition(staticTile.transform.position);
        Vector2Int movingPos = BoardManager.Instance.GetGridPositionFromWorldPosition(movingTile.transform.position);
        Vector2Int direction = movingPos - staticPos;

        if ((Mathf.Abs(direction.x) + Mathf.Abs(direction.y)) > movingTile.number || 
            (direction.x != 0 && direction.y != 0))
        {
            Debug.LogWarning("TileMerger: Merge failed. Tiles are not within the allowed distance or direction.");
            return false;
        }

        // Perform the merge
        staticTile.number += movingTile.number;
        staticTile.UpdateVisuals();
        Object.Destroy(movingTile.gameObject);

        // Update the board state
        BoardManager.Instance.ClearCell(movingPos);
        BoardManager.Instance.SetTileAtPosition(staticPos, staticTile);

        // Track the merged cell
        BoardManager.Instance.lastMergedCellPosition = staticPos;

        // Add score for the merge
        ScoreManager.Instance.AddMergeScore(staticTile.number);

        // Trigger special tile spawning after the merge
        BoardManager.Instance.TriggerSpecialTileSpawn(staticPos);

        // Delegate splitting logic to TileSplitter
        if (staticTile.number > splitThreshold)
        {
            TileSplitter.SplitTile(staticTile, staticPos);
        }

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
