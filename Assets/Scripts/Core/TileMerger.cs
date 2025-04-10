using UnityEngine;
using System.Collections.Generic;

public static class TileMerger
{
    /// <summary>
    /// Merges two tiles, increasing the value of the target tile.
    /// Destroys the source tile after merging.
    /// </summary>
    /// <param name="targetTile">The tile that will remain after merging</param>
    /// <param name="sourceTile">The tile that will be destroyed after merging</param>
    /// <returns>True if the merge was successful, false otherwise</returns>
    public static bool MergeTiles(Tile targetTile, Tile sourceTile)
    {
        if (targetTile == null || sourceTile == null)
        {
            Debug.LogError("TileMerger: Cannot merge null tiles!");
            return false;
        }

        // Check that the tiles have the same color
        if (!AreSameColor(targetTile.tileColor, sourceTile.tileColor))
        {
            Debug.LogError("TileMerger: Cannot merge tiles of different colors!");
            return false;
        }

        Debug.Log($"TileMerger: Merging tiles {sourceTile.number} and {targetTile.number}");

        // Log the original values for verification
        int originalSourceNumber = sourceTile.number;
        int originalTargetNumber = targetTile.number;
        
        // Store the position of the target tile for board reference
        Vector2Int targetPosition = BoardManager.Instance.GetGridPositionFromWorldPosition(targetTile.transform.position);

        // Add the values of the two tiles
        targetTile.number += sourceTile.number;

        Debug.Log($"TileMerger: Merge result: {originalSourceNumber} + {originalTargetNumber} = {targetTile.number}");

        // Update the target tile's visuals
        targetTile.UpdateVisuals();
        targetTile.SetState(Tile.TileState.Merging);

        // Update score
        ScoreManager.Instance?.AddScore(targetTile.number);

        // Store reference to the source tile GameObject before destroying it
        GameObject sourceTileObject = sourceTile.gameObject;

        // Update board reference to the merged cell position
        BoardManager.Instance.lastMergedCellPosition = targetPosition;

        // Destroy the source tile
        if (sourceTileObject != null)
        {
            // Check if the source object still exists - it should!
            if (sourceTileObject)
            {
                Debug.Log($"TileMerger: Destroying source tile GameObject with value {originalSourceNumber}");
                Object.Destroy(sourceTileObject);
            }
            else
            {
                Debug.LogWarning("TileMerger: Source tile GameObject was already destroyed before TileMerger could destroy it!");
            }
        }

        // Verify source tile destruction after a short delay
        if (Application.isPlaying)
        {
            // We can't use coroutines in a static class, so we'll add a verification component
            // to the target tile that will check after a delay
            MergeVerifier verifier = targetTile.gameObject.AddComponent<MergeVerifier>();
            verifier.Initialize(sourceTileObject, originalSourceNumber, originalTargetNumber, targetPosition);
        }

        return true;
    }

    private static bool AreSameColor(Color a, Color b)
    {
        const float tolerance = 0.01f; // Adjust tolerance if needed
        return Mathf.Abs(a.r - b.r) < tolerance && 
               Mathf.Abs(a.g - b.g) < tolerance && 
               Mathf.Abs(a.b - b.b) < tolerance;
    }
}

/// <summary>
/// Helper MonoBehaviour to verify that a source tile was properly destroyed after merging
/// </summary>
public class MergeVerifier : MonoBehaviour
{
    private GameObject _sourceTileObject;
    private int _sourceValue;
    private int _targetValue;
    private Vector2Int _mergePosition;
    private float _checkTime = 0.3f; // Time to wait before checking
    private float _timer = 0f;
    private bool _verified = false;

    public void Initialize(GameObject sourceTileObject, int sourceValue, int targetValue, Vector2Int mergePosition)
    {
        _sourceTileObject = sourceTileObject;
        _sourceValue = sourceValue;
        _targetValue = targetValue;
        _mergePosition = mergePosition;
    }

    private void Update()
    {
        if (_verified) return;
        
        _timer += Time.deltaTime;
        
        if (_timer >= _checkTime)
        {
            _verified = true;
            
            // Check if the source tile still exists
            if (_sourceTileObject != null)
            {
                Debug.LogError($"MergeVerifier: Source tile with value {_sourceValue} was not properly destroyed after merging at position {_mergePosition}. Destroying it now.");
                Destroy(_sourceTileObject);
            }
            else
            {
                Debug.Log($"MergeVerifier: Source tile destruction verified for merge ({_sourceValue} + {_targetValue} = {_sourceValue + _targetValue}) at position {_mergePosition}.");
            }
            
            // Self-destruct after verification
            Destroy(this);
        }
    }
}