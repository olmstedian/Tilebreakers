using UnityEngine;
using System.Collections;
using Tilebreakers.Board; // Add this to import the TileMergeHandler in its new namespace

/// <summary>
/// State that handles tile movement before checking for merges.
/// </summary>
public class MovingTilesState : GameState
{
    private Vector2Int startPosition;
    private Vector2Int targetPosition;
    private Tile selectedTile;

    public override void Enter()
    {
        Debug.Log("MovingTilesState: Moving tiles...");

        // Get the current selected tile position
        selectedTile = TileSelectionHandler.Instance.GetSelectedTile();
        startPosition = TileSelectionHandler.Instance.GetSelectedTilePosition();
        targetPosition = TileSelectionHandler.Instance.GetTargetTilePosition();

        if (selectedTile == null)
        {
            Debug.LogWarning("MovingTilesState: No tile selected. Cannot move.");
            GameStateManager.Instance.SetState(new WaitingForInputState());
            return;
        }

        // Validate the move again
        if (!BoardManager.Instance.IsValidMove(startPosition, targetPosition, selectedTile, out bool pathClear) || !pathClear)
        {
            Debug.LogWarning($"MovingTilesState: Invalid move from {startPosition} to {targetPosition}.");
            GameStateManager.Instance.SetState(new WaitingForInputState());
            return;
        }

        // Start the move operation
        GameStateManager.Instance.StartCoroutine(MoveOperation());
    }

    private IEnumerator MoveOperation()
    {
        Debug.Log($"MovingTilesState: Moving tile from {startPosition} to {targetPosition}.");
        
        // Check if the target position contains a tile for merging
        Tile targetTile = BoardManager.Instance.GetTileAtPosition(targetPosition);
        
        // If target cell has a tile with same color, it's a merge operation
        if (targetTile != null && TileMergeHandler.Instance.CompareColors(selectedTile.tileColor, targetTile.tileColor))
        {
            // Will be handled by MergingTilesState
            Debug.Log("MovingTilesState: Target position has a tile with matching color. Will merge.");
            CompleteMove(startPosition, targetPosition, selectedTile, true);
            yield break;
        }
        
        // Move to empty cell if no tile at target or different color
        if (targetTile == null)
        {
            Debug.Log("MovingTilesState: Moving to empty cell.");
            CompleteMove(startPosition, targetPosition, selectedTile, false);
            yield break;
        }
        
        // If target tile exists but is different color, this is an invalid move
        if (targetTile != null)
        {
            Debug.LogWarning("MovingTilesState: Target position has a tile with different color. Invalid move.");
            GameStateManager.Instance.SetState(new WaitingForInputState());
            yield break;
        }
    }

    private void CompleteMove(Vector2Int startPosition, Vector2Int targetPosition, Tile selectedTile, bool willMerge)
    {
        // Move the tile on the board
        BoardManager.Instance.MoveTile(selectedTile, startPosition, targetPosition);
        
        // CRITICAL FIX: Ensure move is counted by calling EndTurn on GameManager
        if (!willMerge)
        {
            Debug.Log("MovingTilesState: Counting move by calling GameManager.EndTurn()");
            GameManager.Instance.EndTurn();
        }
        else
        {
            // For merges, EndTurn will be called after the merge is complete
            Debug.Log("MovingTilesState: Move will be counted after merge completion");
        }
        
        // Clear selection
        BoardManager.Instance.ClearSelection();
        
        // Transition to next state based on whether this is a merge operation
        if (willMerge)
        {
            GameStateManager.Instance.SetState(new MergingTilesState());
        }
        else
        {
            // Spawn a new tile after the move
            BoardManager.Instance.SpawnTileAfterMove();
            
            // Check game over condition
            GameStateManager.Instance.SetState(new CheckingGameOverState());
        }
    }

    public override void Update()
    {
        // State logic is handled in the coroutine
    }

    public override void Exit()
    {
        Debug.Log("MovingTilesState: Exited state");
    }

    public override void HandleInput(Vector2Int position)
    {
        // No input handling in this state
    }
}
