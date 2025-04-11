using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Merging tiles state - handles tile merging logic.
/// </summary>
public class MergingTilesState : GameState
{
    private Tile sourceTile;
    private Tile targetTile;
    private Vector2Int sourcePosition;
    private Vector2Int targetPosition;

    public override void Enter()
    {
        Debug.Log("MergingTilesState: Merging tiles...");

        // Get the selected and target tiles from BoardManager using its accessor methods
        sourcePosition = BoardManager.Instance.GetSelectedTilePosition();
        targetPosition = BoardManager.Instance.GetTargetTilePosition();
        sourceTile = BoardManager.Instance.GetTileAtPosition(sourcePosition);
        targetTile = BoardManager.Instance.GetTileAtPosition(targetPosition);

        if (sourceTile == null || targetTile == null)
        {
            Debug.LogWarning("MergingTilesState: Source or target tile is null. Skipping merge operation.");
            GameStateManager.Instance.SetState(new PostMergeEvaluationState());
            return;
        }

        // Trigger merging logic
        BoardManager.Instance.StartCoroutine(HandleTileMerges(() =>
        {
            // After merges are complete, transition to the next state
            TransitionToNextState();
        }));
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("MergingTilesState: Exited state");
    }

    /// <summary>
    /// Handles all tile merges and invokes a callback when complete.
    /// </summary>
    private IEnumerator HandleTileMerges(System.Action onComplete)
    {
        if (sourceTile == null || targetTile == null)
        {
            Debug.LogWarning("MergingTilesState: Cannot merge null tiles.");
            onComplete?.Invoke();
            yield break;
        }
        
        Debug.Log($"MergingTilesState: Merging tile {sourceTile.number} into tile {targetTile.number}");

        // Wait a short delay before merging
        yield return new WaitForSeconds(Constants.TILE_MOVE_DURATION);

        // Use TileMergeHandler.Instance to perform the merge
        bool mergeSuccessful = TileMergeHandler.Instance.MergeTiles(sourceTile, targetTile);
        
        // Remember the position for potential logic after merge
        BoardManager.Instance.lastMergedCellPosition = targetPosition;
        
        // Specifically check if the target tile now exceeds splitting threshold
        if (mergeSuccessful && targetTile != null && targetTile.number > 12)
        {
            Debug.Log($"MergingTilesState: Merged tile at {targetPosition} has value {targetTile.number} > 12, will be split");
            // Make sure this tile position is identified for splitting
            Vector2Int splitTilePos = targetPosition;
            TileSplitHandler.RegisterTilesToSplit(new List<Vector2Int>() { splitTilePos });
        }

        // Give a small delay after merging before proceeding
        yield return new WaitForSeconds(0.2f);

        // Invoke the callback after merges are complete
        onComplete?.Invoke();
    }

    private void TransitionToNextState()
    {
        // Find any additional tiles that need to be split (with value > 12)
        List<Vector2Int> tilesToSplit = TileSplitHandler.FindTilesToSplit();
        List<Vector2Int> registeredTiles = TileSplitHandler.GetTilesToSplit();
        
        // FIXED: Log more information for debugging the decision
        Debug.Log($"MergingTilesState: Found {tilesToSplit.Count} tiles to split by scanning, and {registeredTiles.Count} previously registered tiles");
        
        // CRITICAL FIX: Preserve already registered tiles when transitioning
        if (tilesToSplit.Count > 0 || registeredTiles.Count > 0)
        {
            // Register newly found tiles without clearing existing ones
            if (tilesToSplit.Count > 0) {
                Debug.Log($"MergingTilesState: Adding {tilesToSplit.Count} additional tiles that need splitting.");
                foreach (var pos in tilesToSplit)
                {
                    if (!registeredTiles.Contains(pos))
                    {
                        registeredTiles.Add(pos);
                    }
                }
                TileSplitHandler.RegisterTilesToSplit(registeredTiles);
            }
            
            Debug.LogWarning("MergingTilesState: Transitioning to SplittingTilesState for tiles with value > 12.");
            GameStateManager.Instance?.SetState(new SplittingTilesState());
        }
        else
        {
            // If no tiles to split, proceed to the post-merge evaluation state
            Debug.Log("MergingTilesState: No tiles need splitting. Transitioning to PostMergeEvaluationState.");
            GameStateManager.Instance?.SetState(new PostMergeEvaluationState());
        }
    }
}
