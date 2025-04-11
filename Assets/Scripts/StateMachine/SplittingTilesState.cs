using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// State that handles the splitting of high-value tiles into multiple smaller tiles.
/// </summary>
public class SplittingTilesState : GameState
{
    private float splitDelay = 0.2f; // Delay between splitting operations
    private bool splittingComplete = false;
    
    public override void Enter()
    {
        Debug.Log("SplittingTilesState: Entered state");
        splittingComplete = false;
        
        // Start splitting tiles asynchronously
        GameStateManager.Instance.StartCoroutine(SplitRegisteredTiles());
    }

    private IEnumerator SplitRegisteredTiles()
    {
        // Get all tiles registered for splitting
        List<Vector2Int> tilesToSplit = TileSplitHandler.GetTilesToSplit();
        Debug.Log($"SplittingTilesState: Processing {tilesToSplit.Count} tiles to split");
        
        if (tilesToSplit.Count == 0)
        {
            Debug.LogWarning("SplittingTilesState: No tiles to split were registered");
            TransitionToNextState();
            yield break;
        }
        
        // Process each tile one by one with a delay between operations
        foreach (Vector2Int position in tilesToSplit)
        {
            Tile tile = BoardManager.Instance.GetTileAtPosition(position);
            
            if (tile != null && tile.number > 12)
            {
                Debug.Log($"SplittingTilesState: Splitting tile at {position} with value {tile.number}");
                // Use TileSplitHandler instead of TileSplitter
                TileSplitHandler.PerformSplitOperation(tile, position);
                
                // Add a delay between splits for better visual feedback
                yield return new WaitForSeconds(splitDelay);
            }
            else
            {
                Debug.LogWarning($"SplittingTilesState: Tile at {position} is not valid for splitting or no longer exists");
            }
        }
        
        // Clear registered tiles after processing
        TileSplitHandler.ClearRegisteredTiles();
        
        // All splitting operations are complete
        splittingComplete = true;
        TransitionToNextState();
    }
    
    private void TransitionToNextState()
    {
        // Move to SpawningNewTileState after splitting is complete
        GameStateManager.Instance.SetState(new SpawningNewTileState());
    }

    public override void Update()
    {
        // Logic is handled in the coroutine
    }

    public override void Exit()
    {
        Debug.Log("SplittingTilesState: Exited state");
    }

    public override void HandleInput(Vector2Int position)
    {
        // No input handling in this state
    }
}
