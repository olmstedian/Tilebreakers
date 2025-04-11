using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// State that handles evaluation after merges to determine if any tiles need to be split
/// or if other post-merge effects need to be triggered.
/// </summary>
public class PostMergeEvaluationState : GameState
{
    private float evaluationDelay = 0.3f; // Short delay for visual feedback before evaluation
    private bool evaluationComplete = false;

    public override void Enter()
    {
        Debug.Log("PostMergeEvaluationState: Entered state");
        evaluationComplete = false;
        
        // Find tiles that need to be split (those with value > 12)
        List<Vector2Int> tilesToSplit = TileSplitHandler.FindTilesToSplit();

        if (tilesToSplit.Count > 0)
        {
            Debug.Log($"PostMergeEvaluationState: Found {tilesToSplit.Count} tiles to split with values > 12");
            
            // Register tiles to split in the TileSplitHandler
            TileSplitHandler.RegisterTilesToSplit(tilesToSplit);
            
            // Transition to SplittingTilesState to handle the splitting
            GameStateManager.Instance.EnterSplittingTilesState();
        }
        else
        {
            // Double check for any tiles with values > 12 that might have been missed
            bool foundHighValueTiles = false;
            
            // Scan the entire board to be absolutely sure
            for (int x = 0; x < BoardManager.Instance.width; x++)
            {
                for (int y = 0; y < BoardManager.Instance.height; y++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    Tile tile = BoardManager.Instance.GetTileAtPosition(position);
                    
                    if (tile != null && tile.number > 12)
                    {
                        Debug.LogWarning($"PostMergeEvaluationState: Additional high-value tile found at {position} with value {tile.number}");
                        foundHighValueTiles = true;
                        tilesToSplit.Add(position);
                    }
                }
            }
            
            if (foundHighValueTiles)
            {
                Debug.Log($"PostMergeEvaluationState: Found additional {tilesToSplit.Count} high-value tiles in secondary check");
                TileSplitHandler.RegisterTilesToSplit(tilesToSplit);
                GameStateManager.Instance.EnterSplittingTilesState();
            }
            else
            {
                Debug.Log("PostMergeEvaluationState: No tiles need splitting, proceeding to spawn new tiles");
                
                // Schedule transition to SpawningNewTileState after a short delay
                GameStateManager.Instance.SetStateWithDelay(new SpawningNewTileState(), evaluationDelay);
            }
        }

        evaluationComplete = true;
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
