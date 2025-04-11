using UnityEngine;
using System.Collections;

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
        TileMergeHandler.Instance.MergeTiles(sourceTile, targetTile);
        
        // Remember the position for potential logic after merge
        BoardManager.Instance.lastMergedCellPosition = targetPosition;

        // Give a small delay after merging before proceeding
        yield return new WaitForSeconds(0.2f);

        // Invoke the callback after merges are complete
        onComplete?.Invoke();
    }

    private void TransitionToNextState()
    {
        // Check if there are tiles registered for splitting
        // For now, proceed directly to post-merge evaluation since we can't check for splits)  // Changed method name to a likely alternative
        Debug.Log("MergingTilesState: Transitioning to PostMergeEvaluationState.");
        GameStateManager.Instance?.SetState(new PostMergeEvaluationState());
        
        // Note: This skips the SplittingTilesState since we cannot detect if tiles need splitting.
        // TODO: Implement proper detection of tiles needing splits when TileSplitHandler is updated.
    }
}
