using UnityEngine;
using Tilebreakers.Special; // Add this namespace to access SpecialTileManager

/// <summary>
/// Waiting for input state - waits for player input.
/// </summary>
public class WaitingForInputState : GameState
{
    public override void Enter()
    {
        Debug.Log("WaitingForInputState: Waiting for player input...");
        
        // Verify board state on entering WaitingForInputState
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.ValidateEmptyCellsCollection();
        }
    }

    public override void Update() { }

    public override void HandleInput(Vector2Int gridPosition)
    {
        // Additional validation for the clicked position
        if (!BoardManager.Instance.IsWithinBounds(gridPosition))
        {
            Debug.LogWarning($"WaitingForInputState: Position {gridPosition} is out of bounds");
            return;
        }
        
        Debug.Log($"WaitingForInputState: Player input at {gridPosition}");
        
        // Check if the position contains a special tile
        SpecialTile specialTile = SpecialTileManager.Instance?.GetSpecialTileAtPosition(gridPosition);
        if (specialTile != null)
        {
            Debug.Log($"WaitingForInputState: Special tile '{specialTile.specialAbilityName}' found at {gridPosition}. Activating...");
            specialTile.Activate();
            GameStateManager.Instance.SetState(new CheckingGameOverState());
            return;
        }

        // If we have a selected tile already, this could be a move or a merge
        bool hasSelectedTile = BoardManager.Instance.HasSelectedTile();
        
        // Get the tile at the clicked position
        Tile targetTile = BoardManager.Instance.GetTileAtPosition(gridPosition);
        
        // We're clicking on a tile
        if (targetTile != null)
        {
            Debug.Log($"WaitingForInputState: Selected regular tile at {gridPosition}.");
            
            if (hasSelectedTile)
            {
                // Check if the clicked tile can be merged with the selected tile
                BoardManager.Instance.HandleTileSelection(gridPosition);
            }
            else
            {
                // Select this tile
                BoardManager.Instance.HandleTileSelection(gridPosition);
            }
        }
        // We're clicking on an empty cell
        else if (hasSelectedTile)
        {
            Debug.Log($"WaitingForInputState: Empty cell clicked at {gridPosition} with a selected tile.");
            BoardManager.Instance.HandleTileMoveConfirmation(gridPosition);
        }
        else
        {
            Debug.Log($"WaitingForInputState: Empty cell clicked at {gridPosition} with no tile selected.");
        }
    }

    public override void Exit()
    {
        Debug.Log("WaitingForInputState: Exiting state.");
    }
}
