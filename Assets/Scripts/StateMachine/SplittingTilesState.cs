using UnityEngine;
using System.Collections;

/// <summary>
/// Splitting tiles state - handles tile splitting logic.
/// </summary>
public class SplittingTilesState : GameState
{
    public override void Enter()
    {
        Debug.Log("SplittingTilesState: Splitting tiles...");

        // Trigger splitting logic
        BoardManager.Instance.StartCoroutine(HandleTileSplits(() =>
        {
            // Transition to SpawningNewTileState after splits are complete
            GameStateManager.Instance.SetState(new SpawningNewTileState(BoardManager.Instance.lastMergedCellPosition));
        }));
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("SplittingTilesState: Exiting state.");
    }

    /// <summary>
    /// Handles all tile splits and invokes a callback when complete.
    /// </summary>
    private IEnumerator HandleTileSplits(System.Action onComplete)
    {
        // Simulate split animations (replace with actual logic if needed)
        yield return new WaitForSeconds(Constants.TILE_MOVE_DURATION);

        // Example: Use TileSplitter for splitting logic
        Tile tileToSplit = BoardManager.Instance.GetTileAtPosition(BoardManager.Instance.lastMergedCellPosition.Value);
        if (tileToSplit != null)
        {
            TileSplitter.SplitTile(tileToSplit, BoardManager.Instance.lastMergedCellPosition.Value);
        }

        // Invoke the callback after splits are complete
        onComplete?.Invoke();
    }
}
