using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// State that handles evaluation after merges to determine if any tiles need to be split
/// or if other post-merge effects need to be triggered.
/// </summary>
public class PostMergeEvaluationState : GameState
{
    private float evaluationDelay = 0.3f; // Short delay for visual feedback before evaluation
    // private bool evaluationComplete = false;

    public override void Enter()
    {
        Debug.Log("PostMergeEvaluationState: Entered state");
        // Removed: evaluationComplete = false;
        
        // First ensure the board's emptyCells collection is accurate
        BoardManager.Instance.ValidateEmptyCellsAfterStateChange();
        
        // Check if we have any registered tiles from previous operation
        List<Vector2Int> registeredTiles = TileSplitHandler.GetTilesToSplit();
        Debug.Log($"PostMergeEvaluationState: Found {registeredTiles.Count} previously registered tiles");
        
        // Find additional tiles that need to be split (those with value > 12)
        List<Vector2Int> tilesToSplit = TileSplitHandler.FindTilesToSplit();
        Debug.Log($"PostMergeEvaluationState: Found {tilesToSplit.Count} additional tiles to split with values > 12");
        
        // Combine both lists (maintaining uniqueness)
        if (tilesToSplit.Count > 0)
        {
            foreach (var pos in tilesToSplit)
            {
                if (!registeredTiles.Contains(pos))
                {
                    registeredTiles.Add(pos);
                }
            }
        }
        
        // Final check: Log the actual board state to verify high-value tiles
        Debug.Log("PostMergeEvaluationState: Performing full board scan to verify high-value tiles");
        for (int x = 0; x < BoardManager.Instance.width; x++)
        {
            for (int y = 0; y < BoardManager.Instance.height; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                Tile tile = BoardManager.Instance.GetTileAtPosition(position);
                
                if (tile != null)
                {
                    Debug.Log($"PostMergeEvaluationState: Position {position} has tile with value {tile.number}");
                    if (tile.number > 12 && !registeredTiles.Contains(position))
                    {
                        Debug.LogWarning($"PostMergeEvaluationState: MISSED high-value tile at {position} with value {tile.number}! Adding to split list.");
                        registeredTiles.Add(position);
                    }
                }
            }
        }

        if (registeredTiles.Count > 0)
        {
            Debug.Log($"PostMergeEvaluationState: Total of {registeredTiles.Count} tiles need splitting");
            
            // Make sure all high-value tiles are registered for splitting
            TileSplitHandler.ClearAndSetTilesToSplit(registeredTiles);
            
            // Transition to SplittingTilesState to handle the splitting
            GameStateManager.Instance.EnterSplittingTilesState();
        }
        else
        {
            Debug.Log("PostMergeEvaluationState: No tiles need splitting, proceeding to spawn new tiles");
            
            // Final board cleanup before proceeding
            BoardManager.Instance.PostMergeCleanup();
            
            // Schedule transition to SpawningNewTileState after a short delay
            GameStateManager.Instance.SetStateWithDelay(new SpawningNewTileState(), evaluationDelay);

            // Show popup for points if needed, but only once and only +points (not duplicated)
            // Example: (if you want to show a popup here, do it like this)
            // int points = ...; // get points to show if needed
            // Vector2 topCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.85f);
            // string popupText = points >= 0 ? $"+{points}" : points.ToString();
            // ScoreUtility.ShowPopupAtScreenPosition(points, topCenter, popupText);
        }

        // Removed: evaluationComplete = true;
    }

    public override void Update()
    {
        // State logic is primarily handled in Enter
    }

    public override void Exit()
    {
        Debug.Log("PostMergeEvaluationState: Exited state");
    }

    public override void HandleInput(Vector2Int position)
    {
        // No input handling in this state
    }
}
