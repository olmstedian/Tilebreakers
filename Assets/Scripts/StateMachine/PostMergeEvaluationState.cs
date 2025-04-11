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
            Debug.Log($"PostMergeEvaluationState: Found {tilesToSplit.Count} tiles to split");
            
            // Register tiles to split in the TileSplitHandler
            TileSplitHandler.RegisterTilesToSplit(tilesToSplit);
            
            // Transition to SplittingTilesState to handle the splitting
            GameStateManager.Instance.EnterSplittingTilesState();
        }
        else
        {
            Debug.Log("PostMergeEvaluationState: No tiles need splitting, proceeding to spawn new tiles");
            
            // Schedule transition to SpawningNewTileState after a short delay
            GameStateManager.Instance.SetStateWithDelay(new SpawningNewTileState(), evaluationDelay);
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
