using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// State that handles the splitting of high-value tiles into multiple smaller tiles.
/// </summary>
public class SplittingTilesState : GameState
{
    private float splitDelay = 0.2f; // Delay between splitting operations
    
    public override void Enter()
    {
        Debug.Log("SplittingTilesState: Entered state");
        
        // Start splitting tiles asynchronously
        GameStateManager.Instance.StartCoroutine(SplitRegisteredTiles());
        
        // If there's any code that calls AddSplitScore here, use the same pattern as above
        // Replace calls to ScoreManager.Instance.AddSplitScore(value) with:
        // int bonusPoints = Mathf.RoundToInt(value * 0.5f);
        // ScoreManager.Instance.AddScoreWithoutPopup(bonusPoints);
        // ScoreUtility.ShowPopupAtScreenPosition(bonusPoints, new Vector2(...));
    }

    private IEnumerator SplitRegisteredTiles()
    {
        // Get all tiles registered for splitting
        List<Vector2Int> tilesToSplit = TileSplitHandler.GetTilesToSplit();
        Debug.LogWarning($"SplittingTilesState: Processing {tilesToSplit.Count} registered tiles to split");
        
        if (tilesToSplit.Count == 0)
        {
            Debug.LogError("SplittingTilesState: No tiles to split were registered! This should not happen.");
            TransitionToNextState();
            yield break;
        }
        
        // Print all the tiles to be split for debugging
        foreach (Vector2Int pos in tilesToSplit)
        {
            Tile tile = BoardManager.Instance.GetTileAtPosition(pos);
            Debug.Log($"SplittingTilesState: Scheduled to split tile at {pos} with value {(tile != null ? tile.number : 0)}");
        }
        
        // Process each tile one by one with a delay between operations
        foreach (Vector2Int position in tilesToSplit)
        {
            Tile tile = BoardManager.Instance.GetTileAtPosition(position);
            
            if (tile != null)
            {
                Debug.LogWarning($"SplittingTilesState: About to split tile at {position} with value {tile.number}");
                
                // Additional check to ensure we're splitting valid tiles
                if (tile.number > 12)
                {
                    Debug.Log($"SplittingTilesState: Splitting tile at {position} with value {tile.number}");
                    
                    // Use TileSplitHandler to perform the splitting
                    TileSplitHandler.PerformSplitOperation(tile, position);
                    
                    // Check that the position is now properly cleared in the board array
                    if (BoardManager.Instance.GetTileAtPosition(position) != null)
                    {
                        Debug.LogError($"SplittingTilesState: After splitting, position {position} still has a tile! Forcing clear.");
                        BoardManager.Instance.ClearCell(position);
                    }
                    
                    // Verify position is now in emptyCells
                    if (!BoardManager.Instance.emptyCells.Contains(position))
                    {
                        Debug.LogError($"SplittingTilesState: After splitting, position {position} is not in emptyCells! Adding.");
                        BoardManager.Instance.emptyCells.Add(position);
                    }
                    
                    // Add a delay between splits for better visual feedback
                    yield return new WaitForSeconds(splitDelay);
                }
                else 
                {
                    Debug.LogError($"SplittingTilesState: Tile at {position} has value {tile.number} <= 12, skipping split");
                }
            }
            else
            {
                Debug.LogWarning($"SplittingTilesState: Tile at {position} no longer exists or is invalid");
            }
        }
        
        // Clear registered tiles after processing
        TileSplitHandler.ClearRegisteredTiles();
        
        // Verify board consistency before transitioning
        BoardManager.Instance.ValidateEmptyCellsAfterStateChange();
        
        // All splitting operations are complete
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
