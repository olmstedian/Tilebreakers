using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// State that handles the destruction of tiles with animations and effects.
/// </summary>
public class DestructionState : GameState
{
    private List<(Tile tile, Vector2Int position)> tilesToDestroy;
    // private bool destructionComplete;
    private float destructionDelay = 0.05f; // Delay between each tile destruction for cascading effect
    
    public DestructionState(List<(Tile tile, Vector2Int position)> tilesToDestroy)
    {
        this.tilesToDestroy = tilesToDestroy ?? new List<(Tile, Vector2Int)>();
    }
    
    public override void Enter()
    {
        Debug.Log($"DestructionState: Entered state with {tilesToDestroy.Count} tiles to destroy");
        // Removed: destructionComplete = false;
        
        if (tilesToDestroy.Count == 0)
        {
            Debug.LogWarning("DestructionState: No tiles to destroy. Transitioning immediately to next state.");
            TransitionToNextState();
            return;
        }
        
        // Start the destruction process with cascading animation
        GameStateManager.Instance.StartCoroutine(
            TileDestructionHandler.Instance.DestroyTilesWithCascadingAnimation(
                tilesToDestroy, 
                destructionDelay
            ).OnComplete(OnDestructionComplete)
        );
    }
    
    private void OnDestructionComplete()
    {
        Debug.Log("DestructionState: All tile destruction animations completed");
        // Removed: destructionComplete = true;
        
        // Verify the board state is consistent after destruction
        BoardManager.Instance.ValidateEmptyCellsAfterStateChange();
        BoardManager.Instance.PostMergeCleanup();
        
        // Transition to the next state
        TransitionToNextState();
    }
    
    private void TransitionToNextState()
    {
        // Determine the next state based on game context
        if (LevelManager.Instance != null && LevelManager.Instance.IsLevelComplete())
        {
            Debug.Log("DestructionState: Level objectives met. Transitioning to level complete check.");
            LevelManager.Instance.CheckLevelCompletion();
        }
        else if (!BoardManager.Instance.HasValidMove())
        {
            Debug.Log("DestructionState: No valid moves remain. Transitioning to game over check.");
            GameStateManager.Instance.SetState(new CheckingGameOverState());
        }
        else
        {
            Debug.Log("DestructionState: Transitioning to SpawningNewTileState");
            GameStateManager.Instance.SetState(new SpawningNewTileState());
        }
    }
    
    public override void Update()
    {
        // Logic is handled in the coroutine
    }
    
    public override void Exit()
    {
        Debug.Log("DestructionState: Exited state");
    }
    
    public override void HandleInput(Vector2Int position)
    {
        // No input handling in this state
    }
}

/// <summary>
/// Extension method to make coroutine callbacks cleaner.
/// </summary>
public static class CoroutineExtensions
{
    public static System.Collections.IEnumerator OnComplete(this System.Collections.IEnumerator coroutine, System.Action onComplete)
    {
        yield return coroutine;
        onComplete?.Invoke();
    }
}
