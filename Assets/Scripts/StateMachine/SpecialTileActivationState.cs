using UnityEngine;
using Tilebreakers.Special; // Add this namespace to access SpecialTileManager

/// <summary>
/// Special tile activation state - handles player interaction with special tiles.
/// </summary>
public class SpecialTileActivationState : GameState
{
    public override void Enter()
    {
        Debug.Log("SpecialTileActivationState: Waiting for player to activate a special tile...");
    }

    public override void Update()
    {
        // No specific update logic for this state
    }

    public override void HandleInput(Vector2Int gridPosition)
    {
        SpecialTile specialTile = SpecialTileManager.Instance.GetSpecialTileAtPosition(gridPosition);
        if (specialTile != null)
        {
            GameStateManager.Instance.ActivateSpecialTile(gridPosition);
        }
        else
        {
            Debug.LogWarning("SpecialTileActivationState: No special tile found at the selected position.");
        }
    }

    public override void Exit()
    {
        Debug.Log("SpecialTileActivationState: Exiting state.");
    }
}
