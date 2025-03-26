using UnityEngine;

/// <summary>
/// Abstract base class for all game states
/// </summary>
public abstract class GameState
{
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
    
    public virtual void HandleInput(Vector2Int gridPosition) { }
}

/// <summary>
/// Initial game setup with no player interaction yet
/// </summary>
public class InitState : GameState
{
    public override void Enter()
    {
        // Initialize the game board
        BoardManager.Instance.GenerateRandomStartingTiles();
    }

    public override void Update()
    {
        // No update logic needed, immediately transition to player turn
        GameStateManager.Instance.SetState(new PlayerTurnState());
    }

    public override void Exit()
    {
        // No additional logic needed
    }
}

/// <summary>
/// Player's turn state - handles tile selection and movement
/// </summary>
public class PlayerTurnState : GameState
{
    // Using static variables to ensure selection state persists across state transitions
    // BUT is properly shared between PlayerTurnState instances
    private static Tile selectedTile;
    private static Vector2Int selectedPosition;
    private bool hasMoved = false;

    public override void Enter()
    {
        // Always reset selection state on enter - crucial for merge cleanup
        ClearAllSelectionState();
        hasMoved = false;
    }

    public override void Update()
    {
        // State logic handled through HandleInput
    }

    public override void HandleInput(Vector2Int gridPosition)
    {
        if (hasMoved) return;
        
        Tile clickedTile = BoardManager.Instance.GetTileAtPosition(gridPosition);
        
        // First selection - selecting a tile
        if (selectedTile == null)
        {
            if (clickedTile != null)
            {
                selectedTile = clickedTile;
                selectedPosition = gridPosition;
                BoardManager.Instance.HighlightValidMoves(gridPosition, clickedTile.number);
            }
        }
        // Second selection - moving the selected tile or unselecting
        else
        {
            // If clicking on the same tile that's already selected, unselect it
            if (clickedTile == selectedTile)
            {
                BoardManager.Instance.ClearHighlights();
                selectedTile = null;
                selectedPosition = Vector2Int.zero;
                return;
            }
            
            // If clicking on another tile with matching color, try to merge
            if (clickedTile != null && clickedTile != selectedTile)
            {
                if (BoardManager.Instance.IsAdjacent(selectedPosition, gridPosition) &&
                    BoardManager.Instance.CompareColors(selectedTile.tileColor, clickedTile.tileColor))
                {
                    // Store a temporary reference to the selected tile and position
                    Tile tempTile = selectedTile;
                    Vector2Int tempPos = selectedPosition;
                    
                    // CRITICAL: Clear ALL selection state BEFORE initiating merge
                    ClearAllSelectionState();
                    hasMoved = true;
                    
                    // Perform merge operation using the stored references
                    BoardManager.Instance.PerformMergeOperation(tempTile, clickedTile, tempPos, gridPosition);
                    
                    // Double-check that the selection state is still cleared after initiating merge
                    ClearAllSelectionState();
                    
                    // Transition to post-turn state after a delay
                    GameStateManager.Instance.SetStateWithDelay(new PostTurnState(), 0.5f);
                    return; // Critical - exit immediately
                }
                else
                {
                    // Selecting a different tile - clear existing highlight and select the new one
                    BoardManager.Instance.ClearHighlights();
                    selectedTile = clickedTile;
                    selectedPosition = gridPosition;
                    BoardManager.Instance.HighlightValidMoves(gridPosition, clickedTile.number);
                }
            }
            // If clicking on an empty, valid cell, move the tile
            else if (clickedTile == null && 
                     BoardManager.Instance.IsValidMove(selectedPosition, gridPosition, selectedTile.number))
            {
                // Store references before clearing state
                Tile tempTile = selectedTile;
                Vector2Int tempPos = selectedPosition;
                
                // Clear selection state completely
                selectedTile = null;
                selectedPosition = Vector2Int.zero;
                hasMoved = true;
                BoardManager.Instance.ClearHighlights();
                
                // Move the tile using stored references
                BoardManager.Instance.MoveTile(tempTile, tempPos, gridPosition);
                
                // Transition to post-turn state after a delay
                GameStateManager.Instance.SetStateWithDelay(new PostTurnState(), 0.5f);
            }
            // If invalid move, keep the selection or clear it based on the click
            else if (clickedTile == null)
            {
                // If clicking on an empty cell that's not a valid move, clear the selection
                BoardManager.Instance.ClearHighlights();
                selectedTile = null;
                selectedPosition = Vector2Int.zero;
            }
        }
    }

    public override void Exit()
    {
        // CRITICAL: Ensure all selection state is cleared when exiting
        ClearAllSelectionState();
    }
    
    // Add this helper method to explicitly reset selection state
    public static void ClearAllSelectionState()
    {
        selectedTile = null;
        selectedPosition = Vector2Int.zero;
        
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.ClearSelection();
            BoardManager.Instance.ClearHighlights();
        }
    }
}

/// <summary>
/// Post-turn state - handles end of turn actions like spawning new tiles
/// </summary>
public class PostTurnState : GameState
{
    public override void Enter()
    {
        // Make extra sure selection state is cleared
        PlayerTurnState.ClearAllSelectionState();
        
        // Spawn new tiles
        BoardManager.Instance.GenerateRandomStartingTiles(1, 1);
        
        // Check for game over
        if (!BoardManager.Instance.HasValidMove())
        {
            GameStateManager.Instance.SetState(new GameOverState());
        }
        else
        {
            GameStateManager.Instance.SetState(new PlayerTurnState());
        }
    }

    public override void Update()
    {
        // No update logic needed, handled in Enter
    }

    public override void Exit()
    {
        // No additional logic needed
    }
}

/// <summary>
/// Game over state - the player has no more valid moves
/// </summary>
public class GameOverState : GameState
{
    public override void Enter()
    {
        // Show game over UI, calculate final score, etc.
    }

    public override void Update()
    {
        // Wait for player input to restart
    }

    public override void HandleInput(Vector2Int gridPosition)
    {
        // Restart game on any input in this state
        GameStateManager.Instance.RestartGame();
    }

    public override void Exit()
    {
        // No additional logic needed
    }
}
