using UnityEngine;
using System.Collections.Generic;

public class TileMerger : MonoBehaviour
{
    /// <summary>
    /// Merges movingTile into staticTile if they have matching colors.
    /// The staticTile's number increases by movingTile's number.
    /// Splits the tile if it exceeds the threshold after merging.
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

        staticTile.number += movingTile.number;
        staticTile.UpdateVisuals();
        Object.Destroy(movingTile.gameObject);

        // Track the merged cell
        BoardManager.Instance.lastMergedCellPosition = BoardManager.Instance.GetGridPositionFromWorldPosition(staticTile.transform.position);

        // Add score for the merge
        ScoreManager.Instance.AddMergeScore(staticTile.number);

        staticTile.ClearSelectionState();

        // Delegate splitting logic to TileSplitter
        if (staticTile.number > splitThreshold)
        {
            TileSplitter.SplitTile(staticTile, BoardManager.Instance.GetGridPositionFromWorldPosition(staticTile.transform.position));
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
